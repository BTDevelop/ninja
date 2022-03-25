using System;
using System.Collections.Concurrent;

namespace Ninja.CircuitBreaker
{
    public class CircuitBreakerStateStore : ICircuitBreakerStateStore
    {
        private readonly ConcurrentDictionary<string, CircuitBreakerStateModel> _store = new();

        public void ChangeLastStateChangedDateUtc(string key, DateTime date)
        {
            if (!_store.TryGetValue(key, out var stateModel)) return;

            stateModel.LastStateChangedDateUtc = date;
            _store[key] = stateModel;
        }

        public void ChangeState(string key, CircuitBreakerStateEnum state)
        {
            if (!_store.TryGetValue(key, out var stateModel)) return;

            stateModel.State = state;
            _store[key] = stateModel;
        }

        public int GetExceptionAttempt(string key)
        {
            var exceptionAttempt = 0;
            if(_store.TryGetValue(key, out var stateModel))
            {
                exceptionAttempt = stateModel.ExceptionAttempt;
            }

            return exceptionAttempt;
        }

        public void IncreaseExceptionAttempt(string key)
        {
            if(_store.TryGetValue(key, out var stateModel))
            {
                stateModel.ExceptionAttempt += 1;
                _store[key] = stateModel;
            }
            else
            {
                stateModel = new CircuitBreakerStateModel();
                stateModel.ExceptionAttempt += 1;

                AddStateModel(key, stateModel);
            }
        }

        public DateTime GetLastStateChangedDateUtc(string key)
        {
            var lastStateChangedDateUtc = default(DateTime);
            if(_store.TryGetValue(key, out var stateModel))
            {
                lastStateChangedDateUtc = stateModel.LastStateChangedDateUtc;
            }

            return lastStateChangedDateUtc;
        }

        public int GetSuccessAttempt(string key)
        {
            var successAttempt = 0;
            if(_store.TryGetValue(key, out var stateModel))
            {
                successAttempt = stateModel.SuccessAttempt;
            }

            return successAttempt;
        }

        public void IncreaseSuccessAttempt(string key)
        {
            if (!_store.TryGetValue(key, out var stateModel)) return;

            stateModel.SuccessAttempt += 1;
            _store[key] = stateModel;
        }

        public bool IsClosed(string key)
        {
            var isClosed = true;
            if(_store.TryGetValue(key, out var stateModel))
            {
                isClosed = stateModel.IsClosed;
            }

            return isClosed;
        }

        public void RemoveState(string key)
        {
            _store.TryRemove(key, out var stateModel);
        }

        public void SetLastException(string key, Exception ex)
        {
            if (!_store.TryGetValue(key, out var stateModel)) return;
            stateModel.LastException = ex;
            _store[key] = stateModel;
        }

        public Exception GetLastException(string key)
        {
            Exception lastException = null;
            if(_store.TryGetValue(key, out var stateModel))
            {
                lastException = stateModel.LastException;
            }

            return lastException;
        }

        public void AddStateModel(string key, CircuitBreakerStateModel circuitBreakerStateModel)
        {
            _store.TryAdd(key, circuitBreakerStateModel);
        }
    }
}