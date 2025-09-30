using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.CustomExceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.UseCases.Authentication.Commands.ForgotPassword
{
    public class ForgotPasswordCommandHandler(UserManager<IdentityUser> userManager) : IRequestHandler<ForgotPasswordCommand, string>
    {
        public async Task<string> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            // TODO: Implement the logic to handle the forgot password command
            // This typically involves generating a new password and sending it to the user's email
            // or sending a password reset link to the user's email.
            // You may also want to validate the user's email and check if the user exists in the system.
            // For now, we will just generate a new password and return it to the client.
            // In a real-world application, you would not return the new password to the client.
            var newPassword = Guid.NewGuid().ToString();
            var user = await userManager.FindByEmailAsync(request.Email) ?? throw new NotFoundException(null, "Email", $"User with email {request.Email} not found");
            //var token = userManager.GeneratePasswordResetTokenAsync(user);
            //var result = await userManager.ResetPasswordAsync(user, token.Result, newPassword);
            //if (!result.Succeeded)
            //{
            //    throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            //}

            // Send the new password to the user's email

            //var resetPasswordLink = $"https://example.com/reset-password?token={token.Result}&email={request.Email}";
            //await emailService.SendEmailAsync(user.Email, "Password Reset", $"Click here to reset your password: {resetPasswordLink}");

            // Recently, we haven't implement email service yet, so we will change the password directly and return to client
            var result = await userManager.ChangePasswordAsync(user, user.PasswordHash!, newPassword);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }
            return newPassword;
        }
    }
}
