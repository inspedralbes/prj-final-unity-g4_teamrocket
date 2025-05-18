using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class CustomNetworkManager : NetworkManager
{
    [Header("Steam Settings")]
    [Tooltip("Activa la integración con Steam")]
    public bool useSteam = true;
    
    [Tooltip("ID del lobby actual de Steam")]
    public CSteamID currentLobbyID;

    // Callbacks de Steam
    private Callback<LobbyCreated_t> _lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> _lobbyJoinRequested;
    private Callback<LobbyEnter_t> _lobbyEntered;

    #region Initialization
    public override void Awake()
    {
        InitializeSteamInEditor();
        base.Awake();
        ForceMainMenuScene();
    }

    private void InitializeSteamInEditor()
    {
        #if UNITY_EDITOR
        if (!useSteam) return;

        try
        {
            if (!SteamManager.Initialized && !SteamAPI.Init())
            {
                Debug.LogWarning("Steam no inicializado. Cambiando a modo offline.");
                useSteam = false;
                return;
            }

            Debug.Log($"Steam inicializado en Editor (AppID: {SteamUtils.GetAppID()})");
            
            // Configura el transporte FizzySteam si existe
            if (TryGetComponent<Transport>(out var transport))
            {
                transport.enabled = true;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error al iniciar Steam: {e.Message}");
            useSteam = false;
        }
        #endif
    }

    private void ForceMainMenuScene()
    {
        if (SceneManager.GetActiveScene().name != "MainMenuUnseen")
        {
            SceneManager.LoadScene("MainMenuUnseen");
        }
    }

    public override void Start()
    {
        base.Start();
        InitializeSteamCallbacks();
    }

    private void InitializeSteamCallbacks()
    {
        if (!useSteam || !SteamManager.Initialized) return;

        _lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }
    #endregion

    #region Lobby Management
    public void HostSteamLobby()
    {
        if (!useSteam || !SteamManager.Initialized)
        {
            StartOfflineHost();
            return;
        }

        _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
    }

    private void StartOfflineHost()
    {
        Debug.Log("[STEAM] Iniciando lobby en modo offline");
        StartHost();
        ServerChangeScene("Lobby");
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError($"[STEAM] Error al crear lobby: {callback.m_eResult}");
            return;
        }

        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        ConfigureSteamLobby();
        StartHost();
        ServerChangeScene("Lobby");
    }

    private void ConfigureSteamLobby()
    {
        SteamMatchmaking.SetLobbyData(currentLobbyID, "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(currentLobbyID, "name", "Darkness Unseen Lobby");
        Debug.Log($"[STEAM] Lobby creado (ID: {currentLobbyID})");
    }
    #endregion

    #region Client Connection
    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log($"[STEAM] Solicitud de unión al lobby: {callback.m_steamIDLobby}");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return;

        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        networkAddress = SteamMatchmaking.GetLobbyData(currentLobbyID, "HostAddress");
        
        Debug.Log($"[STEAM] Uniéndose a lobby: {networkAddress}");
        StartClient();
    }
    #endregion

    #region Scene Management
    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);
        
        if (sceneName == "Lobby" && useSteam && currentLobbyID.IsValid())
        {
            SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyID);
            Debug.Log("[STEAM] Overlay de invitaciones activado");
        }
    }

    public override void OnStopHost()
    {
        CleanupSteamLobby();
        base.OnStopHost();
        ReturnToMainMenu();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();
        ReturnToMainMenu();
    }

    private void CleanupSteamLobby()
    {
        if (!useSteam || !currentLobbyID.IsValid()) return;
        
        SteamMatchmaking.LeaveLobby(currentLobbyID);
        currentLobbyID.Clear();
        Debug.Log("[STEAM] Lobby cerrado");
    }

    private void ReturnToMainMenu()
    {
        if (SceneManager.GetActiveScene().name != "MainMenuUnseen")
        {
            SceneManager.LoadScene("MainMenuUnseen");
        }
    }
    #endregion

    #region Debug Tools
    private void OnGUI()
    {
        if (!useSteam) return;
        
        GUILayout.BeginArea(new Rect(10, 40, 300, 200));
        GUILayout.Label($"Estado Steam: {SteamManager.Initialized}");
        GUILayout.Label($"AppID: {SteamUtils.GetAppID()}");
        
        if (currentLobbyID.IsValid())
        {
            GUILayout.Label($"Lobby ID: {currentLobbyID}");
            GUILayout.Label($"Mi SteamID: {SteamUser.GetSteamID()}");
        }
        
        GUILayout.EndArea();
    }
    #endregion
}