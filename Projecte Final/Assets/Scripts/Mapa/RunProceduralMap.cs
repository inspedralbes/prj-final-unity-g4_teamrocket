using UnityEngine;

public class RunProceduralMap : MonoBehaviour

{
    public AbstractDungeonGenerator generator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        generator.GenerateDungeon();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
