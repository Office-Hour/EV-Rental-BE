using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CustomExceptions
{
    public class InvalidTokenException(string? message = "Refresh token is invalid or expired") : Exception(message)
    {
    }
}
