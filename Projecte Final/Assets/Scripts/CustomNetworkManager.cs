using Mirror;
using Steamworks;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    [Header("Steam Settings")]
    public bool useSteam = true;
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> joinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;

    public CSteamID currentLobbyID;

    public override void Start()
    {
        if (useSteam && !SteamManager.Initialized)
        {
            Debug.LogError("Steam no inicializado. Desactiva 'useSteam' o configura Steamworks.");
            return;
        }

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        joinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        base.Start();
    }

    public void HostSteamLobby()
    {
        if (useSteam)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, maxConnections);
        }
        else
        {
            StartHost();
        }
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
{
    if (callback.m_eResult != EResult.k_EResultOK) return;
    
    // Forma más robusta de crear el CSteamID
    currentLobbyID = new CSteamID();
    currentLobbyID.m_SteamID = callback.m_ulSteamIDLobby; // Asignación directa del valor ulong
    
    SteamMatchmaking.SetLobbyData(currentLobbyID, "name", "TeamRocket Lobby");
    StartHost();
}

    private void OnJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
{
    if (NetworkServer.active) return;

    // Obtiene el SteamID del lobby correctamente
    CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
    networkAddress = lobbyID.ToString(); // Usamos el ID como dirección
    
    StartClient();
}
}