using Application.DTOs;
using Application.DTOs.BookingManagement;
using Application.DTOs.StationManagement;
using Application.UseCases.BookingManagement.Queries.FilterVehiclesAvailable;
using Application.UseCases.StationManagement.Queries.ViewStationDetails;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Pages
{
    public class StationModel(IMediator mediator) : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartTime { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndTime { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 9;

        public StationDetailsDto? Station { get; set; }
        public PagedResult<VehicleDto>? AvailableVehicles { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Get station details
                var stationQuery = new ViewStationDetailsQuery
                {
                    StationId = Id,
                    PageNumber = 1,
                    PageSize = 1
                };

                Station = await mediator.Send(stationQuery);

                // Get available vehicles with optional time filtering
                var vehiclesQuery = new FilterVehiclesAvailableQuery
                {
                    StationId = Id,
                    StartTime = StartTime,
                    EndTime = EndTime,
                    PageNumber = PageNumber,
                    PageSize = PageSize
                };

                AvailableVehicles = await mediator.Send(vehiclesQuery);

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không th? t?i thông tin tr?m. Vui lòng th? l?i sau.";
                return Page();
            }
        }
    }
}
