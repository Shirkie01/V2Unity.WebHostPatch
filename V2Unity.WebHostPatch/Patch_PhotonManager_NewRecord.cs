using HarmonyLib;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using V2Unity.Model;

namespace V2Unity.WebHostPatch
{
    [HarmonyPatch(typeof(PhotonManager), "_NewRecord")]
    public class Patch_PhotonManager_NewRecord
    {
        class NewRecordEnumerable : IEnumerable
        {
            public IEnumerator GetEnumerator()
            {
                Record? newRecord = null;

                // Get the user's current records for this level
                var stage = SceneManager.GetActiveScene().name;
                using (var www = UnityWebRequest.Get(Plugin.URL + $"/v2unity/records/{SystemInfo.deviceUniqueIdentifier}?stage={stage}"))
                {
                    Plugin.Log.LogDebug("Requesting records...");

                    www.SendWebRequest();
                    while (!www.isDone)
                    {
                        yield return null;
                    }

                    Plugin.Log.LogDebug(www.responseCode + ": " + www.error);

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        Plugin.Log.LogDebug("Got records. Check if we exceeded them.");
                        var difficulty = (Difficulty)GameManager.instance.DAT_C6E;
                        var totalEnemiesDestroyed = GameManager.instance.DAT_CC4;

                        bool createNewRecord = false;

                        var records = JsonConvert.DeserializeObject<List<Record>>(www.downloadHandler.text) ?? new List<Record>();

                        Plugin.Log.LogDebug($"Found {records.Count} records for {stage} on {difficulty}");

                        // If there are records, check if we exceed
                        // any existing record by checking the difficulty
                        // and the total enemies destroyed
                        if (records.Any())
                        {
                            foreach (var record in records)
                            {
                                if (record.Difficulty == difficulty)
                                {
                                    if (record.TotalEnemiesDestroyed < totalEnemiesDestroyed)
                                    {
                                        createNewRecord = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else if (totalEnemiesDestroyed > 0)
                        {
                            // If there are no records, then we are the first!
                            createNewRecord = true;
                        }

                        if (createNewRecord)
                        {
                            newRecord = new Record()
                            {
                                DeviceId = SystemInfo.deviceUniqueIdentifier,
                                Difficulty = difficulty,
                                Stage = stage,
                                TotalEnemiesDestroyed = totalEnemiesDestroyed,
                                Vehicle = (_VEHICLE)GameManager.instance.vehicles[0]
                            };
                        }
                    }
                }

                GameManager.instance.EndMini();

                // We have a new record! Submit it to the database
                if (newRecord != null)
                {                    
                    GameManager.instance.LoadMini();

                    string json = JsonConvert.SerializeObject(newRecord);

                    // Due to a bug in the UnityWebRequest.Post method, we
                    // use Put and change it to Post by setting the www.method field.
                    using (UnityWebRequest www = UnityWebRequest.Put(Plugin.URL + $"/v2unity/records/{SystemInfo.deviceUniqueIdentifier}", json))
                    {
                        www.method = "POST";
                        www.SetRequestHeader("Content-Type", MediaTypeNames.Application.Json);

                        Plugin.Log.LogDebug($"Creating new record for {newRecord.Stage} on {newRecord.Difficulty}");

                        www.SendWebRequest();
                        while (!www.isDone)
                        {
                            yield return null;
                        }
                        if (www.result == UnityWebRequest.Result.Success)
                        {
                            Plugin.Log.LogDebug("New Record!");
                        }
                        else
                        {
                            Plugin.Log.LogDebug("Error: New Record. " + www.responseCode + " " + www.error);
                        }
                    }

                    GameManager.instance.EndMini();
                }
            }
        }

        [HarmonyPrefix]
        public static bool _NewRecord(PhotonManager __instance, ref IEnumerator __result)
        {
            Plugin.Log.LogDebug("Harmony:" + nameof(_NewRecord));
            __result = new NewRecordEnumerable().GetEnumerator();
            return false;
        }
    }
}
