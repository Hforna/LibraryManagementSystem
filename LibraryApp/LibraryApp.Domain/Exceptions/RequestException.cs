using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Exceptions
{
    public class RequestException : BaseException
    {
        public RequestException(string error) : base(error)
        {
            
        }

        public RequestException(List<string> errors) : base(errors)
        {
            
        }
    }

    public class UnexpectedErrorException : BaseException
    {
        public UnexpectedErrorException(string error) : base(error)
        {

        }

        public UnexpectedErrorException(List<string> errors) : base(errors)
        {

        }
    }
}
