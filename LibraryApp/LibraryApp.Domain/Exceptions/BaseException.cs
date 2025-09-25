using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LibraryApp.Domain.Exceptions
{
    public abstract class BaseException : Exception
    {
        public List<string> Errors { get; }

        public BaseException(string error) : base(error)
        {
            Errors = new List<string>() { error };
        }

        public BaseException(List<string> errors) : base(string.Empty)
        {
            Errors = errors;
        }

        public string GetMessages() => string.Join(Environment.NewLine, Errors);
    }
}
