namespace Domain.Entities.BookingManagement;

public class Renter
{
    public Guid RenterId { get; set; }
    public string? DriverLicenseNo { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Address { get; set; }
    public int RiskScore { get; set; }

    public Guid UserId { get; set; } = Guid.Empty; // Foreign key to AspNetUsers table
    /// <summary>
    /// Show all bookings made by this renter.
    /// </summary>
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    /// <summary>
    /// Show all KYC documents submitted by this renter.
    /// </summary>
    public virtual ICollection<Kyc> Kycs { get; set; } = new List<Kyc>();
}
