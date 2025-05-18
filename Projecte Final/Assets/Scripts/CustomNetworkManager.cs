using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CustomNetworkManager : NetworkManager
{
    [Header("Configuración Steam")]
    public bool useSteam = true;
    
    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> joinRequested;
    private Callback<LobbyEnter_t> lobbyEntered;
    
    [Header("Datos del Lobby")]
    public CSteamID currentLobbyID;

    public override void Start()
    {
        if (useSteam)
        {
            StartCoroutine(InitializeSteamComponents());
        }
        else
        {
            Debug.Log("Modo offline activado");
            base.Start();
        }
    }

    private IEnumerator InitializeSteamComponents()
    {
        Debug.Log("Esperando inicialización de Steam...");
        
        float timeout = 15f;
        float startTime = Time.time;

        while (!SteamManager.Initialized)
        {
            if (Time.time - startTime > timeout)
            {
                Debug.LogError("Tiempo de espera agotado para Steam! Cambiando a modo offline");
                useSteam = false;
                base.Start();
                yield break;
            }
            yield return null;
        }

        Debug.Log("Configurando callbacks de Steam...");
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        base.Start();
    }

    public void HostSteamLobby()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam no está inicializado! No se puede crear lobby");
            return;
        }

        Debug.Log("Creando lobby de Steam...");
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"Error creando lobby: {callback.m_eResult}");
            return;
        }

        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(currentLobbyID, "name", "Lobby TeamRocket");
        SteamMatchmaking.SetLobbyData(currentLobbyID, "host", SteamUser.GetSteamID().m_SteamID.ToString());
        
        Debug.Log($"Lobby creado exitosamente. ID: {currentLobbyID.m_SteamID}");
        StartHost();
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log($"Solicitud para unirse a lobby: {callback.m_steamIDLobby}");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return;

        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        string hostSteamID = SteamMatchmaking.GetLobbyData(lobbyID, "host");
        
        if(string.IsNullOrEmpty(hostSteamID))
        {
            hostSteamID = SteamMatchmaking.GetLobbyOwner(lobbyID).m_SteamID.ToString();
            Debug.LogWarning("Usando dueño del lobby como host alternativo");
        }

        Debug.Log($"Conectando al host: {hostSteamID}");
        networkAddress = hostSteamID;
        StartClient();
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        base.OnServerDisconnect(conn);
        Debug.Log($"Cliente desconectado: {conn.connectionId}");
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        Debug.Log("Desconectado del servidor");
        
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}