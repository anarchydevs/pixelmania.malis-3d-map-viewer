using System;
using System.Linq;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using AOSharp.Common.Unmanaged.DbObjects;

public static class DungeonMap
{
    private static Rect _roomBorder = new Rect();
    private static Vector3 _floorCenter = new Vector3();
    private static Dictionary<int, List<EdgeData>> _edgeMesh = new Dictionary<int, List<EdgeData>>();
    private static List<SphereEntity> _circles = new List<SphereEntity>();
    private static List<LineEntity> _lines = new List<LineEntity>();
    private static List<CubeEntity> _cubes = new List<CubeEntity>();
    private static List<PyramidEntity> _pyramids = new List<PyramidEntity>();
    private static List<SquareEntity> _squares = new List<SquareEntity>();
    public static List<int> FilteredZoneIds = new List<int>();
    public static Vector2 Offset = new Vector2(0.75f, 0.65f);
    public static float Scale = 250;
    public static bool IsStatic = false;
    public static bool MissionPing = false;
    private static int? _cachedFloor = null;

    public static void AddCircle(string name, float size, Vector3 color)
    {
        if (_circles.Any(x => x.DynelName == name))
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _circles.Add(new SphereEntity { DynelName = name, Size = size, Color = color });
    }

    public static void AddCircle(IdentityType identityType, float size, Vector3 color)
    {
        if (_circles.Any(x => x.IdentityType == identityType))
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _circles.Add(new SphereEntity { IdentityType = identityType, Size = size, Color = color });
    }

    public static void RemoveCircle(string name)
    {
        if (_circles.Any(x => x.DynelName == name))
        {
            Chat.WriteLine("Element not found.");
            return;
        }
        _circles.Remove(_circles.Where(x => x.DynelName == name).FirstOrDefault());
    }

    public static void RemoveCircle(IdentityType identityType)
    {
        if (_circles.Any(x => x.IdentityType == identityType))
        {
            Chat.WriteLine("Element not found.");
            return;
        }
        _circles.Remove(_circles.Where(x => x.IdentityType == identityType).FirstOrDefault());
    }

    public static void AddCube(string name, float size, Vector3 color)
    {
        if (_cubes.Any(x => x.DynelName == name))
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _cubes.Add(new CubeEntity { DynelName = name, Size = size, Color = color });
    }

    public static void AddCube(IdentityType identityType, float size, Vector3 color)
    {
        if (_cubes.Any(x => x.IdentityType == identityType))
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _cubes.Add(new CubeEntity { IdentityType = identityType, Size = size, Color = color });
    }

    public static void RemoveCube(string name)
    {
        if (_cubes.Any(x => x.DynelName == name))
        {
            Chat.WriteLine("Element not found.");
            return;
        }
        _cubes.Remove(_cubes.Where(x => x.DynelName == name).FirstOrDefault());
    }
    public static void RemoveCube(IdentityType identityType)
    {
        if (_cubes.Any(x => x.IdentityType == identityType))
        {
            Chat.WriteLine("Element not found.");
            return;
        }
        _cubes.Remove(_cubes.Where(x => x.IdentityType == identityType).FirstOrDefault());
    }
    public static void AddPyramid(string name, float size, Vector3 color)
    {
        if (_pyramids.Any(x => x.DynelName == name))
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _pyramids.Add(new PyramidEntity { DynelName = name, Size = size, Color = color });
    }
    public static void AddPyramid(IdentityType identityType, float size, Vector3 color)
    {
        if (_pyramids.Where(x => x.IdentityType == identityType).FirstOrDefault() != null)
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _pyramids.Add(new PyramidEntity { IdentityType = identityType, Size = size, Color = color });
    }
    public static void RemovePyramid(string name)
    {
        if (_pyramids.Where(x => x.DynelName == name).FirstOrDefault() == null)
        {
            Chat.WriteLine("Element not found.");
            return;
        }
        _pyramids.Remove(_pyramids.Where(x => x.DynelName == name).FirstOrDefault());
    }

