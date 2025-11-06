using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Commands.CheckinBooking;
using Application.UseCases.BookingManagement.Queries.GetBookingDetails;
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
                TempData["ErrorMessage"] = "ID ??t ch? không h?p l?.";
                return RedirectToPage("./Index");
            }

            try
            {
                var query = new GetBookingDetailsQuery { BookingId = Id };
                Booking = await _mediator.Send(query);

                if (Booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm th?y ??t ch?.";
                    return RedirectToPage("./Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking {BookingId}", Id);
                TempData["ErrorMessage"] = "?ã x?y ra l?i khi t?i thông tin ??t ch?.";
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
                // Get StaffId from claims (assuming Staff has UserId that maps to StaffId)
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Không tìm th?y thông tin staff.";
                    return RedirectToPage();
                }

                // TODO: Map UserId to StaffId - for now using userId as staffId
                var staffId = Guid.Parse(userId);

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
                    ? "??t ch? ?ã ???c duy?t thành công!"
                    : "??t ch? ?ã b? t? ch?i.";

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying booking {BookingId}", Id);
                ModelState.AddModelError(string.Empty, $"Không th? x? lý: {ex.Message}");
                Booking = await _mediator.Send(new GetBookingDetailsQuery { BookingId = Id });
                return Page();
            }
        }
    }
}
