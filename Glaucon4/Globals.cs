using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terwiel.Glaucon
{
    public static class Globals
    {
        public static bool IncludeGeometricStability;
        public static bool Shear;
        public static bool AlmostEqual(double a, double b)
        {
            return Math.Abs(a - b) < 1e-12;
        }
    }
}
