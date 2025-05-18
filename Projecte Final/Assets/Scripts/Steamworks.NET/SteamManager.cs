#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif

using UnityEngine;
using System.IO;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour 
{
#if !DISABLESTEAMWORKS
    private static SteamManager s_instance;
    private static bool s_EverInitialized = false;

    private bool m_bInitialized = false;
    private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

    public static bool Initialized
    {
        get
        {
            if (s_instance == null)
            {
                return false;
            }
            return s_instance.m_bInitialized;
        }
    }

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    private static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning($"[Steamworks] {pchDebugText}");
    }

    private void Awake()
    {
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;

        if (s_EverInitialized)
        {
            Debug.LogError("SteamAPI ya fue inicializado!");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // Crear steam_appid.txt si no existe
        string appIdPath = Path.Combine(Application.dataPath, "../steam_appid.txt");
        if (!File.Exists(appIdPath))
        {
            File.WriteAllText(appIdPath, "480");
            Debug.Log("Created steam_appid.txt with AppID 480");
        }

        InitializeSteamworks();
    }

    private void InitializeSteamworks()
    {
        try
        {
            Debug.Log("Inicializando Steamworks...");

            if (SteamAPI.RestartAppIfNecessary(new AppId_t(480)))
            {
                Debug.Log("Reiniciando a través de Steam...");
                Application.Quit();
                return;
            }

            if (!Packsize.Test())
            {
                Debug.LogError("Prueba Packsize falló!");
                return;
            }

            if (!DllCheck.Test())
            {
                Debug.LogError("Prueba DllCheck falló!");
                return;
            }

            m_bInitialized = SteamAPI.Init();
            if (!m_bInitialized)
            {
                Debug.LogError("SteamAPI.Init() falló! Causas posibles:");
                Debug.LogError("- Steam no está ejecutándose");
                Debug.LogError("- steam_appid.txt no existe o es inválido");
                Debug.LogError("- La aplicación no está configurada en Steamworks");
                return;
            }

            s_EverInitialized = true;
            Debug.Log($"Steam inicializado correctamente. AppID: {SteamUtils.GetAppID()}");
            Debug.Log($"Usuario: {SteamFriends.GetPersonaName()} (ID: {SteamUser.GetSteamID()})");
        }
        catch (System.DllNotFoundException e)
        {
            Debug.LogError("No se encontraron las DLLs de Steam: " + e);
            Debug.LogError("Asegúrate que steam_api.dll/steam_api64.dll están en Assets/Plugins");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error inicializando Steamworks: " + e);
        }
    }

    private void OnEnable()
    {
        if (m_bInitialized && m_SteamAPIWarningMessageHook == null)
        {
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
    }

    private void Update()
    {
        if (!m_bInitialized) return;

        try
        {
            SteamAPI.RunCallbacks();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error ejecutando callbacks de Steam: " + e);
        }
    }

    private void OnDestroy()
    {
        if (s_instance != this) return;

        if (m_bInitialized)
        {
            SteamAPI.Shutdown();
            Debug.Log("SteamAPI cerrado correctamente");
        }
        s_instance = null;
    }
#else
    public static bool Initialized => false;
#endif
}