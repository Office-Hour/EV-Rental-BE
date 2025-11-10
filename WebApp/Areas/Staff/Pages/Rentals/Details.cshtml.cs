using Application.DTOs.RentalManagement;
using Application.UseCases.RentalManagement.Queries.GetRentalDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Areas.Staff.Pages.Rentals
{
    [Authorize(Roles = "Staff")]
    public class DetailsModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IMediator mediator, ILogger<DetailsModel> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public RentalDetailsDto? Rental { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "ID thuê xe không hợp lý.";
                return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
            }

            try
            {
                var query = new GetRentalDetailsQuery { RentalId = Id };
                Rental = await _mediator.Send(query);

                if (Rental == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thuê xe.";
                    return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rental {RentalId}", Id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin thuê xe.";
                return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
            }
        }
    }
}
