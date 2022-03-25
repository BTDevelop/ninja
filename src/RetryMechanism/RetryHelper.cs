using System;
using System.Threading.Tasks;

namespace Ninja.RetryMechanism
{
    public class RetryHelper
    {
        public async Task<T> Retry<T>(Func<Task<T>> func, RetryMechanismOptions retryMechanismOptions)
        {
            IRetryMechanismStrategy retryMechanism = retryMechanismOptions.RetryPolicies switch
            {
                RetryPolicies.Linear => new RetryLinearMechanismStrategy(retryMechanismOptions),
                RetryPolicies.Exponential => new RetryExponentiallyMechanismStrategy(retryMechanismOptions),
                _ => null
            };

            return await retryMechanism.ExecuteAsync(func);
        }
    }
}