    public static void RemovePyramid(IdentityType identityType)
    {
        if (_pyramids.Where(x => x.IdentityType == identityType).FirstOrDefault() == null)
        {
            Chat.WriteLine("Element not found.");
            return;
        }
        _pyramids.Remove(_pyramids.Where(x => x.IdentityType == identityType).FirstOrDefault());
    }

    public static void AddSquare(string name, float size, Vector3 color)
    {
        if (_squares.Where(x => x.DynelName == name).FirstOrDefault() != null)
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _squares.Add(new SquareEntity { DynelName = name, Size = size, Color = color });
    }
    public static void AddSquare(IdentityType identityType, float size, Vector3 color)
    {
        if (_squares.Where(x => x.IdentityType == identityType).FirstOrDefault() != null)
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _squares.Add(new SquareEntity { IdentityType = identityType, Size = size, Color = color });
    }

    public static void RemoveSquare(string name)
    {
        if (_squares.Where(x => x.DynelName == name).FirstOrDefault() == null)
        {
            Chat.WriteLine("Element not found.");
            return;
        }

        _squares.Remove(_squares.Where(x => x.DynelName == name).FirstOrDefault());
    }

    public static void RemoveSquare(IdentityType identityType)
    {
        if (_squares.Where(x => x.IdentityType == identityType).FirstOrDefault() == null)
        {
            Chat.WriteLine("Element not found.");
            return;
        }
        _squares.Remove(_squares.Where(x => x.IdentityType == identityType).FirstOrDefault());
    }

    public static void AddLine(string name, Vector3 color)
    {
        if (_lines.Where(x => x.DynelName == name).FirstOrDefault() != null)
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _lines.Add(new LineEntity { DynelName = name, Color = color });
    }

    public static void AddLine(IdentityType identityType, Vector3 color)
    {
        if (_lines.Where(x => x.IdentityType == identityType).FirstOrDefault() != null)
        {
            Chat.WriteLine("Legend element already exists.");
            return;
        }

        _lines.Add(new LineEntity { IdentityType = identityType, Color = color });
    }

    public static void RemoveLine(string name)
    {
        if (_lines.Where(x => x.DynelName == name).FirstOrDefault() == null)
        {
            Chat.WriteLine("Element not found.");
            return;
        }

        _lines.Remove(_lines.Where(x => x.DynelName == name).FirstOrDefault());
    }

    public static void RemoveLine(IdentityType identityType)
    {
        if (_lines.Where(x => x.IdentityType == identityType).FirstOrDefault() == null)
        {
            Chat.WriteLine("Element not found.");
            return;
        }

        _lines.Remove(_lines.FirstOrDefault(x => x.IdentityType == identityType));
    }

    internal static void RenderMap(Vector3 mapColor, bool showLegend)
    {
        if (FilteredZoneIds.Contains(Playfield.ModelIdentity.Instance))
            return;

        if (_edgeMesh.Count == 0)
            return;

        if (DynelManager.LocalPlayer.Room == null)
            return;

        if (Camera.Pointer == IntPtr.Zero)
            return;

        DrawMap(mapColor);

        if (!showLegend)
            return;

        DrawLegend();
    }
    private static void DrawMap(Vector3 color)
    {
        FindFloorCenter();

        foreach (EdgeData edgeData in _edgeMesh[DynelManager.LocalPlayer.Room.Floor].ToList())
        {

            if (!edgeData.Visited)
            {
                if (DynelManager.LocalPlayer.Room.Instance == edgeData.Instance)
                    edgeData.Visited = true;
            }

            Vector3 colors = edgeData.Visited ? DebuggingColor.Green : DebuggingColor.Yellow;

            foreach (var edge in edgeData.Edge)
            {
                if (!(edge.V1.X > _roomBorder.MinX - 1 && edge.V1.X < _roomBorder.MaxX + 1))
                    continue;

                Debug.DrawLine(RecalculateMesh(edge.V1 - _floorCenter), RecalculateMesh(edge.V2 - _floorCenter), colors);
            }
        }
    }

