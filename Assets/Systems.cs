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

    public static bool IsEmpty(ref NativeHashMap<int, int> hashMap, int x, int y)
    {
        return GetPowderIndex(ref hashMap, x, y) == -1;
    }

    public static void SchdeduleRemovePowder(ref NativeHashMap<int, int>.Concurrent toRemove, int index)
    {
        toRemove.TryAdd(index, index);
    }
}

struct Neighbors
{
    public Powder p;
    public int index;
    public NativeHashMap<int, int> positions;
    int top;
    int topLeft;
    int topRight;
    int left;
    int right;
    int bottom;
    int bottomLeft;
    int bottomRight;
    public Neighbors(ref NativeHashMap<int, int> positions, Powder p, int index)
    {
        this.p = p;
        this.index = index;
        this.positions = positions;
        top = topLeft = topRight = left = right = bottomRight = bottomLeft = bottom = -2;
    }

    public bool TopEmpty()
    {
        return Top() == -1;
    }

    public int Top()
    {
        if (top == -2)
            top = PowderSystemUtils.GetPowderIndex(ref positions, p.coord.x, p.coord.y + 1);
        return top;
    }

    public bool TopLeftEmpty()
    {
        return TopLeft() == -1;
    }

    public int TopLeft()
    {
        if (topLeft == -2)
            topLeft = PowderSystemUtils.GetPowderIndex(ref positions, p.coord.x - 1, p.coord.y + 1);
        return topLeft;
    }

    public bool TopRightEmpty()
    {
        return TopRight() == -1;
    }

    public int TopRight()
    {
        if (topRight == -2)
            topRight = PowderSystemUtils.GetPowderIndex(ref positions, p.coord.x + 1, p.coord.y + 1);
        return topRight;
    }

    public bool LeftEmpty()
    {
        return Left() == -1;
    }

    public int Left()
    {
        if (left == -2)
            left = PowderSystemUtils.GetPowderIndex(ref positions, p.coord.x - 1, p.coord.y);
        return left;
    }

    public bool RightEmpty()
    {
        return Right() == -1;
    }

    public int Right()
    {
        if (right == -2)
            right = PowderSystemUtils.GetPowderIndex(ref positions, p.coord.x + 1, p.coord.y);
        return right;
    }

    public bool BottomEmpty()
    {
        return Bottom() == -1;
    }

    public int Bottom()
    {
        if (bottom == -2)
            bottom = PowderSystemUtils.GetPowderIndex(ref positions, p.coord.x, p.coord.y - 1);
        return bottom;
    }

    public bool BottomLeftEmpty()
    {
        return BottomLeft() == -1;
    }

    public int BottomLeft()
    {
        if (bottomLeft == -2)
            bottomLeft = PowderSystemUtils.GetPowderIndex(ref positions, p.coord.x - 1, p.coord.y - 1);
        return bottomLeft;
    }

    public bool BottomRightEmpty()
    {
        return BottomRight() == -1;
    }

    public int BottomRight()
    {
        if (bottomRight == -2)
            bottomRight = PowderSystemUtils.GetPowderIndex(ref positions, p.coord.x + 1, p.coord.y - 1);
        return bottomRight;
    }
}

struct Rand
{
    // Uses LCG
    public int seed;
    public const int a = 1664525;
    public const int c = 1013904223;

    public Rand(int seed)
    {
        this.seed = seed;
    }

    public int NextInt()
    {
        seed = (seed * a + c) % int.MaxValue;
        return seed;
    }

    public bool Chance(int chance)
    {
        return (NextInt() % chance) == 0;
    }

    public int Range(int min, int max)
    {
        return min + NextInt() % (max - min);
    }

    public static Rand Create()
    {
        return new Rand(UnityEngine.Random.Range(0, int.MaxValue));
    }
}

