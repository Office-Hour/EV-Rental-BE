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
            public Guid VehicleId { get; set; }

            [Required]
            public DateTime StartTime { get; set; }

            [Required]
            public DateTime EndTime { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (BookingId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Booking ID không h?p l?.";
                return RedirectToPage("/Bookings/Index");
            }

            try
            {
                var query = new GetBookingDetailsQuery { BookingId = BookingId };
                Booking = await _mediator.Send(query);

                if (Booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm th?y booking.";
                    return RedirectToPage("/Bookings/Index");
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
                TempData["ErrorMessage"] = "?ã x?y ra l?i.";
                return RedirectToPage("/Bookings/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Booking = await _mediator.Send(new GetBookingDetailsQuery { BookingId = BookingId });
                return Page();
            }

            try
            {
                var command = new CreateRentalCommand
                {
                    BookingId = BookingId,
                    VehicleId = Input.VehicleId,
                    StartTime = Input.StartTime,
                    EndTime = Input.EndTime
                };

                var rentalId = await _mediator.Send(command);

                _logger.LogInformation("Rental {RentalId} created for booking {BookingId}", rentalId, BookingId);

                TempData["SuccessMessage"] = "Rental ?ã ???c t?o thành công!";
                return RedirectToPage("/Rentals/Details", new { id = rentalId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating rental for booking {BookingId}", BookingId);
                ModelState.AddModelError(string.Empty, $"Không th? t?o rental: {ex.Message}");
                Booking = await _mediator.Send(new GetBookingDetailsQuery { BookingId = BookingId });
                return Page();
            }
        }
    }
}
