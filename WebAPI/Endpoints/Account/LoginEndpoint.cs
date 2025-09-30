using Application.DTOs;
using Application.UseCases.Authentication.Commands.Login;
using FastEndpoints;
using MediatR;
using WebAPI.Requests.Account;
using WebAPI.Responses;

namespace WebAPI.Endpoints.Account
{
    public class LoginEndpoint(IMediator mediator) : Endpoint<LoginRequest, ApiResponse<AuthDto>>
    {
        public override void Configure()
        {
            Post("/api/account/login");
            AllowAnonymous();
            Description(x => x
                .WithTags("Account")
                .Produces<ApiResponse<AuthDto>>(200)
                .Produces<ErrorMessage>(400)
            );
            Summary(s =>
            {
                s.Summary = "User Login";
                s.Description = "Authenticates a user and returns an authentication token.";
                s.Response<ApiResponse<AuthDto>>(200, "Login successful");
            });
        }

        public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
        {
            var command = new LoginCommand
            {
                Email = req.Email,
                Password = req.Password
            };
            var result = await mediator.Send(command, ct);

            await SendAsync(new ApiResponse<AuthDto>(result, "Login successful"), 200, ct);
        }
    }
}
