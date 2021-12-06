using System;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    // TODO MBO: This has been descoped for now.
    internal class Color : IXmlSerializable
    {
        private float _x;
        private float _y;
        private float _z;
        private string _colorspace;
        
        public float X
        {
            get { return _x; }
            set { _x = value; }
        }

        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }
        
        public float Z
        {
            get { return _z; }
            set { _z = value; }
        }

        public string Colorspace
        {
            get { return _colorspace; }
            set { _colorspace = value; }
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
                            while (reader.MoveToNextAttribute())
                            {
                                // Cache the local name to prevent multiple calls to the LocalName property.
                                var localname = reader.LocalName;

                                if (MetadataXml.ColorXAttribute == localname)         // Do a comparison between the object references. This just compares pointers.
                                {
                                    //_x = Util.ReadFloatValue(reader);
                                }
                                else if (MetadataXml.ColorYAttribute == localname)
                                {
                                    //_y = Util.ReadFloatValue(reader);
                                }
                                else if (MetadataXml.ColorZAttribute == localname)
                                {
                                    //_z = Util.ReadFloatValue(reader);
                                }
                                else if (MetadataXml.ColorspaceAttribute == localname)
                                {
                                    //_colorspace = Util.ReadStringValue(reader);
                                }
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == MetadataXml.ColorElement)
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
