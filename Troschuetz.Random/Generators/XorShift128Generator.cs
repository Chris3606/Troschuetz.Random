/*
 * Copyright � 2006 Stefan Trosch�tz (stefan@troschuetz.de)
 * Copyright � 2012-2014 Alessio Parma (alessio.parma@gmail.com)
 *
 * This file is part of Troschuetz.Random Class Library.
 *
 * Troschuetz.Random is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */

#region Original Summary

/*
/// <summary>
///   A fast random number generator for .NET Colin Green, January 2005
/// 
///   September 4th 2005 Added NextBytesUnsafe() - commented out by default. Fixed bug in
///   Reinitialise() - y,z and w variables were not being reset.
/// 
///   Key points:
///   1) Based on a simple and fast xor-shift pseudo random number generator (RNG) specified in:
///      Marsaglia, George. (2003). Xorshift RNGs. http://www.jstatsoft.org/v08/i14/xorshift.pdf
/// 
///   This particular implementation of xorshift has a period of 2^128-1. See the above paper to see
///   how this can be easily extened if you need a longer period. At the time of writing I could
///   find no information on the period of System.Random for comparison.
/// 
///   2) Faster than System.Random. Up to 15x faster, depending on which methods are called.
/// 
///   3) Direct replacement for System.Random. This class implements all of the methods that
///      System.Random does plus some additional methods. The like named methods are functionally equivalent.
/// 
///   4) Allows fast re-initialisation with a seed, unlike System.Random which accepts a seed at
///      construction time which then executes a relatively expensive initialisation routine. This
///      provides a vast speed improvement if you need to reset the pseudo-random number sequence
///      many times, e.g. if you want to re-generate the same sequence many times. An alternative
///      might be to cache random numbers in an array, but that approach is limited by memory
///      capacity and the fact that you may also want a large number of different sequences cached.
///      Each sequence can each be represented by a single seed value (int) when using FastRandom.
/// 
///   Notes. A further performance improvement can be obtained by declaring local variables as
///   static, thus avoiding re-allocation of variables on each call. However care should be taken if
///   multiple instances of FastRandom are in use or if being used in a multi-threaded environment.
/// </summary>
 */

#endregion Original Summary

namespace Troschuetz.Random.Generators
{
    using System;
    using System.Runtime.CompilerServices;

    /// <summary>
    ///   Represents a xorshift pseudo-random number generator with period 2^128-1.
    /// </summary>
    /// <remarks>
    ///   The <see cref="XorShift128Generator"/> type bases upon the implementation presented in the
    ///   CP article "<a href="http://www.codeproject.com/csharp/fastrandom.asp">A fast equivalent
    ///   for System.Random</a>" and the theoretical background on xorshift random number generators
    ///   published by George Marsaglia in this paper "<a href="http://www.jstatsoft.org/v08/i14/xorshift.pdf">Xorshift RNGs</a>".
    /// </remarks>
    [Serializable]
    public sealed class XorShift128Generator : AbstractGenerator<XorShift128Generator>
    {
        #region Constants

        /// <summary>
        ///   Represents the seed for the <see cref="_y"/> variable. This field is constant.
        /// </summary>
        /// <remarks>The value of this constant is 362436069.</remarks>
        public const uint SeedY = 362436069U;

        /// <summary>
        ///   Represents the seed for the <see cref="_z"/> variable. This field is constant.
        /// </summary>
        /// <remarks>The value of this constant is 521288629.</remarks>
        public const uint SeedZ = 521288629U;

        /// <summary>
        ///   Represents the seed for the <see cref="_w"/> variable. This field is constant.
        /// </summary>
        /// <remarks>The value of this constant is 88675123.</remarks>
        public const uint SeedW = 88675123U;

        #endregion Constants

        #region Fields

        /// <summary>
        ///   Stores the last generated unsigned random number.
        /// </summary>
        uint _w;

        /// <summary>
        ///   Stores the last but three unsigned random number.
        /// </summary>
        uint _x;

        /// <summary>
        ///   Stores the last but two unsigned random number.
        /// </summary>
        uint _y;

        /// <summary>
        ///   Stores the last but one unsigned random number.
        /// </summary>
        uint _z;

        #endregion Fields

        #region Construction

        /// <summary>
        ///   Initializes a new instance of the <see cref="XorShift128Generator"/> class, using a
        ///   time-dependent default seed value.
        /// </summary>
        public XorShift128Generator() : base((uint) Math.Abs(Environment.TickCount))
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="XorShift128Generator"/> class, using the
        ///   specified seed value.
        /// </summary>
        /// <param name="seed">
        ///   A number used to calculate a starting value for the pseudo-random number sequence. If
        ///   a negative number is specified, the absolute value of the number is used.
        /// </param>
        public XorShift128Generator(int seed) : base((uint) Math.Abs(seed))
        {
        }

        /// <summary>
        ///   Initializes a new instance of the <see cref="XorShift128Generator"/> class, using the
        ///   specified seed value.
        /// </summary>
        /// <param name="seed">
        ///   An unsigned number used to calculate a starting value for the pseudo-random number sequence.
        /// </param>
        public XorShift128Generator(uint seed) : base(seed)
        {
        }

        #endregion Construction

        #region IGenerator members

        /// <summary>
        ///   Gets a value indicating whether the random number generator can be reset,
        ///   so that it produces the same random number sequence again.
        /// </summary>
        public override bool CanReset => true;

        /// <summary>
        ///   Resets the random number generator using the specified seed, so that it produces the same random number sequence again.
        ///   To understand whether this generator can be reset, you can query the <see cref="CanReset"/> property.
        /// </summary>
        /// <param name="seed">The seed value used by the generator.</param>
        /// <returns>
        ///   True if the random number generator was reset; otherwise, false.
        /// </returns>
        public override bool Reset(uint seed)
        {
            // "The seed set for xor128 is four 32-bit integers x,y,z,w not all 0, ..." (George
            // Marsaglia) To meet that requirement the y, z, w seeds are constant values greater 0.
            _x = seed;
            _y = SeedY;
            _z = SeedZ;
            _w = SeedW;
            return true;
        }

        /// <summary>
        ///   Returns an unsigned random number.
        /// </summary>
        /// <returns>
        ///   A 32-bit unsigned integer greater than or equal to <see cref="uint.MinValue"/> and
        ///   less than or equal to <see cref="uint.MaxValue"/>.
        /// </returns>
#if PORTABLE

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
        public override uint NextUInt()
        {
            var t = (_x ^ (_x << 11));
            _x = _y;
            _y = _z;
            _z = _w;
            return (_w = (_w ^ (_w >> 19)) ^ (t ^ (t >> 8)));
        }

        #endregion IGenerator members
    }
}
