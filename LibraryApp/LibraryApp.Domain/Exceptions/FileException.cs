using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Exceptions
{
    public class FileException : BaseException
    {
        public FileException(string error) : base(error)
        {

        }

        public FileException(List<string> errors) : base(errors)
        {

        }
    }
}
