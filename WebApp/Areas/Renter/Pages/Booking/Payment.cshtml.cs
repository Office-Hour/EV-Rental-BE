using Application.DTOs.BookingManagement;
using Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Areas.Renter.Pages.Booking
{
    [Authorize(Roles = "Renter")]
    public class PaymentModel : PageModel
    {
        [BindProperty]
        public DepositFeeDto Input { get; set; } = new();

        public CreateBookingDto? BookingData { get; set; }
        public decimal DepositAmount { get; set; }
        public TimeSpan RentalDuration { get; set; }

        public IActionResult OnGet()
        {
            // Retrieve booking data from TempData
            var bookingDataJson = TempData.Peek("BookingData") as string;
            if (string.IsNullOrEmpty(bookingDataJson))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đặt chỗ. Vui lòng thử lại.";
                return RedirectToPage("./Create");
            }

            BookingData = System.Text.Json.JsonSerializer.Deserialize<CreateBookingDto>(bookingDataJson);
            if (BookingData == null)
            {
                TempData["ErrorMessage"] = "Dữ liệu đặt chỗ không hợp lệ.";
                return RedirectToPage("./Create");
            }

            // Calculate deposit (example: 20% of estimated rental cost)
            RentalDuration = BookingData.EndTime - BookingData.StartTime;
            var estimatedCost = CalculateEstimatedCost(RentalDuration);
            DepositAmount = Math.Round(estimatedCost * 0.2m, 2, MidpointRounding.AwayFromZero);

            // Pre-fill deposit info
            Input.Type = FeeType.Deposit;
            Input.Description = $"Tiền đặt cọc cho đặt chỗ từ {BookingData.StartTime:dd/MM/yyyy HH:mm} đến {BookingData.EndTime:dd/MM/yyyy HH:mm}";
            Input.Amount = DepositAmount;
            Input.Currency = Currency.VND;
            Input.Method = PaymentMethod.Unknown;

            return Page();
        }

        public IActionResult OnPost()
        {
            // Retrieve booking data
            var bookingDataJson = TempData.Peek("BookingData") as string;
            if (string.IsNullOrEmpty(bookingDataJson))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đặt chỗ.";
                return RedirectToPage("./Create");
            }

            BookingData = System.Text.Json.JsonSerializer.Deserialize<CreateBookingDto>(bookingDataJson);

            if (!ModelState.IsValid)
            {
                // Recalculate for display
                RentalDuration = BookingData!.EndTime - BookingData.StartTime;
                var estimatedCost = CalculateEstimatedCost(RentalDuration);
                DepositAmount = Math.Round(estimatedCost * 0.2m, 2, MidpointRounding.AwayFromZero);
                return Page();
            }

            // Validate payment method selected
            if (Input.Method == PaymentMethod.Unknown)
            {
                ModelState.AddModelError("Input.Method", "Vui lòng chọn phương thức thanh toán");
                RentalDuration = BookingData!.EndTime - BookingData.StartTime;
                var estimatedCost = CalculateEstimatedCost(RentalDuration);
                DepositAmount = Math.Round(estimatedCost * 0.2m, 2, MidpointRounding.AwayFromZero);
                return Page();
            }

            // Mock payment processing
            Input.AmountPaid = Input.Amount;
            Input.PaidAt = DateTime.UtcNow;
            Input.ProviderReference = $"MOCK_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

            // Store payment data in TempData for confirmation
            TempData["PaymentData"] = System.Text.Json.JsonSerializer.Serialize(Input);
            TempData["BookingData"] = bookingDataJson; // Keep for next step

            return RedirectToPage("./Confirm");
        }

        private decimal CalculateEstimatedCost(TimeSpan duration)
        {
            // Example pricing: 100,000 VND per day
            var days = (decimal)duration.TotalDays;
            if (days < 1) days = 1; // Minimum 1 day
            return days * 100000m;
        }
    }
}
