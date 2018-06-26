using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    private static EntityArchetype m_PowderArchetype;

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

    private static void DefaultPopulate(EntityManager mgr)
    {
        for (var i = 0; i < 200; ++i)
        {
            for (var j = 0; j < 200; ++j)
            {
                var e = mgr.CreateEntity(m_PowderArchetype);
                var type = Random.Range(0, PowderTypes.values.Length);
                mgr.SetComponentData(e, new Powder { coord = new Vector2Int(i, j), index = i * 200 + j, type = type });
            }
        }
    }
}
