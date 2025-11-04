using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Commands.CreateBooking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WebApp.Areas.Renter.Pages.Booking
{
    [Authorize(Roles = "Renter")]
    public class ConfirmModel(IMediator mediator) : PageModel
    {
        public CreateBookingDto? BookingData { get; set; }
        public DepositFeeDto? PaymentData { get; set; }
        public TimeSpan RentalDuration { get; set; }
        public bool IsProcessing { get; set; }
        public string? ErrorMessage { get; set; }

        public IActionResult OnGet()
        {
            // Retrieve data from TempData
            var bookingDataJson = TempData.Peek("BookingData") as string;
            var paymentDataJson = TempData.Peek("PaymentData") as string;

            if (string.IsNullOrEmpty(bookingDataJson) || string.IsNullOrEmpty(paymentDataJson))
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đặt chỗ hoặc thanh toán. Vui lòng thử lại.";
                return RedirectToPage("./Create");
            }

            BookingData = System.Text.Json.JsonSerializer.Deserialize<CreateBookingDto>(bookingDataJson);
            PaymentData = System.Text.Json.JsonSerializer.Deserialize<DepositFeeDto>(paymentDataJson);

            if (BookingData == null || PaymentData == null)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return RedirectToPage("./Create");
            }

            RentalDuration = BookingData.EndTime - BookingData.StartTime;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Retrieve data from TempData
            var bookingDataJson = TempData["BookingData"] as string;
            var paymentDataJson = TempData["PaymentData"] as string;

            if (string.IsNullOrEmpty(bookingDataJson) || string.IsNullOrEmpty(paymentDataJson))
            {
                TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng thử lại.";
                return RedirectToPage("./Create");
            }

            var bookingData = System.Text.Json.JsonSerializer.Deserialize<CreateBookingDto>(bookingDataJson);
            var paymentData = System.Text.Json.JsonSerializer.Deserialize<DepositFeeDto>(paymentDataJson);

            if (bookingData == null || paymentData == null)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return RedirectToPage("./Create");
            }

            try
            {
                IsProcessing = true;

                // Get RenterId from claims
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    TempData["ErrorMessage"] = "Không thể xác định thông tin người dùng.";
                    return RedirectToPage("./Create");
                }

                var renterId = Guid.Parse(User.FindFirstValue("RenterId"));
                // Create booking command
                var command = new CreateBookingCommand
                {
                    RenterId = renterId,
                    CreateBookingDto = bookingData,
                    DepositFeeDto = paymentData
                };

                // Send command via MediatR
                await mediator.Send(command);

                // Clear TempData
                TempData.Remove("BookingData");
                TempData.Remove("PaymentData");

                // Set success message
                TempData["StatusMessage"] = "Đặt chỗ của bạn đã được tạo thành công! Vui lòng đến trạm để xác minh danh tính.";

                // Redirect to profile
                return RedirectToPage("/RenterProfile", new { area = "Renter" });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi khi tạo đặt chỗ: {ex.Message}";
                
                // Restore data for retry
                BookingData = bookingData;
                PaymentData = paymentData;
                RentalDuration = bookingData.EndTime - bookingData.StartTime;
                
                TempData["BookingData"] = bookingDataJson;
                TempData["PaymentData"] = paymentDataJson;
                
                return Page();
            }
        }
    }
}
