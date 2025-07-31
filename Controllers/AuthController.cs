using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SignInAPI.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using static Azure.Core.HttpHeader;

namespace SignInAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Common _common;
        private readonly PasswordHasher<User> _hasher = new();
        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            _common = new Common(_configuration.GetConnectionString("DefaultConnection"));
        }
        /// <summary>
        /// Registers a new user with a username, email, and password.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost("register")]
        public IActionResult Register(User user)
        {

            // Hash the password securely
            string hashedPassword = _hasher.HashPassword(user, user.Password);

            // Check if user with this email already exists
            string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email";
            var checkParams = new Dictionary<string, object>
            {
                { "Email", user.Email }
            };

            int count = Convert.ToInt32(_common.ExecuteScalarQuery(checkQuery, checkParams));
            if (count > 0)
            {
                return BadRequest("User with this email already exists.");
            }

            // Insert the new user
            string insertQuery = "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)";
            var insertParams = new Dictionary<string, object>
            {
                { "Username", user.Username },
                { "Email", user.Email },
                { "Password", hashedPassword }
            };

            int rows = _common.ExecuteNonQuery(insertQuery, insertParams);
            if (rows > 0)
                return Ok("User Registered Successfully");
            else
                return BadRequest("Registration Failed");
        }
        /// <summary>
        /// Authenticates a user with email and password, returning a success message and username if successful.
        /// </summary>
        /// <param name="loginData"></param>
        /// <returns></returns>
        [HttpPost("signin")]
        public IActionResult SignIn([FromBody] User loginData)
        {
            string storedHashedPassword = string.Empty;
            string username = string.Empty;

            string query = "SELECT Username, Password FROM Users WHERE Email = @Email";
            var parameters = new Dictionary<string, object>
            {
                { "Email", loginData.Email }
            };

            var userRow = _common.ExecuteReader(query, parameters);

            if (userRow == null)
            {
                return Unauthorized("Invalid email or password");
            }

            username = userRow["Username"]?.ToString() ?? string.Empty;
            storedHashedPassword = userRow["Password"]?.ToString() ?? string.Empty;


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
