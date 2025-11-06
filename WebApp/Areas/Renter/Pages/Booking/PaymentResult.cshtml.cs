using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Areas.Renter.Pages.Booking
{
    public class PaymentResultModel : PageModel
    {
        private readonly ILogger<PaymentResultModel> _logger;

        public PaymentResultModel(ILogger<PaymentResultModel> logger)
        {
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid BookingId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TransactionId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public decimal Amount { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Success { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Message { get; set; } = string.Empty;

        public void OnGet()
        {
            if (Success)
            {
                _logger.LogInformation("Payment successful for booking {BookingId}, transaction: {TransactionId}",
                    BookingId, TransactionId);
            }
            else
            {
                _logger.LogWarning("Payment failed for booking {BookingId}, message: {Message}",
                    BookingId, Message);
            }
        }
    }
}
