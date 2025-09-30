using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CustomExceptions
{
    public class InvalidTokenException() : Exception("Refresh token is invalid or expired")
    {
    }
}
