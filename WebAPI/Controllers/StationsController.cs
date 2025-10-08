using Application.DTOs;
using Application.UseCases.StationManagement.Queries.ViewStation;
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
    /// List stations (paged, optional search & sort).
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>A paged list of stations</returns>
    /// <response code="200">Returns the paged list of stations</response>
    /// <response code="400">If the request is invalid</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StationDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorMessage), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<PagedResult<StationDto>>>> GetStations(
        [FromQuery] ViewStationRequest request,
        CancellationToken ct = default)
    {
        var query = new ViewStationQuery
        {
            Paging = new PagingDto
            {
                Page = request.Page,
                PageSize = request.PageSize,
            }
        };

        var result = await mediator.Send(query, ct);

        return Ok(new ApiResponse<PagedResult<StationDto>>(result, "Stations retrieved successfully"));
    }
}