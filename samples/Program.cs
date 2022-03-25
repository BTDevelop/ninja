using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ninja.CircuitBreaker;
using Ninja.RetryMechanism;

namespace Ninja.Samples
{
    internal class Program
    {
        private static void Main()
        {
            var rateWithRetryMechanism = RetryMechanismSample(amount: 1000, from: "USD", to: "EUR").Result;
            var rateWithCircuitBreaker = CircuitBreakerSample(amount: 1000, from: "USD", to: "EUR").Result;

            Console.WriteLine($"Retry works: {rateWithRetryMechanism}");
            Console.WriteLine($"Circuit Breaker works: {rateWithCircuitBreaker}");
            Console.ReadLine();
        }

        static async Task<double> RetryMechanismSample(double amount, string from, string to)
        {
            var currentRate = await Ninja.Instance
                                .UseRetry(new RetryMechanismOptions(RetryPolicies.Linear,
                                                                    retryCount: 3,
                                                                    interval: TimeSpan.FromSeconds(5)))
                                .ExecuteAsync<double>(async () => {
                                    // Some API calls...
                                    var rate = await CurrencyConverterSampleAPI(amount, from, to);

                                    return rate;
                                });

            return currentRate;
        }

        private static async Task<double> RetryMechanismWithFallbackSample(double amount, string from, string to)
        {
            var currentRate = await Ninja.Instance
                                .UseRetry(new RetryMechanismOptions(RetryPolicies.Linear,
                                                                    retryCount: 3,
                                                                    interval: TimeSpan.FromSeconds(5)))
                                .ExecuteAsync<double>(async () => {
                                    // Some API calls...
                                    var rate = await CurrencyConverterSampleAPI(amount, from, to);

                                    return rate;
                                }, async () => {
                                    // Some fallback scenario.
                                    double rate = 100;

                                    return await Task.FromResult(rate);                                    
                                });

            return currentRate;
        }

        private static async Task<double> CircuitBreakerSample(double amount, string from, string to)
        {
            var currentRate = await Ninja.Instance
                                .UseCircuitBreaker(new CircuitBreakerOptions(key: "CurrencyConverterSampleAPI",
                                                                             exceptionThreshold: 5,
                                                                             successThresholdWhenCircuitBreakerHalfOpenStatus: 5,
                                                                             durationOfBreak: TimeSpan.FromSeconds(5)))
                                .ExecuteAsync<double>(async () => {
                                    // Some API calls...
                                    double rate = await CurrencyConverterSampleAPI(amount, from, to);

                                    return rate;
                                });

            return currentRate;
        }

        static async Task<double> CurrencyConverterSampleAPI(double amount, string from, string to)
        {
            using var client = new WebClient();
            var url = $"https://finance.google.com/finance/converter?a={amount}&from={from}&to={to}";
            var response = await client.DownloadStringTaskAsync(url);

            var match = Regex.Match(response, "<span[^>]*>(.*?)</span>");

            var rate = Convert.ToDouble(match.Groups[1].Value.Replace($" {to}", ""), CultureInfo.InvariantCulture);

            return rate;
        }
    }
}