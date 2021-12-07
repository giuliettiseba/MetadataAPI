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
        /// <param name="candidateTypes"></param>
        /// <param name="maxItems"></param>
        /// <param name="startTime"></param>
        /// <param name="direction"></param>
        /// <param name="deviceGuid"></param>
        /// <param name="uniqueValues"></param>
        /// <returns></returns>
        public IEnumerable<MetadataStream> PullMetadata(int minutes, string[] candidateTypes, int maxItems, DateTime? startTime, string direction, Guid deviceGuid, bool uniqueValues)
        {
            bool isFirst = true;

            Direction _direction = (Direction)Enum.Parse(typeof(Direction), direction);

            DateTime _startTimeUtc = (startTime == null) ? DateTime.UtcNow : ((DateTime)startTime).ToUniversalTime();

            DateTime _endTimeUtc = (_direction == Direction.Backward) ? _startTimeUtc.Add(new TimeSpan(0, -minutes, 0)) : _startTimeUtc.Add(new TimeSpan(0, minutes, 0));

            var selectedItem = SelectItem(deviceGuid);                                              // *Select metadata device* if is a camara select related metadata, if metadata just select it

            var dataSource = new MetadataPlaybackSource(selectedItem);                              // Get MetadataPlaybackSource
            dataSource.Init();                                                                      // Initialize Datasource

            if (startTime == null)                                                                 // If start time is not provided start from the end. 
                startTime = dataSource.GetEnd().DateTime;

            bool condition;                                                                         // Aux cut condition

            List<MetadataStream> metadataStreams = new List<MetadataStream>();                      // Empty list to populete with the output

            Dictionary<int, bool> objectIds = new Dictionary<int, bool>();                          // Aux, store objectId to provide unique values in the output. 
            do
            {
                MetadataPlaybackData metadataPlaybackData = MetadataFetch(ref isFirst, startTime.Value, dataSource, _direction);                    // Fetch MetadatasPlaybackData
                if (metadataPlaybackData != null)
                {
                    MetadataStream metadataStream = metadataPlaybackData.Content.GetMetadataStream();                                       // Extract metadata Object 
                    string xml = metadataPlaybackData.Content.GetMetadataString();
                    IEnumerable<Frame> frames = metadataStream.GetAllFrames();                                                              // Get Frames

                    foreach (Frame frame in frames)                                                                     // For each frame
                        foreach (OnvifObject onvifObject in frame.Objects)                                              // For each object
                            if (!uniqueValues || !objectIds.ContainsKey(onvifObject.ObjectId))                          // If unique values is true, check that the objectId is not in the aux dic
                            {
                                if (uniqueValues) objectIds.Add(onvifObject.ObjectId, true);                                              // Add objectId to dictionary
                                if (candidateTypes.FirstOrDefault() == null)                                            // If no classType has been provided
                                {
                                    metadataStreams.Add(metadataStream);                // Add metadatastream to the output list 
                                }
                                else if (onvifObject?.Appearance?.Class?.ClassCandidates != null)                               // is an Analytic object ?
                                    foreach (ClassCandidate classCandidate in onvifObject.Appearance.Class.ClassCandidates)     // For each candidate add a new metadataStream to the output (?????) TODO: this should be on the same output object
                                        if (candidateTypes.Contains(classCandidate.Type))                                       // Is the candidate in the list of passed candidates 
                                        {
                                            metadataStreams.Add(metadataStream);            // Add metadatastream to the output list 
                                        }
                            }

                    condition = CutCondition(_endTimeUtc, maxItems, metadataStreams, metadataPlaybackData, _direction);                     // Evaluate cut condition 
                }
                else break;
            }
            while (condition);

            if (_direction == Direction.Forward) metadataStreams.Reverse();

            return metadataStreams;
        }


        /// <summary>
        /// Auxiliary method to break the loop
        /// </summary>
        /// <param name="minutes"></param>
        /// <param name="maxItems"></param>
        /// <param name="metadata"></param>
        /// <param name="metadataPlaybackData"></param>
        /// <returns></returns>
        private static bool CutCondition(DateTime endTime, int maxItems, List<MetadataStream> metadata, MetadataPlaybackData metadataPlaybackData, Direction direction)
        {
            return !(metadata.Count >= maxItems) &&
                isEndTimeReached(endTime, metadataPlaybackData, direction) &&
                metadataPlaybackData.PreviousDateTime != null;
        }

        private static bool isEndTimeReached(DateTime endTime, MetadataPlaybackData metadataPlaybackData, Direction direction)
        {
            return direction == Direction.Backward ? endTime.CompareTo(metadataPlaybackData.DateTime) <= 0 : endTime.CompareTo(metadataPlaybackData.DateTime) > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="dataSource"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        private MetadataPlaybackData MetadataFetch(ref bool isFirst, DateTime dateTime, MetadataPlaybackSource dataSource, Direction direction = Direction.Backward)
        {
            try
            {
                MetadataPlaybackData metadata = null;
                if (!isFirst)
                {
                    if (dateTime == DateTime.MinValue)
                        metadata = dataSource.GetBegin();
                    else
                    {
                        switch (direction)
                        {
                            case Direction.Forward:
                                metadata = dataSource.GetNext();
                                break;
                            case Direction.Backward:
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
                else
                {
                    switch (direction)
                    {
                        case Direction.Forward:
                            metadata = dataSource.GetNext(dateTime);
                            break;
                        case Direction.Backward:
                            metadata = dataSource.GetPrevious(dateTime);
                            break;
                        default:
                            metadata = dataSource.GetNearest(dateTime);
                            break;
                    }
                    isFirst = false;
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
