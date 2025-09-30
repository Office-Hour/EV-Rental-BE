using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.CustomExceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Profile.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler(UserManager<IdentityUser> userManager) : IRequestHandler<ChangePasswordCommand>
    {
        public async Task Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            // Get user by ID, throw exception if not found
            var user = await userManager.FindByIdAsync(request.UserId) ?? throw new UserNotFoundException(request.UserId);

            // Check if password change was successful
            var result = await userManager.ChangePasswordAsync(user!, request.OldPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
