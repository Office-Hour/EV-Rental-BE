using System.Security.Claims;
using Application.DTOs;
using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Commands.CreateBooking;
using Application.UseCases.BookingManagement.Commands.UploadKyc;
using Application.UseCases.BookingManagement.Queries.FilterVehiclesAvailable;
using Domain.Enums;
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
    /// Filter available vehicles at a station
    /// </summary>
    /// <param name="request">Filter criteria including station, vehicle, time range, and pagination</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of available vehicles</returns>
    /// <response code="200">Returns the list of available vehicles</response>
    /// <response code="400">Validation error (e.g., StartTime >= EndTime)</response>
    [HttpGet("vehicles/available")]
    [ProducesResponseType(typeof(PagedResult<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<VehicleDto>>> FilterVehiclesAvailable(
        [FromQuery] FilterVehicleAvailableRequest request,
        CancellationToken ct = default)
    {
        var query = new FilterVehiclesAvailableQuery
        {
            StationId = request.StationId,
            VehicleId = request.VehicleId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        var result = await mediator.Send(query, ct);

        return Ok(result);
    }

    /// <summary>
    /// Create a new booking
    /// </summary>
    /// <param name="request">Booking creation details</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Booking created successfully</response>
    /// <response code="400">Validation error or invalid booking details</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    [HttpPost]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse>> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken ct = default)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var command = new CreateBookingCommand
        {
            RenterId = Guid.Parse(userId),
            CreateBookingDto = new CreateBookingDto
            {
                VehicleAtStationId = request.VehicleAtStationId,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            },
            DepositFeeDto = new DepositFeeDto
            {
                Type = FeeType.Deposit,
                Description = request.DepositDescription,
                Amount = request.DepositAmount,
                Currency = request.DepositCurrency,
                Method = request.PaymentMethod,
                AmountPaid = request.AmountPaid,
                PaidAt = request.PaidAt,
                CreatedAt = DateTime.UtcNow,
                ProviderReference = request.ProviderReference
            }
        };

        await mediator.Send(command, ct);

        return Ok(new ApiResponse("Booking created successfully"));
    }

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