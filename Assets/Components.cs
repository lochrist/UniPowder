using Unity.Entities;
using UnityEngine;

public struct Powder : IComponentData
{
    public Vector2Int coord;
    public int type;
    public int life;
}