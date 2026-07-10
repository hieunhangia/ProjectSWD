using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSWD.Services.Admin
{
    public class RefundService
    {
        private readonly ApplicationDbContext _context;
        private readonly MockPaymentService _paymentService;

        public RefundService(ApplicationDbContext context, MockPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        public async Task<List<Refund>> GetRefundsAsync(RefundStatus? statusFilter = null)
        {
            var query = _context.Refunds
                .Include(r => r.Order)
                    .ThenInclude(o => o.Customer)
                .AsQueryable();

            if (statusFilter.HasValue)
            {
                query = query.Where(r => r.Status == statusFilter.Value);
            }

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Refund?> GetRefundDetailsAsync(int id)
        {
            return await _context.Refunds
                .Include(r => r.Order)
                    .ThenInclude(o => o.Customer)
                .Include(r => r.RefundItems)
                    .ThenInclude(ri => ri.Product)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<(bool Success, string Message)> ApproveRefundAsync(int refundId)
        {
            var refund = await _context.Refunds
                .Include(r => r.RefundItems)
                .FirstOrDefaultAsync(r => r.Id == refundId);

            if (refund == null)
            {
                return (false, "Refund request not found.");
            }

            if (refund.Status != RefundStatus.PendingReview)
            {
                return (false, "This request is not in a pending review state.");
            }

            bool paymentSuccess = await _paymentService.ProcessRefundAsync(refund.Amount);
            if (!paymentSuccess)
            {
                return (false, "Refund payment processing failed via payment gateway. Please try again or contact the provider.");
            }

            refund.Status = RefundStatus.Refunded;
            refund.CompletedAt = DateTime.UtcNow;

            foreach (var item in refund.RefundItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                {
                    product.Quantity += item.Quantity;
                }
            }

            await _context.SaveChangesAsync();
            return (true, "Refund approved and processed successfully.");
        }

        public async Task<(bool Success, string Message)> RejectRefundAsync(int refundId)
        {
            var refund = await _context.Refunds.FindAsync(refundId);
            if (refund == null)
            {
                return (false, "Refund request not found.");
            }

            if (refund.Status != RefundStatus.PendingReview)
            {
                return (false, "This request is not in a pending review state.");
            }

            refund.Status = RefundStatus.Rejected;
            refund.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return (true, "Refund request has been rejected.");
        }
    }
}
