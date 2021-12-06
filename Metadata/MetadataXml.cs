using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace VideoOS.Platform.Metadata
{
    /// <summary>
    /// A nametable for Metadata XML used for speeding up processing and centralizing names.
    /// This works by only instantiating a specific string once, thereby making reference
    /// comparisons possible for string matching. All XML names (elements and attributes)
    /// *must* be public static read-only string. Otherwise the reflection logic will fail
    /// to add the names to the <see cref="XmlNameTable"/>.
    /// </summary>
    internal static class MetadataXml
    {
        public static readonly TimeSpan LogIgnoreTimeSpand = TimeSpan.FromMinutes(1);

        public static CultureInfo Culture = CultureInfo.InvariantCulture;
        public static NumberStyles FloatStyle = NumberStyles.Float;
        public static NumberStyles UintStyle = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite;

        private static readonly Lazy<NameTable> NameTableField = new Lazy<NameTable>(CreateNameTable, true);

        public static NameTable NameTable { get { return NameTableField.Value; } }

        public static readonly string OnvifPrefix = "tt";
        public static readonly string OnvifNamespace = "http://www.onvif.org/ver10/schema";

        // Root element
        public static readonly string MetadataStreamElement = "MetadataStream";

        // Sub-element to MetadataStreamElement
        public static readonly string VideoAnalyticsElement = "VideoAnalytics";

        // Sub-element to VideoAnalyticsElement
        public static readonly string FrameElement = "Frame";

        // Sub-element to FrameElement
        public static readonly string UtcTimeAttribute = "UtcTime";
        public static readonly string ObjectElement = "Object";

        // Sub-elements to ObjectElement
        public static readonly string AppearanceElement = "Appearance";
        public static readonly string BehaviourElement = "Behaviour";
        public static readonly string ObjectIdAttribute = "ObjectId";

        // Sub-elements to AppearanceElement
        public static readonly string TransformationElement = "Transformation";
        public static readonly string ShapeElement = "Shape";
        public static readonly string ColorClusterElement = "ColorCluster";
        public static readonly string ClassElement = "Class";

        // Sub-elements to ClassElement
        public static readonly string ClassCandidateElement = "ClassCandidate";
        
        // Sub-elements to ClassCandidate
        public static readonly string LikelihoodElement = "Likelihood";
        public static readonly string TypeElement = "Type";

        // Sub-Element to ColorClusterElement
        public static readonly string ColorElement = "Color";

        // Sub-elements to Transformation
        public static readonly string TranslateElement = "Translate";
        public static readonly string ScaleElement = "Scale";

        // Sub-element to Vector (TranslateElement and ScaleElement)
        public static readonly string VectorXAttribute = "x";
        public static readonly string VectorYAttribute = "y";

        // Sub-element to ShapeElement
        public static readonly string BoundingBoxElement = "BoundingBox";
        public static readonly string CenterOfGravityElement = "CenterOfGravity";
        public static readonly string PolygonElement = "Polygon";

        // Sub-elements to Rectangle (BoundingBoxes)
        public static readonly string BottomAttribute = "bottom";
        public static readonly string TopAttribute = "top";
        public static readonly string RightAttribute = "right";
        public static readonly string LeftAttribute = "left";

        // Sub-elements to Color
        public static readonly string ColorXAttribute = "X";
        public static readonly string ColorYAttribute = "Y";
        public static readonly string ColorZAttribute = "Z";
        public static readonly string ColorspaceAttribute = "Colorspace";

        // Sub-element to PolygonElement
        public static readonly string PointElement = "Point";

        // Sub-elements to Behaviour
        public static readonly string BehaviourRemovedElement = "Removed";
        public static readonly string BehaviourIdleElement = "Idle";

        // Extension elements
        public static readonly string ExtensionElement = "Extension";
        public static readonly string OriginalDataElement = "OriginalData";
        public static readonly string NavigationalDataElement = "NavigationalData";
        public static readonly string VersionAttribute = "version";
        public static readonly string LatitudeElement = "Latitude";
        public static readonly string AltitudeElement = "Altitude";
        public static readonly string AzimuthElement = "Azimuth";
        public static readonly string GeodeticSystemElement = "GeodeticSystem";
        public static readonly string HorizontalAccuracyElement = "HorizontalAccuracy";
        public static readonly string LongitudeElement = "Longitude";
        public static readonly string VerticalAccuracyElement = "VerticalAccuracy";
        public static readonly string SpeedElement = "Speed";
        public static readonly string DescriptionElement = "Description";
        public static readonly string BoundingBoxAppearanceElement = "BoundingBoxAppearance";
        public static readonly string FillElement = "Fill";
        public static readonly string LineElement = "Line";
        public static readonly string ColorAttribute = "color";
        public static readonly string DisplayedThicknessInPixelsAttribute = "displayedThicknessInPixels";
        public static readonly string SizeAttribute = "size";
        public static readonly string BoldAttribute = "bold";
        public static readonly string ItalicAttribute = "italic";
        public static readonly string FontFamilyAttribute = "fontFamily";
        
        private static NameTable CreateNameTable()
        {
            var nameTable = new NameTable();

            var xmlNames =
                typeof (MetadataXml).GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(field => field.FieldType == typeof(string))
                    .Where(field => field.IsInitOnly)
                    .Select(field => (string)field.GetValue(null));

            foreach (var xmlName in xmlNames)
            {
                nameTable.Add(xmlName);
            }

            return nameTable;
        }
    }
}