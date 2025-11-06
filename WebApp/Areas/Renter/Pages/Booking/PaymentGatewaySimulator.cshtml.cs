using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp.Services;

namespace WebApp.Areas.Renter.Pages.Booking
{
    public class PaymentGatewaySimulatorModel : PageModel
    {
        private readonly PaymentSimulationService _paymentService;
        private readonly ILogger<PaymentGatewaySimulatorModel> _logger;

        public PaymentGatewaySimulatorModel(
            PaymentSimulationService paymentService,
            ILogger<PaymentGatewaySimulatorModel> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid BookingId { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal Amount { get; set; }

        [BindProperty(SupportsGet = true)]
        public string TxnRef { get; set; } = string.Empty;

        [BindProperty]
        public PaymentSimulationResult SimulationResult { get; set; } = PaymentSimulationResult.Success;

        public void OnGet()
        {
            // Display simulated payment gateway
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                _logger.LogInformation("Processing simulated payment for booking {BookingId} with result {Result}",
                    BookingId, SimulationResult);

                // Simulate processing delay (like real gateway)
                await Task.Delay(2000);

                // Process payment and update database
                var result = await _paymentService.ProcessPaymentAsync(
                    BookingId,
                    TxnRef,
                    SimulationResult
                );

                // Redirect to payment result page
                return RedirectToPage("./PaymentResult",
                    new
                    {
                        bookingId = result.BookingId,
                        transactionId = result.TransactionId,
                        amount = result.Amount,
                        success = result.Success,
                        message = result.Message
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing simulated payment for booking {BookingId}", BookingId);
                
                return RedirectToPage("./PaymentResult",
                    new
                    {
                        bookingId = BookingId,
                        transactionId = TxnRef,
                        amount = Amount,
                        success = false,
                        message = $"Payment processing error: {ex.Message}"
                    });
            }
        }
    }
}
