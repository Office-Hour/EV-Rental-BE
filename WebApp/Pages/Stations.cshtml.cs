using Application.DTOs;
using Application.DTOs.StationManagement;
using Application.UseCases.StationManagement.Queries.FilterStation;
using Application.UseCases.StationManagement.Queries.ViewStation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    public class StationsModel(IMediator mediator) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? SearchName { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchAddress { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 9;

        public PagedResult<StationDto>? Stations { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // If search parameters are provided, use FilterStation
            if (!string.IsNullOrWhiteSpace(SearchName) || !string.IsNullOrWhiteSpace(SearchAddress))
            {
                var filterQuery = new FilterStationQuery
                {
                    Name = SearchName,
                    Address = SearchAddress,
                    PageNumber = PageNumber,
                    PageSize = PageSize
                };

                Stations = await mediator.Send(filterQuery);
            }
            else
            {
                // Otherwise, get all stations
                var viewQuery = new ViewStationQuery
                {
                    Paging = new Application.DTOs.PagingDto
                    {
                        Page = PageNumber,
                        PageSize = PageSize
                    }
                };

                Stations = await mediator.Send(viewQuery);
            }

            return Page();
        }
    }
}
