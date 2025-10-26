using System.Security.Claims;
using Application.DTOs;
using Application.DTOs.BookingManagement;
using Application.DTOs.Profile;
using Application.UseCases.BookingManagement.Commands.CreateBooking;
using Application.UseCases.BookingManagement.Commands.UploadKyc;
using Application.UseCases.BookingManagement.Queries.FilterVehiclesAvailable;
using Application.UseCases.BookingManagement.Queries.GetBookingByRenter;
using Application.UseCases.BookingManagement.Queries.ViewVehicleDetails;
using Application.UseCases.BookingManagement.Queries.ViewVehiclesByStation;
using Application.UseCases.Profile.Queries.GetRenterProfile;
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
    /// Get renter profile
    /// </summary>
    /// <param name="userId">The user ID (optional - defaults to authenticated user)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Renter profile details</returns>
    /// <response code="200">Returns the renter profile</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    /// <response code="404">Renter not found</response>
    /// <remarks>
    /// If userId is not provided, returns the profile of the authenticated user.
    /// Staff can provide a userId to view any renter's profile.
    /// </remarks>
    [HttpGet("renter-profile")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(RenterProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RenterProfileDto>> GetRenterProfile(
        [FromQuery] Guid? userId = null,
        CancellationToken ct = default)
    {
        var authenticatedUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        
        // If userId is not provided, use authenticated user's ID
        var targetUserId = userId ?? Guid.Parse(authenticatedUserId);

        var query = new GetRenterProfileQuery
        {
            UserId = targetUserId
        };

        var result = await mediator.Send(query, ct);

        return Ok(result);
    }

    /// <summary>
    /// Get bookings by renter ID
    /// </summary>
    /// <param name="renterId">The ID of the renter</param>
    /// <param name="pageNumber">Page number for pagination (default: 1)</param>
    /// <param name="pageSize">Page size for pagination (default: 10)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of booking details for the renter</returns>
    /// <response code="200">Returns the list of bookings</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    /// <response code="404">Renter not found</response>
    /// <remarks>
    /// Both renters and staff must provide the renterId parameter.
    /// Renters can view their own bookings, staff can view any renter's bookings.
    /// </remarks>
    [HttpGet]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [ProducesResponseType(typeof(PagedResult<BookingDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<BookingDetailsDto>>> GetBookingByRenter(
        [FromQuery] Guid renterId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new GetBookingByRenterQuery
        {
            RenterId = renterId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await mediator.Send(query, ct);

        return Ok(result);
    }

    /// <summary>
    /// View detailed information about a specific vehicle
    /// </summary>
    /// <param name="vehicleId">The ID of the vehicle to view</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Detailed vehicle information including pricing and upcoming bookings</returns>
    /// <response code="200">Returns the vehicle details</response>
    /// <response code="404">Vehicle not found or not available at any station</response>
    [HttpGet("vehicles/{vehicleId}")]
    [ProducesResponseType(typeof(VehicleDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleDetailsDto>> ViewVehicleDetails(
        [FromRoute] Guid vehicleId,
        CancellationToken ct = default)
    {
        var query = new ViewVehicleDetailsQuery
        {
            VehicleId = vehicleId
        };

        var result = await mediator.Send(query, ct);

        return Ok(result);
    }

    /// <summary>
    /// View vehicles at a station with optional availability filtering
    /// </summary>
    /// <param name="request">Filter criteria including station, optional vehicle, optional time range, and pagination</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Paginated list of vehicles at the station</returns>
    /// <response code="200">Returns the list of vehicles at the station</response>
    /// <response code="400">Validation error (e.g., StartTime >= EndTime)</response>
    /// <remarks>
    /// When StartTime and EndTime are provided, this endpoint checks availability against existing bookings.
    /// When time range is not provided, it simply returns all available vehicles at the station.
    /// </remarks>
    [HttpGet("vehicles/by-station")]
    [ProducesResponseType(typeof(PagedResult<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<VehicleDto>>> ViewVehiclesByStation(
        [FromQuery] ViewVehiclesByStationRequest request,
        CancellationToken ct = default)
    {
        // If time range is provided, use FilterVehiclesAvailable for booking conflict checking
        if (request.StartTime.HasValue || request.EndTime.HasValue)
        {
            var filterQuery = new FilterVehiclesAvailableQuery
            {
                StationId = request.StationId,
                VehicleId = request.VehicleId,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            var filterResult = await mediator.Send(filterQuery, ct);
            return Ok(filterResult);
        }

        // Otherwise, use simple ViewVehiclesByStation
        var query = new ViewVehiclesByStationQuery
        {
            StationId = request.StationId,
            VehicleId = request.VehicleId,
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