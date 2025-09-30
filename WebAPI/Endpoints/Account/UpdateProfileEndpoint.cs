using System.Security.Claims;
using Application.UseCases.Profile.Commands.UpdateProfile;
using FastEndpoints;
using MediatR;
using WebAPI.Requests.Account;
using WebAPI.Responses;

namespace WebAPI.Endpoints.Account
{
    public class UpdateProfileEndpoint(IMediator mediator) : Endpoint<UpdateProfileRequest, ApiResponse>
    {
        public override void Configure()
        {
            Put("/api/account/update-profile");
            AuthSchemes("Bearer"); // Equivalent to [Authorize]

            Description(x => x
                .WithTags("Account")
                .Produces<ApiResponse>(200)
                .Produces<ErrorMessage>(400)
                .Produces<ErrorMessage>(401)
            );
        }

        public override async Task HandleAsync(UpdateProfileRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var command = new UpdateProfileCommand
            {
                UserId = userId,
                UserName = req.UserName,
                Email = req.Email,
                PhoneNumber = req.PhoneNumber
            };
            await mediator.Send(command, ct);
            await SendAsync(new ApiResponse("Profile updated successfully"), 200, ct);
        }
    }
}
