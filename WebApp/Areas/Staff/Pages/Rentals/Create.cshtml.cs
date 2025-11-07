using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Queries.GetBookingDetails;
using Application.UseCases.RentalManagement.Commands.CreateRental;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace WebApp.Areas.Staff.Pages.Rentals
{
    [Authorize(Roles = "Staff")]
    public class CreateModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IMediator mediator, ILogger<CreateModel> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid BookingId { get; set; }

        public BookingDetailsDto? Booking { get; set; }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {

            [Required]
            public DateTime StartTime { get; set; }

            [Required]
            public DateTime EndTime { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (BookingId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Booking ID không hợp lệ.";
                return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
            }

            try
            {
                var query = new GetBookingDetailsQuery { BookingId = BookingId };
                Booking = await _mediator.Send(query);

                if (Booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy booking.";
                    return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
                }

                // Pre-fill form
                Input.StartTime = Booking.StartTime;
                Input.EndTime = Booking.EndTime;
                // TODO: Get VehicleId from Booking.VehicleAtStationId
                // For now, we need to query VehicleAtStation to get VehicleId

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading booking {BookingId}", BookingId);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi.";
                return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var command = new CreateRentalCommand
                {
                    BookingId = BookingId,
                    StartTime = Input.StartTime,
                    EndTime = Input.EndTime
                };

                var rentalId = await _mediator.Send(command);

                _logger.LogInformation("Rental {RentalId} created for booking {BookingId}", rentalId, BookingId);

                TempData["SuccessMessage"] = "Rental đã được tạo thành công!";
                return RedirectToPage("/Staff/Rentals/Details", new { area = "Staff", id = rentalId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rental for booking {BookingId}", BookingId);
                ModelState.AddModelError(string.Empty, $"Không thể tạo rental: {ex.Message}");
                Booking = await _mediator.Send(new GetBookingDetailsQuery { BookingId = BookingId });
                return Page();
            }
        }
    }
}
