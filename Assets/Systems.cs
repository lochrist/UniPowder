using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms2D;
using UnityEngine;

public class PowderSystemUtils
{
    public static bool Chance(int chance, int seed, int addon)
    {
        return ((seed + addon) % chance) == 0;
    }

    public static int GetPowderIndex(ref NativeHashMap<int, int> hashMap, Vector2Int coord)
    {
        return GetPowderIndex(ref hashMap, coord.x, coord.y);
    }

    public static int GetPowderIndex(ref NativeHashMap<int, int> hashMap, int x, int y)
    {
        int index;
        if (hashMap.TryGetValue(PowderGame.CoordKey(x, y), out index))
        {
            return index;
        }
        return -1;
    }

    public static bool IsOccupied(ref NativeHashMap<int, int> hashMap, int x, int y)
    {
        return GetPowderIndex(ref hashMap, x, y) != -1;
    }

    public static void RemovePowder(ref EntityCommandBuffer.Concurrent cmdBuffer, ref EntityArray entities, int index)
    {
        cmdBuffer.DestroyEntity(entities[index]);
        PowderGame.powderCount--;
    }
}

public class SimulateBarrier : BarrierSystem
{ }

// [BurstCompile]
struct SimulateJob : IJobParallelFor
{
    public ComponentDataArray<Powder> powders;
    public ComponentDataArray<Position2D> positions;
    public EntityCommandBuffer.Concurrent cmdBuffer;
    [ReadOnly] public EntityArray entities;
    [ReadOnly] public int seed;
    [ReadOnly] public NativeHashMap<int, int> hashMap;

    public void Execute(int index)
    {
        powders[index] = Simulate(powders[index], seed, index);
        positions[index] = new Position2D() { Value = PowderGame.CoordToWorld(powders[index].coord) };
    }

    Powder Simulate(Powder p, int seed, int index)
    {
        if (p.life == 0 || p.coord.x < 0 || p.coord.x > PowderGame.width || p.coord.y < 0 || p.coord.y > PowderGame.height)
        {
            PowderSystemUtils.RemovePowder(ref cmdBuffer, ref entities, index);
            return p;
        }

        if (p.life != -1)
        {
            p.life--;
        }

        switch (PowderTypes.values[p.type].state)
        {
            case PowderState.Gas:
                var aboveIndex = PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x, p.coord.y + 1);
                if (aboveIndex == -1)
                {
                    p.coord.y++;
                }
                else if (PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y) == -1 && PowderSystemUtils.Chance(3, seed, index))
                {
                    p.coord.x--;
                }
                else if (PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y) == -1 && PowderSystemUtils.Chance(3, seed, index))
                {
                    p.coord.x++;
                }
                break;
            case PowderState.Liquid:
                {
                    if (PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x, p.coord.y - 1) == -1)
                    {
                        // Nothing below, fall
                        p.coord.y--;
                    }
                    else
                    {
                        var lowerLeftEmpty = PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y - 1) == -1;
                        var lowerRightEmpty = PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y - 1) == -1;
                        var leftEmpty = PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y) == -1;
                        var rightEmpty = PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y) == -1;
                        if (lowerLeftEmpty)
                        {
                            if (lowerRightEmpty && PowderSystemUtils.Chance(2, seed, index))
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
                        else if (leftEmpty && PowderSystemUtils.Chance(2, seed, index))
                        {
                            p.coord.x--;
                        }
                        else if (rightEmpty && PowderSystemUtils.Chance(2, seed, index))
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
                    var belowIndex = PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x, p.coord.y - 1);
                    if (belowIndex == -1)
                    {
                        p.coord.y--;
                    }
                    else if (PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y) == -1 && 
                        PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x - 1, p.coord.y - 1) == -1 && 
                        PowderSystemUtils.Chance(3, seed, index))
                    {
                        p.coord.x--;
                    }
                    else if (PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y) == -1 && 
                        PowderSystemUtils.GetPowderIndex(ref hashMap, p.coord.x + 1, p.coord.y - 1) == -1 && 
                        PowderSystemUtils.Chance(3, seed, index))
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

    bool IsOccupied(int x, int y)
    {
        return PowderSystemUtils.GetPowderIndex(ref hashMap, x, y) != -1;
    }

    void Sand(ref Powder p, int seed, int index)
    {

    }

    void Wood(ref Powder p, int seed, int index)
    {

    }

    void Glass(ref Powder p, int seed, int index)
    {

    }

    void Acid(ref Powder p, int seed, int index)
    {

    }

    void Water(ref Powder p, int seed, int index)
    {

    }

    void Fire(ref Powder p, int seed, int index)
    {

    }

    void Steam(ref Powder p, int seed, int index)
    {

    }

    void Stone(ref Powder p, int seed, int index)
    {

    }

    void Smoke(ref Powder p, int seed, int index)
    {

    }
}

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

