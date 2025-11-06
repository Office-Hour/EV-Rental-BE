using Application.CustomExceptions;
using Application.DTOs.BookingManagement;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.UseCases.BookingManagement.Commands.CancelChecking;

public class CancelCheckinCommandHandler(IUnitOfWork uow, IMapper mapper, UserManager<IdentityUser> userManager)
    : IRequestHandler<CancelCheckinCommand, DepositFeeDto>
{
    public async Task<DepositFeeDto> Handle(CancelCheckinCommand request, CancellationToken cancellationToken)
    {
        var booking = await uow.Repository<Booking>().GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found or access denied.");
        if (booking.Status != BookingStatus.Pending_Verification)
        {
            throw new InvalidTokenException("Only pending bookings can be canceled.");
        }
        // Business logic to determine refund amount
        var fee = await uow.Repository<Fee>().AsQueryable()
            .FirstOrDefaultAsync(f => f.BookingId == booking.BookingId && f.Type == FeeType.Deposit)
            ?? throw new NotFoundException("Deposit fee not found for this booking.");
        var payment = await uow.Repository<Payment>().AsQueryable()
            .FirstOrDefaultAsync(p => p.FeeId == fee.FeeId)
            ?? throw new NotFoundException("Deposit payment not found for this booking.");
        var user = await userManager.FindByIdAsync(request.UserId.ToString())
            ?? throw new NotFoundException("User not found or access denied.");
        var claims = await userManager.GetClaimsAsync(user);

        var code = claims.FirstOrDefault(c => c.Type == "CancelCheckinCode")?.Value;
        var codeExpiryStr = claims.FirstOrDefault(c => c.Type == "CancelCheckinCodeExpiry")?.Value;

        if (code == null || code != request.CancelCheckinCode)
        {
            throw new InvalidTokenException("Invalid or missing cancellation code.");
        }
        if (codeExpiryStr == null || !DateTime.TryParse(codeExpiryStr, out var codeExpiry) || codeExpiry < DateTime.UtcNow)
        {
            throw new InvalidTokenException("Cancellation code has expired.");
        }

        decimal transactionFee = payment.AmountPaid * 0.05m; // Assume a 5% transaction fee
        decimal refundAmount = payment.AmountPaid - transactionFee; // Simplified for this example

        booking.Status = BookingStatus.Cancelled;
        booking.CancelReason = request.CancelReason;
        booking.CancelReason += $". Refund Amount: {refundAmount} {fee.Currency}";
        payment.Status = PaymentStatus.Refunded;
        // Here you would integrate with a payment gateway to process the refund
        if (request.RenterBankAccount != null)
        {
            // paymentService.Refund(payment, refundAmount);
        }
        // Otherwise, assume the refund is processed back in cash

        // Remove the used code
        await userManager.RemoveClaimAsync(user, claims.First(c => c.Type == "CancelCheckinCode"));
        await userManager.RemoveClaimAsync(user, claims.First(c => c.Type == "CancelCheckinCodeExpiry"));

        await uow.Repository<Booking>().UpdateAsync(booking.BookingId, booking, cancellationToken);
        await uow.Repository<Payment>().UpdateAsync(payment.PaymentId, payment, cancellationToken);

        await uow.SaveChangesAsync(cancellationToken);
        //await uow.SaveChangesAsync(cancellationToken); // Dontt need, already called in CommitTransactionAsync
        var depositDto = mapper.Map<DepositFeeDto>(payment);
        var feeDto = mapper.Map<DepositFeeDto>(fee);

        var depositFeeDto = new DepositFeeDto
        {
            Type = feeDto.Type,
            Description = feeDto.Description,
            Amount = feeDto.Amount,
            Currency = feeDto.Currency,
            CreatedAt = feeDto.CreatedAt,
            Method = depositDto.Method,
            AmountPaid = refundAmount,
            ProviderReference = depositDto.ProviderReference
        };

        return depositFeeDto;
    }
}
