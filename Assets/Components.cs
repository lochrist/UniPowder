using Unity.Entities;
using UnityEngine;

public struct Powder : IComponentData
{
    public Vector2Int coord;
    public int type;
    public int life;

    public bool Same(Powder p)
    {
        return coord == p.coord && type == p.type && life == p.life;
    }
}