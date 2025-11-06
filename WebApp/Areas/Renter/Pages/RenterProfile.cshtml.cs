using Application.DTOs;
using Application.DTOs.BookingManagement;
using Application.UseCases.BookingManagement.Queries.GetBookingByRenter;
using Application.UseCases.BookingManagement.Queries.ViewKycByRenter;
using Application.UseCases.Profile.Queries.GetRenterProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApp.Areas.Renter
{
    [Authorize]
    public class RenterProfileModel(IMediator mediator) : PageModel
    {
        public Guid RenterId { get; set; }
        public string? DriverLicenseNo { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public int RiskScore { get; set; }
        public PagedResult<KycDto>? Kycs { get; set; }
        public PagedResult<BookingDetailsDto>? Bookings { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var renterProfile = await mediator.Send(new GetRenterProfileQuery
            {
                UserId = Guid.Parse(userId!)
            });

            RenterId = renterProfile.RenterId;
            DriverLicenseNo = renterProfile.DriverLicenseNo;
            DateOfBirth = renterProfile.DateOfBirth;
            Address = renterProfile.Address;
            RiskScore = renterProfile.RiskScore;

            var kycs = await mediator.Send(new ViewKycByRenterQuery
            {
                RenterId = renterProfile.RenterId,
                PageNumber = 1,
                PageSize = 100
            });

            Kycs = kycs;

            var bookings = await mediator.Send(new GetBookingByRenterQuery
            {
                RenterId = renterProfile.RenterId,
                PageNumber = 1,
                PageSize = 10
            });

            Bookings = bookings;

            return Page();
        }
    }
}
