using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Queries.GetBookingDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WebApp.Areas.Renter.Pages.Booking
{
    [Authorize(Roles = "Renter")]
    public class DetailsModel(IMediator mediator) : PageModel
    {
        public BookingDetailsDto? Booking { get; set; }
        public string? ErrorMessage { get; set; }
        public bool CanCancel { get; set; }
        public bool CanRequestCancel { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                var query = new GetBookingDetailsQuery
                {
                    BookingId = id
                };

                Booking = await mediator.Send(query);

                // Authorization check - ensure renter owns this booking
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Booking.RenterId.ToString() != userId)
                {
                    return Forbid();
                }

                // Determine available actions
                CanRequestCancel = Booking.Status == Domain.Enums.BookingStatus.Pending_Verification;
                CanCancel = Booking.Status == Domain.Enums.BookingStatus.Pending_Verification;

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = "Không thể tải thông tin đặt chỗ. Vui lòng thử lại sau.";
                return Page();
            }
        }
    }
}
