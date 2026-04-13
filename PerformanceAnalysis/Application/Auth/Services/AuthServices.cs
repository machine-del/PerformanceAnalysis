using Domain.Auth;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PerformanceAnalysis.Application.Auth.Dtos;
using PerformanceAnalysis.Application.Auth.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PerformanceAnalysis.Application.Auth;

public class AuthService : IAuthServices
{
    private readonly AuthDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(AuthDbContext dbContext, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Login == request.Login || u.Email == request.Email, ct);

        if (existingUser != null)
        {
            throw new InvalidOperationException("Пользователь с таким логином или email уже существует");
        }

        var group = await _dbContext.Groups.FindAsync(new object[] { request.GroupId }, ct);
        if (group == null)
        {
            throw new InvalidOperationException("Группа не найдена");
        }

        var user = new User
        {
            Login = request.Login,
            Email = request.Email,
            Passwordhash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Firstname = request.FirstName,
            Middlename = request.MiddleName,
            Lastname = request.LastName,
            Role = "Student",
            Createdat = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        var student = new Student
        {
            Userid = user.Id,
            Phone = request.Phone ?? string.Empty,
            Vkprofilelink = request.VkProfile ?? string.Empty
        };

        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync(ct);

        student.Groups.Add(group);
        await _dbContext.SaveChangesAsync(ct);

        var accessToken = GenerateAccessToken(user);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes"));

        return new AuthResponse
        {
            UserId = user.Id,
            Login = user.Login,
            Email = user.Email,
            FirstName = user.Firstname,
            LastName = user.Lastname,
            Role = user.Role,
            AccessToken = accessToken,
            AccessTokenExpiredAt = accessTokenExpiresAt
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u =>
                (u.Login == request.LoginOrEmail || u.Email == request.LoginOrEmail), ct);

        if (user == null)
        {
            throw new InvalidOperationException("Неверный лин или пароль");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Passwordhash))
        {
            throw new InvalidOperationException("Неверный логин или пароль");
        }

        var accessToken = GenerateAccessToken(user);
        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes"));

        return new AuthResponse
        {
            UserId = user.Id,
            Login = user.Login,
            Email = user.Email,
            FirstName = user.Firstname,
            LastName = user.Lastname,
            Role = user.Role,
            AccessToken = accessToken,
            AccessTokenExpiredAt = accessTokenExpiresAt
        };
    }

    private string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var expirationMinutes = jwtSettings.GetValue<int>("AccessTokenExpirationMinutes");

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<AuthResponse> LoginWithCookieAsync(LoginWithCookieRequest request, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
           .Include(u => u.Student)
           .FirstOrDefaultAsync(u =>
               (u.Login == request.LoginOrEmail || u.Email == request.LoginOrEmail), ct);

        if (user == null)
        {
            throw new InvalidOperationException("Неверный лин или пароль");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Passwordhash))
        {
            throw new InvalidOperationException("Неверный логин или пароль");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        if (user.Role == "Student" && user.Student != null)
        {
            claims = claims.Append(new Claim("Phone", user.Student.Phone ?? string.Empty)).ToArray();
            claims = claims.Append(new Claim("VkProfileLink", user.Student.Vkprofilelink ?? string.Empty)).ToArray();
        }

        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext == null)
        {
            throw new InvalidOperationException("HttpContext недоступен");
        }

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
}
