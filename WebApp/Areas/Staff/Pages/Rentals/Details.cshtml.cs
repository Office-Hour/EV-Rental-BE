using Application.DTOs.RentalManagement;
using Application.UseCases.RentalManagement.Commands.ReceiveVehicle;
using Application.UseCases.RentalManagement.Queries.GetRentalDetails;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace WebApp.Areas.Staff.Pages.Rentals
{
    [Authorize(Roles = "Staff")]
    public class DetailsModel : PageModel
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IMediator mediator, ILogger<DetailsModel> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public RentalDetailsDto? Rental { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (Id == Guid.Empty)
            {
                TempData["ErrorMessage"] = "ID thuê xe không hợp lệ.";
                return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
            }

            try
            {
                var query = new GetRentalDetailsQuery { RentalId = Id };
                Rental = await _mediator.Send(query);

                if (Rental == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thuê xe.";
                    return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading rental {RentalId}", Id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin thuê xe.";
                return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
            }
        }

        public async Task<IActionResult> OnPostReceiveVehicleAsync()
        {
            try
            {
                var staffIdAsString = User.FindFirstValue("StaffId");
                if (string.IsNullOrEmpty(staffIdAsString) || !Guid.TryParse(staffIdAsString, out var staffId))
                {
                    TempData["ErrorMessage"] = "Không thể xác định thông tin nhân viên.";
                    return RedirectToPage(new { id = Id });
                }

                var command = new ReceiveVehicleCommand
                {
                    RentalId = Id,
                    ReceivedAt = DateTime.UtcNow,
                    ReceivedByStaffId = staffId
                };

                await _mediator.Send(command);

                _logger.LogInformation("Vehicle received for rental {RentalId} by staff {StaffId}", Id, staffId);

                TempData["SuccessMessage"] = "Đã nhận xe thành công! Rental đang trong quá trình thuê.";
                return RedirectToPage(new { id = Id });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation while receiving vehicle for rental {RentalId}", Id);
                TempData["ErrorMessage"] = $"Không thể nhận xe: {ex.Message}";
                return RedirectToPage(new { id = Id });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Rental {RentalId} not found", Id);
                TempData["ErrorMessage"] = "Không tìm thấy thuê xe.";
                return RedirectToPage("/Staff/Bookings/Index", new { area = "Staff" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error receiving vehicle for rental {RentalId}", Id);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi nhận xe.";
                return RedirectToPage(new { id = Id });
            }
        }
    }
}
