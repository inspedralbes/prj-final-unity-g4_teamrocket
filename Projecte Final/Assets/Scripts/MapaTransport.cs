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

    private Transform jugador;
    private bool jugadorEnRango;
    private CustomNetworkManager networkManager;

    private void Awake()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius = distanciaInteraccion;
        collider.isTrigger = true;

        if (interactUI) interactUI.SetActive(false);
        if (highlightParticles) highlightParticles.Stop();

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

        float distancia = Vector3.Distance(transform.position, jugador.position);
        jugadorEnRango = distancia <= distanciaInteraccion;

        ActualizarFeedback();

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
        if (interactUI) 
        {
            interactUI.SetActive(jugadorEnRango);
        }

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
        if (!NetworkClient.active && !NetworkServer.active)
        {
            if (mostrarDebug) Debug.Log("Iniciando transporte OFFLINE a Mansion");
            GameObject.FindGameObjectWithTag(tagJugador).transform.position = Vector3.zero;
            CargarEscenaSeguro("ProceduralMapGeneration");
            return;
        }

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
            foreach (var player in GameObject.FindGameObjectsWithTag(tagJugador))
            {
                player.transform.position = Vector3.zero;
            }
            networkManager.ServerChangeScene("ProceduralMapGeneration");
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
        foreach (var player in GameObject.FindGameObjectsWithTag(tagJugador))
        {
            player.transform.position = Vector3.zero;
        }
        networkManager.ServerChangeScene("ProceduralMapGeneration");
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
        }
    }

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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position, distanciaInteraccion);
    }

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