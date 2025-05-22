using NavMeshPlus.Components;
using UnityEngine;
using Mirror;

public class RunProceduralMap : NetworkBehaviour
{
    public AbstractDungeonGenerator generator;
    
    void Start()
    {
        if (isServer)
        {
            generator.GenerateDungeon();
        }
    }
}