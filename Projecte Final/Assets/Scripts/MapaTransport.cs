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
    public bool mostrarDebug = true;

    [Header("Feedback Visual")]
    public GameObject interactUI;
    public ParticleSystem highlightParticles;
    public string mensajeInteraccion = "Presiona M para entrar a la Mansión";

    // Variables privadas
    private Transform jugador;
    private bool jugadorEnRango;
    private CustomNetworkManager networkManager;

    private void Awake()
    {
        // Configuración inicial
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius = distanciaInteraccion;
        collider.isTrigger = true;

        if (interactUI) interactUI.SetActive(false);
        if (highlightParticles) highlightParticles.Stop();

        // Solo buscar NetworkManager si estamos en modo online
        if (NetworkClient.active)
        {
            networkManager = FindFirstObjectByType<CustomNetworkManager>();
            if (mostrarDebug && networkManager == null)
                Debug.LogWarning("NetworkManager no encontrado en modo online");
        }
    }

    private void Update()
    {
        BuscarJugadorSiNoExiste();

        if (jugador == null)
        {
            if (mostrarDebug) Debug.LogWarning($"No se encontró jugador con tag: {tagJugador}");
            return;
        }

        // Verificar distancia
        float distancia = Vector3.Distance(transform.position, jugador.position);
        jugadorEnRango = distancia <= distanciaInteraccion;

        // Feedback visual
        ActualizarFeedback();

        // Comprobar interacción
        if (jugadorEnRango && Input.GetKeyDown(teclaTransporte))
        {
            if (mostrarDebug) Debug.Log("Tecla de transporte presionada");
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
                if (mostrarDebug) Debug.Log($"Jugador encontrado: {jugador.name}");
            }
        }
    }

    private void ActualizarFeedback()
    {
        // UI de interacción
        if (interactUI) 
        {
            interactUI.SetActive(jugadorEnRango);
        }

        // Sistema de partículas
        if (highlightParticles)
        {
            if (jugadorEnRango && !highlightParticles.isPlaying) 
                highlightParticles.Play();
            else if (!jugadorEnRango && highlightParticles.isPlaying)
                highlightParticles.Stop();
        }
    }

    private void Transportar()
    {
        // Modo offline (incluyendo pruebas en editor)
        if (!NetworkClient.active && !NetworkServer.active)
        {
            if (mostrarDebug) Debug.Log("Iniciando transporte OFFLINE a Mansion");
            CargarEscenaSeguro("Mansion");
            return;
        }

        // Modo online
        if (networkManager == null)
        {
            networkManager = FindFirstObjectByType<CustomNetworkManager>();
            if (networkManager == null)
            {
                Debug.LogError("No se encontró NetworkManager en modo online!");
                return;
            }
        }

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
            if (mostrarDebug) Debug.Log($"Cargando escena: {nombreEscena}");
            SceneManager.LoadScene(nombreEscena);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al cargar {nombreEscena}: {e.Message}");
            Debug.Log("Verifica que:");
            Debug.Log("1. La escena está en Build Settings");
            Debug.Log("2. El nombre coincide exactamente");
            Debug.Log("3. No hay errores de compilación");
        }
    }

    // Métodos de trigger (backup)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            jugadorEnRango = true;
            jugador = other.transform;
            if (mostrarDebug) Debug.Log("Jugador entró en zona de transporte");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagJugador))
        {
            jugadorEnRango = false;
            if (mostrarDebug) Debug.Log("Jugador salió de zona de transporte");
            if (highlightParticles) highlightParticles.Stop();
        }
    }

    // Visualización en editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position, distanciaInteraccion);
    }

    // UI alternativa
    private void OnGUI()
    {
        if (jugadorEnRango && interactUI == null)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(new Rect(Screen.width/2 - 150, Screen.height - 100, 300, 50), 
                     mensajeInteraccion, style);
        }
    }
}