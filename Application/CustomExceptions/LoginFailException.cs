using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.CustomExceptions
{
    public class LoginFailException() : Exception("Invalid email or password")
    {
    }
}
