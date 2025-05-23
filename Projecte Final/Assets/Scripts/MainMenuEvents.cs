using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuEvents : MonoBehaviour
{
    [Header("UI References")]
    public UIDocument menuDocument;
    private VisualElement _root;
    private Button _startButton;
    private Button _createRoomButton;
    private Button _joinRoomButton;
    private Button _exitButton;
    private Button _settingsButton;
    private VisualElement _container;
    private TextField _codeInputField;
    private Label _generatedCodeLabel;
    private AudioSource _audioSource;

    [Header("Menu References")]
    private VisualElement _settingsMenu;
    private VisualElement _startS;
    private VisualElement _multiplayerMenu;

    [Header("Settings References")]
    private Slider _volumeSlider;
    private Label _volumeLabel;
    private Button _englishButton;
    private Button _spanishButton;
    private Button _backButton;
    private Button _startOfflineButton;
    private Button _backFromStartSButton;

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
        SafeHideAllSubMenus();
    }

    private void InitializeComponents()
    {
        _audioSource = GetComponent<AudioSource>();
        _networkManager = FindFirstObjectByType<CustomNetworkManager>();
        
        if (menuDocument == null)
        {
            Debug.LogError("UIDocument no asignado en el inspector!");
            return;
        }
        
        _root = menuDocument.rootVisualElement;
    }

    private void SetupUIReferences()
    {
        try
        {
            // Main Buttons
            _startButton = _root.Q<Button>("StartGameButton");
            _createRoomButton = _root.Q<Button>("CreateR");
            _joinRoomButton = _root.Q<Button>("JoinR");
            _exitButton = _root.Q<Button>("Exit");
            _settingsButton = _root.Q<Button>("Settings");
            _container = _root.Q<VisualElement>("Container");
            _codeInputField = _root.Q<TextField>("CodeInputField");
            _generatedCodeLabel = _root.Q<Label>("GeneratedCodeLabel");

            // Menus
            _settingsMenu = _root.Q<VisualElement>("SettingsMenu");
            _startS = _root.Q<VisualElement>("StartS");
            _multiplayerMenu = _root.Q<VisualElement>("MultiplayerMenu");

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
        catch (System.Exception e)
        {
            Debug.LogError("Error asignando referencias UI: " + e.Message);
        }
    }

    private void SafeHideAllSubMenus()
    {
        try
        {
            if (_settingsMenu != null) _settingsMenu.style.display = DisplayStyle.None;
            if (_startS != null) _startS.style.display = DisplayStyle.None;
            if (_multiplayerMenu != null) _multiplayerMenu.style.display = DisplayStyle.None;
            if (_codeInputField != null) _codeInputField.style.display = DisplayStyle.None;
            if (_generatedCodeLabel != null) _generatedCodeLabel.style.display = DisplayStyle.None;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error ocultando menús: " + e.Message);
        }
    }
    #endregion

    #region Localization
    private void InitializeTranslations()
    {
        _translations = new Dictionary<string, string>
        {
            // English
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

            // Spanish
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

    private void SetLanguage(string lang)
    {
        _currentLanguage = lang;
        UpdateUITexts();
    }

    private void UpdateUITexts()
    {
        try
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
        catch (System.Exception e)
        {
            Debug.LogError("Error actualizando textos: " + e.Message);
        }
    }
    #endregion

    #region Event Handlers
    private void RegisterCallbacks()
    {
        try
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
        catch (System.Exception e)
        {
            Debug.LogError("Error registrando callbacks: " + e.Message);
        }
    }

    private void OnStartButtonClick()
    {
        try
        {
            _container.style.display = DisplayStyle.None;
            _startS.style.display = DisplayStyle.Flex;
            ShowMultiplayerButtons();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error en StartButton: " + e.Message);
        }
    }

    private void ShowMultiplayerButtons()
    {
        try
        {
            _createRoomButton.style.display = DisplayStyle.Flex;
            _joinRoomButton.style.display = DisplayStyle.Flex;
            _startOfflineButton.style.display = DisplayStyle.Flex;
            _backFromStartSButton.style.display = DisplayStyle.Flex;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error mostrando botones: " + e.Message);
        }
    }

    private void OnCreateRoomClick()
    {
        try
        {
            if (_networkManager == null)
            {
                Debug.LogError("NetworkManager no encontrado!");
                return;
            }

            _createRoomButton.SetEnabled(false);
            StartCoroutine(ShowConnectionStatus());
            
            // Usa directamente el método del NetworkManager
            _networkManager.HostSteamLobby();
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error creando sala: " + e.Message);
        }
    }

    private IEnumerator ShowConnectionStatus()
    {
        var statusLabel = new Label(_currentLanguage == "es" ? "Creando lobby..." : "Creating lobby...");
        statusLabel.style.color = Color.white;
        statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        _container.Add(statusLabel);

        yield return new WaitForSeconds(3f);

        if (SceneManager.GetActiveScene().name != "Lobby")
        {
            statusLabel.text = _currentLanguage == "es" ? "Error al conectar" : "Connection failed";
            yield return new WaitForSeconds(2f);
            _container.Remove(statusLabel);
            _createRoomButton.SetEnabled(true);
        }
    }

    private void OnJoinRoomClick()
    {
        try
        {
            if (!string.IsNullOrEmpty(_codeInputField.text))
            {
                Debug.Log("Uniéndose con código: " + _codeInputField.text);
                if (_networkManager != null)
                {
                    _networkManager.StartClient();
                }
            }
            else if (_networkManager.IsSteamReady())
            {
                Debug.Log("Esperando invitación de Steam...");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error uniéndose a sala: " + e.Message);
        }
    }

    private void OnStartOfflineClick()
    {
        try
        {
            if (_networkManager != null)
            {
                _networkManager.useSteam = false;
                _networkManager.StartHost();
                SceneManager.LoadScene("Lobby");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error iniciando offline: " + e.Message);
        }
    }

    private void OnSettingsButtonClick()
    {
        try
        {
            _container.style.display = DisplayStyle.None;
            _settingsMenu.style.display = DisplayStyle.Flex;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error abriendo configuración: " + e.Message);
        }
    }

    private void OnBackButtonClick()
    {
        try
        {
            _settingsMenu.style.display = DisplayStyle.None;
            _container.style.display = DisplayStyle.Flex;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error volviendo atrás: " + e.Message);
        }
    }

    private void OnBackFromStartSClick()
    {
        try
        {
            _startS.style.display = DisplayStyle.None;
            _container.style.display = DisplayStyle.Flex;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error volviendo al menú: " + e.Message);
        }
    }

    private void OnVolumeChanged(ChangeEvent<float> evt)
    {
        try
        {
            _audioSource.volume = evt.newValue;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error cambiando volumen: " + e.Message);
        }
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