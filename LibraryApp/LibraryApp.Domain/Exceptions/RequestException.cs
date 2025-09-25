using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Exceptions
{
    public class RequestException : BaseException
    {
        public override List<string> Errors { get; set; } = [];

        public RequestException(string error) : base(error)
        {
            
        }

        public RequestException(List<string> errors) : base(errors)
        {
            
        }
    }
}
