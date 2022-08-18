using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.GameData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MalisDungeonViewer
{
    public class MainWindow : AOSharpWindow
    {
        public Views Views;
        public MainWindow(string name, string path, WindowStyle windowStyle = WindowStyle.Default, WindowFlags flags = WindowFlags.AutoScale | WindowFlags.NoFade) : base(name, path, windowStyle, flags)
        {
            Views = new Views();
        }

        protected override void OnWindowCreating()
        {
            try
            {
                if (Window.FindView("OffsetX", out Views.OffsetX))
                    Views.OffsetX.Value = Main.Settings["OffsetX"].AsFloat();
                if (Window.FindView("OffsetY", out Views.OffsetY))
                    Views.OffsetY.Value = Main.Settings["OffsetY"].AsFloat();
                if (Window.FindView("OffsetZ", out Views.OffsetZ))
                    Views.OffsetZ.Value = Main.Settings["OffsetZ"].AsFloat();
                if (Window.FindView("Scale", out Views.Scale))
                    Views.Scale.Value = Main.Settings["Scale"].AsFloat();
                if (Window.FindView("Static", out Views.Static))
                    Views.Static.SetValue(Main.Settings["Static"].AsBool());
                if (Window.FindView("Mission", out Views.Mission))
                    Views.Mission.SetValue(Main.Settings["Mission"].AsBool());
            }
            catch (Exception e)
            {
                Chat.WriteLine(e);
            }
        }
        public void ReadSliderSettings()
        {
            if (Window == null)
                return;

            if (!Window.IsValid)
                return;

            if (!Window.IsVisible)
                return;

            DungeonMap.Offset.X = Views.OffsetX.GetValue();
            DungeonMap.Offset.Y = Views.OffsetY.GetValue();
            DungeonMap.Scale = Views.Scale.GetValue();
            DungeonMap.IsStatic = Views.Static.IsChecked;
            DungeonMap.MissionPing = Views.Mission.IsChecked;
        }
    }

    public class Views
    {
        public SliderView OffsetX;
        public SliderView OffsetY;
        public SliderView OffsetZ;
        public SliderView Scale;
        public Checkbox Static;
        public Checkbox Mission;
    }
}