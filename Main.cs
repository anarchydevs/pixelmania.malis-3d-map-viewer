using System;
using System.Linq;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using System.Collections.Generic;
using AOSharp.Common.GameData.UI;
using AOSharp.Recast;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MalisDungeonViewer
{
    public class Main : AOPluginEntry
    {
        public static Settings Settings;
        private MainWindow _window;
        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("- Mali's Dungeon Map -");
                Game.TeleportEnded += Game_OnTeleportEnded;
                Game.OnUpdate += OnUpdate;

                Settings = new Settings("Mali3DMap_Settings");
                Settings.AddVariable("OffsetX", 0.48f);
                Settings.AddVariable("OffsetY", 0.20f);
                Settings.AddVariable("Scale", 450);
                Settings.AddVariable("Static", false);
                Settings.AddVariable("Mission", false);
                Settings.Save();

                _window = new MainWindow("Mali's Dungeon Map", $"{pluginDir}\\XML\\Settings.xml");
                _window.Show();

                DungeonMap.AddCube(IdentityType.Terminal, 3f, DebuggingColor.Purple);
                DungeonMap.AddSquare(IdentityType.Door, 2f, DebuggingColor.Yellow);
                DungeonMap.AddSquare(IdentityType.Container, 1f, DebuggingColor.LightBlue);
                DungeonMap.AddPyramid(IdentityType.SimpleChar, 2f, DebuggingColor.Red);
                DungeonMap.AddPyramid(DynelManager.LocalPlayer.Name, 3f, DebuggingColor.Green);
                DungeonMap.AddLine(IdentityType.Terminal, DebuggingColor.Purple);
                DungeonMap.FilteredZoneIds.AddRange(new List<int> { PlayfieldIds.Grid, PlayfieldIds.FixerGrid });
                DungeonMap.CreateMeshTask();

                Chat.RegisterCommand("mapsettings", (string command, string[] param, ChatWindow chatWindow) => _window.Show());
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private unsafe void OnUpdate(object s, float deltaTime)
        {
            DungeonMap.RenderMap(DebuggingColor.Yellow, true);
           _window.ReadSliderSettings();
        }

        private void Game_OnTeleportEnded(object s, EventArgs e)
        {
            DungeonMap.CreateMeshTask();
        }

        public override void Teardown()
        {
            Settings["OffsetX"] = DungeonMap.Offset.X;
            Settings["OffsetY"] = DungeonMap.Offset.Y;
            Settings["Scale"] = DungeonMap.Scale;
            Settings["Static"] = DungeonMap.IsStatic;
            Settings["Mission"] = DungeonMap.MissionPing;
            Settings.Save();
        }
    }
    public class PlayfieldIds
    {
        public const int Grid = 357954;
        public const int FixerGrid = 355703;
    }
}
