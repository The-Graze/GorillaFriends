using GorillaFriends.Scripts;
using HarmonyLib;
using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaFriends.Patches
{
    [HarmonyPatch(typeof(GorillaPlayerScoreboardLine))]
    [HarmonyPatch("Start", MethodType.Normal)]
    internal class LinePatch
    {
        private static void Prefix(GorillaPlayerScoreboardLine __instance)
        {
            __instance.playerName.supportRichText = true;
            GameObject FriendButton = GameObject.Instantiate(__instance.muteButton.gameObject);
            FriendButton.transform.GetChild(0).localScale = new Vector3(0.032f, 0.032f, 1.0f);
            FriendButton.transform.GetChild(0).name = "Friend Text";
            FriendButton.transform.SetParent(__instance.transform, false);
            FriendButton.name = "FriendButton";
            FriendButton.transform.localPosition = new Vector3(18.0f, 0.0f, 0.0f);

            GorillaPlayerLineButton delme = FriendButton.GetComponent<GorillaPlayerLineButton>();

            FriendButton fb = FriendButton.AddComponent<FriendButton>();

            fb.parentLine = __instance;
            fb.offMaterial = delme.offMaterial;
            fb.onMaterial = new Material(delme.onMaterial);
            Component.Destroy(delme);
        }
    }

    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("Start", MethodType.Normal)]
    internal class VrigTextPatch
    {
        private static void Prefix(VRRig __instance)
        {
            __instance.playerText.supportRichText = true;
        }
    }

    [HarmonyPatch(typeof(GorillaScoreBoard))]
    [HarmonyPatch("Awake", MethodType.Normal)]
    internal class BoardListPatch
    {
        private static void Postfix(GorillaScoreBoard __instance)
        {
            Plugin.p_listScoreboards.Add(__instance);
        }
    }
    [HarmonyPatch(typeof(GorillaScoreBoard))]
    [HarmonyPatch("RedrawPlayerLines", MethodType.Normal)]
    internal class GorillaScoreBoardRedrawPlayerLines
    {
        private static bool Prefix(GorillaScoreBoard __instance)
        {

            __instance.lines.Sort((Comparison<GorillaPlayerScoreboardLine>)((line1, line2) => line1.playerActorNumber.CompareTo(line2.playerActorNumber)));
            __instance.boardText.text = __instance.GetBeginningString();
            __instance.buttonText.text = "";
            for (int index = 0; index < __instance.lines.Count; ++index)
            {
                __instance.lines[index].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0.0f, (float)(__instance.startingYValue - __instance.lineHeight * index), 0.0f);
                Text boardText = __instance.boardText;
                var usrid = __instance.lines[index].linePlayer.UserId;
                var txtusr = __instance.lines[index].playerVRRig.playerText;
                boardText.supportRichText = true;
                if (Plugin.IsInFriendList(usrid))
                {
                    boardText.text = boardText.text + Plugin.s_clrFriend + __instance.NormalizeName(true, __instance.lines[index].linePlayer.NickName) + "</color>";
                    txtusr.color = Plugin.p_clrFriend;
                }
                else if (Plugin.IsVerified(usrid))
                {
                    boardText.text = boardText.text + Plugin.s_clrVerified + __instance.NormalizeName(true, __instance.lines[index].linePlayer.NickName) + "</color>";
                    txtusr.color = Plugin.p_clrVerified;
                    if (__instance.lines[index].linePlayer.IsLocal) GorillaTagger.Instance.offlineVRRig.playerText.color = Plugin.p_clrVerified;
                }
                else if (!Plugin.NeedToCheckRecently(usrid) && Plugin.HasPlayedWithUsRecently(usrid) == Plugin.RecentlyPlayed.Before)
                {
                    boardText.text = boardText.text + Plugin.s_clrPlayedRecently + __instance.NormalizeName(true, __instance.lines[index].linePlayer.NickName) + "</color>";
                    txtusr.color = Plugin.p_clrPlayedRecently;
                }
                else
                {
                    boardText.text = boardText.text + "\n " + __instance.NormalizeName(true, __instance.lines[index].linePlayer.NickName);
                    txtusr.color = Color.white;
                }
                if (__instance.lines[index].linePlayer != PhotonNetwork.LocalPlayer)
                {
                    if (__instance.lines[index].reportButton.isActiveAndEnabled)
                    {
                        __instance.buttonText.text += "MUTE                                REPORT\n";
                    }
                    else
                    {
                        __instance.buttonText.text += "MUTE                HATE SPEECH    TOXICITY      CHEATING      CANCEL\n";
                    }
                }
                else
                {
                    __instance.buttonText.text += "\n";
                }
            }
            return false;
        }
    }
}
