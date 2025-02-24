using HarmonyLib;
using Newtonsoft.Json;
using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using V2Unity.Model;

namespace V2Unity.WebHostPatch
{
    [HarmonyPatch(typeof(PhotonManager), "PopulateUserList")]
    public class Patch_PhotonManager_PopulateUserList
    {
        public class PopulateUserListEnumerable : IEnumerable
        {
            private PhotonManager __instance;
            private bool friendsOnly;
            private int userListId;

            public PopulateUserListEnumerable(PhotonManager __instance, bool friendsOnly)
            {
                this.__instance = __instance;
                this.friendsOnly = friendsOnly;
            }

            public IEnumerator GetEnumerator()
            {
                GameManager.instance.LoadMini();
                
                // Get all the users
                using (var www = UnityWebRequest.Get(Plugin.URL + "/v2unity/users"))
                {
                    www.SendWebRequest();
                    while (!www.isDone)
                    {
                        yield return null;
                    }

                    Plugin.Log.LogDebug(www.responseCode + ": " + www.downloadHandler.text);

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        List<User> users = (JsonConvert.DeserializeObject<List<User>>(www.downloadHandler.text) ?? new List<User>());
                        
                        // Convert the users to just the usernames
                        var userList = users.Select(user => user.Name).ToArray();                        

                        List<string> friendsUserIds = new List<string>();
                        foreach (var username in userList)
                        {
                            if (PlayerPrefs.GetInt("FRIEND_" + username, 0) == 1)
                            {
                                friendsUserIds.Add(username);
                            }
                        }

                        // Set the value of 'friendsUserIds' on the instance.
                        var instanceType = __instance.GetType();
                        instanceType.GetField("friendsUserIds", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, friendsUserIds);

                        if (friendsOnly)
                        {
                            Plugin.Log.LogDebug("Finding friends for friends-only game...");
                            string[] friendsToFind;
                            if (friendsUserIds.Count > 512)
                            {
                                friendsToFind = friendsUserIds.GetRange(0, 512).ToArray();
                            }
                            else
                            {
                                friendsToFind = friendsUserIds.ToArray();
                            }
                            if (friendsToFind != null && friendsToFind.Length != 0)
                            {
                                instanceType.GetField("friendSearch", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, true);
                                PhotonNetwork.LeaveRoom(true);

                                Action findFriends = delegate ()
                                {
                                    PhotonNetwork.FindFriends(friendsToFind);
                                };
                                instanceType.GetEvent("onMasterServer").AddEventHandler(__instance, findFriends);
                            }
                            else
                            {
                                GameObject.Find("JoinPanel").GetComponent<RewiredEventTrigger>().enabled = true;
                            }
                        }
                        else
                        {
                            Plugin.Log.LogDebug("Finding all games...");
                            string[] friendsToFind = new string[512];
                            if (userList.Length > 512)
                            {
                                int length = (userList.Length - this.userListId >= 512) ? 512 : (userList.Length - this.userListId);
                                Array.Copy(userList, this.userListId, friendsToFind, 0, length);
                                this.userListId += 512;
                                if (this.userListId >= userList.Length)
                                {
                                    this.userListId = 0;
                                }

                                instanceType.GetField("userListId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, userListId);
                            }
                            else
                            {
                                Array.Copy(userList, friendsToFind, userList.Length);
                            }

                            if (friendsToFind != null && friendsToFind.Length > 1)
                            {
                                instanceType.GetField("friendSearch", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, true);
                                PhotonNetwork.LeaveRoom(true);
                                Action findFriends = delegate ()
                                {
                                    PhotonNetwork.FindFriends(friendsToFind);
                                };
                                instanceType.GetEvent("onMasterServer").AddEventHandler(__instance, findFriends);
                            }
                            else
                            {
                                GameObject.Find("JoinPanel").GetComponent<RewiredEventTrigger>().enabled = true;
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPrefix]
        public static bool PopulateUserList(bool friendsOnly, PhotonManager __instance, ref IEnumerator __result)
        {
            Plugin.Log.LogDebug("Harmony:" + nameof(PopulateUserList));
            __result = new PopulateUserListEnumerable(__instance, friendsOnly).GetEnumerator();
            return false;
        }
    }
}
