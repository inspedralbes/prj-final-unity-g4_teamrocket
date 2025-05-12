using NavMeshPlus.Components;
using UnityEngine;

public class RunProceduralMap : MonoBehaviour

{
    public AbstractDungeonGenerator generator;
    public NavMeshSurface navMesh;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generator.GenerateDungeon();
        navMesh.BuildNavMesh();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
