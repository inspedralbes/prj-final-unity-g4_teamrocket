using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    [Header("Steam Settings")]
    public bool useSteam = true;
    
    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> joinRequested;
    private Callback<LobbyEnter_t> lobbyEntered;
    
    public CSteamID currentLobbyID;
    private bool steamInitialized = false;

    public override void Start()
    {
        if (useSteam && !steamInitialized)
        {
            InitializeSteam();
        }
        base.Start();
    }

    private void InitializeSteam()
    {
        try
        {
            steamInitialized = SteamAPI.Init();
            if (!steamInitialized)
            {
                Debug.LogError("SteamAPI no pudo inicializarse. Cambiando a modo offline.");
                useSteam = false;
                return;
            }

            lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
            
            Debug.Log("Steam inicializado correctamente");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error inicializando Steam: " + e.Message);
            useSteam = false;
        }
    }

    public void HostSteamLobby()
    {
        if (!steamInitialized)
        {
            Debug.LogError("Steam no está inicializado. Usando modo offline.");
            useSteam = false;
            StartHost();
            SceneManager.LoadScene("Lobby");
            return;
        }

        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Error creando lobby: " + callback.m_eResult);
            return;
        }

        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(currentLobbyID, "name", "Darkness Unseen Lobby");
        SteamMatchmaking.SetLobbyData(currentLobbyID, "host", SteamUser.GetSteamID().ToString());

        Debug.Log($"Lobby creado exitosamente - ID: {currentLobbyID.m_SteamID}");

        // Comportamiento diferente entre Editor y Build
        if (Application.isEditor)
        {
            Debug.Log("Modo Editor: Cargando escena manualmente");
            StartHost();
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            Debug.Log("Modo Build: Usando flujo normal");
            StartHost();
            ServerChangeScene("Lobby");
        }
    }

    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log($"Solicitud de unión a lobby recibida: {callback.m_steamIDLobby}");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return;

        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(currentLobbyID, "host");

        if (string.IsNullOrEmpty(hostAddress))
        {
            hostAddress = SteamMatchmaking.GetLobbyOwner(currentLobbyID).ToString();
            Debug.LogWarning("No se encontró host en datos del lobby, usando dueño del lobby");
        }

        Debug.Log($"Uniéndose a lobby. Host: {hostAddress}");
        networkAddress = hostAddress;
        StartClient();
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        Debug.Log($"Escena del servidor cambiada a: {sceneName}");
        base.OnServerSceneChanged(sceneName);
    }

    public override void OnClientDisconnect()
    {
        Debug.Log("Cliente desconectado");
        base.OnClientDisconnect();
        
        if (SceneManager.GetActiveScene().name != "MainMenuUnseen")
        {
            SceneManager.LoadScene("MainMenuUnseen");
        }
    }

    public override void OnApplicationQuit()
    {
        Debug.Log("Cerrando aplicación...");
        if (steamInitialized)
        {
            SteamAPI.Shutdown();
            Debug.Log("Steam API cerrada correctamente");
        }
        
        base.OnApplicationQuit();
    }
}