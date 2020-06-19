using SendBirdAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ChatAdminConsole
{
    static class Program
    {
        const string ConfigFileName = "jsconfig.json";
        const string EncryptionKey = "wUu0Hnuzuj6BnMYIOOW5XuVL9H5WO9SHGWtH0W5A6TdjvFtHFnhzWjvaLce6s3VRKw5jIllRM5c0pJowpGxsf0H54cF8SLgyHSXCiIMGDTzsqMEDikufiSZtHKF1c4DawuK1dd1aW47mX8uCLSh6nzX3japXKH6fwrV1OlQImZiryduiU03QI4FgmJVAuWqvAwVJos2h"; //lul i'm so random!
        public static Tuple<string, string> SeparateFirst(string sIn)
        {
            string s1 = sIn.Split(' ')[0];
            string s2 = sIn.Substring(s1.Length).TrimStart(' ');
            return new Tuple<string, string>(s1, s2);
        }
        private static void ValidateList(bool Ban, string sParam, Chat oChat)
        {
            string sCommand = Ban ? "ban" : "mute";
            if (int.TryParse(sParam, out int CutOut))
            {
                List<string> lstRet;
                if (Ban)
                    lstRet = oChat.GetBannedUsers(CutOut);
                else
                    lstRet = oChat.GetMutedUsers(CutOut);
                lstRet = lstRet.Where(x => x != "[deleted]").OrderBy(x => x).ToList();
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(string.Join(',', lstRet));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Faltan parametros, {sCommand}list <ultimos X {(sCommand == "mute" ? sCommand : sCommand + "ne")}ados, Int>");
                Console.ResetColor();
            }
        }
        private static void ValidateUserQuery(bool? ban, string sParam, Chat oChat)
        {
            string sCommand = "";
            if (ban == true)
                sCommand = "banned";
            if (ban == false)
                sCommand = "muted";
            if (string.IsNullOrEmpty(sParam))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Faltan parametros, {sCommand}user <nickname>");
                Console.ResetColor();
            }
            else
            {
                List<string> lstResults;
                switch (ban)
                {
                    case true:
                        lstResults = oChat.FindBannedUser(sParam);
                        break;
                    case false:
                        lstResults = oChat.FindMutedUser(sParam);
                        break;
                    default:
                        lstResults = new List<string>();
                        lstResults.Add(oChat.GetUser(sParam));
                        break;

                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                if (lstResults.Any() && !string.IsNullOrWhiteSpace(lstResults.FirstOrDefault()))
                    lstResults.ForEach(x => Console.WriteLine(x));
                else
                    Console.WriteLine("Sin Resultados");
                Console.ResetColor();
            }
        }
        static void ValidateBanMuted(bool ban, string sParam, Chat oChat)
        {
            string sCommand = ban ? "ban" : "mute";
            if (string.IsNullOrEmpty(sParam))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Faltan parametros, {sCommand} <nickname> <segundos> <descripcion>");
                Console.ResetColor();
            }
            else
            {
                Tuple<string, string> x = SeparateFirst(sParam);
                string sNickname = x.Item1;
                x = SeparateFirst(x.Item2);
                string sSecs = x.Item1;
                string sDesc = x.Item2;
                if (long.TryParse(sSecs, out long iSecs))
                {
                    ValidateUserQuery(null, sNickname, oChat);
                    string sQuestion = $"{(sCommand == "mute" ? sCommand : sCommand + "ne")}ar a este usuario? <s/n>";
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(sQuestion);
                    Console.ResetColor();
                    if (GetYN(sQuestion))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(oChat.BanMute(ban, iSecs, sDesc));
                        Console.ResetColor();
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Faltan parametros, {sCommand} <nickname> <segundos> <descripcion>");
                    Console.ResetColor();
                }
            }
        }
        static void ValidateUnBanMuted(bool ban, string sParam, Chat oChat)
        {
            string sCommand = ban ? "ban" : "mute";
            if (string.IsNullOrEmpty(sParam))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Faltan parametros, {sCommand} <nickname>");
                Console.ResetColor();
            }
            else
            {
                ValidateUserQuery(ban, sParam, oChat);
                string sQuestion = $"des-{(sCommand == "mute" ? sCommand : sCommand + "ne")}ar a este usuario? <s/n>";
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(sQuestion);
                Console.ResetColor();
                if (GetYN(sQuestion))
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(oChat.UnBanMute(ban));
                    Console.ResetColor();
                }
            }
        }
        static string GetValue(string sParam, string sDefault, bool isPassword = false)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{sParam} [{(isPassword ? new string('*', sDefault.Length) : sDefault)}]:");
            Console.ResetColor();
            string sRetu;
            if (isPassword)
                sRetu = GetPassword();
            else
                sRetu = Console.ReadLine();
            return string.IsNullOrWhiteSpace(sRetu) ? sDefault : sRetu;
        }
        static string GetPassword()
        {
            StringBuilder pass = new StringBuilder();
            do
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    pass.Append(key.KeyChar);
                    Console.Write("*");
                }
                else
                {
                    if (key.Key == ConsoleKey.Backspace && pass.Length > 0)
                    {
                        pass.Remove(pass.Length - 1, 1);
                        Console.Write("\b \b");
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            } while (true);
            Console.WriteLine();
            return pass.ToString();
        }
        static void GetLoginData(JsonConfig jRet, bool DecryptPwd)
        {
            jRet.Channel = GetValue("Canal", jRet.Channel);
            jRet.AppId = GetValue("App Id (no tocar!)", jRet.AppId);
            jRet.User = GetValue("Usuario", jRet.User);
            jRet.Password = GetValue("Contraseña", DecryptPwd ? DecryptData(jRet.Password, EncryptionKey) : jRet.Password, true);
        }
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Reddit group chat manager v1.0");
            Console.ResetColor();
            JsonConfig jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonConfig>(System.IO.File.ReadAllText(ConfigFileName));
            GetLoginData(jRet, true);
            Chat oChat = new Chat(jRet.Channel, jRet.AppId, jRet.User, jRet.Password);
            while (!oChat.LoggedIn)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error al tratar de conectar, intenta de nuevo");
                Console.ResetColor();
                GetLoginData(jRet, false);
                oChat = new Chat(jRet.Channel, jRet.AppId, jRet.User, jRet.Password);
            }
            string sIn = "";
            while (sIn != "exit")
            {
                sIn = Console.ReadLine();
                var x = SeparateFirst(sIn);
                string sCommand = x.Item1;
                string sParam = x.Item2;
                switch (sCommand.ToLower())
                {
                    case "banlist":
                        ValidateList(true, sParam, oChat);
                        break;
                    case "user":
                        ValidateUserQuery(null, sParam, oChat);
                        break;
                    case "banneduser":
                        ValidateUserQuery(true, sParam, oChat);
                        break;
                    case "mutelist":
                        ValidateList(false, sParam, oChat);
                        break;
                    case "muteduser":
                        ValidateUserQuery(false, sParam, oChat);
                        break;
                    case "ban":
                        ValidateBanMuted(true, sParam, oChat);
                        break;
                    case "mute":
                        ValidateBanMuted(false, sParam, oChat);
                        break;
                    case "unban":
                        ValidateUnBanMuted(true, sParam, oChat);
                        break;
                    case "unmute":
                        ValidateUnBanMuted(false, sParam, oChat);
                        break;
                    case "help":
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("ban <nickname> <segundos> <descripcion>");
                        Console.WriteLine("banlist <maxresults>");
                        Console.WriteLine("banneduser <nickname>");
                        Console.WriteLine("exit - salir");
                        Console.WriteLine("help - muestra esto");
                        Console.WriteLine("mute <nickname> <segundos> <descripcion>");
                        Console.WriteLine("mutelist <maxresults>");
                        Console.WriteLine("muteduser <nickname>");
                        Console.WriteLine("unban <nickname>");
                        Console.WriteLine("unmute <nickname>");
                        Console.WriteLine("user <nickname>");
                        Console.ResetColor();
                        break;
                    case "exit":
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Chau chau!");
                        Console.ResetColor();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine("comando desconocido, usa help para ayuda");
                        Console.ResetColor();
                        break;
                }
            }
            jRet.Password = EncryptData(jRet.Password, EncryptionKey);
            System.IO.File.WriteAllText(ConfigFileName, Newtonsoft.Json.JsonConvert.SerializeObject(jRet));
        }
        public static bool GetYN(string sQuestion)
        {
            string sAns = Console.ReadLine();
            while (sAns.ToLower() != "s" && sAns.ToLower() != "n")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(sQuestion);
                Console.ResetColor();
                sAns = Console.ReadLine();
            }
            return sAns == "s";
        }

        public static string EncryptData(string textData, string Encryptionkey)
        {
            RijndaelManaged objrij = new RijndaelManaged();
            //set the mode for operation of the algorithm
            objrij.Mode = CipherMode.CBC;
            //set the padding mode used in the algorithm.
            objrij.Padding = PaddingMode.PKCS7;
            //set the size, in bits, for the secret key.
            objrij.KeySize = 0x80;
            //set the block size in bits for the cryptographic operation.
            objrij.BlockSize = 0x80;
            //set the symmetric key that is used for encryption & decryption.
            byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
            //set the initialization vector (IV) for the symmetric algorithm
            byte[] EncryptionkeyBytes = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            int len = passBytes.Length;
            if (len > EncryptionkeyBytes.Length)
            {
                len = EncryptionkeyBytes.Length;
            }
            Array.Copy(passBytes, EncryptionkeyBytes, len);
            objrij.Key = EncryptionkeyBytes;
            objrij.IV = EncryptionkeyBytes;
            //Creates symmetric AES object with the current key and initialization vector IV.
            ICryptoTransform objtransform = objrij.CreateEncryptor();
            byte[] textDataByte = Encoding.UTF8.GetBytes(textData);
            //Final transform the test string.
            return Convert.ToBase64String(objtransform.TransformFinalBlock(textDataByte, 0, textDataByte.Length));
        }
        public static string DecryptData(string EncryptedText, string Encryptionkey)
        {
            RijndaelManaged objrij = new RijndaelManaged();
            objrij.Mode = CipherMode.CBC;
            objrij.Padding = PaddingMode.PKCS7;
            objrij.KeySize = 0x80;
            objrij.BlockSize = 0x80;
            byte[] encryptedTextByte = Convert.FromBase64String(EncryptedText);
            byte[] passBytes = Encoding.UTF8.GetBytes(Encryptionkey);
            byte[] EncryptionkeyBytes = new byte[0x10];
            int len = passBytes.Length;
            if (len > EncryptionkeyBytes.Length)
            {
                len = EncryptionkeyBytes.Length;
            }
            Array.Copy(passBytes, EncryptionkeyBytes, len);
            objrij.Key = EncryptionkeyBytes;
            objrij.IV = EncryptionkeyBytes;
            byte[] TextByte = objrij.CreateDecryptor().TransformFinalBlock(encryptedTextByte, 0, encryptedTextByte.Length);
            return Encoding.UTF8.GetString(TextByte);  //it will return readable string
        }
    }
    public class JsonConfig
    {
        public string AppId { get; set; }
        public string Channel { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
    }
}
