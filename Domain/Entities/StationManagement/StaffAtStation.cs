using Domain.Entities.BookingManagement;

namespace Domain.Entities.StationManagement;

public class StaffAtStation
{
    public Guid StaffId { get; set; }
    public DateTime StartTime { get; set; }
    public Guid StationId { get; set; }
    /// <summary>
    /// If null, the staff is currently working at this station
    /// </summary>
    public DateTime? EndTime { get; set; }
    public string? RoleAtStation { get; set; }


    public virtual Staff Staff { get; set; } = null!;
    public virtual Station Station { get; set; } = null!;
}