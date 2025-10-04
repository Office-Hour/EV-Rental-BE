using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.CustomExceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Profile.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler(UserManager<IdentityUser> userManager) : IRequestHandler<UpdateProfileCommand>
    {
        public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            // Get user by email, throw exception if not found
            var user = await userManager.FindByIdAsync(request.UserId) ?? throw new UserNotFoundException(request.UserId);

            // Update user properties
            if(!string.IsNullOrEmpty(request.UserName))
                user.UserName = request.UserName;
            if (!string.IsNullOrEmpty(request.Email))
                user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;
            // Update user in database
            var result = await userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                throw new InvalidTokenException(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
