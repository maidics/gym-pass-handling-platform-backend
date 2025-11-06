using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitPass.Application.Common.Exceptions;
public class ConflictException : Exception
{
    public ConflictException(string propertyName) : base($"{propertyName} is already in use.")
    {
        
    }
}
