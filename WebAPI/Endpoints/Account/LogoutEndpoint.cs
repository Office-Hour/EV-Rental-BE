using System.Security.Claims;
using Application.UseCases.Authentication.Commands.Logout;
using FastEndpoints;
using MediatR;
using WebAPI.Requests.Account;
using WebAPI.Responses;

namespace WebAPI.Endpoints.Account
{
    public class LogoutEndpoint(IMediator mediator) : Endpoint<LogoutRequest, ApiResponse>
    {
        public override void Configure()
        {
            Post("/api/account/logout");
            AuthSchemes("Bearer"); // Equivalent to [Authorize]
            Description(x => x
                .WithTags("Account")
                .Produces<ApiResponse>(200)
                .Produces<ErrorMessage>(400)
                .Produces<ErrorMessage>(401)
            );
            Summary(s =>
            {
                s.Summary = "User Logout";
                s.Description = "Logs out the user and invalidates the session.";
                s.Response<ApiResponse>(200, "Logout successful");
            });
        }

        public override async Task HandleAsync(LogoutRequest req, CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var command = new LogoutCommand
            {
                RefreshToken = req.RefreshToken
            };
            await mediator.Send(command, ct);
            await SendAsync(new ApiResponse("Logout successful"), 200, ct);
        }
    }
}
