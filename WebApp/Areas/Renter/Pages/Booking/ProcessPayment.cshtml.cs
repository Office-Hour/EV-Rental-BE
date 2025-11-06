using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;

namespace WebApp.Areas.Renter.Pages.Booking
{
    [Authorize(Roles = "Renter")]
    public class ProcessPaymentModel : PageModel
    {
        private readonly PaymentSimulationService _paymentService;
        private readonly ILogger<ProcessPaymentModel> _logger;

        public ProcessPaymentModel(
            PaymentSimulationService paymentService,
            ILogger<ProcessPaymentModel> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid BookingId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal Amount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Description { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
        public string? PaymentUrl { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (BookingId == Guid.Empty || Amount <= 0)
            {
                TempData["ErrorMessage"] = "Thông tin thanh toán không h?p l?.";
                return RedirectToPage("/RenterProfile", new { area = "Renter" });
            }

            try
            {
                _logger.LogInformation("Creating simulated payment URL for booking {BookingId}", BookingId);

                // ? Create simulated payment URL
                var response = await _paymentService.CreatePaymentUrlAsync(
                    BookingId,
                    Amount,
                    Description
                );

                if (!response.Success || string.IsNullOrEmpty(response.PaymentUrl))
                {
                    throw new Exception("Không th? t?o liên k?t thanh toán.");
                }

                _logger.LogInformation("Simulated payment URL created: {PaymentUrl}", response.PaymentUrl);

                // ? Redirect to simulated payment gateway
                return Redirect(response.PaymentUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating simulated payment URL for booking {BookingId}", BookingId);
                
                ErrorMessage = $"Không th? k?t n?i ??n c?ng thanh toán: {ex.Message}";
                return Page();
            }
        }
    }
}
