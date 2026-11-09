using BepInEx;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace DressmakerArchipelago
{
    public class PlayerJson
    {
        public static string filepath => Path.Combine(Paths.PluginPath, $"playerArchipelago_{Plugin.saveSlot}.json");
        public int goldCount;
        public HashSet<string> allLocationsReached = new HashSet<string>();
        public string seed;
        public PlayerJson()
        {
            if (!File.Exists(filepath))
            {
                SaveFile();
            }
        }

        public static PlayerJson Loadfile()
        {
            if (File.Exists(filepath))
            {
                string jsonText = File.ReadAllText(filepath);
                PlayerJson data = JsonConvert.DeserializeObject<PlayerJson>(jsonText);
                return data;
            }
            return new PlayerJson();
        }

        public void SaveFile()
        {
            string jsonText = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filepath, jsonText);
        }
    }
}
