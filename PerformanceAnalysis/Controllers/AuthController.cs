using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PerformanceAnalysis.Application.Auth.Dtos;
using PerformanceAnalysis.Application.Auth.Services;

namespace PerformanceAnalysis.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authService;

    public AuthController(IAuthServices authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("login-with-cookie")]
    public async Task<IActionResult> LoginWithCookie([FromBody] LoginWithCookieRequest request)
    {
        try
        {
            var result = await _authService.LoginWithCookieAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize(AuthenticationSchemes = "Cookies")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _authService.LogoutAsync();
            return Ok("Вы успешно вышли из системы");
        } catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public IActionResult GetCurrentUser()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

        return Ok();
    }
}
