using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFCootreguaV2.Domain.Exceptions
{
    public class ShowableException: Exception
    {
        public ShowableException() : base() { }

        public ShowableException(string message) : base(message) { }

        public ShowableException(string message, Exception innerException) : base(message, innerException) { }

    }
}
