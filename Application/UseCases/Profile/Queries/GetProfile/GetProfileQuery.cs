using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using MediatR;

namespace Application.UseCases.Profile.Queries.GetProfile
{
    public class GetProfileQuery : IRequest<UserInfoDto>
    {
        public string UserId { get; set; } = string.Empty;
    }
}
