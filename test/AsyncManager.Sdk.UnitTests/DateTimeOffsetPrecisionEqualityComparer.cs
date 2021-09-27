using System;
using System.Collections.Generic;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace AsyncManager.Sdk.UnitTests
{
    public class DateTimeOffsetPrecisionEqualityComparer : IEqualityComparer<DateTimeOffset>
    {
        private readonly TimeSpan _precision;
        public DateTimeOffsetPrecisionEqualityComparer(TimeSpan precision)
        {
            _precision = precision;
        }

        public bool Equals(DateTimeOffset x, DateTimeOffset y)
        {
            var difference = x - y;
            var negated = difference.Negate();

            if (difference < TimeSpan.Zero)
            {
                difference = difference.Negate();
            }

            if (difference < _precision)
            {
                return true;
            }

            return false;
        }

        public int GetHashCode(DateTimeOffset obj)
        {
            throw new FulcrumNotImplementedException();
        }
    }
}