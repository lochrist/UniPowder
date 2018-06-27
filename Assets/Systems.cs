using System;
using System.CodeDom;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms2D;
using UnityEngine;
using Random = System.Random;

public class RenderCmd
{
    public int type;
    public Vector2Int coord;
}
/*
public struct NeighborsHelper
{
    public NeighborsHelper()
    {

    }

    public int left;
    public int right;
    public int top;
    public int bottom;
    public int bottomLeft;
    public int bottomRight;
    public int topRight;
    public int topLeft;
}
*/
public class SimulationSystem : ComponentSystem
{
    struct Group
    {
        public ComponentDataArray<Powder> powders;
        public ComponentDataArray<Position2D> positions;
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
            m_PowderGroup.positions[i] = new Position2D() { Value = PowderGame.CoordToWorld(m_PowderGroup.powders[i].coord) };
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
                else if (GetPowderIndex(p.coord.x - 1, p.coord.y) == -1 && PowderGame.Chance(1f/3))
                {
                    p.coord.x--;
                }
                else if (GetPowderIndex(p.coord.x + 1, p.coord.y) == -1 && PowderGame.Chance(1f / 3))
                {
                    p.coord.x++;
                }
                break;
            case PowderState.Liquid:
                {
                    if (GetPowderIndex(p.coord.x, p.coord.y - 1) == -1)
                    {
                        // Nothing below, fall
                        p.coord.y--;
                    }
                    else
                    {
                        var lowerLeftEmpty = GetPowderIndex(p.coord.x - 1, p.coord.y - 1) == -1;
                        var lowerRightEmpty = GetPowderIndex(p.coord.x + 1, p.coord.y - 1) == -1;
                        var leftEmpty = GetPowderIndex(p.coord.x - 1, p.coord.y) == -1;
                        var rightEmpty = GetPowderIndex(p.coord.x + 1, p.coord.y) == -1;
                        if (lowerLeftEmpty)
                        {
                            if (lowerRightEmpty && PowderGame.Chance(0.5f))
                            {
                                p.coord.x++;
                                p.coord.y--;
                            }
                            else
                            {
                                p.coord.x--;
                                p.coord.y--;
                            }
                        }
                        else if (lowerRightEmpty)
                        {
                            p.coord.x++;
                            p.coord.y--;
                        }
                        else if (leftEmpty && PowderGame.Chance(0.5f))
                        {
                            p.coord.x--;
                        }
                        else if (rightEmpty && PowderGame.Chance(0.5f))
                        {
                            p.coord.x++;
                        }
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
                    else if (GetPowderIndex(p.coord.x - 1, p.coord.y) == -1 &&
                                GetPowderIndex(p.coord.x - 1, p.coord.y - 1) == -1 &&
                                UnityEngine.Random.Range(0, 3) == 0)
                    {
                        p.coord.x--;
                    }
                    else if (GetPowderIndex(p.coord.x + 1, p.coord.y) == -1 &&
                        GetPowderIndex(p.coord.x + 1, p.coord.y - 1) == -1 &&
                        UnityEngine.Random.Range(0, 3) == 0)
                    {
                        p.coord.x++;
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
        if (Input.GetMouseButtonDown(1))
        {
            var coord = PowderGame.ScreenToCoord(Input.mousePosition);
            Debug.Log("MousePos: " + Input.mousePosition +
                " ScreenToWorld: " + PowderGame.mainCamera.ScreenToWorldPoint(Input.mousePosition) +
                " Coord: " + coord + 
                " World: " + PowderGame.CoordToWorld(coord)
            );
        }
        else if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && PowderGame.IsInWorld(Input.mousePosition))
        {
            var coord = PowderGame.ScreenToCoord(Input.mousePosition);
            Spawn(coord, PowderGame.currentPowder);
        }
    }

    private void Spawn(Vector2Int coord, int type)
    {
        var size = 0;
        for (var y = coord.y - PowderGame.brushSize; y <= coord.y + PowderGame.brushSize; ++y)
        {
            for (var x = coord.x - size; x <= coord.x + size; ++x)
            {
                if (!CellOccupied(x, y))
                    PowderGame.Spawn(PostUpdateCommands, x, y, type);
            }
            size += y < coord.y ? 1 : -1;
        }
    }

    private bool CellOccupied(int x, int y)
    {
        for (var i = 0; i < m_PowderGroup.Length; ++i)
        {
            if (m_PowderGroup.powders[i].coord.x == x && m_PowderGroup.powders[i].coord.y == y)
            {
                return true;
            }
        }

        return false;
    }
}
/*
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
*/