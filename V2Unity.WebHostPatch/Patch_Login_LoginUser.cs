using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace V2Unity.WebHostPatch
{
    [HarmonyPatch(typeof(Login), "LoginUser")]
    public class Patch_Login_LoginUser
    {
        class LoginUserEnumerable : IEnumerable
        {
            private Login __instance;

            public LoginUserEnumerable(Login __instance)
            {
                this.__instance = __instance;
            }

            public IEnumerator GetEnumerator()
            {                
                using (var www = UnityWebRequest.Get(Plugin.URL + $"/v2unity/users/{SystemInfo.deviceUniqueIdentifier}/login"))
                {
                    __instance.connectAnim.SetTrigger("Open");
                    www.SendWebRequest();
                    while (!www.isDone)
                    {
                        __instance.connectText.text = "Connecting...";
                        yield return new WaitForSeconds(0.5f);
                        __instance.connectText.text = "Connecting....";
                        yield return new WaitForSeconds(0.5f);
                        __instance.connectText.text = "Connecting.....";
                        yield return new WaitForSeconds(0.5f);
                        __instance.connectText.text = "Connecting......";
                        yield return new WaitForSeconds(0.5f);
                    }

                    // If the user was not found, show the username box
                    if (www.responseCode == 404)
                    {
                        __instance.connectAnim.SetTrigger("Close");
                        __instance.registerAnim.SetTrigger("Open");
                        __instance.registerField.SelectWithoutSound(true, false);
                        __instance.registerField.ActivateInputField();
                        __instance.registerField.onSubmit.AddListener(s =>
                        {
                            __instance.GetType().GetMethod("VerifyOnlineId", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
                        });
                    }
                    else if (www.result != UnityWebRequest.Result.Success)
                    {
                        __instance.connectAnim.SetTrigger("Close");
                        __instance.errorAnim.SetTrigger("Open");
                        __instance.errorText.text = "A connection error has occured.";

#if DEBUG
                        __instance.errorText.text += "\n LoginUser1, " + www.responseCode + " " + www.error;
#endif

                        __instance.errorButton.SelectWithoutSound(true, false);
                        __instance.errorButton.onClick.RemoveAllListeners();
                        __instance.errorButton.onClick.AddListener(delegate ()
                        {
                            __instance.errorAnim.SetTrigger("Close");
                        });
                        __instance.errorButton.onClick.AddListener(delegate ()
                        {
                            __instance.TitleScreen();
                        });
                        Debug.Log(www.downloadHandler.text);
                    }
                    else if (!PhotonManager.IsConnected())
                    {
                        PhotonManager.instance.AuthenticateUser(www.downloadHandler.text);
                        PhotonManager.instance.Connect();
                        int i = 0;
                        while (!PhotonManager.IsConnected())
                        {
                            int num = i;
                            i = num + 1;
                            if (num == 10)
                            {
                                __instance.connectAnim.SetTrigger("Close");
                                __instance.errorAnim.SetTrigger("Open");
                                __instance.errorText.text = "A connection error has occured.";

#if DEBUG
                                __instance.errorText.text += "\n LoginUser2, " + www.responseCode + " " + www.error;
#endif

                                __instance.errorButton.SelectWithoutSound(true, false);
                                __instance.errorButton.onClick.RemoveAllListeners();
                                __instance.errorButton.onClick.AddListener(delegate ()
                                {
                                    __instance.errorAnim.SetTrigger("Close");
                                });
                                __instance.errorButton.onClick.AddListener(delegate ()
                                {
                                    __instance.TitleScreen();
                                });
                                PhotonManager.instance.Disconnect(true);
                                break;
                            }
                            __instance.connectText.text = "Connecting...";
                            yield return new WaitForSeconds(0.5f);
                            __instance.connectText.text = "Connecting....";
                            yield return new WaitForSeconds(0.5f);
                            __instance.connectText.text = "Connecting.....";
                            yield return new WaitForSeconds(0.5f);
                            __instance.connectText.text = "Connecting......";
                            yield return new WaitForSeconds(0.5f);
                        }
                        if (PhotonManager.IsConnected())
                        {
                            __instance.connectAnim.SetTrigger("Close");
                            __instance.TitleScreen();
                        }
                    }
                    else
                    {
                        __instance.connectAnim.SetTrigger("Close");
                        __instance.TitleScreen();
                    }
                }
            }
        }

        [HarmonyPrefix]
        public static bool LoginUser(Login __instance, ref IEnumerator __result)
        {
            Plugin.Log.LogDebug("Harmony:" + nameof(LoginUser));
            __result = new LoginUserEnumerable(__instance).GetEnumerator();
            return false;
        }
    }
}
