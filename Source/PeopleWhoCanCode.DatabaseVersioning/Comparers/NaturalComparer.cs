using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace PeopleWhoCanCode.DatabaseVersioning.Comparers
{
    public class NaturalComparer : IComparer<string>
    {
        private readonly CultureInfo _cultureInfo;

        public NaturalComparer(CultureInfo cultureInfo)
        {
            _cultureInfo = cultureInfo;
        }

        public int Compare(string x, string y)
        {
            // Simple cases.
            if (x == y) return 0;// Also handles null.
            if (x == null) return -1;
            if (y == null) return +1;

            var ix = 0;
            var iy = 0;

            while (ix < x.Length && iy < y.Length)
            {
                if (char.IsDigit(x[ix]) && char.IsDigit(y[iy]))
                {
                    // We found numbers, so grab both numbers.
                    int ix1 = ix++;
                    int iy1 = iy++;
                    while (ix < x.Length && Char.IsDigit(x[ix])) ix++;
                    while (iy < y.Length && Char.IsDigit(y[iy])) iy++;
                    string numberFromX = x.Substring(ix1, ix - ix1);
                    string numberFromY = y.Substring(iy1, iy - iy1);

                    // Pad them with 0's to have the same length
                    int maxLength = Math.Max(numberFromX.Length, numberFromY.Length);
                    numberFromX = numberFromX.PadLeft(maxLength, '0');
                    numberFromY = numberFromY.PadLeft(maxLength, '0');

                    int comparison = _cultureInfo.CompareInfo.Compare(numberFromX, numberFromY);
                    if (comparison != 0) return comparison;
                }
                else
                {
                    int comparison = _cultureInfo.CompareInfo.Compare(x, ix, 1, y, iy, 1);
                    if (comparison != 0) return comparison;
                    ix++;
                    iy++;
                }
            }

            // We still got parts of x left, y comes first.
            if (ix < x.Length) return +1;

            // We still got parts of y left, x comes first.
            return -1;
        }
    }
}
