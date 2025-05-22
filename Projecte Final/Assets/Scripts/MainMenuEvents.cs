using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;

public class MainMenuEvents : MonoBehaviour
{
    // Referencias UI
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

    // Menús
    private VisualElement _settingsMenu;
    private VisualElement _startS;

    // Configuración
    private Slider _volumeSlider;
    private Label _volumeLabel;
    private Button _englishButton;
    private Button _spanishButton;
    private Button _backButton;
    private Button _startOfflineButton;
    private Button _backFromStartSButton;

    // Variables del juego
    private string roomCode;
    private string _currentLanguage = "en";
    private Dictionary<string, string> _translations;
    private bool _useSteam = true;

    // Network
    private CustomNetworkManager _networkManager;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();
        _networkManager = FindFirstObjectByType<CustomNetworkManager>();

        InitializeTranslations();
        SetupUIReferences();
        RegisterCallbacks();
        SetLanguage(_currentLanguage);
    }

    private void InitializeTranslations()
    {
        _translations = new Dictionary<string, string>
        {
            // Inglés (en)
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
            { "StartOfflineButton", "Start Offline" },

            // Español (es)
            { "StartGameButton_es", "Iniciar Juego" },
            { "CreateR_es", "Crear Sala" },
            { "JoinR_es", "Unirse a Sala" },
            { "Exit_es", "Salir" },
            { "Settings_es", "Configuracion" },
            { "BackButton_es", "Volver" },
            { "VolumeLabel_es", "Volumen" },
            { "EnglishButton_es", "Ingles" },
            { "SpanishButton_es", "Castellano" },
            { "CodeInputField_es", "Introduce el Código de la Sala" },
            { "GeneratedCodeLabel_es", "Código de la Sala: {0}" },
            { "StartOfflineButton_es", "Jugar Solo" }
        };
    }

    private void SetupUIReferences()
    {
        _startButton = _document.rootVisualElement.Q<Button>("StartGameButton");
        _createRoomButton = _document.rootVisualElement.Q<Button>("CreateR");
        _joinRoomButton = _document.rootVisualElement.Q<Button>("JoinR");
        _exitButton = _document.rootVisualElement.Q<Button>("Exit");
        _container = _document.rootVisualElement.Q<VisualElement>("Container");
        _codeInputField = _document.rootVisualElement.Q<TextField>("CodeInputField");
        _generatedCodeLabel = _document.rootVisualElement.Q<Label>("GeneratedCodeLabel");

        _settingsMenu = _document.rootVisualElement.Q<VisualElement>("SettingsMenu");
        _settingsButton = _document.rootVisualElement.Q<Button>("Settings");
        _backButton = _settingsMenu.Q<Button>("BackButton");
        _volumeSlider = _settingsMenu.Q<Slider>("VolumeSlider");
        _volumeLabel = _settingsMenu.Q<Label>("VolumeLabel");
        _englishButton = _settingsMenu.Q<Button>("EnglishButton");
        _spanishButton = _settingsMenu.Q<Button>("SpanishButton");

        _startS = _document.rootVisualElement.Q<VisualElement>("StartS");
        _startOfflineButton = _document.rootVisualElement.Q<Button>("StartOffline");
        _backFromStartSButton = _document.rootVisualElement.Q<Button>("BackButton");

        // Configuración inicial de visibilidad
        _startS.style.display = DisplayStyle.None;
        _settingsMenu.style.display = DisplayStyle.None;
        _createRoomButton.style.display = DisplayStyle.None;
        _joinRoomButton.style.display = DisplayStyle.None;
        _startOfflineButton.style.display = DisplayStyle.None;
        _backFromStartSButton.style.display = DisplayStyle.None;
        _codeInputField.style.display = DisplayStyle.None;
        _generatedCodeLabel.style.display = DisplayStyle.None;
    }

    private void RegisterCallbacks()
    {
        // Botones principales
        _startButton.clicked += OnStartButtonClick;
        _exitButton.clicked += OnExitButtonClick;
        _settingsButton.clicked += OnSettingsButtonClick;

        // Configuración
        _volumeSlider.RegisterValueChangedCallback(OnVolumeSliderChange);
        _englishButton.clicked += () => SetLanguage("en");
        _spanishButton.clicked += () => SetLanguage("es");
        _backButton.clicked += OnBackButtonClick;

        // Multiplayer
        _createRoomButton.clicked += OnCreateRoomClick;
        _joinRoomButton.clicked += OnJoinRoomClick;
        _startOfflineButton.clicked += OnStartOfflineButtonClick;
        _backFromStartSButton.clicked += OnBackFromStartSButtonClick;
    }

    #region Steam Lobby Implementation
    private void OnStartButtonClick()
    {
        Debug.Log("Start button clicked");
        
        // Mostrar menú de opciones multiplayer
        _container.style.display = DisplayStyle.None;
        _startS.style.display = DisplayStyle.Flex;
        
        // Mostrar botones relevantes
        _createRoomButton.style.display = DisplayStyle.Flex;
        _joinRoomButton.style.display = DisplayStyle.Flex;
        _startOfflineButton.style.display = DisplayStyle.Flex;
        _backFromStartSButton.style.display = DisplayStyle.Flex;
    }

    private void OnCreateRoomClick()
    {
        Debug.Log("Creating Steam Lobby");
        
        if (_useSteam && SteamManager.Initialized)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
        }
        else if (!_useSteam)
        {
            StartOfflineGame();
        }
        else
        {
            Debug.LogError("Steam no está inicializado");
        }
    }

    // Este método debe ser llamado desde el CustomNetworkManager cuando se crea el lobby
    public void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            Debug.LogError("Error al crear lobby: " + callback.m_eResult);
            return;
        }

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(lobbyId, "name", "Darkness Unseen Lobby");
        
        // Iniciar el juego
        _networkManager.StartHost();
        SceneManager.LoadScene("Lobby");
    }

    private void OnJoinRoomClick()
    {
        string inputCode = _codeInputField.text;
        if (!string.IsNullOrEmpty(inputCode))
        {
            Debug.Log("Joining room with code: " + inputCode);
            // Aquí implementarías la lógica para unirse a un lobby existente
        }
    }

    private void OnStartOfflineButtonClick()
    {
        StartOfflineGame();
    }

    private void StartOfflineGame()
    {
        _networkManager.StartHost();
        SceneManager.LoadScene("Lobby");
    }
    #endregion

    #region UI Management
    private void OnBackFromStartSButtonClick()
    {
        _startS.style.display = DisplayStyle.None;
        _container.style.display = DisplayStyle.Flex;
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

    private void SetLanguage(string language)
    {
        _currentLanguage = language;
        _startButton.text = _translations["StartGameButton" + (language == "es" ? "_es" : "")];
        _createRoomButton.text = _translations["CreateR" + (language == "es" ? "_es" : "")];
        _joinRoomButton.text = _translations["JoinR" + (language == "es" ? "_es" : "")];
        _exitButton.text = _translations["Exit" + (language == "es" ? "_es" : "")];
        _settingsButton.text = _translations["Settings" + (language == "es" ? "_es" : "")];
        _backButton.text = _translations["BackButton" + (language == "es" ? "_es" : "")];
        _volumeLabel.text = _translations["VolumeLabel" + (language == "es" ? "_es" : "")];
        _englishButton.text = _translations["EnglishButton" + (language == "es" ? "_es" : "")];
        _spanishButton.text = _translations["SpanishButton" + (language == "es" ? "_es" : "")];
        _startOfflineButton.text = _translations["StartOfflineButton" + (language == "es" ? "_es" : "")]; // Nueva línea


        _codeInputField.label = _translations["CodeInputField" + (language == "es" ? "_es" : "")];
        _generatedCodeLabel.text = string.Format(_translations["GeneratedCodeLabel" + (language == "es" ? "_es" : "")], roomCode);
    }
    #endregion

    #region Utility Methods
    private void OnVolumeSliderChange(ChangeEvent<float> evt)
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
}