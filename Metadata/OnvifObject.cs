using System;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing an ONVIF XML object.
    /// 
    /// Instances of this class have value-identity, meaning that two classes with the same content are considered equal.
    /// </summary>
    public class OnvifObject : IXmlSerializable, IEquatable<OnvifObject>
    {
        /// <summary>
        /// Create a new instance of <see cref="OnvifObject"/> and initialize <see cref="ObjectId"/> to zero.
        /// </summary>
        public OnvifObject() : this(0) { }

        /// <summary>
        /// Create a new instance of <see cref="OnvifObject"/> and initialize <see cref="ObjectId"/>.
        /// </summary>
        /// <param name="objectId">The value to set <see cref="ObjectId"/> to</param>
        public OnvifObject(int objectId)
        {
            ObjectId = objectId;
        }

        /// <summary>
        /// Gets or sets the ID of the object. This is to track objects from frame to frame.
        /// </summary>
        public int ObjectId { get; set; }

        /// <summary>
        /// Gets or sets the appearance of the object.
        /// </summary>
        public Appearance Appearance { get; set; }

        /// <summary>
        /// Gets or sets the behavior of the object.
        /// </summary>
        public Behaviour Behaviour { get; set; }

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

            ObjectId = 0;
            Appearance = null;
            Behaviour = null;

            var isEmptyElement = reader.IsEmptyElement;
            var rootDepth = reader.Depth;

            ReadObjectIdValue(reader);

            reader.ReadStartElement();
            if (isEmptyElement == false)
            {
                ReadChildren(reader, rootDepth);
                reader.ReadEndElement();
            }
        }

        private void ReadObjectIdValue(XmlReader reader)
        {
            var objectIdValue = reader.GetAttribute(MetadataXml.ObjectIdAttribute);
            int objectId;
            if (int.TryParse(objectIdValue, MetadataXml.FloatStyle, MetadataXml.Culture, out objectId) == false)
            {
                if (EnvironmentManager.Instance != null)
                    EnvironmentManager.Instance.Log(false, "OnvifObject.ReadXml",
                        "Value in ObjectId attribute is not an integer: " + objectIdValue);
            }

            ObjectId = objectId;
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
                if (ReferenceEquals(reader.LocalName, MetadataXml.AppearanceElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        Appearance = new Appearance();
                        Appearance.ReadXml(subtreeReader);
                    }
                }
                if (ReferenceEquals(reader.LocalName, MetadataXml.BehaviourElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        Behaviour = new Behaviour();
                        Behaviour.ReadXml(subtreeReader);
                    }
                }
            } while (reader.Depth != rootDepth && reader.Read());
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(MetadataXml.ObjectIdAttribute, ObjectId.ToString(CultureInfo.InvariantCulture));
            if (Appearance != null)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.AppearanceElement, MetadataXml.OnvifNamespace);
                Appearance.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (Behaviour != null && Behaviour.HasBehaviours)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.BehaviourElement, MetadataXml.OnvifNamespace);
                Behaviour.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(OnvifObject other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return ObjectId == other.ObjectId && Equals(Appearance, other.Appearance) && Equals(Behaviour, other.Behaviour);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OnvifObject) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ObjectId;
                hashCode = (hashCode*397) ^ (Appearance != null ? Appearance.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Behaviour != null ? Behaviour.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public static bool operator ==(OnvifObject left, OnvifObject right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Provides value type inequality semantics
        /// </summary>
        public static bool operator !=(OnvifObject left, OnvifObject right)
        {
            return !Equals(left, right);
        }
    }
}
