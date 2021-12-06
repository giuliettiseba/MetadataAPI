using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing a Frame in ONVIF XML.
    /// </summary>
    public class Frame : IXmlSerializable, IEquatable<Frame>
    {
        private static readonly object Lock = new object();
        private static DateTime _lastMissingTimestampLog;

        private readonly List<OnvifObject> _objects = new List<OnvifObject>();

        /// <summary>
        /// Create a new instance of a <see cref="Frame"/> with the <see cref="UtcTime"/> property set to <see cref="DateTime.MinValue"/>.
        /// </summary>
        public Frame() : this(DateTime.MinValue) { }

        /// <summary>
        /// Create a new instance of a <see cref="Frame"/> and set the <see cref="UtcTime"/> property.
        /// </summary>
        public Frame(DateTime utcTime)
        {
            UtcTime = utcTime;
        }

        /// <summary>
        /// Gets or sets the transformation part of the frame
        /// </summary>
        public Transformation Transformation { get; set; }

        /// <summary>
        /// Gets the list of objects contained in the frame
        /// </summary>
        public List<OnvifObject> Objects { get { return _objects; } }

        /// <summary>
        /// Gets or sets the UTC timestamp of the frame.
        /// </summary>
        public DateTime UtcTime { get; set; }

        /// <summary>
        /// Gets whether <see cref="UtcTime"/> was present in the XML that populated this instance.
        /// This property is false if the instance was not populated by XML.
        /// </summary>
        public bool UtcTimeAttributeWasPresent { get; private set; }

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
            var rootDepth = reader.Depth;
            var isEmptyElement = reader.IsEmptyElement;

            BlankAllFields();
            ReadUtcTime(reader);

            reader.ReadStartElement();

            if (isEmptyElement == false)
            {
                ReadChildren(reader, rootDepth);
                reader.ReadEndElement();
            }
        }

        private void BlankAllFields()
        {
            _objects.Clear();
            Transformation = null;
            UtcTime = DateTime.MinValue;
            UtcTimeAttributeWasPresent = false;
        }

        private void ReadUtcTime(XmlReader reader)
        {
            var utcTimeValue = reader.GetAttribute(MetadataXml.UtcTimeAttribute);
            DateTime utcTime;
            if (DateTime.TryParse(utcTimeValue, MetadataXml.Culture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out utcTime))
            {
                UtcTimeAttributeWasPresent = true;
                UtcTime = utcTime;
            }
            else
            {
                lock (Lock)
                {
                    if (UtcTimeAttributeWasPresent == false && DateTime.UtcNow - _lastMissingTimestampLog > MetadataXml.LogIgnoreTimeSpand)
                    {
                        EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", "Required attribute 'UtcTime' could not be parsed or is missing", null);
                        _lastMissingTimestampLog = DateTime.UtcNow;
                    }
                }
            }
        }

        private void ReadChildren(XmlReader reader, int rootDepth)
        {
            do
            {
                if (ReferenceEquals(reader.NamespaceURI, MetadataXml.OnvifNamespace) == false)
                    continue;
                if (reader.Depth != rootDepth + 1) // Only look at immediate children
                    continue;
                if (reader.NodeType != XmlNodeType.Element)
                    continue;
                if (ReferenceEquals(reader.LocalName, MetadataXml.TransformationElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        Transformation = new Transformation();
                        Transformation.ReadXml(subtreeReader);
                    }
                }
                else if (ReferenceEquals(reader.LocalName, MetadataXml.ObjectElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var obj = new OnvifObject();
                        obj.ReadXml(subtreeReader);
                        Objects.Add(obj);
                    }
                }
            } while (reader.Depth != rootDepth && reader.Read());
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(MetadataXml.UtcTimeAttribute, UtcTime.ToUniversalTime().ToString("o", MetadataXml.Culture));
            if (Transformation != null)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.TransformationElement, MetadataXml.OnvifNamespace);
                Transformation.WriteXml(writer);
                writer.WriteEndElement();
            }
            foreach (var onvifObject in _objects)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.ObjectElement, MetadataXml.OnvifNamespace);
                onvifObject.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(Frame other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return UtcTime.Equals(other.UtcTime) && _objects.SequenceEqual(other._objects) && Equals(Transformation, other.Transformation);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Frame)obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = UtcTime.GetHashCode();
                hashCode = (hashCode * 397) ^ (_objects != null ? _objects.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Transformation != null ? Transformation.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
