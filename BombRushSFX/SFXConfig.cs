using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BombRushSFX
{

    public class SFXConfig // SEX CONFIG!!!
    {
        public Dictionary<string, string> keyValues = new Dictionary<string, string>();
        public SFXConfig(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("[BRSFX] Cannot open " + path + " as a SFXConfig. It doesn't exist!");
                return;
            }
            string[] f = File.ReadAllLines(path);

            foreach (string s in f)
            {
                if (s.StartsWith("#"))
                    continue;
                if (!s.Contains(":"))
                    continue;

                string[] split = s.Split(':');
                if (split.Length != 2)
                    continue;
                
                keyValues.Add(split[0],split[1].Replace(" ", "")); // someones gonna have a space in the file name and its gonna break, im calling it here. and i'm gonna be mad.
            }
            
        }
    }
}