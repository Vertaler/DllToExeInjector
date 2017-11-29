using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DllToExeInjector
{
    static class IntegerExtensions
    {
        public static uint AlignDown(this uint number, uint alignment)
        {
            return (uint)(number & ~(alignment - 1));
        }

        public static uint AlignUp(this uint number, uint alignment)
        {
            var test = number.AlignDown(alignment) + alignment;
            return (uint)(( (number & ~(alignment - 1))!= 0) ? number.AlignDown(alignment) + alignment : number) ;
        }
    }
}
