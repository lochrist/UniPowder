using Unity.Entities;
using UnityEngine;

public static class PowderGame
{
    public static int height = 480;
    public static int width = 640;
    public static int frame = 0;
    public static int powderCount = 0;
    public static int currentPowder = 0;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Initialize()
    {
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitializeWithScene()
    {
        var mgr = World.Active.GetOrCreateManager<EntityManager>();
        PowderTypes.Init(mgr);
    }
}
