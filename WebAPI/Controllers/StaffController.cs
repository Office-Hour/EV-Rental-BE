using Application.DTOs.BookingManagement;
using Application.DTOs.Profile;
using Application.DTOs.RentalManagement;
using Application.UseCases.BookingManagement.Queries.GetBookingFull;
using Application.UseCases.BookingManagement.Queries.GetRenterFull;
using Application.UseCases.RentalManagement.Queries.GetRentalFull;
using Application.UseCases.StationManagement.Queries.GetAllVehicle;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Responses;

namespace WebAPI.Controllers;

/// <summary>
/// Staff management endpoints for viewing all system data
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Produces("application/json")]
public class StaffController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StaffController> _logger;

    public StaffController(IMediator mediator, ILogger<StaffController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all bookings in the system (Staff only)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of all bookings with details</returns>
    /// <response code="200">Returns the list of all bookings</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    /// <response code="403">Forbidden - Staff role required</response>
    /// <remarks>
    /// This endpoint returns all bookings in the system regardless of status or renter.
    /// Staff can use this to monitor all booking activities, verify pending bookings,
    /// and manage cancellations.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/Staff/bookings
    ///     Authorization: Bearer {token}
    /// 
    /// </remarks>
    [HttpGet("bookings")]
    [ProducesResponseType(typeof(ApiResponse<List<BookingDetailsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<BookingDetailsDto>>>> GetAllBookings(
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Staff retrieving all bookings");

            var query = new GetBookingFullQuery();
            var bookings = await _mediator.Send(query, ct);

            _logger.LogInformation("Retrieved {Count} bookings", bookings.Count);

            return Ok(new ApiResponse<List<BookingDetailsDto>>(
                bookings,
                $"Successfully retrieved {bookings.Count} bookings"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all bookings");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorMessage
                {
                    Message = "An error occurred while retrieving bookings",
                    ErrorDetails = new List<ErrorDetail>
                    {
                        new() { ErrorMessage = ex.Message }
                    }
                });
        }
    }

    /// <summary>
    /// Get all renters in the system (Staff only)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of all renters with profile details</returns>
    /// <response code="200">Returns the list of all renters</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    /// <response code="403">Forbidden - Staff role required</response>
    /// <remarks>
    /// This endpoint returns all registered renters in the system.
    /// Staff can use this to view renter profiles, check risk scores,
    /// verify KYC status, and manage renter accounts.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/Staff/renters
    ///     Authorization: Bearer {token}
    /// 
    /// </remarks>
    [HttpGet("renters")]
    [ProducesResponseType(typeof(ApiResponse<List<RenterProfileDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<RenterProfileDto>>>> GetAllRenters(
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Staff retrieving all renters");

            var query = new GetRenterFullQuery();
            var renters = await _mediator.Send(query, ct);

            _logger.LogInformation("Retrieved {Count} renters", renters.Count);

            return Ok(new ApiResponse<List<RenterProfileDto>>(
                renters,
                $"Successfully retrieved {renters.Count} renters"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all renters");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorMessage
                {
                    Message = "An error occurred while retrieving renters",
                    ErrorDetails = new List<ErrorDetail>
                    {
                        new() { ErrorMessage = ex.Message }
                    }
                });
        }
    }

    /// <summary>
    /// Get all rentals in the system (Staff only)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of all rentals with details</returns>
    /// <response code="200">Returns the list of all rentals</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    /// <response code="403">Forbidden - Staff role required</response>
    /// <remarks>
    /// This endpoint returns all rentals in the system regardless of status.
    /// Staff can use this to monitor active rentals, check completed rentals,
    /// manage late returns, and review rental contracts.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/Staff/rentals
    ///     Authorization: Bearer {token}
    /// 
    /// Response includes:
    /// - Rental details (ID, start/end times, status)
    /// - Associated booking information
    /// - Vehicle details
    /// - Contract information (if exists)
    /// - Rating/feedback (if provided)
    /// 
    /// </remarks>
    [HttpGet("rentals")]
    [ProducesResponseType(typeof(ApiResponse<List<RentalDetailsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<RentalDetailsDto>>>> GetAllRentals(
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Staff retrieving all rentals");

            var query = new GetRentalFullQuery();
            var rentals = await _mediator.Send(query, ct);

            _logger.LogInformation("Retrieved {Count} rentals", rentals.Count);

            return Ok(new ApiResponse<List<RentalDetailsDto>>(
                rentals,
                $"Successfully retrieved {rentals.Count} rentals"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all rentals");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorMessage
                {
                    Message = "An error occurred while retrieving rentals",
                    ErrorDetails = new List<ErrorDetail>
                    {
                        new() { ErrorMessage = ex.Message }
                    }
                });
        }
    }

    /// <summary>
    /// Get all vehicles in the system (Staff only)
    /// </summary>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of all vehicles with details</returns>
    /// <response code="200">Returns the list of all vehicles</response>
    /// <response code="401">Unauthorized - Bearer token required</response>
    /// <response code="403">Forbidden - Staff role required</response>
    /// <remarks>
    /// This endpoint returns all vehicles in the system with their current status and details.
    /// Staff can use this to monitor vehicle availability, check battery capacity,
    /// view upcoming bookings, and manage vehicle maintenance.
    /// 
    /// Sample request:
    /// 
    ///     GET /api/Staff/vehicles
    ///     Authorization: Bearer {token}
    /// 
    /// Response includes:
    /// - Vehicle details (ID, make, model, year, range)
    /// - Current battery capacity
    /// - Pricing information (hourly/daily rates, deposit)
    /// - Vehicle status (Available, Booked, Maintenance)
    /// - Upcoming bookings for the vehicle
    /// 
    /// </remarks>
    [HttpGet("vehicles")]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleDetailsDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<List<VehicleDetailsDto>>>> GetAllVehicles(
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Staff retrieving all vehicles");

            var query = new GetAllVehicleQuery();
            var vehicles = await _mediator.Send(query, ct);
            var vehicleList = vehicles.ToList();

            _logger.LogInformation("Retrieved {Count} vehicles", vehicleList.Count);

            return Ok(new ApiResponse<List<VehicleDetailsDto>>(
                vehicleList,
                $"Successfully retrieved {vehicleList.Count} vehicles"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all vehicles");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new ErrorMessage
                {
                    Message = "An error occurred while retrieving vehicles",
                    ErrorDetails = new List<ErrorDetail>
                    {
                        new() { ErrorMessage = ex.Message }
                    }
                });
        }
    }
}
