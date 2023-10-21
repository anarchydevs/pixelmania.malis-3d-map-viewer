using System;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace MalisDungeonViewer
{
    public class Config
    {
        public float OffsetX = 0;
        public float OffsetY = 0;
        public float Scale = 250;
        public float Distance = 1000;
        public bool Static = true;
        public bool Mission = true;

        public void OnLoad()
        {
            Config config = JsonConvert.DeserializeObject<Config>(File.ReadAllText($"{Main.PluginDir}\\JSON\\config.json"));

            OffsetX = config.OffsetX;
            OffsetY = config.OffsetY;
            Scale = config.Scale;
            Distance = config.Distance;
            Static = config.Static;
            Mission = config.Mission;
        }

        public void Save() => File.WriteAllText($"{Main.PluginDir}\\JSON\\config.json", JsonConvert.SerializeObject(this));
    }
}
