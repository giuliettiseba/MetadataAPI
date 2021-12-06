using System;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing an ONVIF XML behavior for objects
    /// 
    /// Instances of this class have value-identity, meaning that two classes with the same content are considered equal.
    /// </summary>
    public class Behaviour : IXmlSerializable, IEquatable<Behaviour>
    {
        /// <summary>
        /// Gets whether there are any behaviors present
        /// </summary>
        public bool HasBehaviours { get { return IsIdle || IsRemoved; } }

        /// <summary>
        /// Gets or sets that the behavior for the object is in the idle state
        /// </summary>
        public bool IsIdle { get; set; }

        /// <summary>
        /// Gets or sets that the behavior for the object is in the removed state
        /// </summary>
        public bool IsRemoved { get; set; }

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

            IsIdle = false;
            IsRemoved = false;

            var isEmptyElement = reader.IsEmptyElement;
            var rootDepth = reader.Depth;

            reader.ReadStartElement();
            if (isEmptyElement == false)
            {
                ReadBehaviourChildren(reader, rootDepth);
                reader.ReadEndElement();
            }
        }

        private void ReadBehaviourChildren(XmlReader reader, int rootDepth)
        {
            do
            {
                if (ReferenceEquals(reader.NamespaceURI, MetadataXml.OnvifNamespace) == false)
                    continue;
                if (reader.Depth != rootDepth + 1) // Only look at immediate children
                    continue;
                if (reader.NodeType != XmlNodeType.Element)
                    continue;
                if (ReferenceEquals(reader.LocalName, MetadataXml.BehaviourRemovedElement))
                    IsRemoved = true;
                if (ReferenceEquals(reader.LocalName, MetadataXml.BehaviourIdleElement))
                    IsIdle = true;
            } while (reader.Depth != rootDepth && reader.Read());
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            if (IsRemoved)
            {
                writer.WriteElementString(MetadataXml.OnvifPrefix, MetadataXml.BehaviourRemovedElement,
                    MetadataXml.OnvifNamespace, string.Empty);
            }
            if (IsIdle)
            {
                writer.WriteElementString(MetadataXml.OnvifPrefix, MetadataXml.BehaviourIdleElement,
                    MetadataXml.OnvifNamespace, string.Empty);
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(Behaviour other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return IsIdle.Equals(other.IsIdle) && IsRemoved.Equals(other.IsRemoved);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Behaviour) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return (IsIdle.GetHashCode()*397) ^ IsRemoved.GetHashCode();
            }
        }
    }
}
