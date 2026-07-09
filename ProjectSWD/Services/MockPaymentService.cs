using System;
using System.Threading.Tasks;

namespace ProjectSWD.Services
{
    public class MockPaymentService
    {
        public Task<bool> ProcessRefundAsync(decimal amount)
        {
            var isSuccess = Random.Shared.Next(100) < 95;
            return Task.FromResult(isSuccess);
        }
    }
}
