using Application.DTOs.BookingManagement;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Areas.Renter.Pages.Booking
{
    [Authorize(Roles = "Renter")]
    public class CreateModel : PageModel
    {
        [BindProperty]
        public CreateBookingDto Input { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public Guid? VehicleAtStationId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartTime { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndTime { get; set; }

        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Pre-fill from query parameters
            if (VehicleAtStationId.HasValue)
            {
                Input.VehicleAtStationId = VehicleAtStationId.Value;
            }

            if (StartTime.HasValue)
            {
                Input.StartTime = StartTime.Value;
            }
            else
            {
                // Default to tomorrow
                Input.StartTime = DateTime.Now.AddDays(1).Date.AddHours(9);
            }

            if (EndTime.HasValue)
            {
                Input.EndTime = EndTime.Value;
            }
            else
            {
                // Default to 3 days later
                Input.EndTime = DateTime.Now.AddDays(4).Date.AddHours(18);
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Validation
            if (Input.StartTime <= DateTime.Now)
            {
                ModelState.AddModelError("Input.StartTime", "Thời gian bắt đầu phải sau thời điểm hiện tại");
                return Page();
            }

            if (Input.EndTime <= Input.StartTime)
            {
                ModelState.AddModelError("Input.EndTime", "Thời gian kết thúc phải sau thời gian bắt đầu");
                return Page();
            }

            // Store booking info in TempData for next step
            TempData["BookingData"] = System.Text.Json.JsonSerializer.Serialize(Input);

            return RedirectToPage("./Payment");
        }
    }
}
