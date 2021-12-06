using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing a VideoAnalytics in ONVIF XML.
    /// </summary>
    public class VideoAnalytics : IXmlSerializable, IEquatable<VideoAnalytics>
    {
        private static readonly object Lock = new object();
        private static DateTime _lastFrameDiscarded;

        private readonly List<Frame> _frames = new List<Frame>();
        
        /// <summary>
        /// Gets the list of frames contained in this instance
        /// </summary>
        public List<Frame> Frames { get { return _frames; } }

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
            var rootDepth = reader.Depth;
            var isEmptyElement = reader.IsEmptyElement;

            _frames.Clear();

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
                if (ReferenceEquals(reader.LocalName, MetadataXml.FrameElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var frame = new Frame();
                        frame.ReadXml(subtreeReader);
                        if (frame.UtcTimeAttributeWasPresent)
                        {
                            Frames.Add(frame);
                        }
                        else
                        {
                            lock (Lock)
                            {
                                if (DateTime.UtcNow - _lastFrameDiscarded > MetadataXml.LogIgnoreTimeSpand)
                                {
                                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", "Element 'Frame' is incomplete and will be discarded", null);
                                    _lastFrameDiscarded = DateTime.UtcNow;
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
            foreach (var frame in _frames)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.FrameElement, MetadataXml.OnvifNamespace);
                frame.WriteXml(writer);
                writer.WriteEndElement();                
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(VideoAnalytics other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _frames.SequenceEqual(other._frames);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((VideoAnalytics) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            return (_frames != null && _frames.Count != 0
                ? _frames.Select(elem => elem.GetHashCode())
                    .Aggregate((v1, v2) => v1.GetHashCode() ^ v2.GetHashCode())
                : 0);
        }
    }
}
