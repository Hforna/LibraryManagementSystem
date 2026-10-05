using System;
using System.Collections.Generic;
using System.Text;

namespace LibraryApp.Domain.Exceptions
{
    // Lançada quando o usuário está autenticado mas não tem permissão (papel) para acessar o recurso
    public class ForbiddenException : BaseException
    {
        public ForbiddenException(string error) : base(error)
        {

        }

        public ForbiddenException(List<string> errors) : base(errors)
        {

        }
    }
}
