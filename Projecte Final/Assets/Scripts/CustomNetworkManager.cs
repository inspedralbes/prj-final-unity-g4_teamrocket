using Mirror;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomNetworkManager : NetworkManager
{
    [Header("Steam Settings")]
    public bool useSteam = true;
    public CSteamID currentLobbyID;

    private Callback<LobbyCreated_t> lobbyCreated;
    private Callback<GameLobbyJoinRequested_t> lobbyJoinRequested;
    private Callback<LobbyEnter_t> lobbyEntered;

    private static CustomNetworkManager instance;

    #region Initialization

    public override void Awake()
    {
        // Singleton para que solo haya uno y no se destruya
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();

#if UNITY_EDITOR
        if (useSteam)
        {
            InitializeSteamInEditor();
        }
#endif
        ForceMainMenuScene();
    }

    private void InitializeSteamInEditor()
    {
        try
        {
            if (SteamManager.Initialized)
            {
                Debug.Log($"SteamManager ya inicializado (AppID: {SteamUtils.GetAppID()})");
                return;
            }

            if (!SteamAPI.Init())
            {
                Debug.LogWarning("SteamAPI no pudo inicializarse. Verifica:");
                Debug.LogWarning("- Steam está ejecutándose");
                Debug.LogWarning("- steam_appid.txt existe con valor 480");
                Debug.LogWarning("- Tienes Spacewar en tu biblioteca");
                useSteam = false;
                return;
            }

            Debug.Log($"Steam inicializado correctamente (AppID: {SteamUtils.GetAppID()})");

            // Habilita el transporte FizzySteam si existe
            var transport = GetComponent<Transport>();
            if (transport != null)
            {
                transport.enabled = true;
                Debug.Log("Transporte FizzySteam habilitado");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error crítico al inicializar Steam: " + e.Message);
            useSteam = false;
        }
    }

    public bool IsSteamReady()
    {
#if UNITY_EDITOR
        return useSteam && SteamManager.Initialized && SteamAPI.IsSteamRunning();
#else
        return useSteam && SteamManager.Initialized;
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
        if (!IsSteamReady()) return;

        // Aseguramos que solo una vez se suscriba cada callback
        if (lobbyJoinRequested == null)
            lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);

        if (lobbyEntered == null)
            lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        Debug.Log("Callbacks de Steam registrados");
    }

    #endregion

    #region Lobby Management

    public void HostSteamLobby()
    {
        if (!IsSteamReady())
        {
            Debug.Log("Steam no está listo, iniciando en modo offline");
            StartOfflineHost();
            return;
        }

        Debug.Log("Creando lobby de Steam...");
        // Asegura que la callback solo se cree una vez (se sobrescribe)
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
    }

    private void StartOfflineHost()
    {
        useSteam = false;

        // Desactiva FizzySteam si existe
        var transport = GetComponent<Transport>();
        if (transport != null)
        {
            transport.enabled = false;
        }

        Debug.Log("Iniciando servidor en modo offline");
        StartHost();
        ServerChangeScene("Lobby");
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Error al crear lobby de Steam: " + callback.m_eResult);
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
        Debug.Log("Lobby de Steam creado (ID: " + currentLobbyID + ")");
    }

    #endregion

    #region Steam Callbacks

    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Solicitud de unión a lobby recibida");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        if (NetworkServer.active) return; // Si ya es host, ignora

        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        networkAddress = SteamMatchmaking.GetLobbyData(currentLobbyID, "HostAddress");
        Debug.Log("Uniéndose a lobby en: " + networkAddress);
        StartClient();
    }

    #endregion

    #region Scene Management

    public override void OnServerSceneChanged(string sceneName)
    {
        base.OnServerSceneChanged(sceneName);

        if (sceneName == "Lobby" && currentLobbyID.IsValid())
        {
            SteamFriends.ActivateGameOverlayInviteDialog(currentLobbyID);
            Debug.Log("Overlay de invitaciones de Steam activado");
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
        if (!currentLobbyID.IsValid()) return;

        SteamMatchmaking.LeaveLobby(currentLobbyID);
        currentLobbyID.Clear();
        Debug.Log("Lobby de Steam cerrado");
    }

    private void ReturnToMainMenu()
    {
        if (SceneManager.GetActiveScene().name != "MainMenuUnseen")
        {
            SceneManager.LoadScene("MainMenuUnseen");
        }
    }

    #endregion

    #region Debug GUI

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 40, 300, 200));
        GUILayout.Label("Estado Steam: " + (IsSteamReady() ? "ACTIVO" : "INACTIVO"));

        if (IsSteamReady())
        {
            GUILayout.Label("AppID: " + SteamUtils.GetAppID());
            GUILayout.Label("SteamID: " + SteamUser.GetSteamID());

            if (currentLobbyID.IsValid())
            {
                GUILayout.Label("Lobby ID: " + currentLobbyID);
            }
        }
        else
        {
            GUILayout.Label("Razón posible:");
            GUILayout.Label("- Steam no iniciado");
            GUILayout.Label("- Falta steam_appid.txt");
            GUILayout.Label("- Spacewar no en biblioteca");
        }

        GUILayout.EndArea();
    }

    #endregion
}
