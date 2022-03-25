using System;

namespace Ninja.CircuitBreaker
{
    public class CircuitBreakerOpenException : Exception
    {
        public CircuitBreakerOpenException(string message, Exception ex)
            :base(message, ex)
        {
            
        }
    }
}