using Application.CustomExceptions;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities.BookingManagement;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Application.UseCases.BookingManagement.Commands.RequestCancelCheckin;

public class RequestCancelCheckinCommandHandler(IUnitOfWork uow, UserManager<IdentityUser> userManager) : IRequestHandler<RequestCancelCheckinCommand>
{
    public async Task Handle(RequestCancelCheckinCommand request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId.ToString()) ?? throw new Exception("User not found");
        var booking = await uow.Repository<Booking>().GetByIdAsync(request.BookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found or access denied.");
        if (booking.Status != BookingStatus.Pending_Verification)
        {
            throw new InvalidTokenException("Only pending bookings can be canceled.");
        }

        // Create a Random 6-digit code
        var random = new Random();
        var code = random.Next(100000, 999999).ToString();
        if (string.IsNullOrEmpty(user.Email) && string.IsNullOrEmpty(user.PhoneNumber))
        {
            throw new Exception("User has no email or phone number to send the code.");
        }
        // Save code to database (ASP.NET Identity table)

        await userManager.AddClaimAsync(user, new Claim("CancelCheckinCode", code));
        await userManager.AddClaimAsync(user, new Claim("CancelCheckinCodeExpiry", DateTime.UtcNow.AddMinutes(15).ToString()));

        // Here you would send the code to the user's email or phone number

        if (!string.IsNullOrEmpty(user.PhoneNumber))
        {
            // smsService.SendCancellationCode(user.PhoneNumber, code);
        }
        else
            // emailService.SendCancellationCode(user.Email, code);


            Console.WriteLine($"CancelCheckinCode code sent to user: {code}");
        return;
    }
}
