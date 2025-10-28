using Application.DTOs;
using Application.DTOs.RentalManagement;
using Application.UseCases.RentalManagement.Commands.CreateContract;
using Application.UseCases.RentalManagement.Commands.CreateRental;
using Application.UseCases.RentalManagement.Queries.GetRentalByRenter;
using Application.UseCases.RentalManagement.Queries.GetRentalDetails;
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

    /// <summary>
    /// Get paginated rentals by renter ID
    /// </summary>
    /// <param name="renterId">Renter ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paged list of rentals</returns>
    /// <response code="200">Returns paged rentals</response>
    [HttpGet("by-renter")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<RentalDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<RentalDto>>>> GetRentalsByRenterId(
        [FromQuery] Guid renterId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new GetRentalsByRenterQuery
        {
            RenterId = renterId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await mediator.Send(query, ct);
        return Ok(new ApiResponse<PagedResult<RentalDto>>(result, "Rentals retrieved successfully"));
    }

    /// <summary>
    /// Get details of a specific rental
    /// </summary>
    /// <param name="rentalId">Rental ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Rental details</returns>
    /// <response code="200">Returns rental details</response>
    /// <response code="404">Rental not found</response>
    [HttpGet("{rentalId}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<RentalDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<RentalDetailsDto>>> GetRentalDetails(
        [FromRoute] Guid rentalId,
        CancellationToken ct = default)
    {
        var query = new GetRentalDetailsQuery
        {
            RentalId = rentalId
        };
        var result = await mediator.Send(query, ct);
        return Ok(new ApiResponse<RentalDetailsDto>(result, "Rental details retrieved successfully"));
    }
}
