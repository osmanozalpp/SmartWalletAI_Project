using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Domain.Exceptions
{
    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
    }
    public class BusinessException : Exception
    {
        public BusinessException(string message) : base(message) { }
    }
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
}
