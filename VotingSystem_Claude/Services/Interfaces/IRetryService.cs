namespace VotingSystem_Claude.Services.Interfaces
{
    public interface IRetryService
    {
        Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int delayMs = 1000);
        Task ExecuteWithRetryAsync(Func<Task> operation, int maxRetries = 3, int delayMs = 1000);
    }
}