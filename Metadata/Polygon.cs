using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    // TODO MBO: Descoped until some later point... 
    // TODO MBO: Prune incomplete vectors
    internal class Polygon : IXmlSerializable
    {
        private readonly List<Vector> _points = new List<Vector>();

        public List<Vector> Points { get { return _points; } }
        
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

                            if (MetadataXml.PointElement == localname)         // Do a comparison between the object references. This just compares pointers.
                            {
                                var point = new Vector();
                                point.ReadXml(reader.ReadSubtree());
                                _points.Add(point);
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == MetadataXml.PolygonElement)
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
