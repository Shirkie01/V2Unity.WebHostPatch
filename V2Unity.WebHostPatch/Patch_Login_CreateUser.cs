using HarmonyLib;
using Newtonsoft.Json;
using System.Collections;
using System.Net.Mime;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;
using V2Unity.Model;

namespace V2Unity.WebHostPatch
{
    [HarmonyPatch(typeof(Login), "CreateUser")]
    public class Patch_Login_CreateUser
    {
        class CreateUserEnumerable : IEnumerable
        {
            private readonly Login __instance;

            public CreateUserEnumerable(Login __instance)
            {
                this.__instance = __instance;
            }

            public IEnumerator GetEnumerator()
            {
                string? name = __instance.GetType().GetField("onlineId", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance) as string;

                if(name == null)
                    yield break;

                var user = new User() { DeviceId = SystemInfo.deviceUniqueIdentifier, Name = name };
                var json = JsonConvert.SerializeObject(user);                

                Plugin.Log.LogDebug(json);

                // Due to a bug in the UnityWebRequest.Post method, we
                // use Put and change it to Post by setting the www.method field.
                using (var www = UnityWebRequest.Put(Plugin.URL + $"/v2unity/users/", json))
                {
                    __instance.registerAnim.SetTrigger("Close");
                    __instance.errorButton.onClick.RemoveAllListeners();
                    __instance.connectAnim.SetTrigger("Open");

                    www.method = "POST";
                    www.SetRequestHeader("Content-Type", MediaTypeNames.Application.Json);
                    www.SendWebRequest();
                    while (!www.isDone)
                    {
                        __instance.connectText.text = "Verifying...";
                        yield return new WaitForSeconds(0.5f);
                        __instance.connectText.text = "Verifying....";
                        yield return new WaitForSeconds(0.5f);
                        __instance.connectText.text = "Verifying.....";
                        yield return new WaitForSeconds(0.5f);
                        __instance.connectText.text = "Verifying......";
                        yield return new WaitForSeconds(0.5f);
                    }
                    
                    if(www.result == UnityWebRequest.Result.Success)
                    {
                        string onlineId = __instance.GetType().GetField("onlineId", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance).ToString();
                        PhotonManager.instance.AuthenticateUser(onlineId);
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
                            __instance.connectText.text = "Verifying...";
                            yield return new WaitForSeconds(0.5f);
                            __instance.connectText.text = "Verifying....";
                            yield return new WaitForSeconds(0.5f);
                            __instance.connectText.text = "Verifying.....";
                            yield return new WaitForSeconds(0.5f);
                            __instance.connectText.text = "Verifying......";
                            yield return new WaitForSeconds(0.5f);
                        }
                        if (PhotonManager.IsConnected())
                        {
                            __instance.connectAnim.SetTrigger("Close");
                            __instance.TitleScreen();
                        }
                    }                   
                    else if (www.responseCode == 409)
                    {
                        __instance.connectAnim.SetTrigger("Close");
                        __instance.errorAnim.SetTrigger("Open");
                        __instance.errorText.text = "This Online ID has already been taken.";
                        __instance.errorButton.SelectWithoutSound(true, false);
                        __instance.errorButton.onClick.RemoveAllListeners();
                        __instance.errorButton.onClick.AddListener(delegate ()
                        {
                            __instance.errorAnim.SetTrigger("Close");
                        });
                        __instance.errorButton.onClick.AddListener(delegate ()
                        {
                            __instance.registerAnim.SetTrigger("Open");
                        });
                        __instance.errorButton.onClick.AddListener(delegate ()
                        {
                            __instance.registerField.SelectWithoutSound(true, false);
                        });
                        __instance.errorButton.onClick.AddListener(delegate ()
                        {
                            __instance.registerField.ActivateInputField();
                        });
                    }
                    else
                    {
                        Plugin.Log.LogError(www.responseCode + ":" + www.error);

                        __instance.connectAnim.SetTrigger("Close");
                        __instance.errorAnim.SetTrigger("Open");
                        __instance.errorText.text = "A connection error has occured.";

#if DEBUG
                        __instance.errorText.text += "\n CreateUser, " + www.responseCode + " " + www.error;
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

                }
            }
        }


        [HarmonyPrefix]
        public static bool CreateUser(Login __instance, ref IEnumerator __result)
        {
            Plugin.Log.LogDebug("Harmony:" + nameof(CreateUser));
            __result = new CreateUserEnumerable(__instance).GetEnumerator();
            return false;
        }
    }
}
