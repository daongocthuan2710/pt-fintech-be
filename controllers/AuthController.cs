using Microsoft.AspNetCore.Mvc;
using TaskManagement_BE.Services;
using TaskManagement_BE.models;
using TaskManagement_BE.DTOs;

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
            try
            {
                var accessToken = await _authService.LoginAsync(model.Username, model.Password);
                return Ok(new { AccessToken = accessToken });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterUserDto registerDto)
        {
            var result = await _authService.RegisterAsync(registerDto);
            if (result.Succeeded)
                return Ok("User registered successfully.");

            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { Errors = errors });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            try
            {
                var tokens = await _authService.RefreshTokenAsync(refreshToken);
                return Ok(new
                {
                    AccessToken = tokens.AccessToken,
                    RefreshToken = tokens.RefreshToken
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}
