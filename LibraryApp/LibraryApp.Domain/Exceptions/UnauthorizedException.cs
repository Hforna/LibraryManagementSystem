using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Exceptions
{
    public class UnauthorizedException : BaseException
    {
        public UnauthorizedException(string error) : base(error)
        {

        }

        public UnauthorizedException(List<string> errors) : base(errors)
        {

        }
    }
}
