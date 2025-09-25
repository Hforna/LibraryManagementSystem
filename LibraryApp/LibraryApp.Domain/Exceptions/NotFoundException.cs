using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Exceptions
{
    public class NotFoundException : BaseException
    {
        public NotFoundException(string error) : base(error)
        {

        }

        public NotFoundException(List<string> errors) : base(errors)
        {

        }
    }
}
