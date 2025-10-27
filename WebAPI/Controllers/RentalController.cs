using Application.UseCases.RentalManagement.Commands.CreateContract;
using Application.UseCases.RentalManagement.Commands.CreateRental;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Requests.RentalManagement;
using WebAPI.Responses;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class RentalController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Create a new rental
    /// </summary>
    /// <param name="request">Rental creation details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Rental ID</returns>
    /// <response code="200">Rental created successfully</response>
    /// <response code="400">Validation error or invalid data</response>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateRental(
        [FromBody] CreateRentalRequest request,
        CancellationToken ct = default)
    {
        var command = new CreateRentalCommand
        {
            BookingId = request.BookingId,
            VehicleId = request.VehicleId,
            StartTime = request.StartTime,
            EndTime = request.EndTime
        };
        var rentalId = await mediator.Send(command, ct);
        return Ok(new ApiResponse<Guid>(rentalId, "Rental created successfully"));
    }

    /// <summary>
    /// Create a new contract for a rental
    /// </summary>
    /// <param name="request">Contract creation details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Contract ID</returns>
    /// <response code="200">Contract created successfully</response>
    /// <response code="400">Validation error or invalid data</response>
    [HttpPost("contract")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateContract(
        [FromBody] CreateContractRequest request,
        CancellationToken ct = default)
    {
        var command = new CreateContractCommand
        {
            RentalId = request.RentalId,
            Provider = request.Provider
        };
        var contractId = await mediator.Send(command, ct);
        return Ok(new ApiResponse<Guid>(contractId, "Contract created successfully"));
    }
}
