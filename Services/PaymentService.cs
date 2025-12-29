using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CoworkingApp.API.Data;
using CoworkingApp.API.DTOs;
using CoworkingApp.API.Enums;
using CoworkingApp.API.Interfaces;
using CoworkingApp.API.Models;

namespace CoworkingApp.API.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly CoworkingContext _context;
        private readonly INotificationService _notificationService; //

        public PaymentService(CoworkingContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            // Use an execution strategy for better resilience (optional but good practice)
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    var reservation = await _context.Reservations
                        .Include(r => r.Space)
                        .Include(r => r.Member) // Include Member to get email for notification
                        .FirstOrDefaultAsync(r => r.ReservationId == request.ReservationId);

                    if (reservation == null) throw new Exception("Reservation not found.");
                    if (reservation.Status == ReservationStatus.CONFIRMED) throw new Exception("Reservation is already paid.");
                    if (reservation.Status == ReservationStatus.CANCELLED) throw new Exception("Cannot pay for a cancelled reservation.");

                    // 1. Mock Payment Gateway Logic
                    bool gatewaySuccess = true; 
                    if (!gatewaySuccess)
                    {
                        throw new Exception("Payment Gateway rejected the transaction.");
                    }

                    // 2. Create Payment Record
                    var payment = new Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        ReservationId = request.ReservationId,
                        Amount = request.Amount,
                        Currency = "USD",
                        PaymentMethod = request.Method,
                        Status = PaymentStatus.COMPLETED,
                        TransactionId = "TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                        PaidAt = DateTime.UtcNow
                    };

                    // 3. Create Invoice & Items
                    var invoice = new Invoice
                    {
                        InvoiceId = Guid.NewGuid(),
                        PaymentId = payment.PaymentId,
                        InvoiceNumber = "INV-" + DateTime.UtcNow.Ticks.ToString().Substring(10),
                        IssueDate = DateTime.UtcNow,
                        DueDate = DateTime.UtcNow,
                        Subtotal = payment.Amount,
                        Tax = payment.Amount * 0.10m, // Mock 10% tax
                        Total = payment.Amount * 1.10m,
                        Items = new List<InvoiceItem>
                        {
                            new InvoiceItem 
                            { 
                                InvoiceItemId = Guid.NewGuid(),
                                Description = $"Reservation for {reservation.Space.Name}",
                                Quantity = 1,
                                UnitPrice = payment.Amount,
                                Amount = payment.Amount
                            }
                        }
                    };

                    // 4. Update Reservation Status
                    reservation.Status = ReservationStatus.CONFIRMED;
                    reservation.Payment = payment; 
                    payment.Invoice = invoice;     

                    // 5. Save Everything
                    _context.Payments.Add(payment);
                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();

                    // 6. Send Notification (Connected Logic)
                    await _notificationService.SendPaymentSuccessAsync(reservation.Member, payment);
                    await _notificationService.SendBookingConfirmationAsync(reservation.Member, reservation);

                    await transaction.CommitAsync();

                    return new PaymentResponse
                    {
                        PaymentId = payment.PaymentId,
                        Status = payment.Status,
                        TransactionId = payment.TransactionId,
                        PaidAt = payment.PaidAt
                    };
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}