    private unsafe static void DrawLegend()
    {
        Mission mission = Mission.List.FirstOrDefault(x => (*(MissionMemStruct*)x.Pointer).Playfield == Playfield.ModelIdentity);
        Dynel mishDynel = null;

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
                    RecalculateMesh(DynelManager.LocalPlayer.Position * new Vector3(1, 0, 1) - _floorCenter),
                    RecalculateMesh(dynelPosition - _floorCenter),
                    DebuggingColor.Purple);
                DrawCube(dynelPosition, dynel.Rotation, 2f, DebuggingColor.Green);
                continue;
            }

            foreach (SphereEntity sphereEntity in _circles)
            {
                if (dynel.Name != sphereEntity.DynelName && sphereEntity.IdentityType != dynel.Identity.Type)
                    continue;

                if (dynel.Name == DynelManager.LocalPlayer.Name && sphereEntity.DynelName != DynelManager.LocalPlayer.Name)
                    continue;

                int scale = dynel.Identity.Type == IdentityType.SimpleChar ? dynel.GetStat(Stat.Scale) / 100 : 1;
                float size = (sphereEntity.Size / 2) * 1;
                Debug.DrawSphere(RecalculateMesh(dynelPosition - _floorCenter), size, sphereEntity.Color);
            }

            foreach (LineEntity lineEntity in _lines)
            {
                if (dynel.Name != lineEntity.DynelName && lineEntity.IdentityType != dynel.Identity.Type)
                    continue;

                if (dynel.Name == DynelManager.LocalPlayer.Name && lineEntity.DynelName != DynelManager.LocalPlayer.Name)
                    continue;

                Debug.DrawLine(
                    RecalculateMesh(DynelManager.LocalPlayer.Position * new Vector3(1, 0, 1) - _floorCenter),
                    RecalculateMesh(dynelPosition - _floorCenter),
                    lineEntity.Color);
            }

            foreach (CubeEntity cubeEntity in _cubes)
            {
                if (dynel.Name != cubeEntity.DynelName && cubeEntity.IdentityType != dynel.Identity.Type)
                    continue;

                if (dynel.Name == DynelManager.LocalPlayer.Name && cubeEntity.DynelName != DynelManager.LocalPlayer.Name)
                    continue;

                int scale = dynel.Identity.Type == IdentityType.SimpleChar ? dynel.GetStat(Stat.Scale) / 100 : 1;
                float size = (cubeEntity.Size / 2) * 1;
                DrawCube(dynelPosition, dynel.Rotation, size, cubeEntity.Color);
            }

            foreach (PyramidEntity pyramidEntity in _pyramids)
            {
                if (dynel.Name != pyramidEntity.DynelName && pyramidEntity.IdentityType != dynel.Identity.Type)
                    continue;

                if (dynel.Name == DynelManager.LocalPlayer.Name && pyramidEntity.DynelName != DynelManager.LocalPlayer.Name)
                    continue;

                int scale = dynel.Identity.Type == IdentityType.SimpleChar ? dynel.GetStat(Stat.Scale) / 100 : 1;
                float size = (pyramidEntity.Size / 2) * 1;
                DrawPyramid(dynelPosition, dynel.Rotation, size, pyramidEntity.Color);
            }

            foreach (SquareEntity squareEntity in _squares)
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

    internal static void CreateMeshTask()
    {
        if (FilteredZoneIds.Contains(Playfield.ModelIdentity.Instance))
            return;

        if (DynelManager.LocalPlayer.Room == null)
            return;

        _cachedFloor = null;

        Task meshBorderTask = new Task(() => FindMeshBorder(CreateDungeonMesh(), 2000));
        meshBorderTask.Start();
    }

    private static Dictionary<int, List<MeshData>> CreateDungeonMesh()
    {
        DungeonRDBTilemap tilemap = Playfield.RDBTilemap as DungeonRDBTilemap;
        Dictionary<int, List<MeshData>> meshList = new Dictionary<int, List<MeshData>>();

        foreach (Room room in Playfield.Rooms)
        {
            if (!meshList.ContainsKey(room.Floor))
                meshList.Add(room.Floor, new List<MeshData>());

            int num = (int)room.LocalRect.MaxX - (int)room.LocalRect.MinX;
            int num2 = (int)room.LocalRect.MaxY - (int)room.LocalRect.MinY;
            float num3 = (float)num * tilemap.TileSize;
            float num4 = (float)num2 * tilemap.TileSize;
            int num5 = num + 1;
            int num6 = num2 + 1;
            Vector3[] array = new Vector3[num5 * num6];
            List<int> list = new List<int>();
            Vector3 vector = new Vector3(1f, 0f, 1f);
            vector.X += (room.Center.X - (float)num / 2f) * tilemap.TileSize;
            vector.Z += (room.Center.Z - (float)num2 / 2f) * tilemap.TileSize;
            int num7 = 0;
            for (int i = 0; i < num6; i++)
            {
                for (int j = 0; j < num5; j++)
                {
                    byte b = tilemap.Heightmap[j + (int)room.LocalRect.MinX - 1, i + (int)room.LocalRect.MinY - 1];
                    Vector3 vector2 = default(Vector3);
                    vector2.X = (float)j * tilemap.TileSize - num3 / 2f;
                    vector2.Y = (float)(int)b * tilemap.HeightmapScale;
                    vector2.Z = (float)i * tilemap.TileSize - num4 / 2f;
                    Vector3 vector3 = vector2;
                    vector3.X -= vector.X;
                    vector3.Z -= vector.Z;
                    array[num7] = vector3;
                    num7++;
                }
            }

            num7 = 0;
            for (int k = 0; k < num2; k++)
            {
                for (int l = 0; l < num; l++)
                {
                    byte b2 = tilemap.CollisionData[l + (int)room.LocalRect.MinX, k + (int)room.LocalRect.MinY];
                    if (b2 > 0 && b2 != 128)
                    {
                        list.Add(k * num5 + l);
                        list.Add((k + 1) * num5 + l);
                        list.Add(k * num5 + l + 1);
                        list.Add((k + 1) * num5 + l);
                        list.Add((k + 1) * num5 + l + 1);
                        list.Add(k * num5 + l + 1);
                    }

                    num7 += 6;
                }
            }

            List<Vector3> list2 = array.ToList();
            List<int> list3 = list;
            int num8 = 0;
            while (num8 < list2.Count)
            {
                if (list3.Contains(num8))
                {
                    num8++;
                    continue;
                }

                list2.RemoveAt(num8);
                for (int m = 0; m < list3.Count; m++)
                {
                    if (list3[m] > num8)
                    {
                        list3[m]--;
                    }
                }
            }

            meshList[room.Floor].Add(new MeshData
            {
                Triangles = list3,
                Vertices = list2,
                Position = room.Position - new Vector3(0f, room.YOffset, 0f),
                Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, (double)room.Rotation * (Math.PI / 180.0)),
                Scale = new Vector3(1f, 1f, 1f),
                Instance = room.Instance
            });
        }
        return meshList;
    }

    private static void FindMeshBorder(Dictionary<int, List<MeshData>> meshDict, int maxDrawDist)
    {
        FindFloorCenter();

        ConcurrentDictionary<int, ConcurrentBag<EdgeData>> edgeMesh = new ConcurrentDictionary<int, ConcurrentBag<EdgeData>>();

        foreach (var meshPerFloor in meshDict)
        {
            var meshes = meshPerFloor.Value;

            edgeMesh.TryAdd(meshPerFloor.Key, new ConcurrentBag<EdgeData>());

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

                EdgeData edgeData = new EdgeData();
                edgeData.Edge = new List<Edge>();
                edgeData.Visited = false;
                edgeData.Instance = mesh.Instance;
                foreach (var edge in edgeCompareDict.Where(x => x.Value == 1).ToList())
                {
                    var bb = new Edge { V1 = (edge.Key.V1) * new Vector3(1, 0, 1), V2 = (edge.Key.V2) * new Vector3(1, 0, 1) };
                    edgeData.Edge.Add(bb);
                }

                edgeMesh[meshPerFloor.Key].Add(edgeData);

                edgeCompareDict = null;
            });
        }
        _edgeMesh = edgeMesh.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToList());
    }

    public class EdgeData
    {
        public List<Edge> Edge;
        public int Instance;
        public bool Visited = false;
    }

    private static void FindFloorCenter()
    {
        if (DynelManager.LocalPlayer.Room == null)
            return;

        if (_cachedFloor == DynelManager.LocalPlayer.Room.Floor)
            return;

        _roomBorder = new Rect { MinY = 1000000, MinX = 1000000 };

        foreach (Room room in Playfield.Rooms.Where(x=>x.Floor == DynelManager.LocalPlayer.Room.Floor))
        {
            if (room.Rect.MaxX > _roomBorder.MaxX)
                _roomBorder.MaxX = room.Rect.MaxX;
            if (room.Rect.MaxY > _roomBorder.MaxY)
                _roomBorder.MaxY = room.Rect.MaxY;
            if (room.Rect.MinY < _roomBorder.MinY)
                _roomBorder.MinY = room.Rect.MinY;
            if (room.Rect.MinX < _roomBorder.MinX)
                _roomBorder.MinX = room.Rect.MinX;
        }
        _floorCenter = new Vector3((_roomBorder.MaxX + _roomBorder.MinX) / 2, 0, (_roomBorder.MaxY + _roomBorder.MinY) / 2);

        _cachedFloor = DynelManager.LocalPlayer.Room.Floor;
    }

    private static void EdgeFilter(Vector3 vert1, Vector3 vert2, Dictionary<Edge, int> edgeCompareDict)
    {
        Edge edge = edgeCompareDict
            .Where(x => x.Key.V1 == vert1 && x.Key.V2 == vert2 || 
                        x.Key.V1 == vert2 && x.Key.V2 == vert1)
            .FirstOrDefault().Key;

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
        Vector3 CalcOffset = Camera.Rotation * (Vector3.Forward * 1 + Vector3.Up * Offset.Y + Vector3.Right * Offset.X);
        return Camera.Position + (RotateY() * (RotateXZ() * (roomCorner / Scale)) + CalcOffset);
    }

    private static void DrawCube(Vector3 position, Quaternion rotation, float size, Vector3 color)
    {
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, -size, size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, size) - _floorCenter), color);

        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, size, size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, size) - _floorCenter), color);

        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, -size, size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, size) - _floorCenter), color);
    }

    private static void DrawSquare(Vector3 position, Quaternion rotation, float size, Vector3 color)
    {
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, 0, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, 0, size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, 0, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, 0, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, 0, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, 0, size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, 0, size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, 0, size) - _floorCenter), color);
    }

    private static void DrawPyramid(Vector3 position, Quaternion rotation, float size, Vector3 color)
    {
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(0, 0, size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(0, 0, size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(0, 0, size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(0, 0, size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(-size, -size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(-size, size, -size) - _floorCenter), color);
        Debug.DrawLine(RecalculateMesh(position + rotation * new Vector3(size, -size, -size) - _floorCenter), RecalculateMesh(position + rotation * new Vector3(size, size, -size) - _floorCenter), color);
    }
    private static Quaternion RotateXZ()
    {
        return IsStatic ? Camera.Rotation : new Quaternion(0,0,0,0);
    }

    private static Quaternion RotateY()
    {

        return IsStatic? new Quaternion(Camera.Rotation * Vector3.Right, (float)((-90 * Math.PI / 180))):new Quaternion(Camera.Rotation * Vector3.Right, (float)((-30 * Math.PI / 180)));
    }

    public class MeshData : Mesh
    {
        public int Instance;
    }

    public class Config
    {
        public Vector3 Offset;
        public float Scale;
        public bool Static;
        public bool MissionPing;
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

    public class Edge
    {
        public Vector3 V1;
        public Vector3 V2;
    }

    [StructLayout(LayoutKind.Explicit, Pack = 0, Size = 0x100)]
    private struct MissionMemStruct
    {
        [FieldOffset(0xB4)]
        public Identity Playfield;
    }
}