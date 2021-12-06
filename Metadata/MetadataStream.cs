using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing a Frame in ONVIF XML.
    /// </summary>
    public class MetadataStream : IXmlSerializable, IEquatable<MetadataStream>
    {
        private readonly List<VideoAnalytics> _videoAnalyticsItems = new List<VideoAnalytics>();
        private readonly List<byte> _originalData = new List<byte>();

        /// <summary>
        /// Gets the list of video analytics elements contained in this instance
        /// </summary>
        public List<VideoAnalytics> VideoAnalyticsItems
        {
            get { return _videoAnalyticsItems; }
        }

        /// <summary>
        /// Gets the list of original data bytes.
        /// </summary>
        public List<byte> OriginalData
	    {
            get { return _originalData; }
	    }

        /// <summary>
        /// Gets or sets the navigational data.
        /// </summary>
        public NavigationalData NavigationalData { get; set; }

        /// <summary>
        /// Gets all frames included in all video analytics elements. In other words, this method returns all
        /// frames in this metadata stream instance.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of all frames in the metadata</returns>
        public IEnumerable<Frame> GetAllFrames()
        {
            return _videoAnalyticsItems.SelectMany(va => va.Frames);
        }

        /// <summary>
        /// Gets the first frame or null if there are no frames. This is useful if the client of this class
        /// can only handle a single frame per metadata stream anyway.
        /// </summary>
        /// <returns>The first frame or null</returns>
        public Frame GetFrame()
        {
            return _videoAnalyticsItems.SelectMany(va => va.Frames).FirstOrDefault();
        }

        /// <summary>
        /// <see cref="IXmlSerializable.GetSchema"/>
        /// </summary>
        System.Xml.Schema.XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>
        /// <see cref="IXmlSerializable.ReadXml"/>
        /// </summary>
        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            var rootDepth = reader.Depth;
            var isEmptyElement = reader.IsEmptyElement;

            BlankAllFields();

            reader.ReadStartElement();

            if (isEmptyElement == false)
            {
                ReadChildren(reader, rootDepth);
                reader.ReadEndElement();
            }
        }

        private void BlankAllFields()
        {
            NavigationalData = null;
            _originalData.Clear();
            _videoAnalyticsItems.Clear();
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
                if (ReferenceEquals(reader.LocalName, MetadataXml.VideoAnalyticsElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var videoAnalytics = new VideoAnalytics();
                        videoAnalytics.ReadXml(subtreeReader);
                        _videoAnalyticsItems.Add(videoAnalytics);
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
                        if (ReferenceEquals(MetadataXml.OriginalDataElement, reader.LocalName))
                        {
                            var base64String = reader.ReadElementString();
                            var bytes = Convert.FromBase64String(base64String);

                            _originalData.AddRange(bytes);
                        }
                        if (ReferenceEquals(MetadataXml.NavigationalDataElement, reader.LocalName))
                        {
                            using (var subtreeReader = reader.ReadSubtree())
                            {
                                NavigationalData = new NavigationalData();
                                NavigationalData.ReadXml(subtreeReader);
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
            foreach (var videoAnalyticsItem in VideoAnalyticsItems)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.VideoAnalyticsElement, MetadataXml.OnvifNamespace);
                videoAnalyticsItem.WriteXml(writer);
                writer.WriteEndElement();
            }
            if (ExtensionDataPresent())
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.ExtensionElement, MetadataXml.OnvifNamespace);
                if (NavigationalData != null)
                {
                    writer.WriteStartElement(MetadataXml.NavigationalDataElement);
                    NavigationalData.WriteXml(writer);
                    writer.WriteEndElement();
                }
                if (_originalData.Count != 0)
                {
                    writer.WriteStartElement(MetadataXml.OriginalDataElement);
                    writer.WriteBase64(_originalData.ToArray(), 0, _originalData.Count);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }

        private bool ExtensionDataPresent()
        {
            return NavigationalData != null || _originalData.Count != 0;
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(MetadataStream other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _videoAnalyticsItems.SequenceEqual(other._videoAnalyticsItems)
                && _originalData.SequenceEqual(other._originalData);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MetadataStream)obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((_videoAnalyticsItems != null && _videoAnalyticsItems.Count != 0
                    ? _videoAnalyticsItems.Select(elem => elem.GetHashCode())
                        .Aggregate((v1, v2) => v1.GetHashCode() ^ v2.GetHashCode())
                    : 0)*397)
                       ^ (_originalData != null ? _originalData.GetHashCode() : 0);
            }
        }
    }
}
