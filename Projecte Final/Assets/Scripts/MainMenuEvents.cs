using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic; // Agrega esta línea para usar Dictionary
using UnityEngine.SceneManagement;


public class MainMenuEvents : MonoBehaviour
{
    private UIDocument _document;
    private Button _startButton;
    private Button _createRoomButton;
    private Button _joinRoomButton;
    private Button _exitButton;
    private Button _settingsButton;   // Botón de Settings
    private VisualElement _container;
    private TextField _codeInputField; // Campo para el código de la sala
    private Label _generatedCodeLabel; // Mostrar código generado
    private AudioSource _audioSource;

    private string roomCode;

    // Nuevos elementos para Settings
    private VisualElement _settingsMenu;
    private Button _backButton;      // Botón de Volver
    private Slider _volumeSlider;    // Slider de volumen
    private Label _volumeLabel;      // Etiqueta del volumen
    private Button _englishButton;   // Botón para inglés
    private Button _spanishButton;   // Botón para español

    // Nuevos elementos para StartS
    private VisualElement _startS;
    private Button _startOfflineButton;  // Botón Start Offline
    private Button _backFromStartSButton; // Botón Back en StartS

    private string _currentLanguage = "en"; // Idioma por defecto

    private Dictionary<string, string> _translations; // Agregado

    private void Awake()
    {
        Debug.Log("Awake");
        _audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();

        // Inicializamos las traducciones
        InitializeTranslations();

        // Obtener referencias a los elementos UI
        _startButton = _document.rootVisualElement.Q<Button>("StartGameButton");
        _createRoomButton = _document.rootVisualElement.Q<Button>("CreateR");
        _joinRoomButton = _document.rootVisualElement.Q<Button>("JoinR");
        _exitButton = _document.rootVisualElement.Q<Button>("Exit");
        _container = _document.rootVisualElement.Q<VisualElement>("Container");
        _codeInputField = _document.rootVisualElement.Q<TextField>("CodeInputField");
        _generatedCodeLabel = _document.rootVisualElement.Q<Label>("GeneratedCodeLabel");

        // Menú de configuraciones
        _settingsMenu = _document.rootVisualElement.Q<VisualElement>("SettingsMenu");
        _settingsButton = _document.rootVisualElement.Q<Button>("Settings");
        _backButton = _settingsMenu.Q<Button>("BackButton");
        _volumeSlider = _settingsMenu.Q<Slider>("VolumeSlider");
        _volumeLabel = _settingsMenu.Q<Label>("VolumeLabel");
        _englishButton = _settingsMenu.Q<Button>("EnglishButton");
        _spanishButton = _settingsMenu.Q<Button>("SpanishButton");

        // Elementos de StartS
        _startS = _document.rootVisualElement.Q<VisualElement>("StartS");
        _startOfflineButton = _document.rootVisualElement.Q<Button>("StartOffline");
        _backFromStartSButton = _document.rootVisualElement.Q<Button>("BackButton");

        // Ocultar elementos al inicio
        _createRoomButton.style.display = DisplayStyle.None;
        _joinRoomButton.style.display = DisplayStyle.None;
        _startOfflineButton.style.display = DisplayStyle.None;
        _backFromStartSButton.style.display = DisplayStyle.None;
        _codeInputField.style.display = DisplayStyle.None;
        _generatedCodeLabel.style.display = DisplayStyle.None;
        _settingsMenu.style.display = DisplayStyle.None;

        // Registrar eventos
        _startButton.RegisterCallback<ClickEvent>(OnStartButtonClick);
        _joinRoomButton.RegisterCallback<ClickEvent>(OnJoinRoomClick);
        _createRoomButton.RegisterCallback<ClickEvent>(OnCreateRoomClick);
        _exitButton.RegisterCallback<ClickEvent>(OnExitButtonClick);
        _settingsButton.RegisterCallback<ClickEvent>(OnSettingsButtonClick);
        _backButton.RegisterCallback<ClickEvent>(OnBackButtonClick);

        // Configuración de volumen
        _volumeSlider.RegisterValueChangedCallback(OnVolumeSliderChange);
        _englishButton.RegisterCallback<ClickEvent>(OnEnglishButtonClick);
        _spanishButton.RegisterCallback<ClickEvent>(OnSpanishButtonClick);

        // Eventos de StartS
        _startOfflineButton.RegisterCallback<ClickEvent>(OnStartOfflineButtonClick);
        _backFromStartSButton.RegisterCallback<ClickEvent>(OnBackFromStartSButtonClick);

        // Establecer idioma inicial
        SetLanguage(_currentLanguage);
    }

