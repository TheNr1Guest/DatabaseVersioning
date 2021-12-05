using System;
using System.Collections.Generic;
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

        public int Compare(string left, string right)
        {
            // Simple cases.
            if (left == right) return 0; // Also handles null.
            if (left == null) return -1;
            if (right == null) return +1;

            var leftIndex = 0;
            var rightIndex = 0;

            while (leftIndex < left.Length && rightIndex < right.Length)
            {
                if (char.IsDigit(left[leftIndex]) && char.IsDigit(right[rightIndex]))
                {
                    // We found numbers, so grab both numbers.
                    var newLeftIndex = leftIndex++;
                    var newRightIndex = rightIndex++;
                    while (leftIndex < left.Length && Char.IsDigit(left[leftIndex])) leftIndex++;
                    while (rightIndex < right.Length && Char.IsDigit(right[rightIndex])) rightIndex++;
                    var numberFromLeft = left.Substring(newLeftIndex, leftIndex - newLeftIndex);
                    var numberFromRight = right.Substring(newRightIndex, rightIndex - newRightIndex);

                    // Pad them with 0's to have the same length.
                    var maxLength = Math.Max(numberFromLeft.Length, numberFromRight.Length);
                    numberFromLeft = numberFromLeft.PadLeft(maxLength, '0');
                    numberFromRight = numberFromRight.PadLeft(maxLength, '0');

                    var comparison = _cultureInfo.CompareInfo.Compare(numberFromLeft, numberFromRight);

                    if (comparison != 0) return comparison;
                }
                else
                {
                    var comparison = _cultureInfo.CompareInfo.Compare(left, leftIndex, 1, right, rightIndex, 1);

                    if (comparison != 0) return comparison;

                    leftIndex++;
                    rightIndex++;
                }
            }

            // We still got parts of 'left' left, 'right' comes first.
            if (leftIndex < left.Length) return +1;

            // We still got parts of 'right' left, 'left' comes first.
            return -1;
        }
    }
}
