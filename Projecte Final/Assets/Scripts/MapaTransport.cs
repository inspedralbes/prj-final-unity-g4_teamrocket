using UnityEngine;
using UnityEngine.SceneManagement;

public class MapaTransport : MonoBehaviour
{
    [Tooltip("Distancia máxima para interactuar")]
    public float distanciaInteraccion = 3f;
    
    [Tooltip("Tecla para activar el transporte")]
    public KeyCode teclaTransporte = KeyCode.M;
    
    private Transform jugador;
    private bool jugadorCerca = false;
    
    private void Start()
    {
        // Buscar al jugador por tag
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
    }
    
    private void Update()
    {
        // Verificar distancia cada frame
        jugadorCerca = Vector3.Distance(transform.position, jugador.position) <= distanciaInteraccion;
        
        // Comprobar si se presiona la tecla M y el jugador está cerca
        if (jugadorCerca && Input.GetKeyDown(teclaTransporte))
        {
            CargarMansion();
        }
    }
    
    private void CargarMansion()
    {
        try
        {
            SceneManager.LoadScene("Mansion");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cargar la escena Mansion: " + e.Message);
            Debug.Log("Verifica que:");
            Debug.Log("- La escena 'Mansion' está en Build Settings");
            Debug.Log("- El nombre está escrito correctamente (sensible a mayúsculas)");
        }
    }
    
    // Visualizar rango de interacción en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaInteraccion);
    }
    
    // Opcional: Mostrar UI cuando el jugador está cerca
    private void OnGUI()
    {
        if (jugadorCerca)
        {
            GUI.Label(new Rect(Screen.width/2 - 100, Screen.height - 50, 200, 30), 
                     $"Presiona {teclaTransporte} para entrar a la Mansión",
                     new GUIStyle { fontSize = 20, normal = { textColor = Color.white } });
        }
    }
}