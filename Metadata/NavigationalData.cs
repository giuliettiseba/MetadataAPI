using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// This class is responsible for holding navigational metadata.
    /// </summary>
    public class NavigationalData : IXmlSerializable, IEquatable<NavigationalData>
    {
        /// <summary>
        /// The maximum value that can be assigned to <see cref="Azimuth"/>.
        /// </summary>
        public const double MaxAzimuth = 180;

        /// <summary>
        /// The minimum value that can be assigned to <see cref="Azimuth"/>.
        /// </summary>
        public const double MinAzimuth = -180;

        /// <summary>
        /// The maximum value that can be assigned to <see cref="Latitude"/>.
        /// </summary>
        public const double MaxLatitude = 90;

        /// <summary>
        /// The minimum value that can be assigned to <see cref="Latitude"/>.
        /// </summary>
        public const double MinLatitude = -90;

        /// <summary>
        /// The maximum value that can be assigned to <see cref="Longitude"/>.
        /// </summary>
        public const double MaxLongitude = 180;

        /// <summary>
        /// The minimum value that can be assigned to <see cref="Longitude"/>.
        /// </summary>
        public const double MinLongitude = -180;

        /// <summary>
        /// The minimum value that can be assigned to <see cref="Speed"/>.
        /// </summary>
        public const double MinSpeed = 0;

        /// <summary>
        /// The minimum value that can be assigned to <see cref="VerticalAccuracy"/> and <see cref="HorizontalAccuracy"/>.
        /// </summary>
        public const double MinAccuracy = double.Epsilon;

        private double? _latitude;
        private double? _longitude;
        private double? _verticalAccuracy;
        private double? _speed;
        private double? _horizontalAccuracy;
        private double? _azimuth;
        
        /// <summary>
        /// Instantiate a new instance of <see cref="NavigationalData"/>
        /// </summary>
        public NavigationalData()
        {
            Version = new Version(1, 0);
        }

        /// <summary>
        /// Gets or set the altitude. It is measured in meters
        /// </summary>
        public double? Altitude { get; set; }

        /// <summary>
        /// Gets or set the azimuth (aka bearing or course). It is measured in degrees and is in the range from -180 to 180
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is not in the range [-180; 180]</exception>
        public double? Azimuth
        {
            get { return _azimuth; }
            set
            {
                if (value > MaxAzimuth || value < MinAzimuth)
                    throw new ArgumentOutOfRangeException("value", @"Azimuth must be a number in the range [-180; 180]");
                _azimuth = value;
            }
        }

        /// <summary>
        /// Gets or set the geodetic system. If a system is not set, WGS84 is assumed.
        /// </summary>
        public string GeodeticSystem { get; set; }

        /// <summary>
        /// Gets or set the horizontal accuracy. It is measured in meters and is a positive number
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is not positive</exception>
        public double? HorizontalAccuracy
        {
            get { return _horizontalAccuracy; }
            set
            {
                if (value < MinAccuracy)
                    throw new ArgumentOutOfRangeException("value", @"Horizontal Accuracy must be a positive number");
                _horizontalAccuracy = value;
            }
        }

        /// <summary>
        /// Gets or set the latitude. It is measured in degrees and is in the range from -90 to 90
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is not in the range [-90; 90]</exception>
        public double? Latitude
        {
            get { return _latitude; }
            set
            {
                if (value > MaxLatitude || value < MinLatitude)
                    throw new ArgumentOutOfRangeException("value", @"Latitude must be a number in the range [-90; 90]");
                _latitude = value;
            }
        }

        /// <summary>
        /// Gets or set the longitude. It is measured in degrees and is in the range from -180 to 180
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is not in the range [-180; 180]</exception>
        public double? Longitude
        {
            get { return _longitude; }
            set
            {
                if (value > MaxLongitude|| value < MinLongitude)
                    throw new ArgumentOutOfRangeException("value", @"Longitude must be a number in the range [-180; 180]");
                _longitude = value;
            }
        }

        /// <summary>
        /// Gets or set the vertical accuracy. It is measured in meters and is a positive number
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is not positive</exception>
        public double? VerticalAccuracy
        {
            get { return _verticalAccuracy; }
            set
            {
                if (value < MinAccuracy)
                    throw new ArgumentOutOfRangeException("value", @"Vertical Accuracy must be a positive number");
                _verticalAccuracy = value;
            }
        }

        /// <summary>
        /// Gets or set the speed. It is measured in m/s and is a non-negative number
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the value is negative</exception>
        public double? Speed
        {
            get { return _speed; }
            set
            {
                if (value < MinSpeed)
                    throw new ArgumentOutOfRangeException("value", @"Speed must not be a negative number");
                _speed = value;
            }
        }

        /// <summary>
        /// Gets or sets the version of the Navigation Data XML
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// <see cref="IXmlSerializable.GetSchema"/>
        /// </summary>
        XmlSchema IXmlSerializable.GetSchema()
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
            ReadAttributes(reader);

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
                if (ReferenceEquals(reader.NamespaceURI, string.Empty) == false)
                    continue;
                if (reader.Depth != rootDepth + 1) // Only look at immediate children
                    continue;
                if (reader.NodeType != XmlNodeType.Element)
                    continue;

                var localname = reader.LocalName;
                if (ReferenceEquals(MetadataXml.AltitudeElement, localname))
                {
                    reader.ReadStartElement();
                    Altitude = reader.ReadContentAsDouble();
                }
                else if (ReferenceEquals(MetadataXml.AzimuthElement, localname))
                {
                    reader.ReadStartElement();
                    Azimuth = reader.ReadContentAsDouble();
                }
                else if (ReferenceEquals(MetadataXml.GeodeticSystemElement, localname))
                {
                    reader.ReadStartElement();
                    GeodeticSystem = reader.ReadContentAsString();
                }
                else if (ReferenceEquals(MetadataXml.HorizontalAccuracyElement, localname))
                {
                    reader.ReadStartElement();
                    HorizontalAccuracy = reader.ReadContentAsDouble();
                }
                else if (ReferenceEquals(MetadataXml.LatitudeElement, localname))
                {
                    reader.ReadStartElement();
                    Latitude = reader.ReadContentAsDouble();
                }
                else if (ReferenceEquals(MetadataXml.LongitudeElement, localname))
                {
                    reader.ReadStartElement();
                    Longitude = reader.ReadContentAsDouble();
                }
                else if (ReferenceEquals(MetadataXml.VerticalAccuracyElement, localname))
                {
                    reader.ReadStartElement();
                    VerticalAccuracy = reader.ReadContentAsDouble();
                }
                else if (ReferenceEquals(MetadataXml.SpeedElement, localname))
                {
                    reader.ReadStartElement();
                    Speed = reader.ReadContentAsDouble();
                }

            } while (reader.Depth != rootDepth && reader.Read());
        }

        private void BlankAllFields()
        {
            Altitude = null;
            Azimuth = null;
            GeodeticSystem = null;
            HorizontalAccuracy = null;
            Latitude = null;
            Longitude = null;
            Speed = null;
            Version = null;
            VerticalAccuracy = null;
        }

        private void ReadAttributes(XmlReader reader)
        {
            while (reader.MoveToNextAttribute())
            {
                var attributeName = reader.LocalName;
                if (ReferenceEquals(MetadataXml.VersionAttribute, attributeName))
                {
                    Version = new Version(reader.ReadContentAsString());
                }
            }
        }

        /// <summary>
        /// <see cref="IXmlSerializable.WriteXml"/>
        /// </summary>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString(MetadataXml.VersionAttribute, Version.ToString(2));
            if (Latitude != null)
                writer.WriteElementString(MetadataXml.LatitudeElement, Latitude.Value.ToString(MetadataXml.Culture));
            if (Longitude != null)
                writer.WriteElementString(MetadataXml.LongitudeElement, Longitude.Value.ToString(MetadataXml.Culture));
            if (Altitude != null)
                writer.WriteElementString(MetadataXml.AltitudeElement, Altitude.Value.ToString(MetadataXml.Culture));
            if (Azimuth != null)
                writer.WriteElementString(MetadataXml.AzimuthElement, Azimuth.Value.ToString(MetadataXml.Culture));
            if (HorizontalAccuracy != null)
                writer.WriteElementString(MetadataXml.HorizontalAccuracyElement, HorizontalAccuracy.Value.ToString(MetadataXml.Culture));
            if (VerticalAccuracy != null)
                writer.WriteElementString(MetadataXml.VerticalAccuracyElement, VerticalAccuracy.Value.ToString(MetadataXml.Culture));
            if (Speed != null)
                writer.WriteElementString(MetadataXml.SpeedElement, Speed.Value.ToString(MetadataXml.Culture));
            if (GeodeticSystem != null)
                writer.WriteElementString(MetadataXml.GeodeticSystemElement, GeodeticSystem);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public bool Equals(NavigationalData other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Altitude.Equals(other.Altitude) && Azimuth.Equals(other.Azimuth) &&
                   string.Equals(GeodeticSystem, other.GeodeticSystem) &&
                   HorizontalAccuracy.Equals(other.HorizontalAccuracy) && Latitude.Equals(other.Latitude) &&
                   Longitude.Equals(other.Longitude) && VerticalAccuracy.Equals(other.VerticalAccuracy) &&
                   Speed.Equals(other.Speed) && Equals(Version, other.Version);
        }

        /// <summary>
        /// Provides value type equality semantics
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((NavigationalData) obj);
        }

        /// <summary>
        /// <see cref="object.GetHashCode"/>
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Altitude.GetHashCode();
                hashCode = (hashCode*397) ^ Azimuth.GetHashCode();
                hashCode = (hashCode*397) ^ (GeodeticSystem != null ? GeodeticSystem.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ HorizontalAccuracy.GetHashCode();
                hashCode = (hashCode*397) ^ Latitude.GetHashCode();
                hashCode = (hashCode*397) ^ Longitude.GetHashCode();
                hashCode = (hashCode*397) ^ VerticalAccuracy.GetHashCode();
                hashCode = (hashCode*397) ^ Speed.GetHashCode();
                hashCode = (hashCode*397) ^ (Version != null ? Version.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}