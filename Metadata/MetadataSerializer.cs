using System;
using System.IO;
using System.Text;
using System.Xml;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for serializing metadata to the ONVIF XML metadata format
    /// </summary>
    public class MetadataSerializer
    {
        // Must be placed before the Settings field as it needs to be initialized first.
        internal static readonly Encoding MetadataEncoding = new UTF8Encoding(false);

        private static readonly XmlWriterSettings Settings = CreateWriterSettings();

        /// <summary>
        /// Write metadata and return the written data as a string.
        /// </summary>
        /// <param name="metadata">The metadata to write</param>
        /// <returns>The metadata XML.</returns>
        public string WriteMetadataXml(MetadataStream metadata)
        {
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            using (var stream = new MemoryStream())
            using (var streamReader = new StreamReader(stream, MetadataEncoding))
            {
                using (var xmlWriter = XmlWriter.Create(stream, Settings))
                {
                    WriteXml(xmlWriter, metadata);
                }
                stream.Position = 0;
                return streamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Write metadata to a client-supplied stream.
        /// </summary>
        /// <param name="output">The output <see cref="Stream"/> where the result will be written</param>
        /// <param name="metadata">The metadata to write</param>
        public void WriteMetadataXml(Stream output, MetadataStream metadata)
        {
            if (output == null) 
                throw new ArgumentNullException("output");
            if (metadata == null)
                throw new ArgumentNullException("metadata");

            using (var xmlWriter = XmlWriter.Create(output, Settings))
            {
                WriteXml(xmlWriter, metadata);
            }
        }

        private static void WriteXml(XmlWriter xmlWriter, MetadataStream metadata)
        {
            xmlWriter.WriteStartDocument();
            xmlWriter.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.MetadataStreamElement, MetadataXml.OnvifNamespace);
            metadata.WriteXml(xmlWriter);
            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndDocument();
        }

        private static XmlWriterSettings CreateWriterSettings()
        {
            return new XmlWriterSettings
            {
                CloseOutput = false,
                ConformanceLevel = ConformanceLevel.Document,
                Encoding = MetadataEncoding,
                Indent = false,
                OmitXmlDeclaration = false
            };
        }
    }
}