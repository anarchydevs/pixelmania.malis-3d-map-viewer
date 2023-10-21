using AOSharp.Common.GameData.UI;
using AOSharp.Core.UI;
using System;

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
                    Views.OffsetX.Value = Main.Settings.OffsetX;

                if (Window.FindView("OffsetY", out Views.OffsetY))
                    Views.OffsetY.Value = Main.Settings.OffsetY;

                if (Window.FindView("Scale", out Views.Scale))
                    Views.Scale.Value = Main.Settings.Scale;

                if (Window.FindView("Static", out Views.Static))
                    Views.Static.SetValue(Main.Settings.Static);

                if (Window.FindView("Mission", out Views.Mission))
                    Views.Mission.SetValue(Main.Settings.Mission);

                if (Window.FindView("Distance", out Views.Distance))
                    Views.Distance.SetValue(Main.Settings.Distance);
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

            Main.Settings.OffsetX = Views.OffsetX.GetValue();
            Main.Settings.OffsetY = Views.OffsetY.GetValue();
            Main.Settings.Scale = Views.Scale.GetValue();
            Main.Settings.Static = Views.Static.IsChecked;
            Main.Settings.Mission = Views.Mission.IsChecked;
            Main.Settings.Distance = Views.Distance.GetValue();
        }
    }

    public class Views
    {
        public SliderView OffsetX;
        public SliderView OffsetY;
        public SliderView Scale;
        public SliderView Distance;
        public Checkbox Static;
        public Checkbox Mission;
    }
}