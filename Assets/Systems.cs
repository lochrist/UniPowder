using System;
using System.CodeDom;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Transforms2D;
using UnityEditor;
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
public class SimulationSystem : JobComponentSystem // ComponentSystem // JobComponentSystem
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
    bool m_PositionsMapAllocated;
    /*
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
    */

    // [BurstCompile]
    struct HashCoordJob : IJobParallelFor
    {
        [ReadOnly] public ComponentDataArray<Powder> powders;
        public NativeHashMap<int, int>.Concurrent hashMap;

        public void Execute(int index)
        {
            var key = PowderGame.CoordKey(powders[index].coord);
            hashMap.TryAdd(key, index);
        }
    }

    // [BurstCompile]
    struct SimulateJob : IJobParallelFor
    {
        public ComponentDataArray<Powder> powders;
        public ComponentDataArray<Position2D> positions;
        [ReadOnly] public int seed;
        [ReadOnly] public NativeHashMap<int, int> hashMap;

        public void Execute(int index)
        {
            powders[index] = Simulate(ref hashMap, powders[index], seed, index);
            positions[index] = new Position2D() { Value = PowderGame.CoordToWorld(powders[index].coord) };
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (m_PositionsMapAllocated)
        {
            m_PositionsMap.Dispose();
            m_PositionsMapAllocated = false;
        }

        // Compute index
        m_PositionsMap = new NativeHashMap<int, int>(m_PowderGroup.Length, Allocator.Temp);
        m_PositionsMapAllocated = true;

        var computeHashJob = new HashCoordJob()
        {
            powders = m_PowderGroup.powders,
            hashMap = m_PositionsMap
        };
        var hashJobHandle = computeHashJob.Schedule(m_PowderGroup.Length, 64, inputDeps);

        // Simulate 
        var simulateJob = new SimulateJob()
        {
            powders = m_PowderGroup.powders,
            positions = m_PowderGroup.positions,
            hashMap = m_PositionsMap,
            seed = (int)(UnityEngine.Random.value * int.MaxValue)
        };

        var simulateJobHandle = simulateJob.Schedule(m_PowderGroup.Length, 64, hashJobHandle);
        inputDeps = simulateJobHandle;

        return inputDeps;
    }

    protected override void OnStopRunning()
    {
        if (m_PositionsMapAllocated)
        {
            m_PositionsMap.Dispose();
            m_PositionsMapAllocated = false;
        }
    }

    private static bool Chance(int chance, int seed, int addon)
    {
        return ((seed + addon) % chance) == 0;
    }

    private static Powder Simulate(ref NativeHashMap<int, int> hashMap, Powder p, int seed, int index)
    {
        if (p.life == 0 || p.coord.x < 0 || p.coord.x > PowderGame.width || p.coord.y < 0 || p.coord.y > PowderGame.height)
        {
            // RemovePowder(index);
            return p;
        }

        if (p.life != -1)
        {
            p.life--;
        }

        switch (PowderTypes.values[p.type].state)
        {
            case PowderState.Gas:
                var aboveIndex = GetPowderIndex(ref hashMap, p.coord.x, p.coord.y + 1);
                if (aboveIndex == -1)
                {
                    p.coord.y++;
                }
                else if (GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y) == -1 && Chance(3, seed, index))
                {
                    p.coord.x--;
                }
                else if (GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y) == -1 && Chance(3, seed, index))
                {
                    p.coord.x++;
                }
                break;
            case PowderState.Liquid:
                {
                    if (GetPowderIndex(ref hashMap, p.coord.x, p.coord.y - 1) == -1)
                    {
                        // Nothing below, fall
                        p.coord.y--;
                    }
                    else
                    {
                        var lowerLeftEmpty = GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y - 1) == -1;
                        var lowerRightEmpty = GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y - 1) == -1;
                        var leftEmpty = GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y) == -1;
                        var rightEmpty = GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y) == -1;
                        if (lowerLeftEmpty)
                        {
                            if (lowerRightEmpty && Chance(2, seed, index))
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
                        else if (leftEmpty && Chance(2, seed, index))
                        {
                            p.coord.x--;
                        }
                        else if (rightEmpty && Chance(2, seed, index))
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
                    var belowIndex = GetPowderIndex(ref hashMap, p.coord.x, p.coord.y - 1);
                    if (belowIndex == -1)
                    {
                        p.coord.y--;
                    }
                    else if (GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y) == -1 &&
                                GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y - 1) == -1 &&
                                Chance(3, seed, index))
                    {
                        p.coord.x--;
                    }
                    else if (GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y) == -1 &&
                        GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y - 1) == -1 &&
                        Chance(3, seed, index))
                    {
                        p.coord.x++;
                    }
                    break;
                }
        }

        switch (p.type)
        {
            case PowderTypes.Sand:
                Sand(ref p, seed, index);
                break;
            case PowderTypes.Acid:
                Acid(ref p, seed, index);
                break;
            case PowderTypes.Fire:
                Fire(ref p, seed, index);
                break;
            case PowderTypes.Glass:
                Glass(ref p, seed, index);
                break;
            case PowderTypes.Smoke:
                Smoke(ref p, seed, index);
                break;
            case PowderTypes.Steam:
                Steam(ref p, seed, index);
                break;
            case PowderTypes.Stone:
                Stone(ref p, seed, index);
                break;
            case PowderTypes.Water:
                Water(ref p, seed, index);
                break;
            case PowderTypes.Wood:
                Wood(ref p, seed, index);
                break;
        }
        return p;
    }

    static int GetPowderIndex(ref NativeHashMap<int, int> hashMap, Vector2Int coord)
    {
        return GetPowderIndex(ref hashMap, coord.x, coord.y);
    }

    static int GetPowderIndex(ref NativeHashMap<int, int> hashMap, int x, int y)
    {
        int index;
        if (hashMap.TryGetValue(PowderGame.CoordKey(x, y), out index))
        {
            return index;
        }

        return -1;
    }

    void RemovePowder(int index)
    {
        // PostUpdateCommands.DestroyEntity(m_PowderGroup.entities[index]);

        PowderGame.powderCount--;
    }

    static void Sand(ref Powder p, int seed, int index)
    {

    }

    static void Wood(ref Powder p, int seed, int index)
    {

    }

    static void Glass(ref Powder p, int seed, int index)
    {

    }

    static void Acid(ref Powder p, int seed, int index)
    {

    }

    static void Water(ref Powder p, int seed, int index)
    {

    }

    static void Fire(ref Powder p, int seed, int index)
    {

    }

    static void Steam(ref Powder p, int seed, int index)
    {

    }

    static void Stone(ref Powder p, int seed, int index)
    {

    }

    static void Smoke(ref Powder p, int seed, int index)
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

    ComponentDataArray<Powder> m_Powders;
    ComponentGroup m_PowderGroup;

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
            m_Powders = m_PowderGroup.GetComponentDataArray<Powder>();
            var coord = PowderGame.ScreenToCoord(Input.mousePosition);
            Spawn(coord, PowderGame.currentPowder);
        }
    }

    protected override void OnCreateManager(int capacity)
    {
        m_PowderGroup = GetComponentGroup(ComponentType.ReadOnly(typeof(Powder)));
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
        for (var i = 0; i < m_Powders.Length; ++i)
        {
            if (m_Powders[i].coord.x == x && m_Powders[i].coord.y == y)
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