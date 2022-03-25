using System;
using System.Threading.Tasks;
using Ninja.CircuitBreaker;
using Ninja.RetryMechanism;

namespace Ninja
{
    public class Ninja : IExecutionOperation
    {
        private RetryMechanismOptions _retryMechanismOptions;
        private CircuitBreakerOptions _circuitBreakerOptions;
        private static ICircuitBreakerStateStore _circuitBreakerStateStore = new CircuitBreakerStateStore();

        public static Ninja Instance => new();

        public Ninja UseRetry(RetryMechanismOptions retryMechanismOptions)
        {
            _retryMechanismOptions = retryMechanismOptions;

            return this;
        }

        public Ninja UseCircuitBreaker(CircuitBreakerOptions circuitBreakerOptions, ICircuitBreakerStateStore stateStore = null)
        {
            _circuitBreakerOptions = circuitBreakerOptions;

            if(stateStore != null)
            {
                _circuitBreakerStateStore = stateStore;
            }

            return this;
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> func)
        {
            if(_retryMechanismOptions == null && _circuitBreakerOptions == null)
            {
                throw new ArgumentNullException("You must use Retry Mechanism or CircuitBreaker method!");
            }

            try
            {
                if (_retryMechanismOptions == null) return await func.Invoke();

                var retryHelper = new RetryHelper();

                return await retryHelper.Retry(func, _retryMechanismOptions);

            }
            catch
            {
                if (_circuitBreakerOptions == null) throw;
                var circuitBreakerHelper = new CircuitBreakerHelper(_circuitBreakerOptions, _circuitBreakerStateStore);

                return await circuitBreakerHelper.ExecuteAsync(func);

            }
        }

        public async Task<T> ExecuteAsync<T>(Func<Task<T>> func, Func<Task<T>> fallbackFunc)
        {
            try
            {
                return await ExecuteAsync(func);
            }
            catch
            {
                if (fallbackFunc == null) throw;
                var result = await fallbackFunc.Invoke();

                return result;
            }  
        }
    }
}