using System;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing an ONVIF XML rectangle.
    /// </summary>
    public class Rectangle : IXmlSerializable, IEquatable<Rectangle>
    {
        private static readonly object Lock = new object();
        private static DateTime _lastMissingAttribute;
        private static DateTime _lastColorParseError;

        /// <summary>
        /// Gets or sets the y-coordinate of the bottom of the rectangle
        /// </summary>
        public float Bottom { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the top of the rectangle
        /// </summary>
        public float Top { get; set; }

        /// <summary>
        /// Gets or sets the x-coordinate of the right side of the rectangle
        /// </summary>
        public float Right { get; set; }

        /// <summary>
        /// Gets or sets the x-coordinate of the left side of the rectangle
        /// </summary>
        public float Left { get; set; }

        /// <summary>
        /// Gets or sets the color of the line used to draw the bounding box.
        /// </summary>
        public DisplayColor LineColor { get; set; }

        /// <summary>
        /// Gets or sets the thickness of the line used to draw the bounding box. This thickness is absolute, even if the image is resized.
        /// </summary>
        public uint? LineDisplayPixelThickness { get; set; }

        /// <summary>
        /// Gets or sets the color of the fill of the bounding box.
        /// </summary>
        public DisplayColor FillColor { get; set; }

        internal bool AllAttributesWerePresent { get; private set; }

        /// <summary>
        /// <see cref="IXmlSerializable.GetSchema"/>
        /// </summary>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// <see cref="IXmlSerializable.ReadXml"/>
        /// </summary>
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();

            BlankAllFields();

            AllAttributesWerePresent = true;
            Top = ReadRequiredFloatAttributeValue(reader, MetadataXml.TopAttribute);
            Bottom = ReadRequiredFloatAttributeValue(reader, MetadataXml.BottomAttribute);
            Left = ReadRequiredFloatAttributeValue(reader, MetadataXml.LeftAttribute);
            Right = ReadRequiredFloatAttributeValue(reader, MetadataXml.RightAttribute);
        }

        private void BlankAllFields()
        {
            Top = Bottom = Left = Right = 0;
            FillColor = null;
            LineColor = null;
            LineDisplayPixelThickness = null;
        }

        private float ReadRequiredFloatAttributeValue(XmlReader reader, string attributeName)
        {
            var xAttributeValue = reader.GetAttribute(attributeName);
            float floatValue;
            if (float.TryParse(xAttributeValue, MetadataXml.FloatStyle, MetadataXml.Culture, out floatValue) == false)
            {
                AllAttributesWerePresent = false;
                lock (Lock)
                {
                    if (DateTime.UtcNow - _lastMissingAttribute > MetadataXml.LogIgnoreTimeSpand)
                    {
                        var message = string.Format(CultureInfo.InvariantCulture, "Required attribute {0} is missing", attributeName);
                        EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", message, null);
                        _lastMissingAttribute = DateTime.UtcNow;
                    }
                }
            }
            return floatValue;
        }

        internal void ReadAppearanceExtensionXml(XmlReader reader)
        {
            var rootDepth = reader.Depth;
            do
            {
                if (ReferenceEquals(reader.NamespaceURI, string.Empty) == false)
                    continue;
                if (reader.Depth != rootDepth + 1) // Only look at immediate children
                    continue;
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                var localname = reader.LocalName;
                if (ReferenceEquals(MetadataXml.FillElement, localname))
                {
                    var colorValue = reader.GetAttribute(MetadataXml.ColorAttribute);
                    if (colorValue != null)
                    {
                        var color = ReadColor(colorValue);
                        FillColor = color;
                    }
                }
                else if (ReferenceEquals(MetadataXml.LineElement, localname))
                {
                    var colorValue = reader.GetAttribute(MetadataXml.ColorAttribute);
                    if (colorValue != null)
                    {
                        var color = ReadColor(colorValue);
                        LineColor = color;
                    }

                    var lineThicknessValue = reader.GetAttribute(MetadataXml.DisplayedThicknessInPixelsAttribute);
                    uint uintValue;
                    if (uint.TryParse(lineThicknessValue, MetadataXml.UintStyle, MetadataXml.Culture, out uintValue))
                    {
                        LineDisplayPixelThickness = uintValue;
                    }
                }
            } while (reader.Read());
        }

        private DisplayColor ReadColor(string colorValue)
        {
            DisplayColor color;
            if (DisplayColor.TryParseArgbString(colorValue, out color) == false)
            {
                lock (Lock)
                {
                    if (DateTime.UtcNow - _lastColorParseError > MetadataXml.LogIgnoreTimeSpand)
                    {
                        var message = string.Format(CultureInfo.InvariantCulture, "Color with value '{0}' could not be parsed", colorValue);
                        EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", message, null);
                        _lastColorParseError = DateTime.UtcNow;
                    }
                }

                return null;
            }

            return color;
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(MetadataXml.BottomAttribute, Bottom.ToString(MetadataXml.Culture));
            writer.WriteAttributeString(MetadataXml.TopAttribute, Top.ToString(MetadataXml.Culture));
            writer.WriteAttributeString(MetadataXml.RightAttribute, Right.ToString(MetadataXml.Culture));
            writer.WriteAttributeString(MetadataXml.LeftAttribute, Left.ToString(MetadataXml.Culture));
        }

        internal void WriteAppearanceExtenstionXml(XmlWriter writer)
        {
            if (FillColor != null)
            {
                writer.WriteStartElement(MetadataXml.FillElement);
                writer.WriteAttributeString(MetadataXml.ColorAttribute, FillColor.ArgbString);
                writer.WriteEndElement();
            }
            if (LineColor != null || LineDisplayPixelThickness.HasValue)
            {
                writer.WriteStartElement(MetadataXml.LineElement);
                if (LineColor != null)
                    writer.WriteAttributeString(MetadataXml.ColorAttribute, LineColor.ArgbString);
                if (LineDisplayPixelThickness.HasValue)
                    writer.WriteAttributeString(MetadataXml.DisplayedThicknessInPixelsAttribute, LineDisplayPixelThickness.Value.ToString(MetadataXml.Culture));
                writer.WriteEndElement();
            }
        }

        internal bool HasExtensionDataPresent()
        {
            return LineColor != null || FillColor != null || LineDisplayPixelThickness.HasValue;
        }

        /// <summary>
        /// Apply a transformation to this instance of a <see cref="Rectangle"/>. This will modify the current
        /// instance.
        /// </summary>
        /// <param name="transformation">The <see cref="Transformation"/> to apply to this rectangle</param>
        public void Apply(Transformation transformation)
        {
            if (transformation != null)
            {
                if (transformation.Scale != null)
                {
                    Left *= transformation.Scale.X;
                    Right *= transformation.Scale.X;
                    Top *= transformation.Scale.Y;
                    Bottom *= transformation.Scale.Y;
                }

                if (transformation.Translate != null)
                {
                    Left += transformation.Translate.X;
                    Right += transformation.Translate.X;
                    Top += transformation.Translate.Y;
                    Bottom += transformation.Translate.Y;
                }
            }
        }

        /// <summary>
        /// Provides a friendly print of the rectangle coordinates.
        /// </summary>
        public override string ToString()
        {
            return string.Format("Rectangle - Left: {0}, Top: {1}, Right: {2}, Bottom: {3}", Left, Top, Right, Bottom);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(Rectangle other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Bottom.Equals(other.Bottom) && Top.Equals(other.Top) && Right.Equals(other.Right) && Left.Equals(other.Left);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Rectangle) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Bottom.GetHashCode();
                hashCode = (hashCode*397) ^ Top.GetHashCode();
                hashCode = (hashCode*397) ^ Right.GetHashCode();
                hashCode = (hashCode*397) ^ Left.GetHashCode();
                return hashCode;
            }
        }
    }
}
