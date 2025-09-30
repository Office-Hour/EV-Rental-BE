using System.Security.Claims;
using Application.DTOs;
using Application.UseCases.Profile.Queries.GetProfile;
using FastEndpoints;
using MediatR;
using WebAPI.Responses;

namespace WebAPI.Endpoints.Account
{
    public class GetProfileEndpoint(IMediator mediator) : EndpointWithoutRequest<ApiResponse<UserInfoDto>>
    {
        public override void Configure()
        {
            Get("/api/account/profile");
            //Policies("User"); // Equivalent to [Authorize(Policy = "User")]
            AuthSchemes("Bearer");// Equivalent to [Authorize]
            // Response type documentation
            Description(d => d
                .WithTags("Account")
                .Produces<ApiResponse<UserInfoDto>>(200, "application/json")
                .Produces<ErrorMessage>(400, "application/json")
            );
            Summary(s =>
            {
                s.Summary = "Get user profile";
                s.Description = "Retrieves the authenticated user's profile information";
                s.Response<ApiResponse<UserInfoDto>>(200, "User profile retrieved successfully");
                s.ResponseExamples[200] = new ApiResponse<UserInfoDto>(
                    new UserInfoDto
                    {
                        // Add example properties based on your UserInfoDto
                        Id = "123",
                        UserName = "John Doe",
                        Email = "john@example.com"
                    }, 
                    "Profile retrieved successfully");
            });
        }
        public override async Task<ApiResponse<UserInfoDto>> ExecuteAsync(CancellationToken ct)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            var command = new GetProfileQuery
            {
                UserId = userId
            };

            var result = await mediator.Send(command, ct);

            return new ApiResponse<UserInfoDto>(result, "Profile retrieved successfully");
        }
    }
}
