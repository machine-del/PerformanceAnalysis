namespace PerformanceAnalysis.Infrastructure.Reports
{
    public interface IDapperExecutor
    {
        Task<IEnumerable<T>> QueryAsync<T>(string sql, object? param = null);
        Task<T?> QueryFirstOrDefaultAsync<T>(string sql, object? param = null);
    }
}
