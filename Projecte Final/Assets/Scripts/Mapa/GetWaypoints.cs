using System.Collections.Generic;
using UnityEngine;

public class GetWaypoints : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RegisterWaypoint(Transform waypoint)
    {
        waypoints.Add(waypoint);
    }
}
