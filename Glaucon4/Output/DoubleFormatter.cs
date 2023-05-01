#region FileHeader
// Project: Glaucon4
// Filename:   DoubleFormatter.cs
// Last write: 4/30/2023 2:29:50 PM
// Creation:   4/24/2023 12:01:10 PM
// Copyright: E.H. Terwiel, 2021,2022, 2023, the Netherlands
// No part of this file may be copied in any form without written consent
// of the programmer, owner and/or copyrightholder.
// This is a C# rewrite of FRAME3DD by H. Gavin cs.
// See https://frame3dd.sourceforge.net/
#endregion FileHeader


namespace Terwiel.Glaucon
{
    public partial class Glaucon
    {
        private class DoubleFormatter : IFormatProvider, ICustomFormatter
        {
            public DoubleFormatter()
            {
                if (Param.Decimals < 1 || Param.Decimals > 10)
                {
                    Lg("Input parameter 'Decimals' out of range (1,10) incl.");
                    Param.Decimals = 4;
                }
            }

            // Implementation of ICustomFormatter:
            public string Format(string format, object arg, IFormatProvider provider)
            {
                // Search for the custom "EE" format specifier:
                if (format == null || !format.StartsWith("H"))
                {
                    return null;
                }

                format = format.Substring(1); // Trim "EE"
                                              // Determine how many digits before we cutoff:
                int digits;
                if (!int.TryParse(format, out digits))
                {
                    //throw new FormatException("Format must contain digits");
                    digits = Param.Decimals;
                }

                // Get the value: (note, this will work for any numeric type)
                var value = Convert.ToDouble(arg);
                if (value == 0d)
                {
                    return "0";
                }

                string output;
                if (Math.Abs(value) >= Math.Pow(10, digits) || Math.Abs(value) < Math.Pow(10, -(digits - 1)))
                {
                    output = value.ToString("0." + new string('#', digits - 1) + "e-0", provider)
                        .Replace("e", "&times;10<sup>") + "</sup>";
                }
                else
                {
                    output = value.ToString($"G{digits}", provider);
                }

                //if (output.EndsWith("e+0"))
                //{
                //    output = output.Remove(output.Length - 3,3);
                //}
                return output;
                // Determine how many digits are showing: (this part isn't culture-compatible)
                //var length = output.Length - output.IndexOf(".");
                //if (length <= digits)
                //{
                //    return output;
                //}
                //else
                //{
                //    string f = "0."+ (new string('#', digits));
                //    return value.ToString(f + "e+0", provider);
                //}
            }

            // Implementation of IFormatProvider:
            public object GetFormat(Type t)
            {
                if (t == typeof(ICustomFormatter))
                {
                    return this;
                }

                return null;
            }
        }
    }
}
