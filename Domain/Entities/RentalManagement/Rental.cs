using Domain.Entities.BookingManagement;
using Domain.Entities.StationManagement;
using Domain.Enums;
namespace Domain.Entities.RentalManagement;

public class Rental
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


    public virtual Booking Booking { get; set; } = null!;
    public virtual Vehicle Vehicle { get; set; } = null!;


    public virtual ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
}