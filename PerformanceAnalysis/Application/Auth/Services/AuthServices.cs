using PerformanceAnalysis.Application.Auth.Dtos;
using PerformanceAnalysis.Application.Auth.Services;

namespace PerformanceAnalysis.Application.Auth;

public class AuthService : IAuthServices
{
    private readonly IJwtAuthService _jwtAuthService;
    private readonly ICookieAuthService _cookieAuthService;

    public AuthService(IJwtAuthService jwtAuthService, ICookieAuthService cookieAuthService)
    {
        _jwtAuthService = jwtAuthService;
        _cookieAuthService = cookieAuthService;
    }

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        => _jwtAuthService.RegisterAsync(request, ct);

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        => _jwtAuthService.LoginAsync(request, ct);

    public Task<AuthResponse> LoginWithCookieAsync(LoginWithCookieRequest request, CancellationToken ct = default)
        => _cookieAuthService.LoginWithCookieAsync(request, ct);

    public Task LogoutAsync(CancellationToken ct = default)
        => _cookieAuthService.LogoutAsync(ct);
}