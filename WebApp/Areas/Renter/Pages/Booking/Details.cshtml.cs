using Application.DTOs.BookingManagement;
using Application.DTOs.Profile;
using Application.UseCases.BookingManagement.Commands.RequestCancelCheckin;
using Application.UseCases.BookingManagement.Queries.GetBookingDetails;
using Application.UseCases.Profile.Queries.GetRenterProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WebApp.Areas.Renter.Pages.Booking
{
    [Authorize(Roles = "Renter")]
    public class DetailsModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IMediator mediator, ILogger<DetailsModel> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public BookingDetailsDto? Booking { get; set; }
        public string? ErrorMessage { get; set; }
        public bool CanCancel { get; set; }
        public bool CanRequestCancel { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            try
            {
                // ✅ 1. Get UserId from token
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID not found in claims");
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                // ✅ 2. Get RenterProfile to get RenterId
                var renterProfile = await _mediator.Send(new GetRenterProfileQuery
                {
                    UserId = Guid.Parse(userId)
                });

                // ✅ 3. Get Booking Details
                var query = new GetBookingDetailsQuery
                {
                    BookingId = id
                };

                Booking = await _mediator.Send(query);

                if (Booking == null)
                {
                    ErrorMessage = "Không tìm thấy đặt chỗ.";
                    return Page();
                }

                // ✅ 4. Authorization check - ensure renter owns this booking
                if (Booking.RenterId != renterProfile.RenterId)
                {
                    _logger.LogWarning("User {UserId} attempted to access booking {BookingId} owned by {RenterId}",
                        userId, id, Booking.RenterId);
                    
                    TempData["ErrorMessage"] = "Bạn không có quyền truy cập đặt chỗ này.";
                    return RedirectToPage("/RenterProfile", new { area = "Renter" });
                }

                // ✅ 5. Determine available actions
                CanRequestCancel = Booking.Status == Domain.Enums.BookingStatus.Pending_Verification;
                CanCancel = Booking.Status == Domain.Enums.BookingStatus.Pending_Verification;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching booking details for ID: {BookingId}", id);
                ErrorMessage = "Không thể tải thông tin đặt chỗ. Vui lòng thử lại sau.";
                return Page();
            }
        }

        // ✅ NEW: Request Cancel Handler
        public async Task<IActionResult> OnPostRequestCancelAsync(Guid id)
        {
            try
            {
                // 1. Get UserId from token
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.";
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                _logger.LogInformation("User {UserId} requesting cancel for booking {BookingId}", userId, id);

                // 2. Send RequestCancelCheckinCommand
                var command = new RequestCancelCheckinCommand
                {
                    BookingId = id,
                    UserId = Guid.Parse(userId)
                };

                await _mediator.Send(command);

                _logger.LogInformation("Cancel code sent for booking {BookingId}", id);

                // 3. Redirect to ConfirmCancel page
                TempData["SuccessMessage"] = "Mã xác nhận hủy đặt chỗ đã được gửi. Vui lòng kiểm tra email hoặc SMS.";
                return RedirectToPage("./ConfirmCancel", new { bookingId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting cancel for booking {BookingId}", id);
                TempData["ErrorMessage"] = $"Không thể gửi mã xác nhận: {ex.Message}";
                return RedirectToPage("./Details", new { id = id });
            }
        }
    }
}
