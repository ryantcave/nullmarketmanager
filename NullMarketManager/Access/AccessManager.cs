using Newtonsoft.Json;
using NullMarketManager.IO;
using NullMarketManager.Models;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NullMarketManager.Access
{
    class AccessManager
    {
        public static string authCode;
        public static bool accessCodeIsValid;
        public static AuthInfo authInfo;
        public enum AccessState { READ_ACCESS_FROM_DISK, GET_AUTH, GET_ACCESS, RENEW_ACCESS, WAIT_SLEEP }
        private static AccessState m_eAccessState;

        public AccessManager()
        {
            accessCodeIsValid = false;
            m_eAccessState = AccessState.READ_ACCESS_FROM_DISK;
        }

        public AuthResult CheckAuth()
        {
            if (m_eAccessState == AccessState.READ_ACCESS_FROM_DISK)
            {
                AccessStateReadDisk();
            }
            
            if (m_eAccessState == AccessState.GET_AUTH)
            {
                ReceiveAuthorization(new[] { "http://localhost/oauth-callback/" });
            }

            if (m_eAccessState == AccessState.GET_ACCESS)
            {
                GetAccessToken();
            }

            if (m_eAccessState == AccessState.RENEW_ACCESS)
            {
                RenewAccessToken();
            }

            if (m_eAccessState == AccessState.WAIT_SLEEP)
            {
                int sleepTime = 0;
                if (authInfo.expiry_time - 60 > DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    sleepTime = (int)((authInfo.expiry_time - DateTimeOffset.Now.ToUnixTimeSeconds() - 60) * 1000);
                }

                Console.WriteLine("Renewing in " + sleepTime);

                return new AuthResult(AccessState.WAIT_SLEEP,sleepTime);

            }

            return new AuthResult(AccessState.READ_ACCESS_FROM_DISK,-1);
        }
        public void RunAccessManager()
        {
            while (true)
            {
                AccessManagerStateMachineDoWork();
            }
        }

        private void AccessManagerStateMachineDoWork()
        {
            switch (m_eAccessState)
            {
                case AccessState.READ_ACCESS_FROM_DISK:
                    AccessStateReadDisk();
                    break;
                case AccessState.GET_AUTH:
                    ReceiveAuthorization(new[] { "http://localhost/oauth-callback/" });
                    break;
                case AccessState.GET_ACCESS:
                    GetAccessToken();
                    break;
                case AccessState.WAIT_SLEEP:
                    // wait until 1 minute before access token expiry
                    int sleepTime = 0;
                    if (authInfo.expiry_time - 60 > DateTimeOffset.Now.ToUnixTimeSeconds())
                    {
                        sleepTime = (int)((authInfo.expiry_time - DateTimeOffset.Now.ToUnixTimeSeconds() - 60) * 1000);
                    }
                    Thread.Sleep(sleepTime);
                    m_eAccessState = AccessState.RENEW_ACCESS;
                    break;
                case AccessState.RENEW_ACCESS:
                    RenewAccessToken();
                    break;
                default:
                    break;
            }
        }

        private void AccessStateReadDisk()
        {
            var bSuccess = DiskManager.DeserializeAuthInfoFromDisk(ref authInfo);

            if (!bSuccess)
            {
                m_eAccessState = AccessState.GET_AUTH;
                return;
            }

            if (bSuccess && string.Equals(authInfo.refresh_token, ""))
            {
                m_eAccessState = AccessState.GET_AUTH;
                return;
            }

            if (bSuccess && DateTimeOffset.Now.ToUnixTimeSeconds() < authInfo.expiry_time)
            {
                accessCodeIsValid = true;
                m_eAccessState = AccessState.WAIT_SLEEP;
            }


            if (bSuccess && DateTimeOffset.Now.ToUnixTimeSeconds() >= authInfo.expiry_time)
            {
                m_eAccessState = AccessState.RENEW_ACCESS;
            }

        }

        private static void ReceiveAuthorization(string[] prefixes)
        {

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "https://login.eveonline.com/oauth/authorize?response_type=code&redirect_uri=http://localhost/oauth-callback&client_id=e4c1b3cd8488434cb8fec58469bec263&scope=publicData esi-calendar.respond_calendar_events.v1 esi-calendar.read_calendar_events.v1 esi-location.read_location.v1 esi-location.read_ship_type.v1 esi-mail.organize_mail.v1 esi-mail.read_mail.v1 esi-mail.send_mail.v1 esi-skills.read_skills.v1 esi-skills.read_skillqueue.v1 esi-wallet.read_character_wallet.v1 esi-wallet.read_corporation_wallet.v1 esi-search.search_structures.v1 esi-clones.read_clones.v1 esi-characters.read_contacts.v1 esi-universe.read_structures.v1 esi-bookmarks.read_character_bookmarks.v1 esi-killmails.read_killmails.v1 esi-corporations.read_corporation_membership.v1 esi-assets.read_assets.v1 esi-planets.manage_planets.v1 esi-fleets.read_fleet.v1 esi-fleets.write_fleet.v1 esi-ui.open_window.v1 esi-ui.write_waypoint.v1 esi-characters.write_contacts.v1 esi-fittings.read_fittings.v1 esi-fittings.write_fittings.v1 esi-markets.structure_markets.v1 esi-corporations.read_structures.v1 esi-characters.read_loyalty.v1 esi-characters.read_opportunities.v1 esi-characters.read_chat_channels.v1 esi-characters.read_medals.v1 esi-characters.read_standings.v1 esi-characters.read_agents_research.v1 esi-industry.read_character_jobs.v1 esi-markets.read_character_orders.v1 esi-characters.read_blueprints.v1 esi-characters.read_corporation_roles.v1 esi-location.read_online.v1 esi-contracts.read_character_contracts.v1 esi-clones.read_implants.v1 esi-characters.read_fatigue.v1 esi-killmails.read_corporation_killmails.v1 esi-corporations.track_members.v1 esi-wallet.read_corporation_wallets.v1 esi-characters.read_notifications.v1 esi-corporations.read_divisions.v1 esi-corporations.read_contacts.v1 esi-assets.read_corporation_assets.v1 esi-corporations.read_titles.v1 esi-corporations.read_blueprints.v1 esi-bookmarks.read_corporation_bookmarks.v1 esi-contracts.read_corporation_contracts.v1 esi-corporations.read_standings.v1 esi-corporations.read_starbases.v1 esi-industry.read_corporation_jobs.v1 esi-markets.read_corporation_orders.v1 esi-corporations.read_container_logs.v1 esi-industry.read_character_mining.v1 esi-industry.read_corporation_mining.v1 esi-planets.read_customs_offices.v1 esi-corporations.read_facilities.v1 esi-corporations.read_medals.v1 esi-characters.read_titles.v1 esi-alliances.read_contacts.v1 esi-characters.read_fw_stats.v1 esi-corporations.read_fw_stats.v1 esi-characterstats.read.v1",
                UseShellExecute = true
            };

            Process.Start(psi);

            if (prefixes == null || prefixes.Length == 0)
                throw new ArgumentException("Prefixes needed");

            HttpListener listener = new HttpListener();

            foreach (string s in prefixes)
            {
                listener.Prefixes.Add(s);
            }

            listener.Start();
            Console.WriteLine("Listening for callback...");

            HttpListenerContext context = listener.GetContext();
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;

            string responseString = "<html lang=\"en\" xmlns=\"http://www.w3.org/1999/xhtml\"><head>    <meta charset=\"utf-8\" />    <title>Authentication Successful</title>    <style>        p, body { color: #FFFFFF; font-family: ProximaNova-Regular, Arial }		h2 { text-align: center; margin-bottom: 10px; text-shadow: 1px 1px 2px #7F7F7F; font-size: 30px; font-family: \"ProximaNova-SemiBold\",Arial }        body { background: #000000 url(https://login.eveonline.com/Images/site-bg.jpg) no-repeat center center fixed }        #content { background-color: rgba(0, 0, 0, 0.7); padding: 30px; width: 400px; margin: auto }    </style></head><body><div id=\"content\"><h2>AUTHENTICATION SUCCESSFUL</h2><p>This browser window can now be closed.</p><script type=\"text/javascript\" language=\"JavaScript\"><!--window.close();// --></script></div></body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(responseString);

            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            authCode = request.QueryString["code"];

            output.Close();
            listener.Stop();

            m_eAccessState = AccessState.GET_ACCESS;
        }

        private static void GetAccessToken()
        {
            Console.WriteLine("AccessManager: Getting Access Token");
            var client = new RestClient("https://login.eveonline.com");
            var request = new RestRequest("/oauth/token", Method.POST);

            var body = new
            {
                grant_type = "authorization_code",
                code = authCode
            };

            var encodeBytes = System.Text.Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["ClientID"] + ":" + ConfigurationManager.AppSettings["SecretKey"]);
            string header = "Basic " + System.Convert.ToBase64String(encodeBytes);

            string json_body = JsonConvert.SerializeObject(body);
            request.AddHeader("Authorization", header);
            request.AddParameter("application/json", json_body, ParameterType.RequestBody);
            request.AddParameter("Content-Type", "application/json");
            var response = client.Post(request);
            var content = response.Content;

            authInfo = JsonConvert.DeserializeObject<AuthInfo>(content);

            m_eAccessState = AccessState.WAIT_SLEEP;
            accessCodeIsValid = true;
            authInfo.expiry_time = authInfo.expires_in + DateTimeOffset.Now.ToUnixTimeSeconds();
            DiskManager.SerializeAuthInfoToDisk(authInfo);
        }

        private static void RenewAccessToken()
        {
            Console.WriteLine("AccessManager: Renewing Access Token");
            var client = new RestClient("https://login.eveonline.com");
            var request = new RestRequest("/oauth/token", Method.POST);

            var body = new
            {
                grant_type = "refresh_token",
                refresh_token = authInfo.refresh_token
            };

            var encodeBytes = System.Text.Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["ClientID"] + ":" + ConfigurationManager.AppSettings["SecretKey"]);
            string header = "Basic " + System.Convert.ToBase64String(encodeBytes);

            string json_body = JsonConvert.SerializeObject(body);
            request.AddHeader("Authorization", header);
            request.AddParameter("application/json", json_body, ParameterType.RequestBody);
            request.AddParameter("Content-Type", "application/json");
            var response = client.Post(request);
            var content = response.Content;

            authInfo = JsonConvert.DeserializeObject<AuthInfo>(content);

            m_eAccessState = AccessState.WAIT_SLEEP;
            accessCodeIsValid = true;
            authInfo.expiry_time = authInfo.expires_in + DateTimeOffset.Now.ToUnixTimeSeconds();
            DiskManager.SerializeAuthInfoToDisk(authInfo);
        }
    }
}
