using Application.CustomExceptions;
using Application.Interfaces;
using Domain.Entities.BookingManagement;
using Domain.Entities.RentalManagement;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Data;

namespace Application.UseCases.Authentication.Commands.Register;

public class RegisterCommandHandler(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork Uow) : IRequestHandler<RegisterCommand>
{
    public async Task Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            Email = request.Email,
            UserName = request.UserName ?? request.Email ,
        };
        var IsExistingUser = await userManager.FindByEmailAsync(request.Email);
        if (IsExistingUser != null)
        {
            throw new InvalidTokenException("Email is already registered.");
        }
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new InvalidTokenException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        await userManager.AddToRoleAsync(user, "RENTER");

        await Uow.Repository<Renter>().AddAsync(new Renter
        {
            UserId = Guid.Parse(user.Id),
            Address = "",
            DateOfBirth = null,
        });
    }
}
