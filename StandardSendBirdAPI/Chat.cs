using ChannelInfo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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
        readonly string UserID;
        public string SessionKey { set; get; }
        public string RootURL { set; get; }
        public long IndividualSearchCutout { set; get; }
        public bool LoggedIn { set; get; }
        public string Token { set; get; }
        public Channels Channels { set; get; }
        public ConsoleLines oConsole { set; get; }
        public Chat(string sToken, string mapplication_id, string sUID)
        {
            oConsole = new ConsoleLines();
            IndividualSearchCutout = 30000; //ni inocentes
            application_id = mapplication_id;
            Token = sToken;
            UserID = $"t2_{sUID}";
            //RootURL = $"https://api-{application_id}.sendbird.com"; Otro login
            RootURL = $"https://sendbirdproxy.chat.redditmedia.com";
        }
        public async Task Connect()
        {
            try
            {
                SessionKey = await GetSocketKey($"{RootURL.Replace("https", "wss")}/?p=JS" +
                                                                            $"&sv=3.0.69" +
                                                                            $"&ai={application_id}" +
                                                                            $"&access_token={Token}" +
                                                                            $"&user_id={UserID}" +
                                                                            $"&pv=Mozilla%2F5.0%20(Windows%20NT%2010.0%3B%20Win64%3B%20x64)%20AppleWebKit%2F537.36%20(KHTML.%20like%20Gecko)%20Chrome%2F83.0.4103.97%20Safari%2F537.36");
                client = new CookieAwareWebClient();
                client.DownloadString("https://reddit.com");
                client.UpdateToken(application_id, SessionKey);
                SelectedUserId = UserID;
                Channels = GetChannelInfo();
                LoggedIn = true;
            }
            catch
            {
                LoggedIn = false;
                throw;
            }
        }
        private async Task<string> GetSocketKey(string sUrl)
        {
            WebSocketClient wsc = new WebSocketClient();
            await wsc.Connect(sUrl);
            var oMessage = wsc.Messages.FirstOrDefault(x => x.Body.Contains("\"key\":"));
            if (oMessage != null)
            {
                oConsole.AddLine("Autenticacion exitosa guacho!", ConsoleColor.Green);
                return oMessage.Body.Split(',').FirstOrDefault(x => x.Contains("\"key\"")).Split(':')[1].Trim('"');
            }
            else
            {
                oConsole.AddLine($"Hubo un error tratando de conectarse al WebSocket {sUrl}, chequea que onda", ConsoleColor.Red, true);
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
        public Channels GetChannelInfo()
        {
            string sRet = client.DownloadString($"{RootURL}/v3/users/{UserID}/my_group_channels?token=&limit=10&show_member=false&show_read_receipt=false&show_empty=true&order=latest_last_message&member_state_filter=joined_only&super_mode=super&public_mode=all");
            return JsonConvert.DeserializeObject<Channels>(sRet);
        }
        public string FreezeChannel()
        {
            return FreezeUnfreeze(true);
        }
        public string UnfreezeChannel()
        {
            return FreezeUnfreeze(false);
        }
        public string FreezeUnfreeze(bool bFreeze)
        {
            try
            {
                client.UploadString($"{RootURL}/v3/group_channels/{channel_url}/freeze", "POST", $"{{\"freeze\": {bFreeze.ToString().ToLower()}}}");
                return "Done";
            }
            catch (Exception err)
            {
                oConsole.AddLine($"Hubo un error tratando de congelar/descongelar el canal, chequea que onda {Environment.NewLine}{err.Message}", ConsoleColor.Red, true);
            }
            return "";
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
            var sres = client.DownloadString($"{RootURL}/v3/group_channels/{channel_url}/ban?limit=100");
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
                    sres = client.DownloadString($"{RootURL}/v3/group_channels/{channel_url}/ban?limit=100&token={jRet.next}");
                    jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<BanList>(sres);
                }
            }
            return localBanned;
        }
        private List<MutedList> GetMutedUsersObj(long CutOut, string NickName)
        {
            client.UpdateToken(application_id, SessionKey);
            var sres = client.DownloadString($"{RootURL}/v3/group_channels/{channel_url}/mute?limit=100");
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
                    sres = client.DownloadString($"{RootURL}/v3/group_channels/{channel_url}/mute?limit=100&token={jRet.next}");
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
    public class ConsoleLine
    {
        public ConsoleLine(string sText, ConsoleColor iColor, bool bError)
        {
            Text = sText;
            ConsoleColor = (int)iColor;
            IsError = bError;
        }
        public string Text { set; get; }
        public int ConsoleColor { set; get; }
        public bool IsError { set; get; }
    }
    public class ConsoleLines : List<ConsoleLine>
    {
        public void AddLine(string sText, ConsoleColor iColor, bool bError = false)
        {
            Add(new ConsoleLine(sText, iColor, bError));
        }
    }
}
