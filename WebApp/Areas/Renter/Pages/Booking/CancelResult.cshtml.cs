using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApp.Areas.Renter.Pages.Booking
{
    [Authorize(Roles = "Renter")]
    public class CancelResultModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid BookingId { get; set; }

        public decimal RefundAmount { get; set; }
        public string RefundCurrency { get; set; } = "VND";
        public decimal OriginalAmount { get; set; }

        public void OnGet()
        {
            // Parse refund info from TempData (stored as strings)
            if (TempData.TryGetValue("RefundAmount", out var refundAmountStr) && 
                decimal.TryParse(refundAmountStr?.ToString(), out var refundAmount))
            {
                RefundAmount = refundAmount;
            }

            if (TempData.TryGetValue("RefundCurrency", out var currency))
            {
                RefundCurrency = currency?.ToString() ?? "VND";
            }

            if (TempData.TryGetValue("OriginalAmount", out var originalAmountStr) && 
                decimal.TryParse(originalAmountStr?.ToString(), out var originalAmount))
            {
                OriginalAmount = originalAmount;
            }
        }
    }
}
