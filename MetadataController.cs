using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Web.Http;
using System.Xml;
using VideoOS.Platform;
using VideoOS.Platform.ConfigurationItems;
using VideoOS.Platform.Data;
using VideoOS.Platform.Messaging;
using VideoOS.Platform.Metadata;
using static MetadataAPI.enums;

namespace MetadataAPI
{

    public partial class MetadataController : ApiController
    {
        /// <summary>
        /// Entry point 
        /// </summary>
        /// <param name="guid">Canera GUID </param>
        /// <param name="type">Onvif Metadata Analytic Class Type</param>
        /// <param name="timeInterval">Amount of time to go back / foward of start time</param>
        /// <param name="maxItems">Item Cap</param>
        /// <param name="start_time">Start date, the first metadata is pulled by this time</param>
        /// <param name="direction">Search direction, Prev or Forward</param>
        /// <returns>
        /// [{"DateTime":,
        /// "PreviousDateTime":,
        /// "NextDateTime":,
        /// "Candidates": [{"Key":"Value"}]}]
        /// </returns>
        public IHttpActionResult GetAllMetadata(string guid, DateTime? start_time, string type = "All", int timeInterval = 24 * 60 * 7, int maxItems = 10, string direction = "Prev", bool uniqueValues = true)
        {
            // Parse strings 
            
            var _guid = new Guid(guid);         

            CandidatesType _type;


            //switch (type)
            //{
            //    case "All":
            //        _type = CandidatesType.All;
            //        break;
            //    case "Human":
            //        _type = CandidatesType.Human;
            //        break;
            //    case "Car":
            //        _type = CandidatesType.Car;
            //        break;
            //    case "Animal":
            //        _type = CandidatesType.Animal;
            //        break;
            //    default:
            //        _type = CandidatesType.All;
            //        break;
            //}

            string[] _types = new string[] { type };

            MetadataWorker metadataWorker = new MetadataWorker();
            IEnumerable<MetadataStream> onvifObjects = metadataWorker.PullMetadata(timeInterval, _types, maxItems, start_time, direction, _guid, uniqueValues); // Get Metadata 
            return Ok(onvifObjects);
        }
    }
}
