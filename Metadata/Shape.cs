using System;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing an ONVIF XML shape.
    /// </summary>
    public class Shape : IXmlSerializable, IEquatable<Shape>
    {
        private static readonly object Lock = new object();
        private static DateTime _lastBoundingBoxNotReadLog;
        private static DateTime _lastBoundingBoxDiscarded;
        private static DateTime _lastCenterOfGravityDiscarded;
        
        /// <summary>
        /// Gets or sets the bounding box of the shape.
        /// </summary>
        public Rectangle BoundingBox { get; set; }

        /// <summary>
        /// Gets or sets the CenterOfGravity of the shape.
        /// </summary>
        public Vector CenterOfGravity { get; set; }

        /// <summary>
        /// Gets whether the shape is valid according to the ONVIF standard, except that we also
        /// allow a missing CenterOfGravity. A valid shape has a bounding box defined (i.e different from null).
        /// </summary>
        public bool IsValid
        {
            get
            {
                return BoundingBox != null;
            }
        }

        // TODO MBO: Support for polygons descoped for now
        // public List<Polygon> Polygons { get; set; }

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

            BoundingBox = null;
            CenterOfGravity = null;

            var isEmptyElement = reader.IsEmptyElement;
            var rootDepth = reader.Depth;

            reader.ReadStartElement();
            if (isEmptyElement == false)
            {
                ReadChildren(reader, rootDepth);
                reader.ReadEndElement();
            }

            lock (Lock)
            {
                if (BoundingBox == null && DateTime.UtcNow - _lastBoundingBoxNotReadLog > MetadataXml.LogIgnoreTimeSpand)
                {
                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", "Element 'BoundingBox' could not be read", null);
                    _lastBoundingBoxNotReadLog = DateTime.UtcNow;
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
                if (ReferenceEquals(reader.LocalName, MetadataXml.BoundingBoxElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var boundingBox = new Rectangle();
                        boundingBox.ReadXml(subtreeReader);
                        if (boundingBox.AllAttributesWerePresent)
                        {
                            BoundingBox = boundingBox;
                        }
                        else
                        {
                            lock(Lock)
                            {
                                if (DateTime.UtcNow - _lastBoundingBoxDiscarded > MetadataXml.LogIgnoreTimeSpand)
                                {
                                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", "Element 'BoundingBox' is incomplete and will be discarded", null);
                                    _lastBoundingBoxDiscarded = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                }
                if (ReferenceEquals(reader.LocalName, MetadataXml.CenterOfGravityElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var centerOfGravity = new Vector();
                        centerOfGravity.ReadXml(subtreeReader);
                        if (centerOfGravity.AllAttributesWerePresent)
                        {
                            CenterOfGravity = centerOfGravity;
                        }
                        else
                        {
                            lock (Lock)
                            {
                                if (DateTime.UtcNow - _lastCenterOfGravityDiscarded > MetadataXml.LogIgnoreTimeSpand)
                                {
                                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", "Element 'CenterOfGravity' is incomplete and will be discarded", null);
                                    _lastCenterOfGravityDiscarded = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                }
                if (ReferenceEquals(reader.LocalName, MetadataXml.ExtensionElement))
                {
                    ReadExtensionData(reader.ReadSubtree());
                }
            } while (reader.Depth != rootDepth && reader.Read());
            
        }

        private void ReadExtensionData(XmlReader reader)
        {
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (ReferenceEquals(MetadataXml.BoundingBoxAppearanceElement, reader.LocalName))
                        {
                            using (var subtreeReader = reader.ReadSubtree())
                            {
                                if (BoundingBox != null)
                                {
                                    BoundingBox.ReadAppearanceExtensionXml(subtreeReader);
                                }
                            }
                        }
                        break;
                }
            } while (reader.Read());
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            if (BoundingBox != null)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.BoundingBoxElement, MetadataXml.OnvifNamespace);
                BoundingBox.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (CenterOfGravity != null)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.CenterOfGravityElement, MetadataXml.OnvifNamespace);
                CenterOfGravity.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (BoundingBox != null && BoundingBox.HasExtensionDataPresent())
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.ExtensionElement, MetadataXml.OnvifNamespace);
                writer.WriteStartElement(MetadataXml.BoundingBoxAppearanceElement);
                BoundingBox.WriteAppearanceExtenstionXml(writer);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(Shape other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(BoundingBox, other.BoundingBox) && Equals(CenterOfGravity, other.CenterOfGravity);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Shape) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((BoundingBox != null ? BoundingBox.GetHashCode() : 0)*397) ^ (CenterOfGravity != null ? CenterOfGravity.GetHashCode() : 0);
            }
        }
    }
}