public class SimulateBarrier : BarrierSystem
{ }

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
    [ReadOnly] public NativeHashMap<int, int> hashMap;
    [ReadOnly] public Vector2Int coord;
    public int type;
    public EntityCommandBuffer cmdBuffer;
    public bool isPainting;
    public void Execute()
    {
        if (PowderTypes.values[type].IsGenerator())
        {
            if (!isPainting)
            {
                for (var x = coord.x - PowderGame.brushSize; x <= coord.x + PowderGame.brushSize; ++x)
                {
                    if (!PowderSystemUtils.IsOccupied(ref hashMap, x, coord.y))
                    {
                        PowderGame.Spawn(ref cmdBuffer, x, coord.y, type);
                    }
                }
            }
        }
        else
        {
            var size = 0;
            for (var y = coord.y - PowderGame.brushSize; y <= coord.y + PowderGame.brushSize; ++y)
            {
                for (var x = coord.x - size; x <= coord.x + size; ++x)
                {
                    if (!PowderSystemUtils.IsOccupied(ref hashMap, x, y))
                    {
                        PowderGame.Spawn(ref cmdBuffer, x, y, type);
                    }
                }
                size += y < coord.y ? 1 : -1;
            }
        }
    }
}

// [BurstCompile]
struct SimulateJob : IJobParallelFor
{
    [ReadOnly] public ComponentDataArray<Powder> powders;
    public ComponentDataArray<Position2D> positions;
    public EntityCommandBuffer.Concurrent cmdBuffer;
    [ReadOnly] public EntityArray entities;
    [ReadOnly] public Rand rand;
    [ReadOnly] public NativeHashMap<int, int> hashMap;
    public NativeHashMap<int, int>.Concurrent toDeleteEntities;

    public void Execute(int index)
    {
        var updatedPowder = Simulate(powders[index], index);
        if (!powders[index].Same(updatedPowder))
        {
            cmdBuffer.SetComponent(entities[index], updatedPowder);
            positions[index] = new Position2D() { Value = PowderGame.CoordToWorld(powders[index].coord) };
        }
    }

    Powder Simulate(Powder p, int index)
    {
        if (p.life == 0 || p.coord.x < 0 || p.coord.x > PowderGame.width || p.coord.y < 0 || p.coord.y > PowderGame.height)
        {
            PowderSystemUtils.SchdeduleRemovePowder(ref toDeleteEntities, index);
            return p;
        }

        if (p.life != -1)
        {
            p.life--;
        }

        var n = new Neighbors(ref hashMap, p, index);
        SimulateState(ref p, ref n);

        switch (p.type)
        {
            case PowderTypes.Acid:
                Acid(ref p, ref n);
                break;
            case PowderTypes.Fire:
                Fire(ref p, ref n);
                break;
            case PowderTypes.Steam:
                Steam(ref p, ref n);
                break;
            case PowderTypes.Water:
                Water(ref p, ref n);
                break;
            case PowderTypes.Lava:
                Lava(ref p, ref n);
                break;
            default:
                if (PowderTypes.values[p.type].IsGenerator())
                {
                    Generate(ref p, ref n);
                }
                break;
        }
        return p;
    }

