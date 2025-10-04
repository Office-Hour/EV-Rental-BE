namespace Domain.Entities.StationManagement;

public class Pricing
{
    public Guid PricingId { get; set; }
    public Guid VehicleId { get; set; }
    public decimal PricePerHour { get; set; }
    public decimal? PricePerDay { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }


    public virtual Vehicle Vehicle { get; set; } = null!;
}