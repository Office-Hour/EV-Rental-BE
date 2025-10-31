using MediatR;

namespace Application.UseCases.RentalManagement.Commands.CreateReport;

public class CreateReportCommand : IRequest<Guid>
{
    public Guid InspectionId { get; set; }
    public string? Notes { get; set; }
    public bool DamageFound { get; set; } = false;
}
