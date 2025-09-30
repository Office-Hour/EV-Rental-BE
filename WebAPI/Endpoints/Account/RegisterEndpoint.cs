using Application.UseCases.Authentication.Commands.Register;
using FastEndpoints;
using MediatR;
using WebAPI.Requests.Account;
using WebAPI.Responses;

namespace WebAPI.Endpoints.Account
{
    public class RegisterEndpoint(IMediator mediator) : Endpoint<RegisterRequest,ApiResponse>
    {
        public override void Configure()
        {
            Post("/api/account/register");
            AllowAnonymous();
            Description(x => x
                .WithTags("Account")
                .Produces<ApiResponse>(200)
                .Produces<ErrorMessage>(400)
            );

            Summary(s =>
            {
                s.Summary = "User Registration";
                s.Description = "Registers a new user and returns a success message.";
                s.Response<ApiResponse>(200, "Registration successful");
            });
        }
        public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
        {
            var command = new RegisterCommand
            {
                Email = req.Email,
                UserName = req.UserName,
                Password = req.Password,
                ConfirmPassword = req.ConfirmPassword
            };
            await mediator.Send(command, ct);
            await SendAsync(new ApiResponse("Registration successful"), 200, ct);
        }
    }
}
