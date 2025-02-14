using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuEvents : MonoBehaviour
{
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

    private string roomCode;

    // Menú de configuraciones
    private VisualElement _settingsMenu;
    private Button _backButton;
    private Slider _volumeSlider;
    private Label _volumeLabel;
    private Button _englishButton;
    private Button _catalanButton;

    // Elementos de StartS
    private VisualElement _startS;
    private Button _startOfflineButton;
    private Button _backFromStartSButton;

    private string _currentLanguage = "en";

    private Dictionary<string, string> _translations;

    private void Awake()
    {
        Debug.Log("Awake");
        _audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();

        InitializeTranslations();

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
        _catalanButton = _settingsMenu.Q<Button>("CatalanButton");

        _startS = _document.rootVisualElement.Q<VisualElement>("StartS");
        _startOfflineButton = _document.rootVisualElement.Q<Button>("StartOffline");
        _backFromStartSButton = _document.rootVisualElement.Q<Button>("BackButton");

        HideMenus();

        // Eventos de botones
        _startButton.RegisterCallback<ClickEvent>(OnStartButtonClick);
        _joinRoomButton.RegisterCallback<ClickEvent>(OnJoinRoomClick);
        _createRoomButton.RegisterCallback<ClickEvent>(OnCreateRoomClick);
        _exitButton.RegisterCallback<ClickEvent>(OnExitButtonClick);
        _settingsButton.RegisterCallback<ClickEvent>(OnSettingsButtonClick);
        _backButton.RegisterCallback<ClickEvent>(OnBackButtonClick);

        _volumeSlider.RegisterValueChangedCallback(OnVolumeSliderChange);
        _englishButton.RegisterCallback<ClickEvent>(OnEnglishButtonClick);
        _catalanButton.RegisterCallback<ClickEvent>(OnCatalanButtonClick);

        _startOfflineButton.RegisterCallback<ClickEvent>(OnStartOfflineButtonClick);
        _backFromStartSButton.RegisterCallback<ClickEvent>(OnBackFromStartSButtonClick);

        SetLanguage(_currentLanguage);
    }

    private void HideMenus()
    {
        _createRoomButton.style.display = DisplayStyle.None;
        _joinRoomButton.style.display = DisplayStyle.None;
        _startOfflineButton.style.display = DisplayStyle.None;
        _backFromStartSButton.style.display = DisplayStyle.None;
        _codeInputField.style.display = DisplayStyle.None;
        _generatedCodeLabel.style.display = DisplayStyle.None;
        _settingsMenu.style.display = DisplayStyle.None;
    }

    private void InitializeTranslations()
    {
        _translations = new Dictionary<string, string>
        {
            { "StartGameButton", "Start Game" },
            { "CreateR", "Create Room" },
            { "JoinR", "Join Room" },
            { "StartOffline", "Start Offline" },
            { "Exit", "Exit" },
            { "Settings", "Settings" },
            { "BackButton", "Back" },
            { "VolumeLabel", "Volume" },
            { "EnglishButton", "English" },
            { "CatalanButton", "Catala" },
            { "CodeInputField", "Enter Room Code" },
            { "GeneratedCodeLabel", "Room Code: {0}" },
            { "Language", "Language" },
            { "Language_ca", "Idioma" },
            
            { "AudioSettings", "Audio Settings" },
            { "AudioSettings_ca", "Configuració d'àudio" },
            { "StartGameButton_ca", "Inicia Joc" },
            { "CreateR_ca", "Crea Sala" },
            { "JoinR_ca", "Uneix Sala" },
            { "StartOffline_ca", "Jugar Offline" },
            { "Exit_ca", "Surt" },
            { "Settings_ca", "Configuracio" },
            { "BackButton_ca", "Enrere" },
            { "VolumeLabel_ca", "Volum" },
            { "EnglishButton_ca", "Angles" },
            { "CatalanButton_ca", "Catala" },
            { "CodeInputField_ca", "Introdueix el codi de la sala" },
            { "GeneratedCodeLabel_ca", "Codi de la Sala: {0}" }
        };
    }

    private void SetLanguage(string language)
{
    _currentLanguage = language;
    string suffix = language == "ca" ? "_ca" : "";

    _startButton.text = _translations["StartGameButton" + suffix];
    _createRoomButton.text = _translations["CreateR" + suffix];
    _joinRoomButton.text = _translations["JoinR" + suffix];
    _startOfflineButton.text = _translations["StartOffline" + suffix];
    _exitButton.text = _translations["Exit" + suffix];
    _settingsButton.text = _translations["Settings" + suffix];
    _backButton.text = _translations["BackButton" + suffix];
    _backFromStartSButton.text = _translations["BackButton" + suffix];
    _volumeLabel.text = _translations["VolumeLabel" + suffix];
    _englishButton.text = _translations["EnglishButton" + suffix];
    _catalanButton.text = _translations["CatalanButton" + suffix];

    _generatedCodeLabel.text = string.Format(_translations["GeneratedCodeLabel" + suffix], roomCode);

    // Actualiza los textos de las etiquetas de configuración
    _settingsMenu.Q<Label>("AudioSettingsLabel").text = _translations["AudioSettings" + suffix];
    _settingsMenu.Q<Label>("LanguageLabel").text = _translations["Language" + suffix];
}

    private void OnEnglishButtonClick(ClickEvent evt) => SetLanguage("en");
    private void OnCatalanButtonClick(ClickEvent evt) => SetLanguage("ca");

    private void OnStartButtonClick(ClickEvent evt)
    {
        _container.style.display = DisplayStyle.None;
        _createRoomButton.style.display = DisplayStyle.Flex;
        _joinRoomButton.style.display = DisplayStyle.Flex;
        _startOfflineButton.style.display = DisplayStyle.Flex;
        _backFromStartSButton.style.display = DisplayStyle.Flex;
        _codeInputField.style.display = DisplayStyle.Flex;
        _generatedCodeLabel.style.display = DisplayStyle.Flex;
    }

    private void OnBackFromStartSButtonClick(ClickEvent evt)
    {
        HideMenus();
        _container.style.display = DisplayStyle.Flex;
    }

    private void OnCreateRoomClick(ClickEvent evt)
    {
        roomCode = "ABC123";
        _generatedCodeLabel.text = _translations["GeneratedCodeLabel" + (_currentLanguage == "ca" ? "_ca" : "")].Replace("{0}", roomCode);
        _generatedCodeLabel.style.display = DisplayStyle.Flex;
    }

    private void OnJoinRoomClick(ClickEvent evt)
    {
        string inputCode = _codeInputField.text;
        Debug.Log(string.IsNullOrEmpty(inputCode) ? "No room code entered." : "Joining room with code: " + inputCode);
    }

    private void OnSettingsButtonClick(ClickEvent evt)
    {
        _settingsMenu.style.display = DisplayStyle.Flex;
        _container.style.display = DisplayStyle.None;
    }

    private void OnBackButtonClick(ClickEvent evt)
    {
        _settingsMenu.style.display = DisplayStyle.None;
        _container.style.display = DisplayStyle.Flex;
    }

    private void OnVolumeSliderChange(ChangeEvent<float> evt) => _audioSource.volume = evt.newValue;

    private void OnStartOfflineButtonClick(ClickEvent evt) => SceneManager.LoadScene("Mansion");

    private void OnExitButtonClick(ClickEvent evt)
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
