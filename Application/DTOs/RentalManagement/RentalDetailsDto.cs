using Application.DTOs.BookingManagement;
using Domain.Enums;

namespace Application.DTOs.RentalManagement
{
    public class RentalDetailsDto
    {
        public Guid RentalId { get; set; }
        public Guid BookingId { get; set; }
        public Guid VehicleId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public RentalStatus Status { get; set; } = RentalStatus.Reserved;

        public int Score { get; set; }
        public string? Comment { get; set; }
        public DateTime RatedAt { get; set; }

        public BookingBriefDto Booking { get; set; } = null!;
        public VehicleDto Vehicle { get; set; } = null!;
        public List<ContractDto> Contracts { get; set; } = new();
    }
}