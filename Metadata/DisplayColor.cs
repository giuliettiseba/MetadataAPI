using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for containing information on the color used to draw metadata objects.
    /// </summary>
    public class DisplayColor : IEquatable<DisplayColor>
    {
        private const byte OpaqueAlpha = 255;

        private static readonly Regex ArgbStringRegex = new Regex("^#([0-9A-Fa-f]{8})$");
        
        private readonly byte _a;
        private readonly byte _r;
        private readonly byte _g;
        private readonly byte _b;
        
        /// <summary>
        /// Create a new instance of a <see cref="DisplayColor"/> with its R, G and B components set.
        /// The A (alpha) channel is set to fully opaque (i.e. 255).
        /// </summary>
        /// <param name="r">The red component value</param>
        /// <param name="g">The green component value</param>
        /// <param name="b">The blue component value</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "r")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "g")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public DisplayColor(byte r, byte g, byte b)
        {
            _a = OpaqueAlpha;
            _r = r;
            _g = g;
            _b = b;
        }

        /// <summary>
        /// Create a new instance of a <see cref="DisplayColor"/> with its A, R, G and B components set.
        /// </summary>
        /// <param name="a">The alpha component value</param>
        /// <param name="r">The red component value</param>
        /// <param name="g">The green component value</param>
        /// <param name="b">The blue component value</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "a")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "r")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "g")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "b")]
        public DisplayColor(byte a, byte r, byte g, byte b)
        {
            _a = a;
            _r = r;
            _g = g;
            _b = b;
        }

        /// <summary>
        /// Create a new instance of a <see cref="DisplayColor"/> from a 32-bit ARGB value.
        /// </summary>
        /// <param name="argb">A value specifying the 32-bit ARGB value.</param>
        public DisplayColor(int argb)
        {
            _a = (byte)(argb >> 24 & 0xFF);
            _r = (byte)(argb >> 16 & 0xFF);
            _g = (byte)(argb >> 8 & 0xFF);
            _b = (byte)(argb & 0xFF);
        }

        /// <summary>
        /// Attempts to parse and ARGB string in the format "#A545F3B5" into an instance of <see cref="DisplayColor"/>.
        /// </summary>
        /// <param name="argbValue">A string with a hexadecimal representation of a ARGB color.
        /// The string must start with '#' followed by 8 hex digits. Example: "#FFBB5D60"</param>
        /// <param name="color">The parsed color. This is null if the parse fails.</param>
        /// <returns>True if <paramref name="argbValue"/> could be converted to an instance of <see cref="DisplayColor"/>, false otherwise</returns>
        public static bool TryParseArgbString(string argbValue, out DisplayColor color)
        {
            if (argbValue == null) throw new ArgumentNullException("argbValue");

            var match = ArgbStringRegex.Match(argbValue.Trim());
            if (match.Success == false)
            {
                color = null;
                return false;
            }

            var noHashCharacter = match.Groups[1].Value;
            int argb;
            if (int.TryParse(noHashCharacter, NumberStyles.HexNumber, MetadataXml.Culture, out argb))
            {
                color = new DisplayColor(argb);
                return true;
            }

            color = null;
            return false;
        }

        /// <summary>
        /// Parses and ARGB string in the format "#A545F3B5" into an instance of <see cref="DisplayColor"/>.
        /// </summary>
        /// <param name="argbValue">A string with a hexadecimal representation of a ARGB color.
        /// The string must start with '\#' followed by 8 hex digits. Example: "\#FFBB5D60"</param>
        /// <returns>The parsed color</returns>
        /// <exception cref="FormatException">If the color format was incorrect</exception>
        public static DisplayColor ParseArgbString(string argbValue)
        {
            DisplayColor color;
            if (TryParseArgbString(argbValue, out color))
            {
                return color;
            }

            throw new FormatException(string.Format(CultureInfo.InvariantCulture, "ARGB string '{0}' is not in a correct format", argbValue));
        }

        /// <summary>
        /// Gets the alpha component value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "A", Justification = "A is a common shorthand for alpha and used by MS themselves")]
        public byte A
        {
            get { return _a; }
        }

        /// <summary>
        /// Gets the red component value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "R", Justification = "R is a common shorthand for red and used by MS themselves")]
        public byte R
        {
            get { return _r; }
        }

        /// <summary>
        /// Gets the green component value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "G", Justification = "G is a common shorthand for green and used by MS themselves")]
        public byte G
        {
            get { return _g; }
        }

        /// <summary>
        /// Gets the blue component value.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "B", Justification = "B is a common shorthand for blue and used by MS themselves")]
        public byte B
        {
            get { return _b; }
        }

        /// <summary>
        /// Gets the ARGB value of the color as a string in the format \#FF00FF00
        /// </summary>
        public string ArgbString {
            get
            {
                return "#" + 
                    A.ToString("X2", MetadataXml.Culture) +
                    R.ToString("X2", MetadataXml.Culture) +
                    G.ToString("X2", MetadataXml.Culture) + 
                    B.ToString("X2", MetadataXml.Culture);
            }
        } 

        /// <summary>
        /// Gets the ARGB value of the color as an integer
        /// </summary>
        public int Argb
        {
            get { return A << 24 | R << 16 | G << 8 | B; }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(DisplayColor other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _a == other._a && _r == other._r && _g == other._g && _b == other._b;
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DisplayColor) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _a.GetHashCode();
                hashCode = (hashCode * 397) ^ _r.GetHashCode();
                hashCode = (hashCode * 397) ^ _g.GetHashCode();
                hashCode = (hashCode * 397) ^ _b.GetHashCode();
                return hashCode;
            }
        }
    }
}