using Application.Interfaces;
using Domain.Entities.Implement.Aggregates.Identity_KyC;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Authentication.Commands.RegisterAdmin;

public class RegisterAdminCommandHandler(UserManager<IdentityUser> userManager, IUnitOfWork UoW) : IRequestHandler<RegisterAdminCommand>
{
    public async Task Handle(RegisterAdminCommand request, CancellationToken cancellationToken)
    {
        var user = new IdentityUser
        {
            Email = request.Email,
            UserName = request.UserName ?? request.Email,
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
        try
        {
            var domainUser = UoW.Repository<DomainUser>().AddAsync(new DomainUser
            {
                IdentityUserId = user.Id,
            }, cancellationToken);
            await UoW.SaveChangesAsync(cancellationToken);
            // Assign the "Admin" role to the user
            var addToRoleResult = await userManager.AddToRoleAsync(user, "Admin");
            if (!addToRoleResult.Succeeded)
            {
                throw new Exception(string.Join(", ", addToRoleResult.Errors.Select(e => e.Description)));
            }
        }
        catch (Exception ex)
        {
            // If saving to the domain user table or assigning role fails, delete the created IdentityUser to maintain consistency
            await userManager.DeleteAsync(user);
            throw new Exception("Registration failed. Please try again.", ex);
        }
        // Optionally, you can add claims or additional roles to the user here later
    }
}
