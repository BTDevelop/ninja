using System;
using System.Threading.Tasks;

namespace Ninja
{
    public interface IExecutionOperation
    {
        Task<T> ExecuteAsync<T>(Func<Task<T>> func);
    }
}