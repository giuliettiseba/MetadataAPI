using Newtonsoft.Json;
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
       
        public IHttpActionResult GetAllMetadata(string deviceGuid, DateTime? startTime, string classFilter = null, int timeInterval = 24 * 60 * 7, int maxItems = 10, string direction = "Prev", bool uniqueValues = true)
        {
            MetadataWorker metadataWorker = new MetadataWorker();
            string[] _types = classFilter != null? JsonConvert.DeserializeObject<string[]>(classFilter): null;
            IEnumerable<MetadataStream> onvifObjects = metadataWorker.PullMetadata(timeInterval, _types, maxItems, startTime, direction, new Guid(deviceGuid), uniqueValues); 
            return Ok(onvifObjects);
        }
    }
}
