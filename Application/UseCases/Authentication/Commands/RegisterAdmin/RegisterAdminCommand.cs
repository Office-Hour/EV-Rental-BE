using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Authentication.Commands.RegisterAdmin
{
    public class RegisterAdminCommand : IRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? UserName { get; set; }
        public IdentityUserRole<Guid> IdentityUserRole { get; set; } = null!;
    }
}
