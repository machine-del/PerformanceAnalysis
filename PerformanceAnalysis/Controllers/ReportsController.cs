using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PerformanceAnalysis.Application.Extensions;
using PerformanceAnalysis.Application.Reports;
using PerformanceAnalysis.Reports.DayOfWeekActivity;
using PerformanceAnalysis.Reports.GroupLeadersAndLaggards;
using PerformanceAnalysis.Reports.GroupTrend;
using PerformanceAnalysis.Reports.StudentMonthlyProgress;
using PerformanceAnalysis.Reports.StudentPassRate;
using PerformanceAnalysis.Reports.StudentPassRateSummary;
using PerformanceAnalysis.Reports.StudentRating;
using PerformanceAnalysis.Reports.StudentTestResults;

namespace PerformanceAnalysis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Лидеры и отстающие в группах. Доступно менеджерам и студентам.
        /// </summary>
        [HttpGet("group-leaders")]
        public async Task<IActionResult> GetGroupLeaders([FromQuery] GroupLeadersAndLaggardsFilter filter)
        {
            var result = await _reportService.GetGroupLeadersAndLaggardsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Результаты тестов студента. Студенты могут смотреть только свои результаты.
        /// </summary>
        [HttpGet("student-test-results")]
        public async Task<IActionResult> GetStudentTestResults([FromQuery] StudentTestResultsFilter filter)
        {
            if (!ValidateStudentAccess(filter.StudentId))
            {
                return Forbid();
            }

            if (HttpContext.IsStudent() && filter.StudentId == null)
            {
                filter.StudentId = (int)HttpContext.GetStudentId();
            }

            var result = await _reportService.GetStudentTestResultsAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Динамика среднего балла по группам. Доступно менеджерам и студентам.
        /// </summary>
        [HttpGet("group-trend")]
        public async Task<IActionResult> GetGroupTrend([FromQuery] GroupTrendFilter filter)
        {
            var result = await _reportService.GetGroupTrendAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Накопленные баллы студента по месяцам. Студенты могут смотреть только свои данные.
        /// </summary>
        [HttpGet("student-monthly-progress")]
        public async Task<IActionResult> GetStudentMonthlyProgress([FromQuery] StudentMonthlyProgressFilter filter)
        {
            if (!ValidateStudentAccess(filter.StudentId))
            {
                return Forbid();
            }

            if (HttpContext.IsStudent() && filter.StudentId == null)
            {
                filter.StudentId = (int)HttpContext.GetStudentId();
            }

            var result = await _reportService.GetStudentMonthlyProgressAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Общий рейтинг студентов. Доступно менеджерам и студентам.
        /// </summary>
        [HttpGet("student-rating")]
        public async Task<IActionResult> GetStudentRating([FromQuery] StudentRatingFilter filter)
        {
            var result = await _reportService.GetStudentRatingAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Процент пройденных тестов. Доступно менеджерам и студентам.
        /// </summary>
        [HttpGet("student-pass-rate")]
        public async Task<IActionResult> GetStudentPassRate([FromQuery] StudentPassRateFilter filter)
        {
            var result = await _reportService.GetStudentPassRateAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Процент пройденных тестов конкретного студента. Студенты могут смотреть только свои данные.
        /// </summary>
        [HttpGet("student-pass-rate-summary")]
        public async Task<IActionResult> GetStudentPassRateSummary([FromQuery] StudentPassRateSummaryFilter filter)
        {
            if (!ValidateStudentAccess(filter.StudentId))
            {
                return Forbid();
            }

            if (HttpContext.IsStudent() && filter.StudentId == null)
            {
                filter.StudentId = (int)HttpContext.GetStudentId();
            }

            var result = await _reportService.GetStudentPassRateSummaryAsync(filter);
            return result is null ? NotFound() : Ok(result);
        }

        /// <summary>
        /// Активность по дням недели. Доступно менеджерам и студентам.
        /// </summary>
        [HttpGet("day-of-week-activity")]
        public async Task<IActionResult> GetDayOfWeekActivity([FromQuery] DayOfWeekActivityFilter filter)
        {
            var result = await _reportService.GetDayOfWeekActivityAsync(filter);
            return Ok(result);
        }

        /// <summary>
        /// Проверка доступа для отчётов с фильтрацией по StudentId.
        /// Менеджеры имеют доступ ко всем данным, студенты - только к своим.
        /// </summary>
        private bool ValidateStudentAccess(int? studentId)
        {
            if (HttpContext.IsManager())
            {
                return true;
            }

            if (HttpContext.IsStudent())
            {
                var currentStudentId = HttpContext.GetStudentId();

                if (studentId == null)
                {
                    return currentStudentId != null;
                }

                return currentStudentId == studentId;
            }

            return false;
        }
    }
}
