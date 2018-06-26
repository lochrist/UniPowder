using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class RenderCmd
{
    public int type;
    public Vector2Int coord;
}

public class CheckOrder : ComponentSystem {
    struct Group
    {
        public ComponentDataArray<Powder> Powders;
        public EntityArray Entities;
        public int Length;
    }

    [Inject] Group m_PowderGroup;

    protected override void OnUpdate()
    {
        for (var i = 0; i < m_PowderGroup.Length; ++i)
        {
            // Debug.Log(string.Format("x:{0}, y: {1}, index: {2}", m_PowderGroup.Powders[i].x, m_PowderGroup.Powders[i].y, m_PowderGroup.Powders[i].index));
        }
    }
}


public class SyncRender : ComponentSystem
{
    struct Group
    {
        public ComponentDataArray<Powder> Powders;
        public EntityArray Entities;
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
