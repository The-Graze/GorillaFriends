using Photon.Pun;
using System;
using UnityEngine;
using UnityEngine.UI;   

namespace GorillaFriends.Scripts
{
    public class FriendButton : MonoBehaviour
    {
        public GorillaPlayerScoreboardLine parentLine = null;
        public string offText = "ADD\nFRIEND";
        public string onText = "FRIEND!";
        public Material offMaterial;
        public Material onMaterial;
        private bool initialized = false;
        private float nextUpdate = 0.0f;
        private static float nextTouch = 0.0f;
        private Renderer rend;
        private bool verified;

        public static bool IsVerified(string userId)
        {
            foreach (string s in Plugin.p_listVerifiedUserIds)
            {
                if (s == userId) return true;
            }
            return false;
        }

        public void Start()
        { 
            if(parentLine.linePlayer == PhotonNetwork.LocalPlayer)
            { 
                if (IsVerified(PhotonNetwork.LocalPlayer.UserId))
                {
                    parentLine.playerName.color = Plugin.p_clrVerified;
                    parentLine.playerVRRig.playerText.color = Plugin.p_clrVerified;
                }
                gameObject.SetActive(false);
            }
            else
            {
                Init();
            }
        }

        void LateUpdate()
        {
            if (Plugin.IsFriend(parentLine.linePlayer.UserId))
            {
                rend.material = onMaterial;
                parentLine.playerVRRig.playerText.color = Plugin.p_clrFriend;
                transform.GetChild(0).GetComponent<Text>().text = onText;
            }
            else
            {
                rend.material = offMaterial;
                parentLine.playerVRRig.playerText.color = Color.white;
                transform.GetChild(0).GetComponent<Text>().text = offText;
            }
        }

        private void Init()
        {
            onMaterial.color = Plugin.p_clrFriend;
            transform.GetChild(0).gameObject.SetActive(true);
            rend = gameObject.GetComponent<Renderer>();
            initialized = true;
        }
        private void OnTriggerEnter(Collider collider)
        {
            GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
            ToggleFriendship();
            if (component != null)
            {
                GorillaTagger.Instance.offlineVRRig.PlayHandTapLocal(67, component.isLeftHand, 0.05f);
                GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
            }

        }
        void ToggleFriendship()
        {
            if (!Plugin.IsFriend(parentLine.linePlayer.UserId))
            {
                PlayerPrefs.SetInt(parentLine.linePlayer.UserId + "_friend", 1);
                Plugin.p_listCurrentSessionFriends.Add(parentLine.linePlayer.UserId);
                Plugin.p_hInstance.UpdateBoards();
                return;
            }
            if (Plugin.IsFriend(parentLine.linePlayer.UserId))
            {
                PlayerPrefs.DeleteKey(parentLine.linePlayer.UserId + "_friend");
                Plugin.p_listCurrentSessionFriends.Remove(parentLine.linePlayer.UserId);
                Plugin.p_hInstance.UpdateBoards();
                return;
            }
        }
    }
}