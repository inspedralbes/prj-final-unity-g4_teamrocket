using Mirror;
using Steamworks;
using UnityEngine;
using System;
using System.Runtime.InteropServices; // Necesario para Marshal

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

        SteamNetworkingUtils.InitRelayNetworkAccess();
        
        // ConfiguraciÃ³n del timeout corregida
        int timeoutMs = 10000; // 10 segundos
        IntPtr timeoutPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)));
        Marshal.WriteInt32(timeoutPtr, timeoutMs);
        
        try
        {
            SteamNetworkingUtils.SetConfigValue(
                ESteamNetworkingConfigValue.k_ESteamNetworkingConfig_TimeoutInitial,
                ESteamNetworkingConfigScope.k_ESteamNetworkingConfig_Global,
                IntPtr.Zero,
                ESteamNetworkingConfigDataType.k_ESteamNetworkingConfig_Int32,
                timeoutPtr
            );
        }
        finally
        {
            Marshal.FreeHGlobal(timeoutPtr);
        }

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
        
        currentLobbyID = new CSteamID(callback.m_ulSteamIDLobby);
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

        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        networkAddress = lobbyID.m_SteamID.ToString();
        StartClient();
    }
}