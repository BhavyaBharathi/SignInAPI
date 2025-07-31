using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SignInAPI
{
    public class Common
    {
        private readonly string _connectionString;

        public Common(string connectionString)
        {
            _connectionString = connectionString;
        }

        public int ExecuteNonQuery(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                        cmd.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }


        public Dictionary<string, object>? ExecuteReader(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                        cmd.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);
                }

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var result = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            result[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                        }
                        return result;
                    }
                }
            }
            return null;
        }

        public object? ExecuteScalarQuery(string sqlQuery, Dictionary<string, object>? parameters = null)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(sqlQuery, conn))
            {
                if (parameters != null)
                {
                    foreach (var param in parameters)
                    {
                        cmd.Parameters.AddWithValue("@" + param.Key, param.Value ?? DBNull.Value);
                    }
                }

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }

    }
}