    void SimulateState(ref Powder p, ref Neighbors n)
    {
        switch (PowderTypes.values[p.type].state)
        {
            case PowderState.Gas:
                if (n.TopEmpty())
                {
                    if (n.TopLeftEmpty() && rand.Chance(3))
                    {
                        if (n.TopRightEmpty() && rand.Chance(3))
                        {
                            p.coord.y++;
                            p.coord.x++;
                        }
                        else
                        {
                            p.coord.y++;
                            p.coord.x--;
                        }
                    }
                    else
                    {
                        p.coord.y++;
                    }
                }
                else if (n.TopLeftEmpty())
                {
                    if (n.TopRightEmpty() && rand.Chance(3))
                    {
                        p.coord.y++;
                        p.coord.x++;
                    }
                    else
                    {
                        p.coord.y++;
                        p.coord.x--;
                    }
                }
                else if (n.TopRightEmpty())
                {
                    p.coord.y++;
                    p.coord.x++;
                }
                break;
            case PowderState.Liquid:
                {
                    if (n.BottomEmpty())
                    {
                        if (n.BottomLeftEmpty() && rand.Chance(5))
                        {
                            if (n.BottomRightEmpty() && rand.Chance(2))
                            {
                                // Nothing below, fall
                                p.coord.x++;
                                p.coord.y--;
                            }
                            else
                            {
                                p.coord.x--;
                                p.coord.y--;
                            }
                        }
                        else
                        {
                            p.coord.y--;
                        }
                    }
                    else if (n.BottomLeftEmpty())
                    {
                        if (n.BottomRightEmpty() && rand.Chance(2))
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
                    else if (n.BottomRightEmpty())
                    {
                        p.coord.x++;
                        p.coord.y--;
                    }
                    else if (n.LeftEmpty() && rand.Chance(2))
                    {
                        p.coord.x--;
                    }
                    else if (n.RightEmpty() && rand.Chance(2))
                    {
                        p.coord.x++;
                    }
                    break;
                }
            case PowderState.Solid:
                break;
            case PowderState.Powder:
                {
                    if (n.BottomEmpty())
                    {
                        if (n.BottomLeftEmpty() && rand.Chance(3))
                        {
                            if (n.BottomRightEmpty() && rand.Chance(2))
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
                        else
                        {
                            p.coord.y--;
                        }
                    }
                    else if (n.BottomLeftEmpty())
                    {
                        if (n.BottomRightEmpty() && rand.Chance(2))
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
                    else if (n.BottomRightEmpty())
                    {
                        p.coord.x++;
                        p.coord.y--;
                    }
                    break;
                }
        }
    }

    void Acid(ref Powder p, ref Neighbors n)
    {
        if (!n.LeftEmpty())
            AcidTouch(n.Left());
        if (!n.RightEmpty())
            AcidTouch(n.Right());
        if (!n.BottomEmpty())
            AcidTouch(n.Bottom());
    }

    void AcidTouch(int index)
    {
        if (powders[index].type != PowderTypes.Acid &&
            powders[index].type != PowderTypes.Fire &&
            powders[index].type != PowderTypes.Water &&
            powders[index].type != PowderTypes.Glass &&
            powders[index].type != PowderTypes.Lava
        )
        {
            PowderSystemUtils.SchdeduleRemovePowder(ref toDeleteEntities, index);
        }
    }

    void Water(ref Powder p, ref Neighbors n)
    {
        if (!n.BottomEmpty())
        {
            if (powders[n.Bottom()].type == PowderTypes.Fire || powders[n.Bottom()].type == PowderTypes.Steam)
            {
                PowderSystemUtils.SchdeduleRemovePowder(ref toDeleteEntities, n.Bottom());
            }
            else if (powders[n.Bottom()].type == PowderTypes.Lava)
            {
                ChangeElement(n.index, PowderTypes.Steam);
            }
        }
    }

    void Fire(ref Powder p, ref Neighbors n)
    {
        if (!n.TopEmpty())
        {
            if (powders[n.Top()].type == PowderTypes.Wood)
            {
                ChangeElement(n.Top(), rand.Chance(2) ? PowderTypes.Fire : PowderTypes.Steam);
            }
            else if (powders[n.Top()].type == PowderTypes.Sand)
            {
                ChangeElement(n.Top(), PowderTypes.Glass);
            }
        }
    }

    void Steam(ref Powder p, ref Neighbors n)
    {
        if (!n.TopEmpty() && powders[n.Top()].type == PowderTypes.Stone)
        {
            ChangeElement(n.index, PowderTypes.Water);
        }
    }

    void Lava(ref Powder p, ref Neighbors n)
    {
        if (!n.BottomEmpty() && 
            powders[n.Bottom()].type != PowderTypes.Fire && 
            powders[n.Bottom()].type != PowderTypes.Lava &&
            powders[n.Bottom()].type != PowderTypes.Stone
            )
        {
            PowderSystemUtils.SchdeduleRemovePowder(ref toDeleteEntities, n.Bottom());
        }
    }

    void Generate(ref Powder p, ref Neighbors n)
    {
        var generatedType = PowderTypes.values[p.type].generatedElementType;
        if (PowderTypes.values[generatedType].state == PowderState.Liquid || PowderTypes.values[generatedType].state == PowderState.Powder)
        {
            if (n.TopEmpty())
            {
                var generatedPowder = PowderTypes.values[generatedType].creator(new Vector2Int(p.coord.x, p.coord.y - 1));
                PowderGame.Spawn(ref cmdBuffer, generatedPowder);
            }
        }
        else if (n.BottomEmpty())
        {
            var generatedPowder = PowderTypes.values[generatedType].creator(new Vector2Int(p.coord.x, p.coord.y + 1));
            PowderGame.Spawn(ref cmdBuffer, generatedPowder);
        }
    }

    void ChangeElement(int index, int newType)
    {
        var p = powders[index];
        PowderSystemUtils.SchdeduleRemovePowder(ref toDeleteEntities, index);
        PowderGame.Spawn(ref cmdBuffer, p.coord.x, p.coord.y, newType);
    }
}

struct DeleteEntitiesJob : IJobParallelFor
{
    [ReadOnly] public EntityArray entities;
    [ReadOnly] public NativeHashMap<int, int> toDeleteEntities;
    public EntityCommandBuffer.Concurrent cmdBuffer;
    public void Execute(int index)
    {
        int dummy;
        if (toDeleteEntities.TryGetValue(index, out dummy))
        {
            cmdBuffer.DestroyEntity(entities[index]);
            PowderGame.powderCount--;
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
    static NativeHashMap<int, int> toDeleteEntities;
    bool m_TempDataAllocated;


    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        CheckDisposeTempData();

        // Compute index
        positionsMap = new NativeHashMap<int, int>(m_PowderGroup.Length, Allocator.Temp);
        toDeleteEntities = new NativeHashMap<int, int>(Mathf.Max(m_PowderGroup.Length / 10, 64), Allocator.Temp);
        m_TempDataAllocated = true;

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
                type = PowderGame.currentPowder,
                isPainting = !Input.GetMouseButtonDown(0)
            };
            previousJobHandle = spawnJob.Schedule(previousJobHandle);
        }

        // Simulate 
        if (PowderGame.simulate)
        {
            var simulateJob = new SimulateJob()
            {
                powders = m_PowderGroup.powders,
                positions = m_PowderGroup.positions,
                hashMap = positionsMap,
                rand = Rand.Create(),
                entities = m_PowderGroup.entities,
                cmdBuffer = m_Barrier.CreateCommandBuffer(),
                toDeleteEntities = toDeleteEntities
            };
            previousJobHandle = simulateJob.Schedule(m_PowderGroup.Length, 64, previousJobHandle);
        }

        var deleteEntitiesJob = new DeleteEntitiesJob()
        {
            entities = m_PowderGroup.entities,
            cmdBuffer = m_Barrier.CreateCommandBuffer(),
            toDeleteEntities = toDeleteEntities
        };

        previousJobHandle = deleteEntitiesJob.Schedule(m_PowderGroup.Length, 64, previousJobHandle);

        inputDeps = previousJobHandle;

        return inputDeps;
    }

    protected override void OnStopRunning()
    {
        CheckDisposeTempData();
    }

    protected void CheckDisposeTempData()
    {
        if (m_TempDataAllocated)
        {
            positionsMap.Dispose();
            toDeleteEntities.Dispose();
            m_TempDataAllocated = false;
        }
    }
}
