using HarmonyLib;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine;
using V2Unity.Model;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace V2Unity.WebHostPatch
{
    [HarmonyPatch(typeof(PhotonManager), "_Leaderboards")]
    public class Patch_PhotonManager_Leaderboards
    {
        class LeaderboardEnumerable : IEnumerable
        {
            private int stage;
            private Difficulty difficulty;

            public LeaderboardEnumerable(int stage, Difficulty difficulty)
            {
                this.stage = stage;
                this.difficulty = difficulty;
            }

            public IEnumerator GetEnumerator()
            {
                GameManager.instance.LoadMini();

                RectTransform contentRect = Utilities.Find("Leaderboards").GetComponent<RectTransform>();
                TextMeshProUGUI contentText = contentRect.GetChild(0).GetChild(2).GetComponentsInChildren<TextMeshProUGUI>(true)[1];
                Image contentImage = contentRect.GetChild(0).GetChild(2).GetChild(2).GetComponent<Image>();
                contentImage.color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, 0);
                contentText.text = "";

                string stageName = Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(stage + 1));
                using (UnityWebRequest www = UnityWebRequest.Get(Plugin.URL + $"/v2unity/records?stage={stageName}&difficulty={difficulty}"))
                {
                    Plugin.Log.LogDebug($"Getting world recorods for {stage} on {difficulty}");

                    yield return www.SendWebRequest();
                    
                    Plugin.Log.LogDebug(www.responseCode + ": " + www.error);

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        Plugin.Log.LogDebug(www.downloadHandler.text);

                        List<Record> records = JsonConvert.DeserializeObject<List<Record>>(www.downloadHandler.text) ?? new List<Record>();

                        RectTransform content = contentRect.GetComponent<ChiScrollRect>().content;
                        for (int i = 0; i < content.childCount; i++)
                        {
                            UnityEngine.Object.Destroy(content.GetChild(i).gameObject);
                        }

                        foreach (var record in records)
                        {
                            if (record.DeviceId == SystemInfo.deviceUniqueIdentifier)
                            {
                                contentImage.color = Color.white;
                                contentImage.sprite = Menu.instance.cars[(int)record.Vehicle];
                                contentText.text = record.TotalEnemiesDestroyed.ToString();
                            }
                        }

                        records.Sort((Record x, Record y) => y.TotalEnemiesDestroyed.CompareTo(x.TotalEnemiesDestroyed));
                        for (int l = 0; l < records.Count; l++)
                        {
                            RectTransform rectTransform = UnityEngine.Object.Instantiate<RectTransform>(Menu.instance.buttonPrefab2, content);
                            if (l == 0)
                            {
                                rectTransform.GetComponent<ButtonSound>().SelectWithoutSound();
                            }
                            Image image = rectTransform.GetChild(0).GetChild(2).GetComponentsInChildren<Image>(true)[2];
                            Image image2 = rectTransform.GetChild(1).GetChild(2).GetComponentsInChildren<Image>(true)[2];
                            TextMeshProUGUI componentInChildren = rectTransform.GetChild(0).GetChild(0).GetComponentInChildren<TextMeshProUGUI>(true);
                            TextMeshProUGUI componentInChildren2 = rectTransform.GetChild(1).GetChild(0).GetComponentInChildren<TextMeshProUGUI>(true);
                            TextMeshProUGUI componentInChildren3 = rectTransform.GetChild(0).GetChild(1).GetComponentInChildren<TextMeshProUGUI>(true);
                            TextMeshProUGUI componentInChildren4 = rectTransform.GetChild(1).GetChild(1).GetComponentInChildren<TextMeshProUGUI>(true);
                            TextMeshProUGUI componentInChildren5 = rectTransform.GetChild(0).GetChild(2).GetComponentInChildren<TextMeshProUGUI>(true);
                            TextMeshProUGUI componentInChildren6 = rectTransform.GetChild(1).GetChild(2).GetComponentInChildren<TextMeshProUGUI>(true);
                            image.sprite = Menu.instance.icons[(int)records[l].Vehicle];
                            image2.sprite = Menu.instance.icons[(int)records[l].Vehicle];
                            componentInChildren.text = (l + 1).ToString();
                            componentInChildren2.text = (l + 1).ToString();
                            componentInChildren3.text = records[l].Name;
                            componentInChildren4.text = records[l].Name;
                            componentInChildren3.color = ((records[l].Name == PhotonNetwork.NickName) ? Color.green : Color.white);
                            componentInChildren5.text = records[l].TotalEnemiesDestroyed.ToString();
                            componentInChildren6.text = records[l].TotalEnemiesDestroyed.ToString();
                        }
                    }
                }

                GameManager.instance.EndMini();
            }
        }

        [HarmonyPrefix]
        public static bool _Leaderboards(int stage, int difficulty, ref IEnumerator __result)
        {
            Plugin.Log.LogDebug("Harmony:" + nameof(_Leaderboards));
            __result = new LeaderboardEnumerable(stage, (Difficulty)difficulty).GetEnumerator();
            return false;
        }
    }
}
