using Microsoft.AspNetCore.Mvc;
using TaskManagement_BE.Services;
using TaskManagement_BE.models;
using TaskManagement_BE.DTOs;
using System.Text.Json;

namespace TaskManagement_BE.controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid request data.");
            }

            try
            {
                var user = await _authService.LoginAsync(model.Username, model.Password);
                var jsonResult = JsonSerializer.Serialize(user, new JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(jsonResult);

                if (user != null)
                {
                    var (accessToken, expires) = await _authService.GenerateAccessTokenAsync(user);
                    var jsonAccessToken = JsonSerializer.Serialize(new { accessToken, expires }, new JsonSerializerOptions { WriteIndented = true });
                    Console.WriteLine(jsonAccessToken);

                    return Ok(new
                    {
                        Id = user.Id,
                        AccessToken = accessToken,
                        Expires = expires
                    });
                }

                return BadRequest("Invalid username or password.");

            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during login: {ex.Message}");
                return StatusCode(500, "An unexpected error occurred during login.");
            }
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto registerDto)
        {
            try
            {
                var result = await _authService.RegisterAsync(registerDto);
                if (result.Succeeded)
                {
                    return Ok(new
                    {
                        code = 201,
                        status = true,
                        message = "User registered successfully."
                    });
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new
                {
                    code = 400,
                    status = false,
                    message = "User registration failed.",
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new
                {
                    code = 500,
                    status = false,
                    message = "An unexpected error occurred.",
                    error = ex.Message
                });
            }
        }


        // [HttpPost("refresh")]
        // public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        // {
        //     try
        //     {
        //         var tokens = await _authService.RefreshTokenAsync(refreshToken);
        //         return Ok(new
        //         {
        //             AccessToken = tokens.AccessToken,
        //             RefreshToken = tokens.RefreshToken
        //         });
        //     }
        //     catch (UnauthorizedAccessException ex)
        //     {
        //         return Unauthorized(ex.Message);
        //     }
        // }
    }
}
