using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RenderCmd
{
    public int type;
    public Vector2Int coord;
}

public class SimulationSystem : ComponentSystem {
    /*
    struct Group
    {
        public ComponentDataArray<Powder> Powders;
        public int Length;
    }

    [Inject] Group m_PowderGroup;
    */

    protected override void OnUpdate()
    {
        Debug.Log("sim");
    }
}

[AlwaysUpdateSystem]
[UpdateAfter(typeof(SimulationSystem))]
public class SpawnSystem : ComponentSystem
{
    struct Group
    {
        public ComponentDataArray<Powder> Powders;
        public int Length;
    }

    [Inject] Group m_PowderGroup;

    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {

            Vector2Int pos = new Vector2Int((int)Input.mousePosition.x, (int)Input.mousePosition.y);
            for (var i = 0; i < m_PowderGroup.Length; ++i)
            {
                if (m_PowderGroup.Powders[i].coord == pos)
                {
                    return;
                }
            }

            PowderTypes.Spawn(PostUpdateCommands, pos, PowderRenderer.CurrentPowder);
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
        public ComponentDataArray<Powder> Powders;
        public int Length;
    }

    [Inject] Group m_PowderGroup;

    protected override void OnUpdate()
    {
        PowderRenderer.NbCmds = m_PowderGroup.Length;

        for (var i = PowderRenderer.Cmds.Count; i < m_PowderGroup.Length; ++i)
        {
            PowderRenderer.Cmds.Add(new RenderCmd());
        }

        for (var i = 0; i < m_PowderGroup.Length; ++i)
        {
            PowderRenderer.Cmds[i].coord = m_PowderGroup.Powders[i].coord;
            PowderRenderer.Cmds[i].type = m_PowderGroup.Powders[i].type;
        }
    }
}
