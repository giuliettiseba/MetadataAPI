using System;
using System.Text;
using System.Xml;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for containing metadata received from the VMS system.
    /// </summary>
    public class MetadataContent
    {
        private readonly string _metadataXml;
        private readonly Lazy<MetadataStream> _deserializedMetadata;
        private readonly MetadataDeserializer _deserializer = new MetadataDeserializer();

        /// <summary>
        /// Create an instance of <see cref="MetadataContent"/> and initialize the metadata
        /// </summary>
        /// <param name="data">The metadata as an UTF-8 encoded byte array</param>
        /// <exception cref="ArgumentNullException">If <paramref name="data"/> is null</exception>
        public MetadataContent(byte[] data)
        {
            if (data == null) throw new ArgumentNullException("data");

            _metadataXml = Encoding.UTF8.GetString(data);
            _deserializedMetadata = new Lazy<MetadataStream>(DeserializeMetadata, true);
        }

        /// <summary>
        /// Get the metadata as a string. This is simply the constructor parameter decoded as UTF-8.
        /// </summary>
        /// <returns></returns>
        public string GetMetadataString()
        {
            return _metadataXml;
        }

        /// <summary>
        /// Gets the metadata as deserialized objects
        /// </summary>
        /// <returns>A <see cref="MetadataStream"/> that acts as the root node for the ONVIF XML.</returns>
        /// <exception cref="XmlException">If the data cannot be deserialized</exception>
        public MetadataStream GetMetadataStream()
        {
            return _deserializedMetadata.Value;
        }

        private MetadataStream DeserializeMetadata()
        {
            return _deserializer.ParseMetadataXml(_metadataXml);
        }
    }
}