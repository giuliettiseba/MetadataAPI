using System;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    // TODO MBO: This functionality has been descoped until further notice.
    internal class ColorCluster : IXmlSerializable
    {
        private Color _color;

        public Color Color
        {
            get { return _color; }
        }

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            do
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.NamespaceURI.Equals(MetadataXml.OnvifNamespace))
                        {
                            // Cache the local name to prevent multiple calls to the LocalName property.
                            var localname = reader.LocalName;

                            if (MetadataXml.ColorElement == localname)         // Do a comparison between the object references. This just compares pointers.
                            {
                                var color = new Color();
                                color.ReadXml(reader);
                                _color = color;
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == MetadataXml.ColorClusterElement)
                        {
                            return;
                        }
                        break;
                }
            } while (reader.Read());

            reader.Close();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
