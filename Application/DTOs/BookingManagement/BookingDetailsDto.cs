using Domain.Enums;

namespace Application.DTOs.BookingManagement;

public class BookingDetailsDto
{
    public Guid BookingId { get; set; }
    public Guid RenterId { get; set; }
    public Guid VehicleAtStationId { get; set; }
    public DateTime BookingCreatedAt { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingStatus Status { get; set; }
    public BookingVerificationStatus VerificationStatus { get; set; }
    public Guid? VerifiedByStaffId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public string? CancelReason { get; set; }
}