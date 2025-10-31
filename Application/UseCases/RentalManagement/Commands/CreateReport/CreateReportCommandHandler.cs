using Application.Interfaces;
using Domain.Entities.RentalManagement;
using MediatR;

namespace Application.UseCases.RentalManagement.Commands.CreateReport;

public class CreateReportCommandHandler(IUnitOfWork uow) : IRequestHandler<CreateReportCommand, Guid>
{
    public async Task<Guid> Handle(CreateReportCommand request, CancellationToken cancellationToken)
    {
        var report = new Report
        {
            ReportId = Guid.NewGuid(),
            InspectionId = request.InspectionId,
            Notes = request.Notes,
            DamageFound = request.DamageFound
        };
        await uow.Repository<Report>().AddAsync(report, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);
        return report.ReportId;
    }
}