struct SpawnJob : IJob
{
    public NativeHashMap<int, int> hashMap;
    [ReadOnly] public Vector2Int coord;
    public int type;
    public EntityCommandBuffer cmdBuffer;
    public void Execute()
    {
        var size = 0;
        for (var y = coord.y - PowderGame.brushSize; y <= coord.y + PowderGame.brushSize; ++y)
        {
            for (var x = coord.x - size; x <= coord.x + size; ++x)
            {
                if (!PowderSystemUtils.IsOccupied(ref hashMap, x, y))
                {
                    PowderGame.Spawn(cmdBuffer, x, y, type);
                }
            }
            size += y < coord.y ? 1 : -1;
        }
    }
}

public class SimulationSystem : JobComponentSystem
{
    struct Group
    {
        public ComponentDataArray<Powder> powders;
        public ComponentDataArray<Position2D> positions;
        [ReadOnly] public EntityArray entities;
        public int Length;
    }
    [Inject] Group m_PowderGroup;
    [Inject] SimulateBarrier m_Barrier;

    static NativeHashMap<int, int> positionsMap;
    bool m_PositionsMapAllocated;


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (m_PositionsMapAllocated)
        {
            positionsMap.Dispose();
            m_PositionsMapAllocated = false;
        }

        // Compute index
        positionsMap = new NativeHashMap<int, int>(m_PowderGroup.Length, Allocator.Temp);
        m_PositionsMapAllocated = true;

        var computeHashJob = new HashCoordJob()
        {
            powders = m_PowderGroup.powders,
            hashMap = positionsMap
        };
        var previousJobHandle = computeHashJob.Schedule(m_PowderGroup.Length, 64, inputDeps);

        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButton(0)) && PowderGame.IsInWorld(Input.mousePosition))
        {
            var coord = PowderGame.ScreenToCoord(Input.mousePosition);
            var spawnJob = new SpawnJob()
            {
                hashMap = positionsMap,
                coord = coord,
                cmdBuffer = m_Barrier.CreateCommandBuffer(),
                type = PowderGame.currentPowder
            };
            previousJobHandle = spawnJob.Schedule(previousJobHandle);
        }

        // Simulate 
        var simulateJob = new SimulateJob()
        {
            powders = m_PowderGroup.powders,
            positions = m_PowderGroup.positions,
            hashMap = positionsMap,
            seed = (int)(UnityEngine.Random.value * int.MaxValue),
            entities = m_PowderGroup.entities,
            cmdBuffer = m_Barrier.CreateCommandBuffer()
        };

        var simulateJobHandle = simulateJob.Schedule(m_PowderGroup.Length, 64, previousJobHandle);
        inputDeps = simulateJobHandle;

        return inputDeps;
    }

    protected override void OnStopRunning()
    {
        if (m_PositionsMapAllocated)
        {
            positionsMap.Dispose();
            m_PositionsMapAllocated = false;
        }
    }
}
