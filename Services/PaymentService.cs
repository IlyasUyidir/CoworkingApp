using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly INotificationService _notificationService;
        private readonly IPaymentGateway _paymentGateway;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(
            CoworkingContext context,
            INotificationService notificationService,
            IPaymentGateway paymentGateway,
            ILogger<PaymentService> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _paymentGateway = paymentGateway;
            _logger = logger;
        }

        public async Task<PaymentResponse> ProcessPaymentAsync(ProcessPaymentRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Amount <= 0) throw new ArgumentException("Amount must be greater than zero.", nameof(request.Amount));

            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Load reservation with related data
                    var reservation = await _context.Reservations
                        .Include(r => r.Space)
                        .Include(r => r.Member)
                        .FirstOrDefaultAsync(r => r.ReservationId == request.ReservationId);

                    if (reservation == null)
                    {
                        _logger.LogWarning("Reservation {ReservationId} not found.", request.ReservationId);
                        throw new InvalidOperationException("Reservation not found.");
                    }

                    if (reservation.Status == ReservationStatus.CONFIRMED)
                    {
                        _logger.LogWarning("Reservation {ReservationId} is already confirmed.", request.ReservationId);
                        throw new InvalidOperationException("Reservation is already paid.");
                    }

                    if (reservation.Status == ReservationStatus.CANCELLED)
                    {
                        _logger.LogWarning("Cannot pay for cancelled reservation {ReservationId}.", request.ReservationId);
                        throw new InvalidOperationException("Cannot pay for a cancelled reservation.");
                    }

                    // 1. Charge via payment gateway (abstraction for testability)
                    var gatewayResult = await _paymentGateway.ChargeAsync(new PaymentGatewayRequest
                    {
                        Amount = request.Amount,
                        Currency = request.Currency ?? "USD",
                        Method = request.Method,
                        Metadata = request.Metadata
                    });

                    if (!gatewayResult.Success)
                    {
                        _logger.LogError("Payment gateway failed for reservation {ReservationId}: {Error}", request.ReservationId, gatewayResult.ErrorMessage);
                        throw new InvalidOperationException($"Payment Gateway error: {gatewayResult.ErrorMessage}");
                    }

                    // 2. Create Payment record
                    var payment = new Payment
                    {
                        PaymentId = Guid.NewGuid(),
                        ReservationId = request.ReservationId,
                        Amount = request.Amount,
                        Currency = gatewayResult.Currency ?? (request.Currency ?? "USD"),
                        PaymentMethod = request.Method,
                        Status = PaymentStatus.COMPLETED,
                        TransactionId = gatewayResult.TransactionId ?? ("TXN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper()),
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
                        Tax = Math.Round(payment.Amount * 0.10m, 2), // Example 10% tax
                        Total = Math.Round(payment.Amount * 1.10m, 2),
                        Items = new List<InvoiceItem>
                        {
                            new InvoiceItem
                            {
                                InvoiceItemId = Guid.NewGuid(),
                                Description = $"Reservation for {reservation.Space?.Name ?? "space"}",
                                Quantity = 1,
                                UnitPrice = payment.Amount,
                                Amount = payment.Amount
                            }
                        }
                    };

                    // 4. Update Reservation and link payment & invoice
                    reservation.Status = ReservationStatus.CONFIRMED;
                    reservation.PaymentId = payment.PaymentId; // if Reservation has PaymentId FK
                    payment.Invoice = invoice;

                    // 5. Persist
                    _context.Payments.Add(payment);
                    _context.Invoices.Add(invoice);
                    await _context.SaveChangesAsync();

                    // 6. Send notifications (best-effort; failures should not break transaction after commit)
                    try
                    {
                        if (reservation.Member != null)
                        {
                            await _notificationService.SendPaymentSuccessAsync(reservation.Member, payment);
                            await _notificationService.SendBookingConfirmationAsync(reservation.Member, reservation);
                        }
                    }
                    catch (Exception notifyEx)
                    {
                        // Log notification errors but do not roll back the payment
                        _logger.LogError(notifyEx, "Failed to send notifications for payment {PaymentId}", payment.PaymentId);
                    }

                    await transaction.CommitAsync();

                    return new PaymentResponse
                    {
                        PaymentId = payment.PaymentId,
                        Status = payment.Status.ToString(),
                        TransactionId = payment.TransactionId,
                        PaidAt = payment.PaidAt
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing payment for reservation {ReservationId}", request?.ReservationId);
                    await transaction.RollbackAsync();
                    throw;
                }
            });
        }
    }
}
