using System.Security.Claims;

namespace PerformanceAnalysis.Application.Extensions
{
    public static class HttpContextExtensions
    {
        public static string? GetUserRole(this HttpContext context)
        {
            return context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? context.User?.FindFirst("Role")?.Value;
        }

        public static bool IsManager(this HttpContext context)
        {
            return context.GetUserRole() == "Manager";
        }

        public static bool IsStudent(this HttpContext context)
        {
            return context.GetUserRole() == "Student";
        }

        public static int? GetStudentId(this HttpContext context)
        {
            var studentIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "StudentId")?.Value;

            return int.TryParse(studentIdClaim, out var studentId) ? studentId : null;
        }
    }
}
