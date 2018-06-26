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
        public int length;
    }
    [Inject] Group m_PowderGroup;

    protected override void OnUpdate()
    {
        var coordMap = new NativeHashMap<int, int>(m_PowderGroup.length, Allocator.Temp);


    }
}

[AlwaysUpdateSystem]
[UpdateAfter(typeof(SimulationSystem))]
public class SpawnSystem : ComponentSystem
{
    struct Group
    {
        public ComponentDataArray<Powder> powders;
        public int length;
    }

    [Inject] Group m_PowderGroup;

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {

            Vector2Int pos = new Vector2Int((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            for (var i = 0; i < m_PowderGroup.length; ++i)
            {
                if (m_PowderGroup.powders[i].coord == pos)
                {
                    return;
                }
            }

            PowderTypes.Spawn(PostUpdateCommands, pos, PowderGame.currentPowder);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Up");
        }
    }
}

[UpdateAfter(typeof(SpawnSystem))]
public class PushRenderCmdsSystem : ComponentSystem
{
    struct Group
    {
        public ComponentDataArray<Powder> powders;
        public int length;
    }
    [Inject] Group m_PowderGroup;

    protected override void OnUpdate()
    {
        PowderRenderer.nbCmds = m_PowderGroup.length;

        for (var i = PowderRenderer.cmds.Count; i < m_PowderGroup.length; ++i)
        {
            PowderRenderer.cmds.Add(new RenderCmd());
        }

        for (var i = 0; i < m_PowderGroup.length; ++i)
        {
            PowderRenderer.cmds[i].coord = m_PowderGroup.powders[i].coord;
            PowderRenderer.cmds[i].type = m_PowderGroup.powders[i].type;
        }
    }
}
