using PerformanceAnalysis.Application.Auth.Dtos;

namespace PerformanceAnalysis.Application.Auth.Services
{
    public interface ICookieAuthService
    {
        Task<AuthResponse> LoginWithCookieAsync(LoginWithCookieRequest request, CancellationToken ct = default);
        Task LogoutAsync(CancellationToken ct = default);
    }
}