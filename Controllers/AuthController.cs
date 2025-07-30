using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SignInAPI.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;

namespace SignInAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _hasher = new();
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult Register(User user)
        {

            // Hash the password securely
            string hashedPassword = _hasher.HashPassword(user, user.Password);

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                // Check if user with this email already exists
                string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@Email", user.Email);

                conn.Open();
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    return BadRequest("User with this email already exists.");
                }

                // Insert the new user into the database
                string query = "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Username", user.Username);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@Password", hashedPassword);

                conn.Open();
                int rows = cmd.ExecuteNonQuery();

                if (rows > 0)
                    return Ok("User Registered Successfully");
                else
                    return BadRequest("Registration Failed");
            }
        }

        [HttpPost("signin")]
        public IActionResult SignIn([FromBody] User loginData)
        {
            string storedHashedPassword = string.Empty;
            string username = string.Empty;

            using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                string query = "SELECT Username, Password FROM Users WHERE Email = @Email";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", loginData.Email);

                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    username = reader["Username"]?.ToString() ?? string.Empty;
                    storedHashedPassword = reader["Password"]?.ToString() ?? string.Empty;
                }
                else
                {
                    return Unauthorized("Invalid email or password");
                }
            }

            // Verify the hashed password
            var tempUser = new User { Email = loginData.Email };
            var result = _hasher.VerifyHashedPassword(tempUser, storedHashedPassword, loginData.Password);

            if (result == PasswordVerificationResult.Success)
            {
                return Ok(new { message = "Login Successful", username });
            }

            return Unauthorized("Invalid email or password");

        }
    }

}
