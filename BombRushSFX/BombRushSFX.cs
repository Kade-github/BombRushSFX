using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Reptile;
using UnityEngine;
using UnityEngine.Networking;

namespace BombRushSFX
{
    [BepInPlugin("kade.bombrushsfx", "Bomb Rush SFX!", "1.0.0.0")]
    [BepInProcess("Bomb Rush Cyberfunk.exe")]
    public class BombRushSFX : BaseUnityPlugin
    {
        // stole this from myself
        public static Dictionary<string, AudioClip> audios = new Dictionary<string, AudioClip> ();

        public static Dictionary<SfxCollectionID, SfxCollection> collections =
            new Dictionary<SfxCollectionID, SfxCollection>();

        public static Dictionary<SfxCollectionID, List<SfxCollection.RandomAudioClipContainer>> containers =
            new Dictionary<SfxCollectionID, List<SfxCollection.RandomAudioClipContainer>>();
        public int shouldBeDone = 0;
        public int done = 0;
        public IEnumerator LoadAudioFile(string filePath, AudioType type)
        {
            string clean = filePath;
            // Escape special characters so we don't get an HTML error when we send the request
            filePath = UnityWebRequest.EscapeURL(filePath);
 
            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///"+filePath, type))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Logger.LogError(www.error);
                }
                else
                {
                    AudioClip myClip = DownloadHandlerAudioClip.GetContent(www);
                    myClip.name = clean;
                    done++;
 
                    audios.Add(clean.Replace("\\", "/"), myClip);

                    Logger.LogInfo("[BRSFX] Loaded " + clean);
                }
            }
        }
        
        public IEnumerator LoadFile(string f)
        {
            AudioType type = AudioType.UNKNOWN;
            switch (f.Split('.').Last().ToLower())
            {
                case "aif":
                case "aiff":
                    type = AudioType.AIFF;
                    break;
                case "it":
                    type = AudioType.IT;
                    break;
                case "mod":
                    type = AudioType.MOD;
                    break;
                case "mp2":
                case "mp3":
                    type = AudioType.MPEG;
                    break;
                case "ogg":
                    type = AudioType.OGGVORBIS;
                    break;
                case "s3m":
                    type = AudioType.S3M;
                    break;
                case "wav":
                    type = AudioType.WAV;
                    break;
                case "xm":
                    type = AudioType.XM;
                    break;
                case "flac":
                    break;
                default:
                    yield return null;
                    break;
            }
            shouldBeDone++;
            StartCoroutine(LoadAudioFile(f, type));
            yield return null;
        }

        public IEnumerator SearchDirectories(string path = "")
        {
            string p = path.Length == 0 ? Application.streamingAssetsPath + "/Mods/BombRushSFX/SFX" : path;
            
            foreach (string f in Directory.GetDirectories(p))
            {
                Logger.LogInfo("[BRSFX] Searching directory " + f);
                StartCoroutine(SearchDirectories(f));
            }
            
            foreach(string f in Directory.GetFiles(p))
            {
                if (f.EndsWith(".txt"))
                    continue;
                StartCoroutine(LoadFile(f));
            }
            
            yield return null;
        }

        public void LoadText(string path)
        {
            path = path.Replace("\\", "/");
            string directory = path.Substring(0, path.LastIndexOf('/'));
            SFXConfig cfg = new SFXConfig(path);
            if (cfg.keyValues.Count == 0)
            {
                Logger.LogError("[BRSFX] Failed to load " + path);
                return;
            }

            string collectionId = cfg.keyValues["collection"];
            SfxCollectionID type = SfxCollectionID.NONE;
            try
            {
                type = (SfxCollectionID)int.Parse(collectionId);
            }
            catch (Exception e)
            {
                Logger.LogError("[BRSFX] Failed to load " + path + ", " + collectionId + " isn't an id for a collection type.");
                return;
            }

            List<AudioClip> clips = new List<AudioClip>();

            try
            {
                string[] s = cfg.keyValues["audios"].Split(',');

                foreach (string sr in s)
                {
                    string yo = directory + "/" + sr;
                    if (audios.ContainsKey(yo))
                        clips.Add(audios[yo]);
                }
            }
            catch (Exception e)
            {
                Logger.LogError("[BRSFX] Failed to load " + path + ", Failed to parse audios. " + e.Message);
                return;
            }
            
            SfxCollection collection = ScriptableObject.CreateInstance<SfxCollection>();
            collection.name = "m_" + (int)type;
            collection.collectionName = type.ToString();
            SfxCollection.RandomAudioClipContainer container = new SfxCollection.RandomAudioClipContainer();
            try
            {
                container.clipID = (AudioClipID)int.Parse(cfg.keyValues["id"]);
            }
            catch (Exception e)
            {
                Logger.LogError("[BRSFX] Failed to load " + path + ", " + cfg.keyValues["id"] + " isn't an id for an audio type.");
                return;
            }

            container.lastRandomClip = 0;
            container.clips = clips.ToArray();

            if (!containers.ContainsKey(type))
                containers.Add(type, new List<SfxCollection.RandomAudioClipContainer>());
            
            if (collections.ContainsKey(type))
            {
                // add our container to it then
                containers[type].Add(container);
            }
            else
            {
                containers[type].Add(container);
                collections.Add(type, collection);
            }
            

            Logger.LogInfo("[BRSFX] Loaded " + container.clips.Length + " audio clips into sfxcollection " + type + " under the tag " + container.clipID);
        }
        
        public void LoadTexts(string path = "")
        {
            string p = path.Length == 0 ? Application.streamingAssetsPath + "/Mods/BombRushSFX/SFX" : path;
            
            foreach (string f in Directory.GetDirectories(p))
            {
                Logger.LogInfo("[BRSFX] Searching directory " + f);
                LoadTexts(f);
            }
            foreach(string f in Directory.GetFiles(p))
            {
                if (!f.EndsWith(".txt"))
                    continue;
                LoadText(f);
            }
        }
        
        public IEnumerator LoadAudios()
        {
            Logger.LogInfo("[BRSFX] Loading SFX...");
            shouldBeDone = 0;
            done = 0;

            yield return StartCoroutine(SearchDirectories());
            
            // loading the text files
            
            LoadTexts();
            
            // load them into the fuckin game

            Logger.LogInfo("[BRSFX] Loading SFXCollections into the game...");
            
            foreach (KeyValuePair<SfxCollectionID, SfxCollection> c in collections)
            {

                c.Value.audioClipContainers = containers[c.Key].ToArray();
                Logger.LogInfo("[BRSFX] Set " + c.Value.name + " audio clip containers to " + c.Value.audioClipContainers.Length);
                Core.Instance.AudioManager.sfxLibrary.sfxCollectionIDDictionary[c.Key] = c.Value;
                Core.Instance.AudioManager.sfxLibrary.sfxCollectionDictionary[c.Value.collectionName] = c.Value;
                
            }
            
            Logger.LogInfo("[BRSFX] Bomb Rush SFX has been loaded!");
        }
        
        public void Awake()
        {
            // setup mod dirs
            if (!Directory.Exists(Application.streamingAssetsPath + "/Mods"))
                Directory.CreateDirectory(Application.streamingAssetsPath + "/Mods");
            if (!Directory.Exists(Application.streamingAssetsPath + "/Mods/BombRushSFX"))
                Directory.CreateDirectory(Application.streamingAssetsPath + "/Mods/BombRushSFX");
            if (!Directory.Exists(Application.streamingAssetsPath + "/Mods/BombRushSFX/SFX"))
                Directory.CreateDirectory(Application.streamingAssetsPath + "/Mods/BombRushSFX/SFX");

            var harmony = new Harmony("kade.bombrushsfx");
            harmony.PatchAll();
            Logger.LogInfo("[BRSFX] Patched...");

            Core.OnCoreInitialized += () =>
            {
                // load em

                StartCoroutine(LoadAudios());
            };

        }
    }
}