using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectSWD.Services.Staff
{
    public class OrderService
    {
        private readonly ApplicationDbContext _context;

        public OrderService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .OrderByDescending(o => o.Time)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Staff)
                .Include(o => o.Shipment)
                    .ThenInclude(s => s.DeliveryPartner)
                .Include(o => o.Bill)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<bool> ConfirmOrderAsync(int orderId, string staffId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null || order.Status != OrderStatus.AwaitingConfirmation)
            {
                return false;
            }

            order.Status = OrderStatus.Confirmed;
            order.StaffId = staffId;

            var shipment = await _context.Shipments.FindAsync(orderId);
            if (shipment != null)
            {
                shipment.Status = ShipmentStatus.Pending;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelOrderAsync(int orderId, string staffId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null || order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
                {
                    return false;
                }

                if (order.OrderItems != null)
                {
                    foreach (var item in order.OrderItems)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId);
                        if (product != null)
                        {
                            product.Quantity += item.Quantity;
                            _context.Products.Update(product);
                        }
                    }
                }

                order.Status = OrderStatus.Cancelled;
                order.StaffId = staffId;

                var shipment = await _context.Shipments.FindAsync(orderId);
                if (shipment != null)
                {
                    shipment.Status = ShipmentStatus.Failed;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string staffId, OrderStatus newStatus)
        {
            var order = await _context.Orders
                .Include(o => o.Shipment)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null || order.Status == OrderStatus.Cancelled || order.Status == OrderStatus.Delivered)
            {
                return false;
            }

            order.Status = newStatus;
            order.StaffId = staffId;

            if (order.Shipment != null)
            {
                if (newStatus == OrderStatus.InTransit)
                {
                    order.Shipment.Status = ShipmentStatus.InTransit;
                }
                else if (newStatus == OrderStatus.Delivered)
                {
                    order.Shipment.Status = ShipmentStatus.Delivered;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
