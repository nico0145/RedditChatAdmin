using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SendBirdAPI
{
    public class Chat
    {
        public string application_id { set; get; }
        public string channel_url { set; get; }
        public string SelectedUserId { set; get; }
        public CookieAwareWebClient client { set; get; }
        private string UserID;
        public string SessionKey { set; get; }
        public string RootURL { set; get; }
        public long IndividualSearchCutout { set; get; }
        public bool LoggedIn { set; get; }
        public CookieAwareWebClient GetLoggedWebClient(string Username, string Password)
        {
            client = new CookieAwareWebClient();
            NameValueCollection oCol = new NameValueCollection();
            oCol.Add("user", Username);
            oCol.Add("passwd", Password);
            oCol.Add("api_type", "json");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Conectandose con la API de Reddit...");
            Console.ResetColor();
            client.UploadValues("https://ssl.reddit.com/api/login", oCol);
            UserID = Username;
            return client;

        }
        public string GetUserID()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Obteniendo ID de usuario...");
            Console.ResetColor();
            string sJson = client.DownloadString($"https://www.reddit.com/user/{UserID}/about.json");
            var jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<About>(sJson);
            return jRet.data.id;
        }
        public string GetSBToken()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Obteniendo Token de autenticacion de SendBird...");
            Console.ResetColor();
            string sJson = client.DownloadString("https://s.reddit.com/api/v1/sendbird/me");
            var jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<SbTokenResponse>(sJson);
            return jRet.sb_access_token;
        }
        public Chat(string mchannel_url, string mapplication_id, string UserName, string Password)
        {
            IndividualSearchCutout = 30000; //ni inocentes
            channel_url = mchannel_url;
            application_id = mapplication_id;
            //RootURL = $"https://api-{application_id}.sendbird.com"; Otro login
            RootURL = $"https://sendbirdproxy.chat.redditmedia.com";
            try
            {
                client = GetLoggedWebClient(UserName, Password);
                var UID = GetUserID();
                var Token = GetSBToken();
                SessionKey = GetSocketKey($"{RootURL.Replace("https", "wss")}/?p=JS" +
                                                                            $"&sv=3.0.69" +
                                                                            $"&ai={application_id}" +
                                                                            $"&access_token={Token}" +
                                                                            $"&user_id=t2_{UID}" +
                                                                            $"&pv=Mozilla%2F5.0%20(Windows%20NT%2010.0%3B%20Win64%3B%20x64)%20AppleWebKit%2F537.36%20(KHTML.%20like%20Gecko)%20Chrome%2F83.0.4103.97%20Safari%2F537.36").Result;
                client.UpdateToken(application_id, SessionKey);
                LoggedIn = true;
            }
            catch
            {
                LoggedIn = false;
            }
        }
        private async Task<string> GetSocketKey(string sUrl)
        {
            WebSocketClient wsc = new WebSocketClient();
            await wsc.Connect(sUrl);
            var oMessage = wsc.Messages.FirstOrDefault(x => x.Body.Contains("\"key\":"));
            if (oMessage != null)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Autenticacion exitosa guacho!");
                Console.ResetColor();
                return oMessage.Body.Split(",").FirstOrDefault(x => x.Contains("\"key\"")).Split(":")[1].Trim('"');
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Hubo un error tratando de conectarse al WebSocket {sUrl}, chequea que onda");
                Console.ResetColor();
                return "";
            }
        }
        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }
        public List<string> GetBannedUsers(int CutOut)
        {
            return GetBannedUsersObj(CutOut, "").Select(x => x.user.nickname).ToList();
        }
        public List<string> FindBannedUser(string sNickname)
        {
            return GetBannedUsersObj(IndividualSearchCutout, sNickname).Select(x => x.ToString()).ToList();
        }
        public string BanMute(bool ban, long seconds, string Description)
        {
            string sCommand = ban ? "ban" : "mute";
            try
            {
                client.UploadString($"{RootURL}/v3/group_channels/{channel_url}/{sCommand}", "POST", $"{{\"user_id\": \"{SelectedUserId}\",\"seconds\": {seconds.ToString()},\"description\": \"{Description}\"}}");
            }
            catch (Exception err)
            {
                return "Error: " + err.Message;
            }
            return "Done";
        }
        public string UnBanMute(bool ban)
        {
            string sCommand = ban ? "ban" : "mute";
            NameValueCollection oData = new NameValueCollection();
            try
            {
                client.UploadValues($"{RootURL}/v3/group_channels/{channel_url}/{sCommand}/{SelectedUserId}", "DELETE", oData);
            }
            catch (Exception err)
            {
                return "Error: " + err.Message;
            }
            return "Done";
        }
        public List<string> GetMutedUsers(int CutOut)
        {
            return GetMutedUsersObj(CutOut, "").Select(x => x.nickname).ToList();
        }
        public List<string> FindMutedUser(string sNickname)
        {
            return GetMutedUsersObj(IndividualSearchCutout, sNickname).Select(x => x.ToString()).ToList();
        }
        private List<BannedList> GetBannedUsersObj(long CutOut, string NickName)
        {
            client.UpdateToken(application_id, SessionKey);
            var sres = client.DownloadString($"{RootURL}/v3/group_channels/sendbird_group_channel_{channel_url}/ban?limit=100");
            var jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<BanList>(sres);
            List<BannedList> localBanned = new List<BannedList>();
            double DateNow = DateTimeToUnixTimestamp(DateTime.Now);
            NickName = NickName.ToLower();
            bool bFound = false;
            int SearchedUsers = 0;
            while (jRet.banned_list.Any() && localBanned.Count < CutOut && SearchedUsers < CutOut && !bFound)
            {
                if (string.IsNullOrWhiteSpace(NickName))
                {
                    var mCount = localBanned.Count;
                    localBanned.AddRange(jRet.banned_list.Where(x => x.end_at > DateNow && !localBanned.Any(y => y.user.user_id == x.user.user_id)));
                    if (mCount == localBanned.Count)
                        bFound = true;
                }
                else
                {
                    localBanned.AddRange(jRet.banned_list.Where(x => x.user.nickname.ToLower() == NickName));
                    SearchedUsers += jRet.banned_list.Count;
                    bFound = localBanned.Any();
                    if (bFound)
                    {
                        SelectedUserId = localBanned.FirstOrDefault().user.user_id;
                    }
                }
                if (!bFound)
                {
                    sres = client.DownloadString($"{RootURL}/v3/group_channels/sendbird_group_channel_{channel_url}/ban?limit=100&token={jRet.next}");
                    jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<BanList>(sres);
                }
            }
            return localBanned;
        }
        private List<MutedList> GetMutedUsersObj(long CutOut, string NickName)
        {
            client.UpdateToken(application_id, SessionKey);
            var sres = client.DownloadString($"{RootURL}/v3/group_channels/sendbird_group_channel_{channel_url}/mute?limit=100");
            var jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<MutedListResult>(sres);
            List<MutedList> localBanned = new List<MutedList>();
            double DateNow = DateTimeToUnixTimestamp(DateTime.Now);
            NickName = NickName.ToLower();
            bool bFound = false;
            int SearchedUsers = 0;
            while (jRet.muted_list.Any() && localBanned.Count < CutOut && SearchedUsers < CutOut && !bFound)
            {
                if (string.IsNullOrWhiteSpace(NickName))
                {
                    var mCount = localBanned.Count;
                    localBanned.AddRange(jRet.muted_list.Where(x => x.end_at > DateNow && !localBanned.Any(y => y.user_id == x.user_id)));
                    if (mCount == localBanned.Count)
                        bFound = true;
                }
                else
                {
                    localBanned.AddRange(jRet.muted_list.Where(x => x.nickname.ToLower() == NickName));
                    SearchedUsers += jRet.muted_list.Count;
                    bFound = localBanned.Any();
                    if (bFound)
                    {
                        SelectedUserId = localBanned.FirstOrDefault().user_id;
                    }
                }
                if (!bFound)
                {
                    sres = client.DownloadString($"{RootURL}/v3/group_channels/sendbird_group_channel_{channel_url}/mute?limit=100&token={jRet.next}");
                    jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<MutedListResult>(sres);
                }
            }
            return localBanned;
        }
        public Member GetUserObj(string sNickname)
        {
            client.UpdateToken(application_id, SessionKey);
            var sres = client.DownloadString($"{RootURL}/v3/group_channels/{channel_url}/members?nickname_startswith={sNickname}");
            var jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<SearchMemberResult>(sres);
            return jRet.members.FirstOrDefault();
        }
        public string GetUser(string sNickname)
        {
            var oUser = GetUserObj(sNickname);
            if (oUser != null)
            {
                SelectedUserId = oUser.user_id;
                return oUser.ToString();
            }
            return "";
        }
    }
}
