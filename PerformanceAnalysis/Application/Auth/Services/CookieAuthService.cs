using Domain.Auth;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using PerformanceAnalysis.Application.Auth.Dtos;
using System.Security.Claims;

namespace PerformanceAnalysis.Application.Auth.Services;

public class CookieAuthService : ICookieAuthService
{
    private readonly AuthDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CookieAuthService(AuthDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResponse> LoginWithCookieAsync(LoginWithCookieRequest request, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u =>
                (u.Login == request.LoginOrEmail || u.Email == request.LoginOrEmail), ct);

        if (user == null)
        {
            throw new InvalidOperationException("Неверный логин или пароль");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Passwordhash))
        {
            throw new InvalidOperationException("Неверный логин или пароль");
        }

        var claims = BuildClaims(user);
        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
        var httpContext = GetHttpContext();

        var expiresAt = DateTime.UtcNow.AddDays(request.RememberMe ? 30 : 8);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = request.RememberMe,
            ExpiresUtc = expiresAt
        };

        await httpContext.SignInAsync("Cookies", new ClaimsPrincipal(claimsIdentity), authProperties);

        return new AuthResponse
        {
            UserId = user.Id,
            Login = user.Login,
            Email = user.Email,
            FirstName = user.Firstname,
            LastName = user.Lastname,
            Role = user.Role,
            UseCookies = true
        };
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync("Cookies");
        }
    }

    private Claim[] BuildClaims(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        if (user.Role == "Student" && user.Student != null)
        {
            claims.Add(new Claim("Phone", user.Student.Phone ?? string.Empty));
            claims.Add(new Claim("VkProfileLink", user.Student.Vkprofilelink ?? string.Empty));
        }

        return claims.ToArray();
    }

    private HttpContext GetHttpContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext недоступен");
        }
        return httpContext;
    }
}