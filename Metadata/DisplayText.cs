using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing text than can be embedded as part of metadata.
    /// This text can then be presented to the users in some form.
    /// </summary>
    public class DisplayText : IXmlSerializable
    {
        private static readonly object Lock = new object();
        private static DateTime _lastColorParseError;

        /// <summary>
        /// Gets or sets the X coordinate of the text. The coordinate is the horizontal center of the text in the ONVIF coordinate system.
        /// </summary>
        public float? CenterX { get; set; }

        /// <summary>
        /// Gets or sets the Y coordinate of the text. The coordinate is the vertical center of the text in the ONVIF coordinate system.
        /// </summary>
        public float? CenterY { get; set; }

        /// <summary>
        /// Gets or sets whether the text should be drawn with a bold face.
        /// </summary>
        public bool IsBold { get; set; }

        /// <summary>
        /// Gets or sets whether the text should be drawn with an italic face.
        /// </summary>
        public bool IsItalic { get; set; }

        /// <summary>
        /// Gets or sets a string describing the font family. There is no guarantee that the font family is available on the target system.
        /// </summary>
        public string FontFamily { get; set; }

        /// <summary>
        /// Gets or sets the relative vertical size of the text. It must be a number between 0 and 2 and is measured in the ONVIF coordinate system.
        /// </summary>
        public float? Size { get; set; }

        /// <summary>
        /// Gets or sets the color of the text.
        /// </summary>
        public DisplayColor Color { get; set; }

        /// <summary>
        /// Gets or sets the actual text to display.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// <see cref="IXmlSerializable.GetSchema"/>
        /// </summary>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// <see cref="IXmlSerializable.ReadXml"/>
        /// </summary>
        public void ReadXml(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            reader.MoveToContent();
            var rootDepth = reader.Depth;
            var isEmptyElement = reader.IsEmptyElement;

            BlankAllFields();
            ReadAttributes(reader);

            reader.ReadStartElement();

            if (isEmptyElement == false)
            {
                ReadChildren(reader, rootDepth);
                reader.ReadEndElement();
            }
        }

        private void ReadChildren(XmlReader reader, int rootDepth)
        {
            do
            {
                if (reader.Depth != rootDepth + 1) // Only look at immediate children
                    continue;
                if (reader.NodeType == XmlNodeType.Text)
                    Value = reader.Value;
            } while (reader.Depth != rootDepth && reader.Read());
        }

        private void ReadAttributes(XmlReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                var attributeName = reader.LocalName;
                if (ReferenceEquals(MetadataXml.ColorAttribute, attributeName))
                {
                    var argbString = reader.ReadContentAsString();
                    var color = ParseColor(argbString);
                    Color = color;
                }
                if (ReferenceEquals(MetadataXml.FontFamilyAttribute, attributeName))
                {
                    FontFamily = reader.ReadContentAsString();
                }
                else if (ReferenceEquals(MetadataXml.VectorXAttribute, attributeName))
                {
                    var attributeValue = reader.ReadContentAsString();
                    float floatValue;
                    if (float.TryParse(attributeValue, MetadataXml.FloatStyle, MetadataXml.Culture, out floatValue))
                    {
                        CenterX = floatValue;
                    }
                }
                else if (ReferenceEquals(MetadataXml.VectorYAttribute, attributeName))
                {
                    var attributeValue = reader.ReadContentAsString();
                    float floatValue;
                    if (float.TryParse(attributeValue, MetadataXml.FloatStyle, MetadataXml.Culture, out floatValue))
                    {
                        CenterY = floatValue;
                    }
                }
                else if (ReferenceEquals(MetadataXml.SizeAttribute, attributeName))
                {
                    var attributeValue = reader.ReadContentAsString();
                    float floatValue;
                    if (float.TryParse(attributeValue, MetadataXml.FloatStyle, MetadataXml.Culture, out floatValue))
                    {
                        Size = floatValue;
                    }
                }
                else if (ReferenceEquals(MetadataXml.BoldAttribute, attributeName))
                {
                    var attributeValue = reader.ReadContentAsString();
                    bool boolValue;
                    if (bool.TryParse(attributeValue, out boolValue))
                    {
                        IsBold = boolValue;
                    }
                }
                else if (ReferenceEquals(MetadataXml.ItalicAttribute, attributeName))
                {
                    var attributeValue = reader.ReadContentAsString();
                    bool boolValue;
                    if (bool.TryParse(attributeValue, out boolValue))
                    {
                        IsItalic = boolValue;
                    }
                }
            }
        }

        private DisplayColor ParseColor(string argbString)
        {
            DisplayColor color;
            if (DisplayColor.TryParseArgbString(argbString, out color) == false)
            {
                lock (Lock)
                {
                    if (DateTime.UtcNow - _lastColorParseError > MetadataXml.LogIgnoreTimeSpand)
                    {
                        var message = string.Format(CultureInfo.InvariantCulture, "Color with value '{0}' could not be parsed", argbString);
                        EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", message, null);
                        _lastColorParseError = DateTime.UtcNow;
                    }
                }
            }
            return color;
        }

        private void BlankAllFields()
        {
            CenterX = null;
            CenterY = null;
            IsBold = false;
            IsItalic = false;
            FontFamily = null;
            Size = null;
            Color = null;
            Value = null;
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public void WriteXml(XmlWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");

            if (CenterX.HasValue)
                writer.WriteAttributeString(MetadataXml.VectorXAttribute, CenterX.Value.ToString(MetadataXml.Culture));
            if (CenterY.HasValue)
                writer.WriteAttributeString(MetadataXml.VectorYAttribute, CenterY.Value.ToString(MetadataXml.Culture));
            if (Size.HasValue)
                writer.WriteAttributeString(MetadataXml.SizeAttribute, Size.Value.ToString(MetadataXml.Culture));
            if (IsBold)
                writer.WriteAttributeString(MetadataXml.BoldAttribute, IsBold.ToString(MetadataXml.Culture).ToLowerInvariant());
            if (IsItalic)
                writer.WriteAttributeString(MetadataXml.ItalicAttribute, IsItalic.ToString(MetadataXml.Culture).ToLowerInvariant());
            if (FontFamily != null)
                writer.WriteAttributeString(MetadataXml.FontFamilyAttribute, FontFamily);
            if (Color != null)
                writer.WriteAttributeString(MetadataXml.ColorAttribute, Color.ArgbString);

            if (Value != null)
                writer.WriteValue(Value);
        }

        /// <summary>
        /// Apply a transformation to this instance of a <see cref="DisplayText"/>. This will modify the current
        /// instance coordinates.
        /// </summary>
        /// <param name="transformation">The <see cref="Transformation"/> to apply to this instance</param>
        public void Apply(Transformation transformation)
        {
            if (transformation != null)
            {
                if (transformation.Scale != null)
                {
                    CenterX *= transformation.Scale.X;
                    CenterY *= transformation.Scale.Y;
                }

                if (transformation.Translate != null)
                {
                    CenterX += transformation.Translate.X;
                    CenterY += transformation.Translate.Y;
                }
            }
        }
    }
}