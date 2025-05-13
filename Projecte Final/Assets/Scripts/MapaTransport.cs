using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SphereCollider))]
public class MapaTransport : NetworkBehaviour
{
    [Header("Configuración")]
    [Tooltip("Distancia de interacción")]
    public float distanciaInteraccion = 3f;
    [Tooltip("Tecla para interactuar")]
    public KeyCode teclaTransporte = KeyCode.M;
    
    [Header("Feedback")]
    [SerializeField] private GameObject interactUI; // UI que muestra "Presiona M"
    [SerializeField] private ParticleSystem highlightParticles;

    private Transform jugador;
    private bool jugadorEnRango;
    private CustomNetworkManager networkManager;

    private void Awake()
    {
        GetComponent<SphereCollider>().radius = distanciaInteraccion;
        networkManager = FindFirstObjectByType<CustomNetworkManager>();
        
        if (interactUI) interactUI.SetActive(false);
        if (highlightParticles) highlightParticles.Stop();
    }

    private void Update()
    {
        if (!TieneJugadorCerca()) return;
        
        // Mostrar UI de interacción
        if (interactUI) interactUI.SetActive(jugadorEnRango);
        if (highlightParticles)
        {
            if (jugadorEnRango && !highlightParticles.isPlaying) 
                highlightParticles.Play();
            else if (!jugadorEnRango && highlightParticles.isPlaying)
                highlightParticles.Stop();
        }

        // Comprobar interacción
        if (jugadorEnRango && Input.GetKeyDown(teclaTransporte))
        {
            TransportarATodos();
        }
    }

    private bool TieneJugadorCerca()
    {
        // Modo offline
        if (!NetworkClient.active)
        {
            if (jugador == null)
                jugador = GameObject.FindGameObjectWithTag("Player")?.transform;
            
            if (jugador == null) return false;
            
            jugadorEnRango = Vector3.Distance(transform.position, jugador.position) <= distanciaInteraccion;
            return true;
        }
        
        // Modo online - solo para jugador local
        if (!isLocalPlayer) return false;
        
        if (jugador == null)
            jugador = NetworkClient.localPlayer.transform;
        
        jugadorEnRango = Vector3.Distance(transform.position, jugador.position) <= distanciaInteraccion;
        return true;
    }

    private void TransportarATodos()
    {
        Debug.Log("Iniciando transporte...");
        
        // Modo offline
        if (!NetworkClient.active)
        {
            SceneManager.LoadScene("Mansion");
            return;
        }

        // Modo online
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
        Debug.Log("Solicitud de transporte recibida en servidor");
        networkManager.ServerChangeScene("Mansion");
    }

    // Visualización en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, distanciaInteraccion);
    }

    // Detección por trigger (opcional)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = true;
            jugador = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorEnRango = false;
            if (highlightParticles) highlightParticles.Stop();
        }
    }
}