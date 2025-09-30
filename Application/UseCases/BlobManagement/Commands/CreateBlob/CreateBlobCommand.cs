using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.BlobManagement.Commands.CreateBlob;

public class CreateBlobCommand : IRequest<Guid>
{
    public string FileName { get; set; } = string.Empty;
    public string ContainerName { get; set; } = string.Empty;
}
