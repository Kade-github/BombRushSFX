using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Reptile;
using UnityEngine;

namespace BombRushSFX
{
    [HarmonyPatch(typeof(SfxCollection), nameof(SfxCollection.GetAudioClipContainerFromID))]
    public class SfxCollection_Patches
    {
        static bool Prefix(SfxCollection __instance, ref SfxCollection.RandomAudioClipContainer __result, ref SfxCollection.RandomAudioClipContainer[] audioClipContainers, AudioClipID clipID)
        {
             __result = audioClipContainers.FirstOrDefault(cont => cont.clipID == clipID);
            
            return false;
        }
    }

    
    
    [HarmonyPatch(typeof(SfxCollection), nameof(SfxCollection.GetRandomAudioClipById))]
    public class SfxCollectionRandom_Patches
    {
        static bool Prefix(SfxCollection __instance, ref AudioClip __result, AudioClipID audioClipID)
        {
            if (__instance.name.StartsWith("m_"))
            {
                KeyValuePair<SfxCollectionID, SfxCollection> col =
                    BombRushSFX.collections.FirstOrDefault(c => c.Value.name == __instance.name);
                __instance.audioClipContainers = BombRushSFX.containers[col.Key].ToArray();
                foreach(SfxCollection.RandomAudioClipContainer container in __instance.audioClipContainers)
                foreach (AudioClip clip in container.clips)
                {
                    if (clip.loadState == AudioDataLoadState.Unloaded)
                        clip.LoadAudioData();
                }
            }

            SfxCollection.RandomAudioClipContainer audioClipContainerFromID = __instance.audioClipContainers.FirstOrDefault(cont => cont.clipID == audioClipID);
            
            if (audioClipContainerFromID != null && audioClipContainerFromID.clipID == audioClipID && audioClipContainerFromID.clips != null)
            {
                int num = audioClipContainerFromID.clips.Length;
                int num2 = Random.Range(0, num);
                if (num2 == audioClipContainerFromID.lastRandomClip)
                {
                    num2 = (num2 + 1) % num;
                }
                audioClipContainerFromID.lastRandomClip = num2;
                __result = audioClipContainerFromID.clips[num2];
            }
            return false;
        }

        static void Postfix(SfxCollection __instance, ref AudioClip __result, AudioClipID audioClipID)
        {
            if (__result == null)
            {
                SfxCollectionID t = SfxCollectionID.NONE;
                if (__instance.name.StartsWith("m_"))
                {
                    KeyValuePair<SfxCollectionID, SfxCollection> col =
                        BombRushSFX.collections.FirstOrDefault(c => c.Value.name == __instance.name);
                    t = col.Key;
                }

                Debug.LogError("[BRSFX] GetRandomAudioClipById Audio clip returned null (ID: " + audioClipID + ", in collection " + t + ")");
            }
        }
    }
}
