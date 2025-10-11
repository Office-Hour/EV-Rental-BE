using System.Security.Claims;
using Application.UseCases.BookingManagement.Commands.UploadKyc;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Requests.BookingManagement;
using WebAPI.Responses;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BookingController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Upload KYC Document
    /// </summary>
    /// <param name="request">KYC document details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">KYC document uploaded successfully</response>
    /// <response code="400">Validation error or invalid document type</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    [HttpPost("upload-kyc")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> UploadKyc(
        [FromBody] UploadKycRequest request,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new UploadKycCommand
        {
            UserId = Guid.Parse(userId),
            Type = request.Type,
            DocumentNumber = request.DocumentNumber
        };

        await mediator.Send(command, ct);

        return Ok(new ApiResponse("KYC document uploaded successfully"));
    }
}