using System;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing an ONVIF XML appearance.
    /// 
    /// Instances of this class have value-identity, meaning that two classes with the same content are considered equal.
    /// </summary>
    public class Appearance : IXmlSerializable, IEquatable<Appearance>
    {
        private static readonly object Lock = new object();
        private static DateTime _lastInvalidShape;

        // TODO: Add support for class elements, color and points in the future.
        //private readonly List<ColorCluster> _colorClusters = new List<ColorCluster>();
        //public List<ColorCluster> ColorClusters { get { return _colorClusters; } }

        /// <summary>
        /// Gets or sets the transformation of the appearance
        /// </summary>
        public Transformation Transformation { get; set; }

        /// <summary>
        /// Gets or sets the shape of the appearance
        /// </summary>
        public Shape Shape { get; set; }

        /// <summary>
        /// Gets or sets the class type of the Appearance
        /// </summary>
        public OnvifClass Class { get; set; }

        /// <summary>
        /// Gets or sets the description of the object. This can be used to display a short text on the screen along with the shape.
        /// </summary>
        public DisplayText Description { get; set; }

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

            var isEmptyElement = reader.IsEmptyElement;
            var rootDepth = reader.Depth;

            reader.ReadStartElement();
            if (isEmptyElement == false)
            {
                ReadChildren(reader, rootDepth);
                reader.ReadEndElement();
            }
        }

        private void BlankAllFields()
        {
            Shape = null;
            Transformation = null;
            Class = null;
            Description = null;
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
                if (ReferenceEquals(reader.LocalName, MetadataXml.ShapeElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var shape = new Shape();
                        shape.ReadXml(subtreeReader);
                        if (shape.IsValid)
                        {
                            Shape = shape;
                        }
                        else
                        {
                            lock (Lock)
                            {
                                if (DateTime.UtcNow - _lastInvalidShape > MetadataXml.LogIgnoreTimeSpand)
                                {
                                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", "Element 'Shape' is invalid and will be discarded. This message is logged at most once per minute", null);
                                    _lastInvalidShape = DateTime.UtcNow;
                                }
                            }
                        }
                    }
                }
                if (ReferenceEquals(reader.LocalName, MetadataXml.TransformationElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var transformation = new Transformation();
                        transformation.ReadXml(subtreeReader);
                        Transformation = transformation;
                    }
                }
                if (ReferenceEquals(reader.LocalName, MetadataXml.ClassElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var onvifClass = new OnvifClass();
                        onvifClass.ReadXml(subtreeReader);
                        Class = onvifClass;
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
                if (reader.NodeType != XmlNodeType.Element)
                    continue;
                if (ReferenceEquals(MetadataXml.DescriptionElement, reader.LocalName) && reader.Depth == 1)
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var text = new DisplayText();
                        text.ReadXml(subtreeReader);
                        Description = text;
                    }
                }
            } while (reader.Read());
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            if (Transformation != null)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.TransformationElement, MetadataXml.OnvifNamespace);
                Transformation.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (Shape != null)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.ShapeElement, MetadataXml.OnvifNamespace);
                Shape.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (Class != null)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.ClassElement, MetadataXml.OnvifNamespace);
                Class.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (ExtensionDataPresent())
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.ExtensionElement, MetadataXml.OnvifNamespace);
                if (Description != null)
                {
                    writer.WriteStartElement(MetadataXml.DescriptionElement);
                    Description.WriteXml(writer);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        private bool ExtensionDataPresent()
        {
            return Description != null;
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(Appearance other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Transformation, other.Transformation) && Equals(Shape, other.Shape);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Appearance) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Transformation != null ? Transformation.GetHashCode() : 0)*397) ^ (Shape != null ? Shape.GetHashCode() : 0);
            }
        }
    }
}
