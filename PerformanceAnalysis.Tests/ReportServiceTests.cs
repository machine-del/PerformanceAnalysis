using Moq;
using PerformanceAnalysis.Application.Reports;
using PerformanceAnalysis.Infrastructure.Reports;
using PerformanceAnalysis.Reports.GroupLeadersAndLaggards;
using PerformanceAnalysis.Reports.StudentTestResults;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceAnalysis.Tests
{
    public class ReportServiceTests
    {
        [Fact]
        public async Task GetGroupLeadersAndLaggards_CallsQueryAsync_WithCorrectSqlAndParams()
        {
            // Arrange
            var expected = new List<GroupLeadersAndLaggardsItem>
            {
                new()
                {
                    GroupId = 1,
                    GroupName = "Group 1",
                    LeaderName = "Ivan",
                    LeaderScore = 100, Course = "fawf",
                    Direction = "fawfaw",
                    LaggardName = "fawfaw",
                    LaggardScore = 20
                }
            };

            var mock = new Mock<IDapperExecutor>();

            mock.Setup(x => x.QueryAsync<GroupLeadersAndLaggardsItem>(
                    It.IsAny<string>(),
                    It.IsAny<object?>()
                ))
                .ReturnsAsync(expected);

            var filter = new GroupLeadersAndLaggardsFilter { CourseId = 5, DirectionId = 10 };
            var service = new ReportService(mock.Object);

            //Act
            var result = await service.GetGroupLeadersAndLaggardsAsync(filter);

            //Assert
            Assert.Equal(expected, result);
            mock.Verify(x => x.QueryAsync<GroupLeadersAndLaggardsItem>(
                    It.IsAny<string>(),
                    It.Is<GroupLeadersAndLaggardsFilter>(f => f.DirectionId == 10 && f.CourseId == 5))
                );
            mock.Verify(x => x.QueryAsync<GroupLeadersAndLaggardsItem>(
                    It.Is<string>(sql => sql.Contains(
                        "MAX(CASE WHEN ss.totalscore = gmm.max_score THEN ss.fullname END) AS leadername")
                    ),
                    It.IsAny<object?>())
                );
        }

        [Fact]
        public async Task GetStudentTestResults_CallsQueryAsync_WithCorrectSqlAndParams()
        {
            //Arrange
            var expected = new List<StudentTestResultsItem>
            {
                new()
                {
                    AttemptsCount = 1,
                    BestScore = 100,
                    CompletedAt = DateTime.UtcNow,
                    MaxPossibleScore = 100,
                    Passed = true,
                    TestId = 24,
                    TestTitle = "Test test"
                }
            };

            var mock = new Mock<IDapperExecutor>();

            mock.Setup(x => x.QueryAsync<StudentTestResultsItem>(
                    It.IsAny<string>(),
                    It.IsAny<object?>()
                ))
                .ReturnsAsync(expected);

            var filter = new StudentTestResultsFilter { StudentId = 1 };
            var service = new ReportService(mock.Object);

            //Act
            var result = await service.GetStudentTestResultsAsync(filter);

            //Assert
            Assert.Equal(expected, result);
            mock.Verify(x => x.QueryAsync<StudentTestResultsItem>(
                    It.IsAny<string>(),
                    It.Is<StudentTestResultsFilter>(f => f.StudentId == 1)
                ));
            mock.Verify(x => x.QueryAsync<StudentTestResultsItem>(
                    It.Is<string>(sql => sql.Contains("COALESCE(ba.score, 0) AS bestscore")),
                    It.IsAny<object?>()
                ));
        }

        [Fact]
        public async Task GetGroupLeadersAndLaggards_ReturnsEmpty_WhenDapperReturnsEmpty()
        {
            var mock = new Mock<IDapperExecutor>();
            mock.Setup(x => x.QueryAsync<GroupLeadersAndLaggardsItem>(
                    It.IsAny<string>(),
                    It.IsAny<object?>()
                ))
                .ReturnsAsync(new List<GroupLeadersAndLaggardsItem>());

            var service = new ReportService(mock.Object);

            var result = await service.GetGroupLeadersAndLaggardsAsync(new GroupLeadersAndLaggardsFilter());

            Assert.Empty(result);
        }
    }
}
