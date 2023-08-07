using BepInEx;
using BepInEx.Configuration;
using GorillaFriends.Scripts;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using UnityEngine;

namespace GorillaFriends
{
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public enum RecentlyPlayed : byte
        {
            None = 0,
            Before = 1,
            Now = 2,
        }
        internal static Plugin p_hInstance;
        internal static List<string> p_listVerifiedUserIds = new List<string>();
        internal static List<string> p_listCurrentSessionFriends = new List<string>();
        internal static List<string> p_listCurrentSessionRecentlyChecked = new List<string>();
        internal static List<GorillaScoreBoard> p_listScoreboards = new List<GorillaScoreBoard>();
        internal static void Log(string msg) => p_hInstance.Logger.LogMessage(msg);
        public static Color p_clrFriend { get; internal set; } = new Color(0.8f, 0.5f, 0.9f, 1.0f);
        internal static string s_clrFriend;
        public static Color p_clrVerified { get; internal set; } = new Color(0.5f, 1.0f, 0.5f, 1.0f);
        internal static string s_clrVerified;
        public static Color p_clrPlayedRecently { get; internal set; } = new Color(1.0f, 0.67f, 0.67f, 1.0f);
        internal static string s_clrPlayedRecently;
        // This is a little settings for us
        // In case our game froze for a second or more
        internal static byte moreTimeIfWeLagging = 5;
        internal static int howMuchSecondsIsRecently = 259200;

        public static string ByteArrayToHexCode(byte[] arr)
        {
            StringBuilder hex = new StringBuilder(arr.Length * 2);
            foreach (byte b in arr)
                hex.AppendFormat("{0:X2}", b);
            return hex.ToString();
        }
        public static bool IsVerified(string userId)
        {
            foreach (string s in p_listVerifiedUserIds)
            {
                if (s == userId) return true;
            }
            return false;
        }
        public static bool IsFriend(string userId)
        {
            return (PlayerPrefs.GetInt(userId + "_friend", 0) != 0);
        }
        public static bool IsInFriendList(string userId)
        {
            foreach (string s in p_listCurrentSessionFriends)
            {
                if (s == userId) return true;
            }
            return false;
        }
        public static bool NeedToCheckRecently(string userId)
        {
            foreach (string s in p_listCurrentSessionRecentlyChecked)
            {
                if (s == userId) return false;
            }
            return true;
        }
        public static RecentlyPlayed HasPlayedWithUsRecently(string userId)
        {
            long time = long.Parse(PlayerPrefs.GetString(userId + "_played", "0"));
            long curTime = ((DateTimeOffset)DateTime.Now).ToUnixTimeSeconds();
            if (time == 0) return RecentlyPlayed.None;
            if (time > curTime - moreTimeIfWeLagging && time <= curTime) return RecentlyPlayed.Now;
            p_listCurrentSessionRecentlyChecked.Add(userId);
            return ((time + howMuchSecondsIsRecently) > curTime) ? RecentlyPlayed.Before : RecentlyPlayed.None;
        }

        void Start()
        {
            HarmonyPatches.ApplyHarmonyPatches();
            var cfg = new ConfigFile(Path.Combine(Paths.ConfigPath, "GorillaFriends.cfg"), true);
            moreTimeIfWeLagging = cfg.Bind("Timings", "MoreTimeOnLag", (byte)5, "This is a little settings for us in case our game froze for a second or more").Value;
            howMuchSecondsIsRecently = cfg.Bind("Timings", "RecentlySeconds", 259200, "How much is \"recently\"?").Value;
            if (howMuchSecondsIsRecently < moreTimeIfWeLagging) howMuchSecondsIsRecently = moreTimeIfWeLagging;
            p_clrPlayedRecently = cfg.Bind("Colors", "RecentlyPlayedWith", p_clrPlayedRecently, "Color of \"Recently played with ...\"").Value;
            p_clrFriend = cfg.Bind("Colors", "Friend", p_clrFriend, "Color of FRIEND!").Value;

            byte[] clrizer = { (byte)(p_clrFriend.r * 255), (byte)(p_clrFriend.g * 255), (byte)(p_clrFriend.b * 255) };
            s_clrFriend = "\n <color=#" + ByteArrayToHexCode(clrizer) + ">";

            clrizer[0] = (byte)(p_clrVerified.r * 255); clrizer[1] = (byte)(p_clrVerified.g * 255); clrizer[2] = (byte)(p_clrVerified.b * 255);
            s_clrVerified = "\n <color=#" + ByteArrayToHexCode(clrizer) + ">";

            clrizer[0] = (byte)(p_clrPlayedRecently.r * 255); clrizer[1] = (byte)(p_clrPlayedRecently.g * 255); clrizer[2] = (byte)(p_clrPlayedRecently.b * 255);
            s_clrPlayedRecently = "\n <color=#" + ByteArrayToHexCode(clrizer) + ">";
            WebVerified.LoadListOfVerified();
            p_hInstance = this;
        }

        public void UpdateBoards()
        {
            foreach (GorillaScoreBoard sb in p_listScoreboards)
            {
                sb.RedrawPlayerLines();
            }
        }
        void Update()
        {
            if (!PhotonNetwork.InRoom && p_listScoreboards.Count > 0) p_listScoreboards.Clear();
        }
    }
    class WebVerified
    {
        public const string p_szURL = "https://raw.githubusercontent.com/The-Graze/GorillaFriends/main/gorillas.verified";
        async public static void LoadListOfVerified()
        {
            HttpClient client = new HttpClient();
            string result = await client.GetStringAsync(p_szURL);
            using (StringReader reader = new StringReader(result))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Plugin.p_listVerifiedUserIds.Add(line);
                }
            }
        }
    }
}