using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Queries.GetBookingFull;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Areas.Staff.Pages.Bookings
{
    [Authorize(Roles = "Staff")]
    public class IndexModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IMediator mediator, ILogger<IndexModel> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public List<BookingDetailsDto> Bookings { get; set; } = new();
        public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                _logger.LogInformation("Staff viewing all bookings");

                var query = new GetBookingFullQuery();
                Bookings = await _mediator.Send(query);

                _logger.LogInformation("Retrieved {Count} bookings", Bookings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings");
                ErrorMessage = "Không th? t?i danh sách ??t ch?. Vui lòng th? l?i sau.";
            }
        }
    }
}
