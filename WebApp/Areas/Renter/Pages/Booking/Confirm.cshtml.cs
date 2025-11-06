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
    public class ConfirmModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConfirmModel> _logger;

        public ConfirmModel(IMediator mediator, ILogger<ConfirmModel> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

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
                var renterIdClaim = User.FindFirstValue("RenterId");
                if (string.IsNullOrEmpty(renterIdClaim))
                {
                    TempData["ErrorMessage"] = "Không thể xác định thông tin người dùng.";
                    return RedirectToPage("./Create");
                }

                var renterId = Guid.Parse(renterIdClaim);

                // Set payment to Pending (NOT Paid yet)
                paymentData.AmountPaid = 0; // Chưa thanh toán
                paymentData.CreatedAt = DateTime.UtcNow;
                paymentData.ProviderReference = null; // Chưa có reference

                // Create booking via MediatR (direct call - no API)
                var command = new CreateBookingCommand
                {
                    RenterId = renterId,
                    CreateBookingDto = bookingData,
                    DepositFeeDto = paymentData
                };

                // Send command and get BookingId
                var bookingId = await _mediator.Send(command);

                _logger.LogInformation("Booking created successfully: {BookingId}", bookingId);

                // Clear TempData
                TempData.Remove("BookingData");
                TempData.Remove("PaymentData");

                // Redirect to payment simulation page
                TempData["StatusMessage"] = "Đặt chỗ đã được tạo. Vui lòng hoàn tất thanh toán.";
                
                return RedirectToPage("./ProcessPayment", new 
                { 
                    bookingId = bookingId,
                    amount = paymentData.Amount,
                    description = paymentData.Description
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                
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
