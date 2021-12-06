using System;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing a two-dimensional vector in ONVIF XML.
    /// </summary>
    public class Vector : IXmlSerializable, IEquatable<Vector>
    {
        private static readonly object Lock = new object();
        private static DateTime _lastMissingAttribute;

        /// <summary>
        /// Gets or sets the x component of the vector
        /// </summary>
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the y component of the vector
        /// </summary>
        public float Y { get; set; }

        internal bool AllAttributesWerePresent
        {
            get;
            private set;
        }

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

            AllAttributesWerePresent = true;
            X = ReadRequiredFloatAttributeValue(reader, MetadataXml.VectorXAttribute);
            Y = ReadRequiredFloatAttributeValue(reader, MetadataXml.VectorYAttribute);
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
                        var message = string.Format(CultureInfo.InvariantCulture, "Required attribute '{0}' could not be parsed or is missing", attributeName);
                        EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", message, null);
                        _lastMissingAttribute = DateTime.UtcNow;
                    }
                }
            }
            return floatValue;
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(MetadataXml.VectorXAttribute, X.ToString(MetadataXml.Culture));
            writer.WriteAttributeString(MetadataXml.VectorYAttribute, Y.ToString(MetadataXml.Culture));
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(Vector other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Vector) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X.GetHashCode()*397) ^ Y.GetHashCode();
            }
        }
    }
}
