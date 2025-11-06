using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Commands.CancelChecking;
using Application.UseCases.BookingManagement.Queries.GetBookingDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace WebApp.Areas.Renter.Pages.Booking
{
    [Authorize(Roles = "Renter")]
    public class ConfirmCancelModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ConfirmCancelModel> _logger;

        public ConfirmCancelModel(IMediator mediator, ILogger<ConfirmCancelModel> logger)
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
            [Required(ErrorMessage = "Vui lòng nh?p mã xác nh?n")]
            [StringLength(6, MinimumLength = 6, ErrorMessage = "Mã xác nh?n ph?i có 6 ch? s?")]
            [RegularExpression(@"^\d{6}$", ErrorMessage = "Mã xác nh?n ph?i là 6 ch? s?")]
            [Display(Name = "Mã xác nh?n")]
            public string CancelCode { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nh?p lý do h?y")]
            [StringLength(500, ErrorMessage = "Lý do h?y không ???c quá 500 ký t?")]
            [Display(Name = "Lý do h?y")]
            public string CancelReason { get; set; } = string.Empty;

            // Optional: Bank account for refund
            [Display(Name = "Tên ngân hàng")]
            public string? BankName { get; set; }

            [Display(Name = "S? tài kho?n")]
            public string? AccountNumber { get; set; }

            [Display(Name = "Tên ch? tài kho?n")]
            public string? AccountHolderName { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (BookingId == Guid.Empty)
            {
                TempData["ErrorMessage"] = "ID ??t ch? không h?p l?.";
                return RedirectToPage("/RenterProfile", new { area = "Renter" });
            }

            try
            {
                // Get booking details to display info
                var query = new GetBookingDetailsQuery
                {
                    BookingId = BookingId
                };

                Booking = await _mediator.Send(query);

                if (Booking == null)
                {
                    TempData["ErrorMessage"] = "Không tìm th?y ??t ch?.";
                    return RedirectToPage("/RenterProfile", new { area = "Renter" });
                }

                // Verify booking can be cancelled
                if (Booking.Status != Domain.Enums.BookingStatus.Pending_Verification)
                {
                    TempData["ErrorMessage"] = "??t ch? này không th? h?y.";
                    return RedirectToPage("./Details", new { id = BookingId });
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading confirm cancel page for booking {BookingId}", BookingId);
                TempData["ErrorMessage"] = "?ã x?y ra l?i. Vui lòng th? l?i sau.";
                return RedirectToPage("./Details", new { id = BookingId });
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload booking details for display
                Booking = await _mediator.Send(new GetBookingDetailsQuery { BookingId = BookingId });
                return Page();
            }

            try
            {
                // Get UserId from token
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "Phiên ??ng nh?p h?t h?n.";
                    return RedirectToPage("/Account/Login", new { area = "Identity" });
                }

                _logger.LogInformation("User {UserId} confirming cancel for booking {BookingId} with code {Code}",
                    userId, BookingId, Input.CancelCode);

                // Build command
                var command = new CancelCheckinCommand
                {
                    BookingId = BookingId,
                    UserId = Guid.Parse(userId),
                    CancelReason = Input.CancelReason,
                    CancelCheckinCode = Input.CancelCode,
                    RenterBankAccount = null // Optional: create RenterBankAccountDto if bank info provided
                };

                // Add bank account if provided
                if (!string.IsNullOrEmpty(Input.BankName) &&
                    !string.IsNullOrEmpty(Input.AccountNumber) &&
                    !string.IsNullOrEmpty(Input.AccountHolderName))
                {
                    command.RenterBankAccount = new RenterBankAccountDto
                    {
                        BankName = Input.BankName,
                        AccountNumber = Input.AccountNumber,
                        AccountHolderName = Input.AccountHolderName
                    };
                }

                // Execute cancel command
                var refundInfo = await _mediator.Send(command);

                _logger.LogInformation("Booking {BookingId} cancelled successfully. Refund amount: {RefundAmount}",
                    BookingId, refundInfo.AmountPaid);

                // ? Store refund info in TempData as strings (TempData cannot serialize decimal)
                TempData["RefundAmount"] = refundInfo.AmountPaid.ToString();
                TempData["RefundCurrency"] = refundInfo.Currency.ToString();
                TempData["OriginalAmount"] = refundInfo.Amount.ToString();
                TempData["SuccessMessage"] = "??t ch? ?ã ???c h?y thành công!";

                // Redirect to cancel result page
                return RedirectToPage("./CancelResult", new { bookingId = BookingId });
            }
            catch (Application.CustomExceptions.InvalidTokenException ex)
            {
                _logger.LogWarning(ex, "Invalid cancel code for booking {BookingId}", BookingId);
                ModelState.AddModelError("Input.CancelCode", "Mã xác nh?n không ?úng ho?c ?ã h?t h?n.");
                Booking = await _mediator.Send(new GetBookingDetailsQuery { BookingId = BookingId });
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", BookingId);
                ModelState.AddModelError(string.Empty, $"Không th? h?y ??t ch?: {ex.Message}");
                Booking = await _mediator.Send(new GetBookingDetailsQuery { BookingId = BookingId });
                return Page();
            }
        }
    }
}
