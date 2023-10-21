using System;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace MalisDungeonViewer
{
    public class Main : AOPluginEntry
    {
        public static Config Settings;
        public static string PluginDir;
        private MainWindow _window;
        private bool _hideMap = false;

        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("- Mali's Dungeon Map -\n /mapsettings - opens settings window \n /togglemap - toggles map");
                Game.TeleportEnded += Game_OnTeleportEnded;
                Game.OnUpdate += OnUpdate;

                PluginDir = pluginDir;  
                Settings = new Config();

                Settings.OnLoad();
                _window = new MainWindow("Mali's Dungeon Map", $"{PluginDir}\\UI\\MainWindow.xml");

                DungeonMap.AddCube(IdentityType.Terminal, 3f, DebuggingColor.Purple);
                DungeonMap.AddSquare(IdentityType.Door, 2f, DebuggingColor.Yellow);
                DungeonMap.AddSquare(IdentityType.Container, 1f, DebuggingColor.LightBlue);
                DungeonMap.AddPyramid(IdentityType.SimpleChar, 2f, DebuggingColor.Red);
                DungeonMap.AddPyramid(DynelManager.LocalPlayer.Name, 3f, DebuggingColor.Green);
                DungeonMap.AddLine(IdentityType.Terminal, DebuggingColor.Purple);

                //you can add more zones by adding them to this list (shift + f9 to see the resourceid of the dungeon)
                DungeonMap.FilteredZoneIds.AddRange(new List<int> 
                { 
                    PlayfieldIds.Grid, 
                    PlayfieldIds.FixerGrid,
                });

                DungeonMap.CreateMeshTask();

                Chat.RegisterCommand("mapsettings", (string command, string[] param, ChatWindow chatWindow) => _window.Show());
                Chat.RegisterCommand("togglemap", (string command, string[] param, ChatWindow chatWindow) => _hideMap = !_hideMap);

            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private unsafe void OnUpdate(object s, float deltaTime)
        {
            if (_hideMap)
                return;

            DungeonMap.RenderMap(DebuggingColor.Yellow, true);
           _window.ReadSliderSettings();
        }

        private void Game_OnTeleportEnded(object s, EventArgs e)
        {
            DungeonMap.CreateMeshTask();
        }

        public override void Teardown()
        {
            Settings.Save();
        }
    }

    public class PlayfieldIds
    {
        public const int Grid = 152;
        public const int FixerGrid = 4107;
    }
}
