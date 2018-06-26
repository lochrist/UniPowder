using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class RenderCmd
{
    public int type;
    public Vector2Int coord;
}

public class SimulationSystem : ComponentSystem
{
    struct Group
    {
        public ComponentDataArray<Powder> powders;
        [ReadOnly] public EntityArray entities;
        public int Length;
    }
    [Inject] Group m_PowderGroup;

    NativeHashMap<int, int> m_PositionsMap;

    protected override void OnUpdate()
    {
        m_PositionsMap = new NativeHashMap<int, int>(m_PowderGroup.Length, Allocator.Temp);
        for (var i = 0; i < m_PowderGroup.Length; ++i)
        {
            var key = PowderGame.CoordKey(m_PowderGroup.powders[i].coord);
            m_PositionsMap.TryAdd(key, i);
        }

        for (var i = 0; i < m_PowderGroup.Length; ++i)
        {
            m_PowderGroup.powders[i] = Simulate(m_PowderGroup.powders[i], i);
        }

        m_PositionsMap.Dispose();
    }

    private Powder Simulate(Powder p, int index)
    {
        if (p.life == 0 || p.coord.x < 0 || p.coord.x > PowderGame.width || p.coord.y < 0 || p.coord.y > PowderGame.height)
        {
            RemovePowder(index);
            return p;
        }

        if (p.life != -1)
        {
            p.life--;
        }

        switch (PowderTypes.values[p.type].state)
        {
            case PowderState.Gas:
                var aboveIndex = GetPowderIndex(p.coord.x, p.coord.y + 1);
                if (aboveIndex == -1)
                {
                    p.coord.y++;
                }
                break;
            case PowderState.Liquid:
                {
                    var belowIndex = GetPowderIndex(p.coord.x, p.coord.y - 1);
                    if (belowIndex == -1)
                    {
                        p.coord.y--;
                    }
                    break;
                }
            case PowderState.Solid:
                break;
            case PowderState.Powder:
                {
                    var belowIndex = GetPowderIndex(p.coord.x, p.coord.y - 1);
                    if (belowIndex == -1)
                    {
                        p.coord.y--;
                    }
                    break;
                }
        }

        switch (p.type)
        {
            case PowderTypes.Sand:
                Sand(ref p, index);
                break;
            case PowderTypes.Acid:
                Acid(ref p, index);
                break;
            case PowderTypes.Fire:
                Fire(ref p, index);
                break;
            case PowderTypes.Glass:
                Glass(ref p, index);
                break;
            case PowderTypes.Smoke:
                Smoke(ref p, index);
                break;
            case PowderTypes.Steam:
                Steam(ref p, index);
                break;
            case PowderTypes.Stone:
                Stone(ref p, index);
                break;
            case PowderTypes.Water:
                Water(ref p, index);
                break;
            case PowderTypes.Wood:
                Wood(ref p, index);
                break;
        }


        return p;
    }

    int GetPowderIndex(Vector2Int coord)
    {
        return GetPowderIndex(coord.x, coord.y);
    }

    int GetPowderIndex(int x, int y)
    {
        int index;
        if (m_PositionsMap.TryGetValue(PowderGame.CoordKey(x, y), out index))
        {
            return index;
        }

        return -1;
    }

    void RemovePowder(int index)
    {
        PostUpdateCommands.DestroyEntity(m_PowderGroup.entities[index]);
        PowderGame.powderCount--;
    }

    void Sand(ref Powder p, int index)
    {

    }

    void Wood(ref Powder p, int index)
    {

    }

    void Glass(ref Powder p, int index)
    {

    }

    void Acid(ref Powder p, int index)
    {

    }

    void Water(ref Powder p, int index)
    {

    }

    void Fire(ref Powder p, int index)
    {

    }

    void Steam(ref Powder p, int index)
    {

    }

    void Stone(ref Powder p, int index)
    {

    }

    void Smoke(ref Powder p, int index)
    {

    }
}

[AlwaysUpdateSystem]
[UpdateAfter(typeof(SimulationSystem))]
public class SpawnSystem : ComponentSystem
{
    struct Group
    {
        public ComponentDataArray<Powder> powders;
        public int Length;
    }

    [Inject] Group m_PowderGroup;

    protected override void OnUpdate()
    {
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && PowderGame.IsInWorld(Input.mousePosition))
        {
            var pos = PowderGame.ToWorldCoord(Input.mousePosition);
            for (var i = 0; i < m_PowderGroup.Length; ++i)
            {
                if (m_PowderGroup.powders[i].coord == pos)
                {
                    return;
                }
            }

            Spawn(pos, PowderGame.currentPowder);
        }
    }

    private void Spawn(Vector2Int pos, int type)
    {
        PostUpdateCommands.CreateEntity(PowderGame.powderArchetype);
        PostUpdateCommands.SetComponent(PowderTypes.values[type].creator(pos));
        ++PowderGame.powderCount;
    }
}

[UpdateAfter(typeof(SpawnSystem))]
public class PushRenderCmdsSystem : ComponentSystem
{
    struct Group
    {
        public ComponentDataArray<Powder> powders;
        public int Length;
    }
    [Inject] Group m_PowderGroup;

    protected override void OnUpdate()
    {
        PowderRenderer.nbCmds = m_PowderGroup.Length;

        for (var i = PowderRenderer.cmds.Count; i < m_PowderGroup.Length; ++i)
        {
            PowderRenderer.cmds.Add(new RenderCmd());
        }

        for (var i = 0; i < m_PowderGroup.Length; ++i)
        {
            PowderRenderer.cmds[i].coord = m_PowderGroup.powders[i].coord;
            PowderRenderer.cmds[i].type = m_PowderGroup.powders[i].type;
        }
    }
}
