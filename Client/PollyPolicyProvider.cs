using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace Client;

public interface IPolicyProvider
{
    IAsyncPolicy<HttpResponseMessage> GetRetryPolicy();
}

public class PollyPolicyProvider(ILogger<PollyPolicyProvider> logger) : IPolicyProvider
{
    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(1), retryCount: 3);
        int counter = 0;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
            .WaitAndRetryAsync(delay, (resp, count) => 
            {
                counter++;
                logger.LogWarning($"Request failed, retry attempt {counter}."); 
            });
    }
}
