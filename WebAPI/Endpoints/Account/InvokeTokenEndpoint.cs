using Application.DTOs;
using Application.UseCases.Authentication.Commands.RefreshToken;
using FastEndpoints;
using MediatR;
using WebAPI.Requests.Account;
using WebAPI.Responses;

namespace WebAPI.Endpoints.Account
{
    public class InvokeTokenEndpoint(IMediator mediator) : Endpoint<InvokeTokenRequest, ApiResponse<AuthDto>>
    {
        public override void Configure()
        {
            Post("/api/account/invoke-token");
            AuthSchemes("Bearer"); // Equivalent to [Authorize]
            Description(x => x
                .WithTags("Account")
                .Produces<ApiResponse<AuthDto>>(200)
                .Produces<ErrorMessage>(400)
                .Produces<ErrorMessage>(401)
            );
            Summary(s =>
            {
                s.Summary = "Invoke Token";
                s.Description = "Invokes a token for the user.";
                s.Response<ApiResponse<AuthDto>>(200, "Token invoked successfully");
            });
        }
        public override async Task HandleAsync(InvokeTokenRequest req, CancellationToken ct)
        {
            var command = new RefreshTokenCommand
            {
                RefreshToken = req.RefreshToken
            };
            var result = await mediator.Send(command, ct);
            await SendAsync(new ApiResponse<AuthDto>(result, "Token invoked successfully"), 200, ct);
        }
    }
}
