using Application.DTOs;
using Application.DTOs.StationManagement;
using Application.UseCases.StationManagement.Queries.FilterStation;
using Application.UseCases.StationManagement.Queries.ViewStationDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Requests.StationManagement;
using WebAPI.Responses;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StationsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// List stations with optional filtering (paged, optional search by name/address).
    /// </summary>
    /// <param name="request">Filter and pagination parameters</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A paged list of stations</returns>
    /// <response code="200">Returns the paged list of stations</response>
    /// <response code="400">If the request is invalid</response>
    /// <remarks>
    /// You can optionally filter stations by name and/or address.
    /// The capacity field shows the current number of vehicles at each station.
    /// </remarks>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResult<StationDto>>>> GetStations(
        [FromQuery] ViewStationRequest request,
        CancellationToken ct = default)
    {
        var query = new FilterStationQuery
        {
            Name = request.Name,
            Address = request.Address,
            PageNumber = request.Page,
            PageSize = request.PageSize
        };

        var result = await mediator.Send(query, ct);

        return Ok(new ApiResponse<PagedResult<StationDto>>(result, "Stations retrieved successfully"));
    }

    /// <summary>
    /// Get detailed information about a specific station including available vehicles
    /// </summary>
    /// <param name="stationId">The ID of the station to view</param>
    /// <param name="pageNumber">Page number for vehicles pagination (default: 1)</param>
    /// <param name="pageSize">Page size for vehicles pagination (default: 10)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Station details with paginated list of available vehicles</returns>
    /// <response code="200">Returns the station details</response>
    /// <response code="404">Station not found</response>
    [HttpGet("{stationId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<StationDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<StationDetailsDto>>> GetStationDetails(
        [FromRoute] Guid stationId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var query = new ViewStationDetailsQuery
        {
            StationId = stationId,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var result = await mediator.Send(query, ct);

        return Ok(new ApiResponse<StationDetailsDto>(result, "Station details retrieved successfully"));
    }
}