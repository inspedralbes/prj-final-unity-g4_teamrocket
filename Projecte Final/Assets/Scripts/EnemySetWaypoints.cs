using UnityEngine;

public static class WaypointAssigner
{
    public static Transform[] GetWaypointsByTag(string tag = "Waypoint")
    {
        GameObject[] waypointObjects = GameObject.FindGameObjectsWithTag(tag);
        Transform[] waypoints = new Transform[waypointObjects.Length];

        for (int i = 0; i < waypointObjects.Length; i++)
        {
            waypoints[i] = waypointObjects[i].transform;
        }

        return waypoints;
    }
}
