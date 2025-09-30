using System.Security.Claims;
using Application.UseCases.Profile.Commands.ChangePassword;
using FastEndpoints;
using MediatR;
using WebAPI.Requests.Account;
using WebAPI.Responses;

namespace WebAPI.Endpoints.Account
{
    public class ChangePasswordEndpoint(IMediator mediator) : Endpoint<ChangePasswordRequest, ApiResponse>
    {
        public override void Configure()
        {
            Post("/api/account/change-password");
            AllowAnonymous();
            Description(x => x
                .WithTags("Account")
                .Produces<ApiResponse>(200)
                .Produces<ErrorMessage>(400)
                .Produces<ErrorMessage>(401)
            );

            Policies("User");
        }
        public override async Task HandleAsync(ChangePasswordRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var command = new ChangePasswordCommand
            {
                UserId = userId,
                OldPassword = req.OldPassword,
                NewPassword = req.NewPassword,
                ConfirmPassword = req.ConfirmPassword
            };
            await mediator.Send(command, ct);
            // Simulate password change logic
            await SendAsync(new ApiResponse("Password changed successfully"), 200, ct);
        }
    }
}
