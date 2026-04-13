namespace PerformanceAnalysis.Application.Auth.Dtos
{
    public class LoginRequest
    {
        public string LoginOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
