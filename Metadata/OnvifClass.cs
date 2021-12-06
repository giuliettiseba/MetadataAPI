using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing an ONVIF XML class.
    /// 
    /// Instances of this class have value-identity, meaning that two classes with the same content are considered equal.
    /// </summary>
    public class OnvifClass : IXmlSerializable, IEquatable<OnvifClass>
    {
        private static readonly object Lock = new object();
        private static DateTime _lastInvalidClassCandidate;

        private readonly List<ClassCandidate> _classCandidateItems = new List<ClassCandidate>();

        /// <summary>
        /// Gets the list of class candidate elements contained in this instance
        /// </summary>
        public IList<ClassCandidate> ClassCandidates
        {
            get { return _classCandidateItems; }
        }

        /// <summary>
        /// <see cref="IXmlSerializable.GetSchema"/>
        /// </summary>
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IXmlSerializable.ReadXml"/>
        /// </summary>
        public void ReadXml(XmlReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            reader.MoveToContent();

            ClassCandidates.Clear();

            var isEmptyElement = reader.IsEmptyElement;
            var rootDepth = reader.Depth;

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
                if (ReferenceEquals(reader.LocalName, MetadataXml.ClassCandidateElement))
                {
                    using (var subtreeReader = reader.ReadSubtree())
                    {
                        var classCandidate = new ClassCandidate();
                        classCandidate.ReadXml(subtreeReader);
                        if (classCandidate.IsValid)
                        {
                            ClassCandidates.Add(classCandidate);
                        }
                        else
                        {
                            lock (Lock)
                            {
                                if (DateTime.UtcNow - _lastInvalidClassCandidate > MetadataXml.LogIgnoreTimeSpand)
                                {
                                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", "Element 'ClassCandidate' is invalid and will be discarded. This message is logged at most once per minute", null);
                                    _lastInvalidClassCandidate = DateTime.UtcNow;
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
            if (writer == null) throw new ArgumentNullException("writer");

            foreach (var classCandidate in ClassCandidates)
            {
                writer.WriteStartElement(MetadataXml.OnvifPrefix, MetadataXml.ClassCandidateElement, MetadataXml.OnvifNamespace);
                classCandidate.WriteXml(writer);
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(OnvifClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _classCandidateItems.SequenceEqual(other._classCandidateItems);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((OnvifClass) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            return _classCandidateItems.GetHashCode();
        }
    }
}