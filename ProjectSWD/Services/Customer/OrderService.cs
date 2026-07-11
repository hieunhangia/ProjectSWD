using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ProjectSWD.Data;
using ProjectSWD.Data.Entities;
using ProjectSWD.Data.Enums;
using ProjectSWD.DTOs;
using ProjectSWD.Repositories;

namespace ProjectSWD.Services.Customer
{
    public class OrderService(ApplicationDbContext context, IOrderRepository orderRepository) : IOrderService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly IOrderRepository _orderRepository = orderRepository;

        public Task<decimal> CalculateShippingFeeAsync(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                return Task.FromResult(0m);
            }

            decimal baseFee = 15000m;
            decimal calculatedFee = baseFee + (address.Length * 500m);
            
            calculatedFee = Math.Clamp(calculatedFee, 15000m, 100000m);

            return Task.FromResult(calculatedFee);
        }

        public async Task<Product?> GetProductByIdAsync(int productId)
        {
            return await _context.Products.FindAsync(productId);
        }
        public async Task<OrderSummaryDTO> ProcessOrderCheckoutAsync(string userId, OrderCheckoutDTO checkoutInfo)
        {
            var cartItems = await _context.CartItems
                .Where(c => c.CustomerId == userId)
                .Include(c => c.Product)
                .ToListAsync();

            if (cartItems == null || cartItems.Count == 0)
            {
                throw new ArgumentException("Giỏ hàng của bạn đang trống.");
            }

            var cartItemsDTO = cartItems.Select(item => new CartItemDTO
            {
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? "Unknown Product",
                ProductImage = string.IsNullOrEmpty(item.Product?.ImageUrl) ? "/images/products/placeholder.jpg" : item.Product.ImageUrl,
                Price = item.Product?.Price ?? 0,
                Quantity = item.Quantity,
                AvailableQuantity = item.Product?.Quantity ?? 0
            }).ToList();

            await Task.Delay(2000);
            bool isPaymentSuccess = true; 

            if (!isPaymentSuccess)
            {
                throw new InvalidOperationException("Thanh toán thất bại qua cổng thanh toán.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var customer = await _context.Customers.FindAsync(userId);
                if (customer == null)
                {
                    throw new InvalidOperationException("Không tìm thấy thông tin khách hàng trong cơ sở dữ liệu.");
                }

                decimal productTotal = 0;

                foreach (var item in cartItems)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        throw new InvalidOperationException($"Sản phẩm '{item.Product?.Name}' (ID: {item.ProductId}) không tồn tại trong hệ thống.");
                    }

                    if (product.Quantity < item.Quantity)
                    {
                        throw new InvalidOperationException($"Số lượng tồn kho không đủ cho sản phẩm '{product.Name}'. Yêu cầu: {item.Quantity:N0}, Hiện có: {product.Quantity:N0}.");
                    }

                    product.Quantity -= item.Quantity;
                    _context.Products.Update(product);

                    productTotal += product.Price * item.Quantity;
                }

                var order = new Order
                {
                    FullName = checkoutInfo.FullName,
                    PhoneNumber = checkoutInfo.PhoneNumber,
                    Time = DateTime.Now,
                    TotalPrice = productTotal + checkoutInfo.ShippingFee,
                    ApprovementStatus = OrderStatus.Processing,
                    CustomerId = customer.Id
                };

                await _orderRepository.AddOrderAsync(order);
                await _orderRepository.SaveChangesAsync();

                foreach (var item in cartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Product?.Price ?? 0
                    };
                    await _orderRepository.AddOrderItemAsync(orderItem);
                }

                var deliveryPartner = await _context.DeliveryPartners.FirstOrDefaultAsync();
                if (deliveryPartner != null)
                {
                    var shipment = new Shipment
                    {
                        OrderId = order.Id,
                        DeliveryPartnerId = deliveryPartner.Id,
                        Status = ShipmentStatus.Pending
                    };
                    await _context.Shipments.AddAsync(shipment);
                }

                var bill = new Bill
                {
                    OrderId = order.Id,
                    PaymentTime = DateTime.Now,
                    ShopName = "Gia Hoa Phat System",
                    ShopEmail = "contact@giahoaphat.com",
                    ShopPhone = "0123456789"
                };
                await _context.Bills.AddAsync(bill);

                await _orderRepository.ClearCartAsync(userId);

                await _orderRepository.SaveChangesAsync();
                await transaction.CommitAsync();

                return new OrderSummaryDTO
                {
                    OrderId = order.Id,
                    FullName = order.FullName,
                    PhoneNumber = order.PhoneNumber,
                    Address = checkoutInfo.Address,
                    Items = cartItemsDTO,
                    ProductTotal = productTotal,
                    ShippingFee = checkoutInfo.ShippingFee,
                    TotalPrice = order.TotalPrice,
                    OrderTime = order.Time,
                    Status = order.ApprovementStatus.ToString(),
                    PaymentMethod = checkoutInfo.PaymentMethod
                };
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
