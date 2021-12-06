using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoOS.Platform;
using VideoOS.Platform.Data;
using VideoOS.Platform.Metadata;
using static MetadataAPI.enums;
using static MetadataAPI.MetadataController;

namespace MetadataAPI
{
    internal class MetadataWorker
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="minutes"></param>
        /// <param name="candidateType"></param>
        /// <param name="maxItems"></param>
        /// <param name="start_time"></param>
        /// <param name="direction"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public IEnumerable<MetadataStream> PullMetadata(int minutes, string[] candidateTypes, int maxItems, DateTime? start_time, string direction, Guid guid, bool uniqueValues)
        {
            Direction _direction = Direction.Near;      // Find the first transverse the rest with GetPrev and GetNext
            Direction __direction = (Direction)Enum.Parse(typeof(Direction), direction);

            var selectedItem = SelectItem(guid);                                                    // *Select metadata device*

            var dataSource = new MetadataPlaybackSource(selectedItem);                              // Get MetadataPlaybackSource
            dataSource.Init();                                                                      // Initialize Datasource

            if (start_time == null)
                start_time = dataSource.GetEnd().DateTime;

            bool condition;                             // Aux cut condition

            List<MetadataStream> metadataStreams = new List<MetadataStream>();

            Dictionary<int, bool> objectIds = new Dictionary<int, bool>();
            do
            {
                MetadataPlaybackData metadataPlaybackData = MetadataFetch(start_time.Value, dataSource, _direction);                                          // Fetch MetadatasPlaybackData
                MetadataStream metadataStream = metadataPlaybackData.Content.GetMetadataStream();                                                           // Extract metadata Object 
                IEnumerable<Frame> frames = metadataStream.GetAllFrames();

                foreach (Frame frame in frames)
                    foreach (OnvifObject onvifObject in frame.Objects)
                    {
                        if (!uniqueValues || !objectIds.ContainsKey(onvifObject.ObjectId))// UNIQUE OBJECT IDS
                        {

                            if (candidateTypes.FirstOrDefault() == "All")
                            {
                                metadataStreams.Add(metadataStream);
                                objectIds.Add(onvifObject.ObjectId, true);
                            }
                            else if (onvifObject?.Appearance?.Class?.ClassCandidates != null)
                                foreach (ClassCandidate classCandidate in onvifObject.Appearance.Class.ClassCandidates)
                                    if (candidateTypes.Contains(classCandidate.Type))
                                    {
                                        metadataStreams.Add(metadataStream);
                                        objectIds.Add(onvifObject.ObjectId, true);
                                    }
                        } 

                    }
                _direction = __direction;                                                                     // Set direction // TODO: Improve
                condition = CutCondition(minutes, maxItems, metadataStreams, metadataPlaybackData);                  // Evaluate cut condition 
            }
            while (condition);

            return metadataStreams;
        }


        /// <summary>
        /// Auxiliary method to break loop
        /// </summary>
        /// <param name="minutes"></param>
        /// <param name="maxItems"></param>
        /// <param name="metadata"></param>
        /// <param name="metadataPlaybackData"></param>
        /// <returns></returns>
        private static bool CutCondition(int minutes, int maxItems, List<MetadataStream> metadata, MetadataPlaybackData metadataPlaybackData)
        {
            return !(metadata.Count >= maxItems) &&
                !(DateTime.Now.Add(new TimeSpan(0, -minutes, 0)).CompareTo(metadataPlaybackData.DateTime) > 0) &&
                metadataPlaybackData.PreviousDateTime != null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="dataSource"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private MetadataPlaybackData MetadataFetch(DateTime dateTime, MetadataPlaybackSource dataSource, Direction direction = Direction.Near)
        {
            try
            {
                MetadataPlaybackData metadata = null;

                if (dateTime == DateTime.MinValue) metadata = dataSource.GetBegin();
                else
                {
                    switch (direction)
                    {
                        case Direction.Next:
                            metadata = dataSource.GetNext();
                            break;
                        case Direction.Prev:
                            metadata = dataSource.GetPrevious();
                            break;
                        default:
                            metadata = dataSource.GetNearest(dateTime);
                            break;
                    }
                    if (metadata != null)
                    {
                        return metadata;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return null;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        private Item SelectItem(Guid guid)
        {
            Item serverItem = VideoOS.Platform.Configuration.Instance.GetItem(EnvironmentManager.Instance.CurrentSite);
            Item item = VideoOS.Platform.Configuration.Instance.GetItem(guid, Kind.Camera);
            if (item == null) item = VideoOS.Platform.Configuration.Instance.GetItem(guid, Kind.Metadata);

            if (item.FQID.Kind == Kind.Camera)
            {
                var related = item.GetRelated();
                return related.Find(x => x.FQID.Kind == Kind.Metadata);
            }
            else if (item.FQID.Kind == Kind.Metadata)
            {
                return item;

            }
            return null;
        }

    }
}
