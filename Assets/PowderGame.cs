using System;
using System.CodeDom;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Transforms2D;
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
    public static Camera mainCamera;

    public static Vector3 strangeOffset = new Vector3(350, 230, 0);

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
        powderArchetype = mgr.CreateArchetype(typeof(Powder), typeof(Position2D), typeof(TransformMatrix), typeof(Heading2D));
        PowderTypes.Init(mgr);

        var proto = GameObject.Find("Main Camera");
        if (proto == null)
        {
            throw new Exception("Cannot find Main Camera");
        }
        mainCamera =  proto.GetComponent<Camera>();

        // Put a solid ground:
        for (var x = 0; x < width; ++x)
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

    public static Vector2Int ToWorldCoord(Vector3 pos)
    {
        return new Vector2Int((int)(pos.x - worldRect.x), (int)(pos.y - worldRect.y));
        // var p = PowderGame.mainCamera.ScreenToWorldPoint(pos);
        // pos -= strangeOffset;
        // return new Vector2Int((int)pos.x, (int)pos.y);
    }

    public static Entity Spawn(EntityManager mgr, int x, int y, int type)
    {
        var e = mgr.CreateEntity(powderArchetype);
        mgr.SetComponentData(e, PowderTypes.values[type].creator(new Vector2Int(x, y)));
        mgr.SetComponentData(e, new Position2D { Value = new float2(x, y) });
        mgr.SetComponentData(e, new Heading2D { Value = new float2(0.0f, 1.0f) });
        // mgr.AddSharedComponentData(e, PowderTypes.values[type].renderer);
        powderCount++;
        return e;
    }

    public static void Spawn(EntityCommandBuffer cmdBuffer, int x, int y, int type)
    {
        cmdBuffer.CreateEntity(PowderGame.powderArchetype);
        cmdBuffer.SetComponent(PowderTypes.values[type].creator(new Vector2Int(x, y)));
        cmdBuffer.SetComponent(new Position2D { Value = new float2(x, y) });
        cmdBuffer.SetComponent(new Heading2D { Value = new float2(0.0f, 1.0f) });
        // cmdBuffer.AddSharedComponent(PowderTypes.values[type].renderer);
        powderCount++;
    }
}
