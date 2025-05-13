using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SphereCollider))]
public class MapaTransport : NetworkBehaviour
{
    [Header("Configuración Básica")]
    public float distanciaInteraccion = 3f;
    public KeyCode teclaTransporte = KeyCode.M;
    public string tagJugador = "Player";

    [Header("Feedback")]
    public bool mostrarDebug = true;
    public GameObject interactUI;
    public string mensajeInteraccion = "Presiona M para entrar";

    private Transform jugador;
    private bool jugadorEnRango;
    private CustomNetworkManager networkManager;

    private void Awake()
    {
        // Configuración inicial del collider
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius = distanciaInteraccion;
        collider.isTrigger = true;

        if (interactUI) interactUI.SetActive(false);
        
        // Solo buscar el NetworkManager si estamos en modo online
        if (NetworkClient.active) 
        {
            networkManager = FindFirstObjectByType<CustomNetworkManager>();
        }
    }

    private void Update()
    {
        BuscarJugadorSiNoExiste();

        if (jugador == null) 
        {
            if (mostrarDebug) Debug.LogWarning("No se encontró jugador con tag: " + tagJugador);
            return;
        }

        // Verificar distancia (modo offline seguro)
        float distancia = Vector3.Distance(transform.position, jugador.position);
        jugadorEnRango = distancia <= distanciaInteraccion;

        // Mostrar UI de feedback
        if (interactUI) 
        {
            interactUI.SetActive(jugadorEnRango);
        }

        // Comprobar interacción
        if (jugadorEnRango && Input.GetKeyDown(teclaTransporte))
        {
            if (mostrarDebug) Debug.Log("Tecla M presionada - Intentando transportar");
            Transportar();
        }
    }

    private void BuscarJugadorSiNoExiste()
    {
        if (jugador == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(tagJugador);
            if (playerObj != null)
            {
                jugador = playerObj.transform;
                if (mostrarDebug) Debug.Log("Jugador encontrado: " + jugador.name);
            }
        }
    }

    private void Transportar()
    {
        if (!NetworkClient.active) // Modo offline
        {
            if (mostrarDebug) Debug.Log("Iniciando transporte OFFLINE a Mansion");
            CargarEscenaSeguro("Mansion");
        }
        else // Modo online
        {
            if (mostrarDebug) Debug.Log("Iniciando transporte ONLINE a Mansion");
            if (isServer)
            {
                networkManager.ServerChangeScene("Mansion");
            }
            else
            {
                CmdSolicitarTransporte();
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdSolicitarTransporte()
    {
        if (mostrarDebug) Debug.Log("Servidor recibió solicitud de transporte");
        networkManager.ServerChangeScene("Mansion");
    }

    private void CargarEscenaSeguro(string nombreEscena)
    {
        try
        {
            SceneManager.LoadScene(nombreEscena);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error al cargar escena: " + e.Message);
            Debug.Log("Asegúrate que:");
            Debug.Log("1. La escena existe en Build Settings");
            Debug.Log("2. El nombre es exacto (sensible a mayúsculas)");
            Debug.Log("3. No hay errores de compilación");
        }
    }

    // Debug visual en el Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaInteraccion);
    }

    // Mensaje en pantalla (alternativa al Canvas)
    private void OnGUI()
    {
        if (jugadorEnRango && interactUI == null)
        {
            Rect rect = new Rect(Screen.width/2 - 100, Screen.height - 50, 200, 30);
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            
            GUI.Label(rect, mensajeInteraccion, style);
        }
    }

    // Detección por trigger (backup)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            jugadorEnRango = true;
            jugador = other.transform;
            if (mostrarDebug) Debug.Log("Jugador entró en rango (trigger)");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            jugadorEnRango = false;
            if (mostrarDebug) Debug.Log("Jugador salió de rango (trigger)");
        }
    }
}