using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using VideoOS.Platform;

namespace MetadataAPI
{
    public static class ConnectionManager
    {

        //private static readonly string hostname = "http://10.1.0.21";
        private static readonly string hostname = "demo.milestonesys.com";
        private static readonly bool secureOnly = false;

        private static readonly Guid IntegrationId = new Guid("FF0B9F27-A2C2-4720-989B-159AA1597BB1");
        private const string IntegrationName = "Metadata API";
        private const string Version = "1.0";
        private const string ManufacturerName = "SGIU";

        public static bool ConnectManagementServer()
        {
            string hostManagementService = hostname;
            if (!hostManagementService.StartsWith("http://") && !hostManagementService.StartsWith("https://"))
                hostManagementService = "http://" + hostManagementService;

            Uri uri = new UriBuilder(hostManagementService).Uri;



            CredentialCache cc = VideoOS.Platform.Login.Util.BuildCredentialCache(uri, "SGIU", "Milestone1!", "Basic");
            //new NetworkCredential("[BASIC]\\SGIU", "Milestone1!")
            VideoOS.Platform.SDK.Environment.AddServer(secureOnly, uri, cc);


            try
            {
                VideoOS.Platform.SDK.Environment.Login(uri, IntegrationId, IntegrationName, Version, ManufacturerName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not logon to management server: " + ex.Message);
                Console.WriteLine("");
                Console.WriteLine("Press any key");
                Console.ReadKey();
                return false;
                throw new ApplicationException("Cannot connect");

            }

            if (EnvironmentManager.Instance.CurrentSite.ServerId.ServerType != ServerId.CorporateManagementServerType)
            {
                Console.WriteLine("{0} is not an XProtect Corporate Management Server", hostManagementService);
                Console.WriteLine("");
                Console.WriteLine("Press any key");
                Console.ReadKey();
                return false;
                throw new ApplicationException("Wrong servertype");
            }

            VideoOS.Platform.Login.LoginSettings loginSettings =
                VideoOS.Platform.Login.LoginSettingsCache.GetLoginSettings(hostManagementService);
            Console.WriteLine("... Token=" + loginSettings.Token);

            return true;
        }

    }
}
