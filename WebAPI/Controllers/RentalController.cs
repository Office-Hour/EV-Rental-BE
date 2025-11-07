using Application.DTOs;
using Application.DTOs.RentalManagement;
using Application.MapperProfiles;
using Application.UseCases.RentalManagement.Commands.CreateContract;
using Application.UseCases.RentalManagement.Commands.CreateRental;
using Application.UseCases.RentalManagement.Commands.ReceiveInspection;
using Application.UseCases.RentalManagement.Commands.ReceiveVehicle;
using Application.UseCases.RentalManagement.Commands.SignContract;
using Application.UseCases.RentalManagement.Queries.GetContractDetails;
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

    /// <summary>
    /// Create a new inspection for a rental
    /// </summary>
    /// <param name="request">Inspection creation details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Inspection ID</returns>
    /// <response code="200">Inspection created successfully</response>
    /// <response code="400">Validation error or invalid data</response>
    /// <response code="404">Rental or Staff not found</response>
    [HttpPost("inspection")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> ReceiveInspection(
        [FromBody] ReceiveInspectionRequest request,
        CancellationToken ct = default)
    {
        var command = new ReceiveInspectionCommand
        {
            RentalId = request.RentalId,
            CurrentBatteryCapacityKwh = request.CurrentBatteryCapacityKwh,
            InspectedAt = request.InspectedAt,
            InspectorStaffId = request.InspectorStaffId,
            URL = request.URL
        };
        var inspectionId = await mediator.Send(command, ct);
        return Ok(new ApiResponse<Guid>(inspectionId, "Inspection created successfully"));
    }

    /// <summary>
    /// Sign a contract for a rental
    /// </summary>
    /// <param name="request">Signature details including contract ID and signer information</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Contract ID</returns>
    /// <response code="200">Contract signed successfully</response>
    /// <response code="400">Validation error or invalid data</response>
    /// <response code="404">Contract or Rental not found</response>
    [HttpPost("contract/sign")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<Guid>>> SignContract(
        [FromBody] SignContractRequest request,
        CancellationToken ct = default)
    {
        var command = new SignContractCommand
        {
            CreateSignaturePayloadDto = request.CreateSignaturePayloadDto,
            ESignPayload = request.ESignPayload
        };
        var contractId = await mediator.Send(command, ct);
        return Ok(new ApiResponse<Guid>(contractId, "Contract signed successfully"));
    }

    /// <summary>
    /// Complete a rental by receiving the vehicle back
    /// </summary>
    /// <param name="request">Vehicle return details including rental ID and staff information</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">Vehicle received and rental completed successfully</response>
    /// <response code="400">Validation error or invalid rental state</response>
    /// <response code="404">Rental not found</response>
    [HttpPost("vehicle/receive")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ReceiveVehicle(
        [FromBody] ReceiveVehicleRequest request,
        CancellationToken ct = default)
    {
        var command = new ReceiveVehicleCommand
        {
            RentalId = request.RentalId,
            ReceivedAt = request.ReceivedAt,
            ReceivedByStaffId = request.ReceivedByStaffId
        };
        await mediator.Send(command, ct);
        return NoContent();
    }

    /// <summary>
    /// Get details of a specific contract
    /// </summary>
    /// <param name="contractId">Contract ID</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Contract details including signatures and status</returns>
    /// <response code="200">Returns contract details</response>
    /// <response code="404">Contract not found</response>
    [HttpGet("contract/{contractId}")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse<ContractDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ContractDetailsDto>>> GetContractDetails(
        [FromRoute] Guid contractId,
        CancellationToken ct = default)
    {
        var query = new GetContractDetailsQuery
        {
            ContractId = contractId
        };
        var result = await mediator.Send(query, ct);
        return Ok(new ApiResponse<ContractDetailsDto>(result, "Contract details retrieved successfully"));
    }
}
