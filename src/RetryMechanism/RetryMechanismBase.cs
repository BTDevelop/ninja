using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ninja.RetryMechanism
{
    public abstract class RetryMechanismBase
    {
        private readonly RetryMechanismOptions _retryMechanismOptions;

        protected RetryMechanismBase(RetryMechanismOptions retryMechanismOptions)
        {
            _retryMechanismOptions = retryMechanismOptions;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
        {
            var currentRetryCount = 0;

            for(;;)
            {
                try
                {
                    return await func.Invoke();
                }
                catch(Exception ex)
                {
                    currentRetryCount++;

                    var isTransient = await IsTransient(ex);
                    if(currentRetryCount > _retryMechanismOptions.RetryCount || !isTransient)
                    {
                        throw;
                    }
                }

                await HandleBackOff(currentRetryCount);
            }
        }

        protected abstract Task HandleBackOff(int currentRetryCount);
        
        private static Task<bool> IsTransient(Exception ex)
        {
            var isTransient = false;

            if(ex is WebException webException)
            {
                isTransient = new[] {WebExceptionStatus.ConnectionClosed,
                              WebExceptionStatus.Timeout,
                              WebExceptionStatus.RequestCanceled,
                              WebExceptionStatus.KeepAliveFailure,
                              WebExceptionStatus.PipelineFailure,
                              WebExceptionStatus.ReceiveFailure,
                              WebExceptionStatus.ConnectFailure,
                              WebExceptionStatus.SendFailure}
                              .Contains(webException.Status);
            }

            return Task.FromResult(isTransient);
        }
    }
}