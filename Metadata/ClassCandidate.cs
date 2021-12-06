using System;
using System.Globalization;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for representing an ONVIF XML class candidate used for conveying information on the type of object
    /// inside a bounding box.
    /// 
    /// Instances of this class have value-identity, meaning that two classes with the same content are considered equal.
    /// </summary>
    public class ClassCandidate : IXmlSerializable, IEquatable<ClassCandidate>
    {
        private const float DefaultLikelihood = 0;
        private const string DefaultType = null;

        private static readonly object Lock = new object();
        private static DateTime _lastTypeErrorLogMessage;
        private static DateTime _lastLikelihoodErrorLogMessage;

        /// <summary>
        /// Gets or sets the type of the object
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "This is the ONVIF name, so we will keep it")]
        public string Type { get; set; }

        /// <summary>
        /// Gets or sets the likelihood that the type is correct
        /// </summary>
        public float Likelihood { get; set; }

        /// <summary>
        /// Gets whether the class candidate is valid according to the ONVIF standard, meaning it has a type
        /// and a likelihood greater in the interval ]0 ; 1].
        /// </summary>
        public bool IsValid
        {
            get
            {
                return string.IsNullOrWhiteSpace(Type) == false &&
                       Likelihood > 0 && Likelihood <= 1;
            }
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

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            if (writer == null) throw new ArgumentNullException("writer");

            writer.WriteElementString(MetadataXml.OnvifPrefix, MetadataXml.TypeElement, MetadataXml.OnvifNamespace, Type);
            writer.WriteElementString(MetadataXml.OnvifPrefix, MetadataXml.LikelihoodElement, MetadataXml.OnvifNamespace,
                Likelihood.ToString(MetadataXml.Culture));
        }

        private void BlankAllFields()
        {
            Type = DefaultType;
            Likelihood = DefaultLikelihood;
        }

        private void ReadChildren(XmlReader reader, int rootDepth)
        {
            bool typeElementPresent = false;
            bool likelihoodElementPresent = false;

            do
            {
                if (ReferenceEquals(reader.NamespaceURI, MetadataXml.OnvifNamespace) == false)
                    continue;
                if (reader.Depth != rootDepth + 1) // Only look at immediate children
                    continue;
                if (reader.NodeType != XmlNodeType.Element)
                    continue;
                if (ReferenceEquals(reader.LocalName, MetadataXml.TypeElement))
                {
                    typeElementPresent = true;
                    ConsumeChildTreeNodes(reader, rootDepth);
                    Type = ParseRequiredStringValue(reader.ReadContentAsString());
                }
                else if (ReferenceEquals(reader.LocalName, MetadataXml.LikelihoodElement))
                {
                    likelihoodElementPresent = true;
                    ConsumeChildTreeNodes(reader, rootDepth);
                    Likelihood = ParseRequiredLikelihoodValue(reader.ReadContentAsString());
                }
            } while (reader.Depth != rootDepth && reader.Read());

            if (typeElementPresent == false)
            {
                LogTypeElementError("Required element 'Type' is missing.");
            }
            if (likelihoodElementPresent == false)
            {
                LogLikelihoodElementError("Required element 'Likelihood' is missing.");
            }
        }

        private static void ConsumeChildTreeNodes(XmlReader reader, int rootDepth)
        {
            while (reader.NodeType != XmlNodeType.Text && reader.Depth >= rootDepth + 1 && reader.Read())
            {
            }
        }

        private string ParseRequiredStringValue(string typeValue)
        {
            if (string.IsNullOrWhiteSpace(typeValue))
            {
                LogTypeElementError("Element 'Type' is empty.");
                return DefaultType;
            }

            return typeValue;
        }

        private float ParseRequiredLikelihoodValue(string likelihoodValue)
        {
            float floatValue;
            if (float.TryParse(likelihoodValue, MetadataXml.FloatStyle, MetadataXml.Culture, out floatValue) == false)
            {
                var message = string.Format(CultureInfo.InvariantCulture, "Required element 'Likelihood' is not a number. Value read: {0}", likelihoodValue);
                LogLikelihoodElementError(message);
                return DefaultLikelihood;
            }

            return floatValue;
        }

        private void LogLikelihoodElementError(string message)
        {
            lock (Lock)
            {
                if (DateTime.UtcNow - _lastLikelihoodErrorLogMessage > MetadataXml.LogIgnoreTimeSpand)
                {
                    
                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", message, null);
                    _lastLikelihoodErrorLogMessage = DateTime.UtcNow;
                }
            }
        }

        private void LogTypeElementError(string message)
        {
            lock (Lock)
            {
                if (DateTime.UtcNow - _lastTypeErrorLogMessage > MetadataXml.LogIgnoreTimeSpand)
                {
                    EnvironmentManager.Instance.Log(GetType().FullName, false, "ReadXml", message, null);
                    _lastTypeErrorLogMessage = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(ClassCandidate other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Type, other.Type) && Likelihood.Equals(other.Likelihood);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ClassCandidate) obj);
        }
        
        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ Likelihood.GetHashCode();
            }
        }
    }
}