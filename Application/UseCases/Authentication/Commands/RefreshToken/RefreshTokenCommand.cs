using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Authentication;
using MediatR;

namespace Application.UseCases.Authentication.Commands.RefreshToken
{
    /// <summary>
    /// Command to refresh the authentication token.
    /// </summary>
    public class RefreshTokenCommand : IRequest<AuthDto>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
