using Application.DTOs.BookingManagement;

namespace Application.DTOs.Profile
{
    public class RenterProfileDto
    {
        public Guid RenterId { get; set; }
        public string UserName { get; set; } = "";
        public string? DriverLicenseNo { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public int RiskScore { get; set; }
        public List<KycDto> Kycs { get; set; } = new();
    }
}