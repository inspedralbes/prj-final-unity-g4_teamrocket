using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class NavMeshBaker : MonoBehaviour
{
    private NavMeshSurface surface;

    void Awake()
    {
        surface = GetComponent<NavMeshSurface>();
    }

    /// <summary>
    /// Llama a este m�todo despu�s de generar o modificar el suelo.
    /// </summary>
    public void BakeNavMesh()
    {
        surface.BuildNavMesh();
    }
}
