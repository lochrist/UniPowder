using System.CodeDom;
using Unity.Entities;
using UnityEngine;

public static class PowderGame
{
    public static int height = 480;
    public static int width = 640;
    public static int frame = 0;
    public static int powderCount = 0;
    public static Rect worldRect;
    public static Color worldBoundariesColor = Color.white;
    public static EntityArchetype powderArchetype;

    public static int brushSize = 2;
    public static int currentPowder = PowderTypes.Sand;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
        var x = (Screen.width - width) / 2;
        var y = (Screen.height - height) / 2;
        worldRect = new Rect(x, y, width, height);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        var mgr = World.Active.GetOrCreateManager<EntityManager>();
        powderArchetype = mgr.CreateArchetype(typeof(Powder));
        PowderTypes.Init(mgr);

        // Put a solid ground:
        for (var x = 0; x < PowderGame.width; ++x)
        {
            for (var y = 0; y < 5; ++y)
            {
                Spawn(mgr, x, y, PowderTypes.Stone);
            }
        }
    }

    public static int CoordKey(Vector2Int coord)
    {
        return coord.x * width + coord.y;
    }

    public static int CoordKey(int x, int y)
    {
        return x * width + y;
    }

    public static bool IsInWorld(Vector2 pos)
    {
        return worldRect.Contains(pos);
    }

    public static Vector2Int ToWorldCoord(Vector2 pos)
    {
        return new Vector2Int((int)(pos.x - worldRect.x), (int)(pos.y - worldRect.y));
    }

    public static Entity Spawn(EntityManager mgr, int x, int y, int type)
    {
        var e = mgr.CreateEntity(powderArchetype);
        mgr.SetComponentData(e, PowderTypes.values[type].creator(new Vector2Int(x, y)));
        powderCount++;
        return e;
    }
}
