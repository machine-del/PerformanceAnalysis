using PerformanceAnalysis.Application.Auth.Dtos;

namespace PerformanceAnalysis.Application.Auth.Services
{
    public interface IJwtAuthService
    {
        Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    }
}