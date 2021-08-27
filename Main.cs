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

namespace Malis3DMapViewer
{
    public class Main : AOPluginEntry
    {
        private static Settings Settings;
        private string PluginDir;
        private static Window SettingsWindow;
        private List<Mesh> DungeonMesh = new List<Mesh>();
        [StructLayout(LayoutKind.Explicit, Pack = 0, Size = 0x100)]
        private struct MissionMemStruct
        {
            [FieldOffset(0xB4)]
            public Identity Playfield;
        }
        public override void Run(string pluginDir)
        {
            try
            {
                PluginDir = pluginDir;
                Chat.WriteLine("- Mali's Map Viewer -");
                Game.TeleportEnded += Game_OnTeleportEnded;
                Game.OnUpdate += OnUpdate;

                LoadSettings();
                ConfigMap3D();
                Map3D.CreateMeshTask(DungeonMesh);

                Chat.RegisterCommand("mapsettings", (string command, string[] param, ChatWindow chatWindow) =>
                {
                    CreateConfigWindow();
                });
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
        private unsafe void OnUpdate(object s, float deltaTime)
        {
            Map3D.DrawMap(DebuggingColor.Yellow);
            Map3D.DrawLegend();
            ReadSliderSettings();
        }
        private void Game_OnTeleportEnded(object s, EventArgs e)
        {
            Map3D.CreateMeshTask(DungeonMesh);
        }
        private void ConfigMap3D()
        {
            Map3D.Legend.AddCube(IdentityType.Terminal, 3f, DebuggingColor.Purple);
            Map3D.Legend.AddSquare(IdentityType.Door, 2f, DebuggingColor.Yellow);
            Map3D.Legend.AddSquare(IdentityType.Container, 1f, DebuggingColor.LightBlue);
            Map3D.Legend.AddPyramid(IdentityType.SimpleChar, 2f, DebuggingColor.Red);
            Map3D.Legend.AddPyramid(DynelManager.LocalPlayer.Name, 3f, DebuggingColor.Green);
            Map3D.Legend.AddLine(IdentityType.Terminal, DebuggingColor.Purple);
        }
        private void CreateConfigWindow()
        {
            SettingsWindow = Window.CreateFromXml("Settings", $"{PluginDir}\\XML\\Settings.xml",
                windowSize: new Rect(0, 0, 0, 0),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            SettingsWindow.Show(true);
        }
        private void ReadSliderSettings()
        {
            if (!SettingsWindow.IsValid)
                return;

            if (SettingsWindow.FindView("OffsetX", out SliderView OffsetX))
            {
                Settings["OffsetX"] = OffsetX.GetValue();
                Map3D.Offset.X = OffsetX.GetValue();

            }

            if (SettingsWindow.FindView("OffsetY", out SliderView OffsetY))
            {
                Settings["OffsetY"] = OffsetY.GetValue();
                Map3D.Offset.Y = OffsetY.GetValue();
            }

            if (SettingsWindow.FindView("OffsetZ", out SliderView OffsetZ))
            {
                Settings["OffsetZ"] = OffsetZ.GetValue();
                Map3D.Offset.Z = OffsetZ.GetValue();
            }

            if (SettingsWindow.FindView("Scale", out SliderView Scale))
            {
                Settings["Scale"] = Scale.GetValue();
                Map3D.Scale = Scale.GetValue();
            }

            if (SettingsWindow.FindView("Static", out Checkbox Static))
            {
                Settings["Static"] = Static.IsChecked;
                Map3D.Static = Static.IsChecked;
            }

            if (SettingsWindow.FindView("Mission", out Checkbox Mission))
            {
                Settings["Mission"] = Mission.IsChecked;
                Map3D.MissionPing = Mission.IsChecked;
            }

            Settings.Save();
        }
        private void LoadSettings()
        {
            CreateConfigWindow();

            Settings = new Settings("Mali3DMap_Settings");
            Settings.AddVariable("OffsetX", 2);
            Settings.AddVariable("OffsetY", 0.5f);
            Settings.AddVariable("OffsetZ", 0);
            Settings.AddVariable("Scale", 30);
            Settings.AddVariable("Static", false);
            Settings.AddVariable("Mission", false);

            Settings.Save();

            if (SettingsWindow.FindView("OffsetX", out SliderView OffsetX))
                OffsetX.Value = Settings["OffsetX"].AsFloat();
            if (SettingsWindow.FindView("OffsetY", out SliderView OffsetY))
                OffsetY.Value = Settings["OffsetY"].AsFloat();
            if (SettingsWindow.FindView("OffsetZ", out SliderView OffsetZ))
                OffsetZ.Value = Settings["OffsetZ"].AsFloat();
            if (SettingsWindow.FindView("Scale", out SliderView Scale))
                Scale.Value = Settings["Scale"].AsFloat();
            if (SettingsWindow.FindView("Static", out Checkbox Static))
                Static.SetValue(Settings["Static"].AsBool());
            if (SettingsWindow.FindView("Mission", out Checkbox Mission))
                Mission.SetValue(Settings["Mission"].AsBool());
        }
        public static class Map3D
        {
            private static Rect roomBorder = new Rect();
            internal static Vector3 floorCenter = new Vector3();
            private static ConcurrentBag<Edge> edgeMesh = new ConcurrentBag<Edge>();
            private static List<SphereEntity> circleList = new List<SphereEntity>();
            private static List<LineEntity> lineList = new List<LineEntity>();
            private static List<CubeEntity> cubeList = new List<CubeEntity>();
            private static List<PyramidEntity> pyramidList = new List<PyramidEntity>();
            private static List<SquareEntity> squareList = new List<SquareEntity>();
            public static Vector3 Offset;
            public static float Scale;
            public static bool Static;
            public static bool MissionPing;

            public class Legend
            {
                public static void AddCircle(string name, float size, Vector3 color)
                {
                    if (circleList.Where(x => x.DynelName == name).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    circleList.Add(new SphereEntity { DynelName = name, Size = size, Color = color });
                }
                public static void AddCircle(IdentityType identityType, float size, Vector3 color)
                {
                    if (circleList.Where(x => x.IdentityType == identityType).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    circleList.Add(new SphereEntity { IdentityType = identityType, Size = size, Color = color });
                }
                public static void RemoveCircle(string name)
                {
                    if (circleList.Where(x => x.DynelName == name).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }
                    circleList.Remove(circleList.Where(x => x.DynelName == name).FirstOrDefault());
                }
                public static void RemoveCircle(IdentityType identityType)
                {
                    if (circleList.Where(x => x.IdentityType == identityType).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }
                    circleList.Remove(circleList.Where(x => x.IdentityType == identityType).FirstOrDefault());
                }
                public static void AddCube(string name, float size, Vector3 color)
                {
                    if (cubeList.Where(x => x.DynelName == name).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    cubeList.Add(new CubeEntity { DynelName = name, Size = size, Color = color });
                }
                public static void AddCube(IdentityType identityType, float size, Vector3 color)
                {
                    if (cubeList.Where(x => x.IdentityType == identityType).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    cubeList.Add(new CubeEntity { IdentityType = identityType, Size = size, Color = color });
                }
                public static void RemoveCube(string name)
                {
                    if (cubeList.Where(x => x.DynelName == name).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }
                    cubeList.Remove(cubeList.Where(x => x.DynelName == name).FirstOrDefault());
                }
                public static void RemoveCube(IdentityType identityType)
                {
                    if (cubeList.Where(x => x.IdentityType == identityType).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }
                    cubeList.Remove(cubeList.Where(x => x.IdentityType == identityType).FirstOrDefault());
                }
                public static void AddPyramid(string name, float size, Vector3 color)
                {
                    if (pyramidList.Where(x => x.DynelName == name).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    pyramidList.Add(new PyramidEntity { DynelName = name, Size = size, Color = color });
                }
                public static void AddPyramid(IdentityType identityType, float size, Vector3 color)
                {
                    if (pyramidList.Where(x => x.IdentityType == identityType).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    pyramidList.Add(new PyramidEntity { IdentityType = identityType, Size = size, Color = color });
                }
                public static void RemovePyramid(string name)
                {
                    if (pyramidList.Where(x => x.DynelName == name).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }
                    pyramidList.Remove(pyramidList.Where(x => x.DynelName == name).FirstOrDefault());
                }
                public static void RemovePyramid(IdentityType identityType)
                {
                    if (pyramidList.Where(x => x.IdentityType == identityType).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }
                    pyramidList.Remove(pyramidList.Where(x => x.IdentityType == identityType).FirstOrDefault());
                }
                public static void AddSquare(string name, float size, Vector3 color)
                {
                    if (squareList.Where(x => x.DynelName == name).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    squareList.Add(new SquareEntity { DynelName = name, Size = size, Color = color });
                }
                public static void AddSquare(IdentityType identityType, float size, Vector3 color)
                {
                    if (squareList.Where(x => x.IdentityType == identityType).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    squareList.Add(new SquareEntity { IdentityType = identityType, Size = size, Color = color });
                }
                public static void RemoveSquare(string name)
                {
                    if (squareList.Where(x => x.DynelName == name).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }

                    squareList.Remove(squareList.Where(x => x.DynelName == name).FirstOrDefault());
                }
                public static void RemoveSquare(IdentityType identityType)
                {
                    if (squareList.Where(x => x.IdentityType == identityType).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }
                    squareList.Remove(squareList.Where(x => x.IdentityType == identityType).FirstOrDefault());
                }
                public static void AddLine(string name, Vector3 color)
                {
                    if (lineList.Where(x => x.DynelName == name).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    lineList.Add(new LineEntity { DynelName = name, Color = color });
                }
                public static void AddLine(IdentityType identityType, Vector3 color)
                {
                    if (lineList.Where(x => x.IdentityType == identityType).FirstOrDefault() != null)
                    {
                        Chat.WriteLine("Legend element already exists.");
                        return;
                    }

                    lineList.Add(new LineEntity { IdentityType = identityType, Color = color });
                }
                public static void RemoveLine(string name)
                {
                    if (lineList.Where(x => x.DynelName == name).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }

                    lineList.Remove(lineList.Where(x => x.DynelName == name).FirstOrDefault());
                }
                public static void RemoveLine(IdentityType identityType)
                {
                    if (lineList.Where(x => x.IdentityType == identityType).FirstOrDefault() == null)
                    {
                        Chat.WriteLine("Element not found.");
                        return;
                    }
                    lineList.Remove(lineList.Where(x => x.IdentityType == identityType).FirstOrDefault());
                }
            };
            internal static void DrawMap(Vector3 color)
            {
                if (DynelManager.LocalPlayer.Room == null)
                    return;

                FindFloorCenter();

                foreach (Edge edge in edgeMesh.ToList())
                {
                    if (!(edge.V1.X > roomBorder.MinX-1  && edge.V1.X < roomBorder.MaxX+1))
                        continue;

                    Debug.DrawLine(RecalculateMesh(edge.V1 - floorCenter), RecalculateMesh(edge.V2 - floorCenter), color);
                }
            }
            internal unsafe static void DrawLegend()
            {
                if (DynelManager.LocalPlayer.Room == null)
                    return;

                Mission mission = Mission.List.FirstOrDefault(x => (*(MissionMemStruct*)x.Pointer).Playfield == Playfield.ModelIdentity);
                Dynel mishDynel = DynelManager.AllDynels.FirstOrDefault();

                if (mission != null && MissionPing)
                {
                    Identity target = new Identity();
                    foreach (MissionAction missionAction in mission.Actions)
                    {
                        switch (missionAction.Type)
                        {
                            case MissionActionType.FindPerson:
                                target = ((FindPersonAction)missionAction).Target;
                                break;
                            case MissionActionType.FindItem:
                                target = ((FindItemAction)missionAction).Target;
                                break;
                            case MissionActionType.UseItemOnItem:
                                if (((UseItemOnItemAction)missionAction).Source != null)
                                    target = ((UseItemOnItemAction)missionAction).Source;
                                else if (((UseItemOnItemAction)missionAction).Destination != null)
                                    target = ((UseItemOnItemAction)missionAction).Destination;
                                break;
                            case MissionActionType.KillPerson:
                                target = ((KillPersonAction)missionAction).Target;
                                break;
                        }
                    }
                    mishDynel = DynelManager.AllDynels.FirstOrDefault(x => x.Identity == target);
                }

                foreach (Dynel dynel in DynelManager.AllDynels)
                {
                    if (dynel.Room.Floor != DynelManager.LocalPlayer.Room.Floor)
                        continue;

                    Vector3 dynelPosition = new Vector3(dynel.Position.X, 0, dynel.Position.Z);

                    if (mishDynel != null && dynel.Identity == mishDynel.Identity && MissionPing)
                    {
                        Debug.DrawLine(
                            RecalculateMesh(DynelManager.LocalPlayer.Position * new Vector3(1, 0, 1) - floorCenter),
                            RecalculateMesh(dynelPosition - floorCenter),
                            DebuggingColor.Purple);
                        DrawCube(dynelPosition, dynel.Rotation, 2f, DebuggingColor.Purple);
                        continue;
                    }

                    foreach (SphereEntity sphereEntity in circleList)
                    {
                        if (dynel.Name != sphereEntity.DynelName && sphereEntity.IdentityType != dynel.Identity.Type)
                            continue;

                        if (dynel.Name == DynelManager.LocalPlayer.Name && sphereEntity.DynelName != DynelManager.LocalPlayer.Name)
                            continue;

                        int scale = dynel.Identity.Type == IdentityType.SimpleChar ? dynel.GetStat(Stat.Scale) / 100 : 1;
                        float size = (sphereEntity.Size / 2) * 1;
                        Debug.DrawSphere(RecalculateMesh(dynelPosition - floorCenter), size, sphereEntity.Color);
                    }

                    foreach (LineEntity lineEntity in lineList)
                    {
                        if (dynel.Name != lineEntity.DynelName && lineEntity.IdentityType != dynel.Identity.Type)
                            continue;

                        if (dynel.Name == DynelManager.LocalPlayer.Name && lineEntity.DynelName != DynelManager.LocalPlayer.Name)
                            continue;

                        Debug.DrawLine(
                            RecalculateMesh(DynelManager.LocalPlayer.Position * new Vector3(1, 0, 1) - floorCenter),
                            RecalculateMesh(dynelPosition - floorCenter),
                            lineEntity.Color);
                    }

                    foreach (CubeEntity cubeEntity in cubeList)
                    {
                        if (dynel.Name != cubeEntity.DynelName && cubeEntity.IdentityType != dynel.Identity.Type)
                            continue;

                        if (dynel.Name == DynelManager.LocalPlayer.Name && cubeEntity.DynelName != DynelManager.LocalPlayer.Name)
                            continue;

                        int scale = dynel.Identity.Type == IdentityType.SimpleChar ? dynel.GetStat(Stat.Scale) / 100 : 1;
                        float size = (cubeEntity.Size / 2) * 1;
                        DrawCube(dynelPosition, dynel.Rotation, size, cubeEntity.Color);
                    }

                    foreach (PyramidEntity pyramidEntity in pyramidList)
                    {
                        if (dynel.Name != pyramidEntity.DynelName && pyramidEntity.IdentityType != dynel.Identity.Type)
                            continue;

                        if (dynel.Name == DynelManager.LocalPlayer.Name && pyramidEntity.DynelName != DynelManager.LocalPlayer.Name)
                            continue;

                        int scale = dynel.Identity.Type == IdentityType.SimpleChar ? dynel.GetStat(Stat.Scale) / 100 : 1;
                        float size = (pyramidEntity.Size / 2) * 1;
                        DrawPyramid(dynelPosition, dynel.Rotation, size, pyramidEntity.Color);
                    }

                    foreach (SquareEntity squareEntity in squareList)
                    {
                        if (dynel.Name != squareEntity.DynelName && squareEntity.IdentityType != dynel.Identity.Type)
                            continue;

                        if (dynel.Name == DynelManager.LocalPlayer.Name && squareEntity.DynelName != DynelManager.LocalPlayer.Name)
                            continue;

                        int scale = dynel.Identity.Type == IdentityType.SimpleChar ? dynel.GetStat(Stat.Scale) / 100 : 1;
                        float size = (squareEntity.Size / 2) * 1;
                        DrawSquare(dynelPosition, dynel.Rotation, size, squareEntity.Color);
                    }
                }
            }
            internal static void CreateMeshTask(List<Mesh> mesh)
            {
                if (DynelManager.LocalPlayer.Room != null)
                {
                    mesh = DungeonTerrain.CreateFromCurrentPlayfield();
                    Task meshBorderTask = new Task(() => Map3D.FindMeshBorder(mesh, 2000));
                    meshBorderTask.Start();
                }
            }
            private static void FindMeshBorder(List<Mesh> meshes, int maxDrawDist)
            {
                FindFloorCenter();
                edgeMesh = new ConcurrentBag<Edge>();
                Parallel.ForEach(meshes.ToList(), mesh =>
                {
                    Dictionary<Edge, int> edgeCompareDict = new Dictionary<Edge, int>();

                    for (int j = 0; j < mesh.Triangles.Count() / 3; j++)
                    {
                        int tri = j * 3;
                        int tri1 = mesh.Triangles[tri];
                        int tri2 = mesh.Triangles[tri + 1];
                        int tri3 = mesh.Triangles[tri + 2];

                        Vector3[] verts = new Vector3[3]
                        {
                            mesh.LocalToWorldMatrix.MultiplyPoint3x4(mesh.Vertices[tri1]),
                            mesh.LocalToWorldMatrix.MultiplyPoint3x4(mesh.Vertices[tri2]),
                            mesh.LocalToWorldMatrix.MultiplyPoint3x4(mesh.Vertices[tri3])
                        };

                        if (verts.Any(x => Vector3.Distance(x, DynelManager.LocalPlayer.Position) > maxDrawDist))
                            continue;

                        EdgeFilter(verts[0], verts[1], edgeCompareDict);
                        EdgeFilter(verts[1], verts[2], edgeCompareDict);
                        EdgeFilter(verts[2], verts[0], edgeCompareDict);

                    }

                    foreach (var edge in edgeCompareDict.Where(x => x.Value == 1).ToList())
                    {
                        edgeMesh.Add(new Edge { V1 = (edge.Key.V1) * new Vector3(1, 0, 1), V2 = (edge.Key.V2) * new Vector3(1, 0, 1) });
                    }

                    edgeCompareDict = null;
                });
            }
            private static void FindFloorCenter()
            {
                if (DynelManager.LocalPlayer.Room == null)
                    return;

                roomBorder = new Rect { MinY = 1000000, MinX = 1000000 };
                foreach (Room room in Playfield.Rooms)
                {
                    if (room.Floor == DynelManager.LocalPlayer.Room.Floor)
                    {
                        if (room.Rect.MaxX > roomBorder.MaxX)
                        {
                            roomBorder.MaxX = room.Rect.MaxX;
                        }
                        if (room.Rect.MaxY > roomBorder.MaxY)
                        {
                            roomBorder.MaxY = room.Rect.MaxY;
                        }
                        if (room.Rect.MinY < roomBorder.MinY)
                        {
                            roomBorder.MinY = room.Rect.MinY;
                        }
                        if (room.Rect.MinX < roomBorder.MinX)
                        {
                            roomBorder.MinX = room.Rect.MinX;
                        }
                    }
                }
                floorCenter = new Vector3((roomBorder.MaxX + roomBorder.MinX) / 2, 0, (roomBorder.MaxY + roomBorder.MinY) / 2);
            }
            private static void EdgeFilter(Vector3 vert1, Vector3 vert2, Dictionary<Edge, int> edgeCompareDict)
            {
                Edge edge = edgeCompareDict.Where(x => x.Key.V1 == vert1 && x.Key.V2 == vert2 || x.Key.V1 == vert2 && x.Key.V2 == vert1).FirstOrDefault().Key;

                if (edge == null)
                {
                    edgeCompareDict.Add(new Edge { V1 = vert1, V2 = vert2 }, 1);
                    return;
                }

                if (edgeCompareDict[edge] == 2)
                    return;

                edgeCompareDict[edge] += 1;
            }
            private static Vector3 RecalculateMesh(Vector3 roomCorner)
            {
                Vector3 CalcOffset = DynelManager.LocalPlayer.Rotation * new Vector3(1, 0, 0) * Offset.X + new Vector3(0, Offset.Y, 0);
                return DynelManager.LocalPlayer.Position + RotateY() * (RotateXZ() * (roomCorner / Scale)) + CalcOffset;
            }
            private static void DrawCube(Vector3 position, Quaternion rotation, float size, Vector3 color)
            {
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, -size, size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, size) - floorCenter), color);

                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, size, size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, size) - floorCenter), color);

                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, -size, size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, size) - floorCenter), color);
            }
            private static void DrawSquare(Vector3 position, Quaternion rotation, float size, Vector3 color)
            {
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, 0, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, 0, size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, 0, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, 0, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, 0, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, 0, size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, 0, size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, 0, size) - floorCenter), color);
            }
            private static void DrawPyramid(Vector3 position, Quaternion rotation, float size, Vector3 color)
            {
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(0, 0, size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(0, 0, size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(0, 0, size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(0, 0, size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - floorCenter), color);
                Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - floorCenter), color);
            }
            private static Quaternion RotateXZ()
            {
                return Static ? DynelManager.LocalPlayer.Rotation : new Quaternion(0, 0, 0, 0);
            }
            private static Quaternion RotateY()
            {
                return new Quaternion(DynelManager.LocalPlayer.Rotation * new Vector3(1, 0, 0), (float)(-Offset.Z * Math.PI / 180));
            }
            private class SphereEntity
            {
                public string DynelName;
                public IdentityType IdentityType;
                public float Size;
                public Vector3 Color;
            }
            private class CubeEntity
            {
                public string DynelName;
                public IdentityType IdentityType;
                public float Size;
                public Vector3 Color;
            }
            private class LineEntity
            {
                public string DynelName;
                public IdentityType IdentityType;
                public Vector3 Color;
            }
            private class PyramidEntity
            {
                public string DynelName;
                public IdentityType IdentityType;
                public float Size;
                public Vector3 Color;
            }
            private class SquareEntity
            {
                public string DynelName;
                public IdentityType IdentityType;
                public float Size;
                public Vector3 Color;
            }
            private class Edge
            {
                public Vector3 V1;
                public Vector3 V2;
            }
        }
    }
}
