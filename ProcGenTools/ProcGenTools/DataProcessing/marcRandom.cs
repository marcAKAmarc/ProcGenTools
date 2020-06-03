using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProcGenTools.DataProcessing
{
    public class marcRandom:System.Random
    {
        private int seed;
        private int originalSeed;
        private int iterations = 0;
        //
        // Summary:
        //     Initializes a new instance of the System.Random class, using a time-dependent
        //     default seed value.
        public marcRandom():base()
        {
            seed = (int)(DateTime.Now.Ticks % int.MaxValue);
        }
        //
        // Summary:
        //     Initializes a new instance of the System.Random class, using the specified seed
        //     value.
        //
        // Parameters:
        //   Seed:
        //     A number used to calculate a starting value for the pseudo-random number sequence.
        //     If a negative number is specified, the absolute value of the number is used.
        public marcRandom(int Seed):base(Seed)
        {
            seed = Seed;
            originalSeed = Seed;
        }

        //
        // Summary:
        //     Returns a non-negative random integer.
        //
        // Returns:
        //     A 32-bit signed integer that is greater than or equal to 0 and less than System.Int32.MaxValue.
        public override int Next()
        {
            seed ^= seed >> 13;
            seed ^= seed << 18;
            return seed & 0x7fffffff;
            /*seed = (int)(((1103515245 * (long)seed + 12345) % 2147483648) % int.MaxValue);
            iterations += 1;
            return seed;*/

        }
        //
        // Summary:
        //     Returns a random integer that is within a specified range.
        //
        // Parameters:
        //   minValue:
        //     The inclusive lower bound of the random number returned.
        //
        //   maxValue:
        //     The exclusive upper bound of the random number returned. maxValue must be greater
        //     than or equal to minValue.
        //
        // Returns:
        //     A 32-bit signed integer greater than or equal to minValue and less than maxValue;
        //     that is, the range of return values includes minValue but not maxValue. If minValue
        //     equals maxValue, minValue is returned.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     minValue is greater than maxValue.
        public override int Next(int minValue, int maxValue)
        {
            var result = Next();
            result = result % (maxValue - minValue);
            result += minValue;
            return result;
        }
        //
        // Summary:
        //     Returns a non-negative random integer that is less than the specified maximum.
        //
        // Parameters:
        //   maxValue:
        //     The exclusive upper bound of the random number to be generated. maxValue must
        //     be greater than or equal to 0.
        //
        // Returns:
        //     A 32-bit signed integer that is greater than or equal to 0, and less than maxValue;
        //     that is, the range of return values ordinarily includes 0 but not maxValue. However,
        //     if maxValue equals 0, maxValue is returned.
        //
        // Exceptions:
        //   T:System.ArgumentOutOfRangeException:
        //     maxValue is less than 0.
        public override int Next(int maxValue)
        {
            return Next() % maxValue;
        }
        //
        // Summary:
        //     Fills the elements of a specified array of bytes with random numbers.
        //
        // Parameters:
        //   buffer:
        //     An array of bytes to contain random numbers.
        //
        // Exceptions:
        //   T:System.ArgumentNullException:
        //     buffer is null.
        public override void NextBytes(byte[] buffer) {
            throw new NotImplementedException();
        }
        //
        // Summary:
        //     Returns a random floating-point number that is greater than or equal to 0.0,
        //     and less than 1.0.
        //
        // Returns:
        //     A double-precision floating point number that is greater than or equal to 0.0,
        //     and less than 1.0.
        public override double NextDouble()
        {
            throw new NotImplementedException();
        }
        //
        // Summary:
        //     Returns a random floating-point number between 0.0 and 1.0.
        //
        // Returns:
        //     A double-precision floating point number that is greater than or equal to 0.0,
        //     and less than 1.0.
        protected override double Sample() {
            throw new NotImplementedException();
        }
        
    }
}
