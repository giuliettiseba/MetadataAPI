using System;
using System.IO;
using System.Text;
using System.Xml;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for deserializing metadata stored in the ONVIF metadata format
    /// </summary>
    public class MetadataDeserializer
    {
        private static readonly XmlReaderSettings Settings = CreateXmlReaderSettings();

        /// <summary>
        /// Parses the ONVIF metadata in the <paramref name="xml"/> into an instance of <see cref="MetadataStream"/>
        /// </summary>
        /// <param name="xml">A string containing the XML representation of the metadata</param>
        /// <returns>An instance of <see cref="MetadataStream"/> with the deserialized metadata</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="xml"/> is null</exception>
        /// <exception cref="XmlException">If the XML is not well-formed</exception>
		public MetadataStream ParseMetadataXml(string xml)
	    {
		    if (xml == null)
                throw new ArgumentNullException("xml");

            using (var textReader = new StringReader(xml))
			using (var reader = XmlReader.Create(textReader, Settings))
			{
				return ParseXml(reader);
			}
	    }

        /// <summary>
        /// Parses the ONVIF metadata in the <paramref name="dataStream"/> into an instance of <see cref="MetadataStream"/>
        /// </summary>
        /// <param name="dataStream">A stream containing the XML representation of the metadata</param>
        /// <returns>An instance of <see cref="MetadataStream"/> with the deserialized metadata</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="dataStream"/> is null</exception>
        /// <exception cref="XmlException">If the XML is not well-formed</exception>
        public MetadataStream ParseMetadataXml(Stream dataStream)
        {
            if (dataStream == null) 
                throw new ArgumentNullException("dataStream");

			using (var reader = XmlReader.Create(dataStream, Settings))
	        {
				return ParseXml(reader);
	        }
        }

        /// <summary>
        /// Parses the ONVIF metadata in the <paramref name="metadataContent"/> into an instance of <see cref="MetadataStream"/>
        /// </summary>
        /// <param name="metadataContent">A byte array containing the XML representation of the metadata encoded as UTF-8</param>
        /// <returns>An instance of <see cref="MetadataStream"/> with the deserialized metadata</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="metadataContent"/> is null</exception>
        /// <exception cref="XmlException">If the XML is not well-formed</exception>
        public MetadataStream ParseMetadataXml(byte[] metadataContent)
        {
            if (metadataContent == null)
                throw new ArgumentNullException("metadataContent");

            using (var memoryStream = new MemoryStream(metadataContent))
            using (var streamReader = new StreamReader(memoryStream, Encoding.UTF8))
            using (var reader = XmlReader.Create(streamReader, Settings))
            {
                return ParseXml(reader);
            }
        }

	    private static MetadataStream ParseXml(XmlReader reader)
	    {
		    do
		    {
			    switch (reader.NodeType)
			    {
			        case XmlNodeType.Element:
			            return HandleXmlElement(reader);
			    }
		    } while (reader.Read());

		    throw new XmlException("Unrecognized input");
	    }

        private static MetadataStream HandleXmlElement(XmlReader reader)
        {
            var localname = reader.LocalName;

            if (ReferenceEquals(MetadataXml.MetadataStreamElement, localname))
            {
                if (reader.NamespaceURI.Equals(MetadataXml.OnvifNamespace) == false)
                {
                    var message = "Incorrect namespace in top-level element: " + reader.NamespaceURI;
                    var xmlInfo = (IXmlLineInfo) reader;

                    throw new XmlException(message, null, xmlInfo.LineNumber, xmlInfo.LinePosition);
                }

                try
                {
                    var metadataStream = new MetadataStream();
                    metadataStream.ReadXml(reader.ReadSubtree());
                    return metadataStream;
                }
                catch (Exception ex)
                {
                    throw new XmlException("Error while deserializing XML: " + ex.Message, ex);
                }
            }
            else
            {
                var message = "Incorrect top-level element: " + localname;
                var xmlInfo = (IXmlLineInfo) reader;

                throw new XmlException(message, null, xmlInfo.LineNumber, xmlInfo.LinePosition);
            }
        }

        private static XmlReaderSettings CreateXmlReaderSettings()
	    {
			// Create a reader that uses the NameTable.
		    var settings = new XmlReaderSettings
		    {
			    IgnoreComments = true,
			    IgnoreProcessingInstructions = true,
			    IgnoreWhitespace = true,
			    NameTable = MetadataXml.NameTable,
		    };
		    return settings;
	    }
    }
}
