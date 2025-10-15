using Microsoft.AspNetCore.Mvc;
using CityPulse.Models;
using CityPulse.Services.Abstractions;    // imports

namespace CityPulse.Controllers
{
    // ----------------------------------------------------------------------------
    // User authentication controller - handles login, register, logout
    public class UserController : Controller
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        //-----------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] UserLoginViewModel model)  // process login via AJAX
        {
            if (string.IsNullOrWhiteSpace(model.UsernameOrEmail) || string.IsNullOrWhiteSpace(model.Password))
            {
                return Json(new { success = false, message = "Please enter both username and password" });
            }

            var user = await _userService.LoginAsync(model.UsernameOrEmail, model.Password);
            if (user != null)
            {
                // Set session
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserFullName", $"{user.FirstName} {user.LastName}");
                
                // Ensure session is committed
                await HttpContext.Session.CommitAsync();

                return Json(new { success = true, message = "Login successful", userId = user.Id.ToString() });
            }

            return Json(new { success = false, message = "Invalid username or password" });
        }

        //-----------------------------------------------------------------------
        [HttpPost]
        public async Task<IActionResult> Register([FromBody] UserRegisterViewModel model)  // process registration via AJAX
        {
            // Basic validation
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email) || 
                string.IsNullOrWhiteSpace(model.Password) || string.IsNullOrWhiteSpace(model.FirstName) || 
                string.IsNullOrWhiteSpace(model.LastName))
            {
                return Json(new { success = false, message = "Please fill in all fields" });
            }

            if (model.Username.Length < 3)
            {
                return Json(new { success = false, message = "Username must be at least 3 characters" });
            }

            if (model.Password.Length < 6)
            {
                return Json(new { success = false, message = "Password must be at least 6 characters" });
            }

            if (model.Password != model.ConfirmPassword)
            {
                return Json(new { success = false, message = "Passwords do not match" });
            }

            var user = await _userService.RegisterAsync(model);
            if (user != null)
            {
                _logger.LogInformation("User registered successfully: {Username}, UserId: {UserId}", user.Username, user.Id);

                return Json(new { success = true, message = "Account created successfully! Please login.", username = user.Username });
            }

            return Json(new { success = false, message = "Username or email already exists" });
        }

        //-----------------------------------------------------------------------
        [HttpPost]
        public IActionResult Logout()  // logout user
        {
            HttpContext.Session.Clear();
            return Json(new { success = true });
        }
    }
}

//----------------------------------------------- <<< End of File >>>--------------------------------

