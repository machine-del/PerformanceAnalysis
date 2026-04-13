namespace PerformanceAnalysis.Application.Auth.Dtos
{
    public class LoginWithCookieRequest
    {
        public string LoginOrEmail { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }
}
