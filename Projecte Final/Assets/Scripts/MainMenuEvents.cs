using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;

public class MainMenuEvents : MonoBehaviour
{
    // UI References
    private UIDocument _document;
    private Button _startButton;
    private Button _createRoomButton;
    private Button _joinRoomButton;
    private Button _exitButton;
    private Button _settingsButton;
    private VisualElement _container;
    private TextField _codeInputField;
    private Label _generatedCodeLabel;
    private AudioSource _audioSource;

    // Menus
    private VisualElement _settingsMenu;
    private VisualElement _startS;
    private VisualElement _multiplayerMenu;

    // Settings
    private Slider _volumeSlider;
    private Label _volumeLabel;
    private Button _englishButton;
    private Button _spanishButton;
    private Button _backButton;
    private Button _startOfflineButton;
    private Button _backFromStartSButton;

    // Game Data
    private string _currentLanguage = "en";
    private Dictionary<string, string> _translations;
    private CustomNetworkManager _networkManager;

    #region Initialization
    private void Awake()
    {
        InitializeComponents();
        SetupUIReferences();
        InitializeTranslations();
        RegisterCallbacks();
        SetLanguage(_currentLanguage);
        HideAllSubMenus();
    }

    private void InitializeComponents()
    {
        _audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();
        _networkManager = FindFirstObjectByType<CustomNetworkManager>();
    }

    private void HideAllSubMenus()
    {
        _settingsMenu.style.display = DisplayStyle.None;
        _startS.style.display = DisplayStyle.None;
        _multiplayerMenu.style.display = DisplayStyle.None;
        _codeInputField.style.display = DisplayStyle.None;
        _generatedCodeLabel.style.display = DisplayStyle.None;
    }
    #endregion

    #region UI Setup
    private void SetupUIReferences()
    {
        var root = _document.rootVisualElement;

        // Main buttons
        _startButton = root.Q<Button>("StartGameButton");
        _createRoomButton = root.Q<Button>("CreateR");
        _joinRoomButton = root.Q<Button>("JoinR");
        _exitButton = root.Q<Button>("Exit");
        _settingsButton = root.Q<Button>("Settings");
        _container = root.Q<VisualElement>("Container");
        _codeInputField = root.Q<TextField>("CodeInputField");
        _generatedCodeLabel = root.Q<Label>("GeneratedCodeLabel");

        // Menus
        _settingsMenu = root.Q<VisualElement>("SettingsMenu");
        _startS = root.Q<VisualElement>("StartS");
        _multiplayerMenu = root.Q<VisualElement>("MultiplayerMenu");

        // Settings
        _volumeSlider = _settingsMenu.Q<Slider>("VolumeSlider");
        _volumeLabel = _settingsMenu.Q<Label>("VolumeLabel");
        _englishButton = _settingsMenu.Q<Button>("EnglishButton");
        _spanishButton = _settingsMenu.Q<Button>("SpanishButton");
        _backButton = _settingsMenu.Q<Button>("BackButton");

        // Multiplayer
        _startOfflineButton = _startS.Q<Button>("StartOffline");
        _backFromStartSButton = _startS.Q<Button>("BackButton");
    }

    private void InitializeTranslations()
    {
        _translations = new Dictionary<string, string>
        {
            // English (en)
            { "StartGameButton", "Start Game" },
            { "CreateR", "Create Room" },
            { "JoinR", "Join Room" },
            { "Exit", "Exit" },
            { "Settings", "Settings" },
            { "BackButton", "Back" },
            { "VolumeLabel", "Volume" },
            { "EnglishButton", "English" },
            { "SpanishButton", "Spanish" },
            { "CodeInputField", "Enter Room Code" },
            { "GeneratedCodeLabel", "Room Code: {0}" },

            // Spanish (es)
            { "StartGameButton_es", "Iniciar Juego" },
            { "CreateR_es", "Crear Sala" },
            { "JoinR_es", "Unirse a Sala" },
            { "Exit_es", "Salir" },
            { "Settings_es", "Configuración" },
            { "BackButton_es", "Volver" },
            { "VolumeLabel_es", "Volumen" },
            { "EnglishButton_es", "Inglés" },
            { "SpanishButton_es", "Español" },
            { "CodeInputField_es", "Código de sala" },
            { "GeneratedCodeLabel_es", "Código: {0}" }
        };
    }
    #endregion

