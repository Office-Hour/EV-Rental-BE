using Application.Interfaces;
using Domain.Entities.Implement.Aggregates.Identity_KyC;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
            throw new Exception("Email is already registered.");
        }
        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
        }
        await userManager.AddToRoleAsync(user, "User");
        try
        {
            var domainUser = Uow.Repository<DomainUser>().AddAsync(new DomainUser
            {
                IdentityUserId = user.Id,
            }, cancellationToken);
            await Uow.SaveChangesAsync(cancellationToken);
        } catch (Exception ex)
        {
            // If saving to the domain user table fails, delete the created IdentityUser to maintain consistency
            await userManager.RemoveFromRoleAsync(user, "User");
            await userManager.DeleteAsync(user);
            throw new Exception("Registration failed. Please try again.", ex);
        }
        // Optionally, you can add claims or roles to the user here later
    }
}
