using System;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing an ONVIF XML transformation.
    /// 
    /// Instances of this class have value-identity, meaning that two classes with the same content are considered equal.
    /// </summary>
    public class Transformation : IXmlSerializable, IEquatable<Transformation>
    {
        private static readonly object Lock = new object();
        private static DateTime _lastScaleDiscarded;
        private static DateTime _lastTranslateDiscarded;

        /// <summary>
        /// Gets or sets the translation coordinates of the transformation.
        /// </summary>
        public Vector Translate { get; set; }

        /// <summary>
        /// Gets or sets the scale of the transformation.
        /// </summary>
        public Vector Scale { get; set; }

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

            Scale = null;
            Translate = null;

            var isEmptyElement = reader.IsEmptyElement;
            var rootDepth = reader.Depth;

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
                if (ReferenceEquals(reader.NamespaceURI, MetadataXml.OnvifNamespace) == false)
                    continue;
                if (reader.Depth != rootDepth + 1) // Only look at immediate children
                    continue;
                if (reader.NodeType != XmlNodeType.Element)
                    continue;
                if (ReferenceEquals(reader.LocalName, MetadataXml.ScaleElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var scale = new Vector();
                        scale.ReadXml(subtreeReader);
                        if (scale.AllAttributesWerePresent)
                        {
                            Scale = scale;
                        }
                        else
                        {
                            lock (Lock)
                            {
                                if (DateTime.UtcNow - _lastScaleDiscarded > MetadataXml.LogIgnoreTimeSpand)
                                {
                                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", "Element 'Scale' is incomplete and will be discarded", null);
                                    _lastScaleDiscarded = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                }
                if (ReferenceEquals(reader.LocalName, MetadataXml.TranslateElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var translate = new Vector();
                        translate.ReadXml(subtreeReader);
                        if (translate.AllAttributesWerePresent)
                        {
                            Translate = translate;
                        }
                        else
                        {
                            lock (Lock)
                            {
                                if (DateTime.UtcNow - _lastTranslateDiscarded > MetadataXml.LogIgnoreTimeSpand)
                                {
                                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", "Element 'Translate' is incomplete and will be discarded", null);
                                    _lastTranslateDiscarded = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                }
            } while (reader.Depth != rootDepth && reader.Read());
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            if (Translate != null)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.TranslateElement, MetadataXml.OnvifNamespace);
                Translate.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (Scale != null)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.ScaleElement, MetadataXml.OnvifNamespace);
                Scale.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(Transformation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Translate, other.Translate) && Equals(Scale, other.Scale);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Transformation) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Translate != null ? Translate.GetHashCode() : 0)*397) ^ (Scale != null ? Scale.GetHashCode() : 0);
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public static bool operator ==(Transformation left, Transformation right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Provides value type inequality semantics
        /// </summary>
        public static bool operator !=(Transformation left, Transformation right)
        {
            return !Equals(left, right);
        }
    }
}