    #region Event Handlers
    private void RegisterCallbacks()
    {
        // Main buttons
        _startButton.clicked += OnStartButtonClick;
        _exitButton.clicked += OnExitButtonClick;
        _settingsButton.clicked += OnSettingsButtonClick;

        // Settings
        _volumeSlider.RegisterValueChangedCallback(OnVolumeChanged);
        _englishButton.clicked += () => SetLanguage("en");
        _spanishButton.clicked += () => SetLanguage("es");
        _backButton.clicked += OnBackButtonClick;

        // Multiplayer
        _createRoomButton.clicked += OnCreateRoomClick;
        _joinRoomButton.clicked += OnJoinRoomClick;
        _startOfflineButton.clicked += OnStartOfflineClick;
        _backFromStartSButton.clicked += OnBackFromStartSClick;
    }

    private void OnStartButtonClick()
    {
        _container.style.display = DisplayStyle.None;
        _startS.style.display = DisplayStyle.Flex;
        ShowMultiplayerButtons();
    }

    private void ShowMultiplayerButtons()
    {
        _createRoomButton.style.display = DisplayStyle.Flex;
        _joinRoomButton.style.display = DisplayStyle.Flex;
        _startOfflineButton.style.display = DisplayStyle.Flex;
        _backFromStartSButton.style.display = DisplayStyle.Flex;
    }

    private void OnCreateRoomClick()
    {
        #if UNITY_EDITOR
        if (!SteamManager.Initialized)
        {
            Debug.Log("Steam no inicializado - Modo offline forzado");
            _networkManager.useSteam = false;
            _networkManager.StartHost();
            SceneManager.LoadScene("Lobby");
            return;
        }
        #endif

        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam no está inicializado!");
            return;
        }

        _createRoomButton.SetEnabled(false);
        StartCoroutine(ShowConnectionStatus());
        _networkManager.HostSteamLobby();
    }

    private IEnumerator ShowConnectionStatus()
    {
        var statusLabel = new Label("Creando lobby...");
        statusLabel.style.color = Color.white;
        statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _container.Add(statusLabel);

        yield return new WaitForSeconds(3f);

        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            statusLabel.text = "Error al conectar";
            yield return new WaitForSeconds(2f);
            _container.Remove(statusLabel);
            _createRoomButton.SetEnabled(true);
        }
    }

    private void OnJoinRoomClick()
    {
        if (!string.IsNullOrEmpty(_codeInputField.text))
        {
            Debug.Log($"Uniéndose con código: {_codeInputField.text}");
            _networkManager.StartClient();
        }
        else if (SteamManager.Initialized)
        {
            Debug.Log("Esperando invitación de Steam...");
        }
    }

    private void OnStartOfflineClick()
    {
        _networkManager.useSteam = false;
        _networkManager.StartHost();
        SceneManager.LoadScene("Lobby");
    }

    private void OnSettingsButtonClick()
    {
        _container.style.display = DisplayStyle.None;
        _settingsMenu.style.display = DisplayStyle.Flex;
    }

    private void OnBackButtonClick()
    {
        _settingsMenu.style.display = DisplayStyle.None;
        _container.style.display = DisplayStyle.Flex;
    }

    private void OnBackFromStartSClick()
    {
        _startS.style.display = DisplayStyle.None;
        _container.style.display = DisplayStyle.Flex;
    }

    private void OnVolumeChanged(ChangeEvent<float> evt)
    {
        _audioSource.volume = evt.newValue;
    }

    private void OnExitButtonClick()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    #endregion

    #region Localization
    private void SetLanguage(string lang)
    {
        _currentLanguage = lang;
        UpdateUITexts();
    }

    private void UpdateUITexts()
    {
        string suffix = _currentLanguage == "es" ? "_es" : "";

        _startButton.text = _translations["StartGameButton" + suffix];
        _createRoomButton.text = _translations["CreateR" + suffix];
        _joinRoomButton.text = _translations["JoinR" + suffix];
        _exitButton.text = _translations["Exit" + suffix];
        _settingsButton.text = _translations["Settings" + suffix];
        _backButton.text = _translations["BackButton" + suffix];
        _volumeLabel.text = _translations["VolumeLabel" + suffix];
        _englishButton.text = _translations["EnglishButton" + suffix];
        _spanishButton.text = _translations["SpanishButton" + suffix];
        _codeInputField.label = _translations["CodeInputField" + suffix];
    }
    #endregion
}