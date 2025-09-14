using VotingSystem_Claude.Services.Interfaces;

namespace VotingSystem_Claude.Services
{
    public class RetryService : IRetryService
    {
        private readonly ILogger<RetryService> _logger;

        public RetryService(ILogger<RetryService> logger)
        {
            _logger = logger;
        }

        public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, int delayMs = 1000)
        {
            var lastException = new Exception();
            
            for (int retry = 0; retry <= maxRetries; retry++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    if (retry == maxRetries)
                    {
                        _logger.LogError(ex, "Operation failed after {MaxRetries} retries", maxRetries);
                        throw;
                    }

                    _logger.LogWarning(ex, "Operation failed on attempt {Attempt}/{MaxRetries}. Retrying in {Delay}ms", 
                        retry + 1, maxRetries + 1, delayMs);

                    await Task.Delay(delayMs * (retry + 1)); // Exponential backoff
                }
            }

            throw lastException;
        }

        public async Task ExecuteWithRetryAsync(Func<Task> operation, int maxRetries = 3, int delayMs = 1000)
        {
            await ExecuteWithRetryAsync(async () =>
            {
                await operation();
                return true; // Dummy return value
            }, maxRetries, delayMs);
        }
    }
}