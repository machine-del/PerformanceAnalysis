using PerformanceAnalysis.Application.Auth.Dtos;

namespace PerformanceAnalysis.Application.Auth.Services
{
    public interface IAuthServices
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginWithCookieAsync(LoginWithCookieRequest request, CancellationToken ct = default);
        Task LogoutAsync(CancellationToken ct = default);
    }
}
