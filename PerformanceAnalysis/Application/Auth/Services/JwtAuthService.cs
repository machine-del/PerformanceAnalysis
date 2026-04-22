using Domain.Auth;
using Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PerformanceAnalysis.Application.Auth.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PerformanceAnalysis.Application.Auth.Services;

public class JwtAuthService : IJwtAuthService
{
    private readonly AuthDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public JwtAuthService(AuthDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
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

        var user = CreateUserFromRequest(request);
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(ct);

        var student = CreateStudentFromRequest(request, user.Id);
        _dbContext.Students.Add(student);
        await _dbContext.SaveChangesAsync(ct);

        student.Groups.Add(group);
        await _dbContext.SaveChangesAsync(ct);

        var accessToken = GenerateAccessToken(user);
        var accessTokenExpiresAt = GetAccessTokenExpiration();

        return CreateAuthResponse(user, accessToken, accessTokenExpiresAt);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await _dbContext.Users
            .Include(u => u.Student)
            .FirstOrDefaultAsync(u =>
                (u.Login == request.LoginOrEmail || u.Email == request.LoginOrEmail), ct);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Passwordhash))
        {
            throw new InvalidOperationException("Неверный логин или пароль");
        }

        var accessToken = GenerateAccessToken(user);
        var accessTokenExpiresAt = GetAccessTokenExpiration();

        return CreateAuthResponse(user, accessToken, accessTokenExpiresAt);
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

    private User CreateUserFromRequest(RegisterRequest request)
    {
        return new User
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
    }

    private Student CreateStudentFromRequest(RegisterRequest request, int userId)
    {
        return new Student
        {
            Userid = userId,
            Phone = request.Phone ?? string.Empty,
            Vkprofilelink = request.VkProfile ?? string.Empty
        };
    }

    private DateTime GetAccessTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(
            _configuration.GetValue<int>("JwtSettings:AccessTokenExpirationMinutes"));
    }

    private AuthResponse CreateAuthResponse(User user, string accessToken, DateTime expiresAt)
    {
        return new AuthResponse
        {
            UserId = user.Id,
            Login = user.Login,
            Email = user.Email,
            FirstName = user.Firstname,
            LastName = user.Lastname,
            Role = user.Role,
            AccessToken = accessToken,
            AccessTokenExpiredAt = expiresAt
        };
    }
}