    private void InitializeTranslations()
    {
        // Diccionario con traducciones
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
            { "GeneratedCodeLabel_es", "Código de la Sala: {0}" }
        };
    }

    private void SetLanguage(string language)
    {
        _currentLanguage = language;

        // Cambiar textos en la interfaz según el idioma
        _startButton.text = _translations["StartGameButton" + (language == "es" ? "_es" : "")];
        _createRoomButton.text = _translations["CreateR" + (language == "es" ? "_es" : "")];
        _joinRoomButton.text = _translations["JoinR" + (language == "es" ? "_es" : "")];
        _exitButton.text = _translations["Exit" + (language == "es" ? "_es" : "")];
        _settingsButton.text = _translations["Settings" + (language == "es" ? "_es" : "")];
        _backButton.text = _translations["BackButton" + (language == "es" ? "_es" : "")];
        _volumeLabel.text = _translations["VolumeLabel" + (language == "es" ? "_es" : "")];
        _englishButton.text = _translations["EnglishButton" + (language == "es" ? "_es" : "")];
        _spanishButton.text = _translations["SpanishButton" + (language == "es" ? "_es" : "")];

        // Establecer marcador de posición para el campo de código
        _codeInputField.RegisterCallback<FocusInEvent>(evt => 
        {
            // Establecer el marcador de posición solo si el campo está vacío
            if (string.IsNullOrEmpty(_codeInputField.value))
            {
                _codeInputField.SetValueWithoutNotify(_translations["CodeInputField" + (language == "es" ? "_es" : "")]);
            }
        });

        _generatedCodeLabel.text = string.Format(_translations["GeneratedCodeLabel" + (language == "es" ? "_es" : "")], roomCode);
    }

    private void OnEnglishButtonClick(ClickEvent evt)
    {
        SetLanguage("en");
    }

    private void OnSpanishButtonClick(ClickEvent evt)
    {
        SetLanguage("es");
    }

    private void OnStartButtonClick(ClickEvent evt)
{
    Debug.Log("Start button clicked!");

    // Ocultar los botones iniciales
    _container.style.display = DisplayStyle.None;

    // Mostrar los nuevos botones después de un pequeño retraso
    ShowNewButtons();
}

private void ShowNewButtons()
{
    // Asegurarse de que todos los botones estén visibles
    _createRoomButton.style.display = DisplayStyle.Flex;
    _joinRoomButton.style.display = DisplayStyle.Flex;
    _startOfflineButton.style.display = DisplayStyle.Flex;
    _backFromStartSButton.style.display = DisplayStyle.Flex;

    // Agregar elementos adicionales si es necesario
    _codeInputField.style.display = DisplayStyle.Flex;
    _generatedCodeLabel.style.display = DisplayStyle.Flex;
}

    private void OnBackFromStartSButtonClick(ClickEvent evt)
{
    Debug.Log("Back button from StartS clicked");

    // Ocultar la nueva pantalla y volver al menú inicial
    _createRoomButton.style.display = DisplayStyle.None;
    _joinRoomButton.style.display = DisplayStyle.None;
    _startOfflineButton.style.display = DisplayStyle.None;
    _backFromStartSButton.style.display = DisplayStyle.None;
    _codeInputField.style.display = DisplayStyle.None;
    _generatedCodeLabel.style.display = DisplayStyle.None;

    // Mostrar de nuevo el contenedor de botones iniciales
    _container.style.display = DisplayStyle.Flex;
}

    private void OnCreateRoomClick(ClickEvent evt)
    {
        roomCode = "ABC123";
        _generatedCodeLabel.text = "Room Code: " + roomCode;
        _generatedCodeLabel.style.display = DisplayStyle.Flex;
    }

    private void OnJoinRoomClick(ClickEvent evt)
    {
        string inputCode = _codeInputField.text;
        if (!string.IsNullOrEmpty(inputCode))
        {
            Debug.Log("Joining room with code: " + inputCode);
        }
        else
        {
            Debug.Log("No room code entered.");
        }
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

    private void OnVolumeSliderChange(ChangeEvent<float> evt)
    {
        _audioSource.volume = evt.newValue;
    }

    private IEnumerator HideButtonsCoroutine(System.Action onComplete)
    {
        var startTime = Time.time;
        float duration = 0.5f; // Duración de la animación en segundos

        while (Time.time - startTime < duration)
        {
            float alpha = 1 - (Time.time - startTime) / duration;
            SetButtonAlpha(alpha);
            yield return null;
        }

        SetButtonAlpha(0);
        onComplete?.Invoke();
    }

    private IEnumerator ShowButtonsCoroutine(Button button1, Button button2, Button button3)
    {
        var startTime = Time.time;
        float duration = 0.5f;

        while (Time.time - startTime < duration)
        {
            float alpha = (Time.time - startTime) / duration;
            SetButtonAlpha(alpha);
            yield return null;
        }

        SetButtonAlpha(1);
    }

    private void SetButtonAlpha(float alpha)
    {
        _startButton.style.opacity = alpha;
        _createRoomButton.style.opacity = alpha;
        _joinRoomButton.style.opacity = alpha;
        _exitButton.style.opacity = alpha;
        _settingsButton.style.opacity = alpha;
    }

    private void OnStartOfflineButtonClick(ClickEvent evt)
{
    Debug.Log("Start Offline clicked");

    // Cambiar a la escena "Carga"
    SceneManager.LoadScene("Lobby");
}

private void OnExitButtonClick(ClickEvent evt)
{
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Detener el juego en el editor
    #else
        Application.Quit(); // Cerrar la aplicación en la build final
    #endif
}

}
