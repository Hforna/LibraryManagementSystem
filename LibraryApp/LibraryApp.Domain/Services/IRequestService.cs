using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Services
{
    public interface IRequestService
    {
        public string? GetBearerToken();
    }
}
