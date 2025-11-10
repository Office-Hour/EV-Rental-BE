using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Commands.CheckinBooking;
using Application.UseCases.BookingManagement.Queries.GetBookingDetails;
using Application.UseCases.RentalManagement.Queries.GetRentalByBookingId;
using Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebApp.Areas.Staff.Pages.Bookings
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

        public BookingDetailsDto? Booking { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required]
            public BookingVerificationStatus VerificationStatus { get; set; }

            public string? CancelReason { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "ID đặt chỗ không hợp lệ.";
                return RedirectToPage("./Index");
            }

            try
            {
                var query = new GetBookingDetailsQuery { BookingId = Id };
                Booking = await _mediator.Send(query);

                if (Booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy đặt chỗ.";
                    return RedirectToPage("./Index");
                }

                var rental = await _mediator.Send(new GetRentalByBookingIdQuery { BookingId = Id });

                ViewData["RentalId"] = rental?.RentalId;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking {BookingId}", Id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin đặt chỗ.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            if (!ModelState.IsValid)
            {
                Booking = await _mediator.Send(new GetBookingDetailsQuery { BookingId = Id });
                return Page();
            }

            try
            {
                var staffIdAsString = User.FindFirstValue("StaffId");
                var staffId = Guid.Parse(staffIdAsString);

                var command = new CheckinBookingCommand
                {
                    BookingId = Id,
                    VerifiedByStaffId = staffId,
                    BookingVerificationStatus = Input.VerificationStatus,
                    CancelReason = Input.CancelReason
                };

                await _mediator.Send(command);

                _logger.LogInformation("Booking {BookingId} verified by staff {StaffId} with status {Status}",
                    Id, staffId, Input.VerificationStatus);

                TempData["SuccessMessage"] = Input.VerificationStatus == BookingVerificationStatus.Approved
                    ? "Đặt chỗ đã được duyệt thành công!"
                    : "Đặt chỗ đã bị từ chối.";

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying booking {BookingId}", Id);
                ModelState.AddModelError(string.Empty, $"Không thể xử lý: {ex.Message}");
                Booking = await _mediator.Send(new GetBookingDetailsQuery { BookingId = Id });
                return Page();
            }
        }
    }
}
