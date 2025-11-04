using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MediatR;
using Application.UseCases.BookingManagement.Commands.UploadKyc;
using System.Security.Claims;
using Domain.Enums;

namespace WebApp.Areas.Renter.Pages
{
    [Authorize(Roles = "Renter")]
    public class UploadKycModel(IMediator mediator) : PageModel
    {
        [BindProperty]
        public KycType Type { get; set; } = KycType.National_ID;

        [BindProperty]
        public string DocumentNumber { get; set; } = string.Empty;

        [BindProperty]
        public DateTime? ExpiryDate { get; set; }

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    ErrorMessage = "Không tìm thấy thông tin người dùng.";
                    return Page();
                }

                var command = new UploadKycCommand
                {
                    UserId = Guid.Parse(userId),
                    Type = Type,
                    DocumentNumber = DocumentNumber,
                    ExpiryDate = ExpiryDate,
                    Status = KycStatus.Submitted
                };

                await mediator.Send(command);

                SuccessMessage = "Tài liệu KYC đã được upload thành công! Chúng tôi sẽ xác minh trong vòng 24-48 giờ.";
                return Redirect("/Renter/RenterProfile");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Có lỗi xảy ra: {ex.Message}";
                return Page();
            }
        }
    }
}
