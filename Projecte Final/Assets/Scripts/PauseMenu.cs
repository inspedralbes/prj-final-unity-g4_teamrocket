using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // Importar SceneManager

public class PauseMenu : MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _pauseMenu;
    private Button _resumeButton, _optionsButton, _exitButton;
    private VisualElement _menuContainer, _optionsMenu;
    private Slider _volumeSlider;
    private Label _volumeLabel, _volumePercentage;
    private Button _englishButton, _spanishButton, _backButton;
    private bool _isPaused = false;
    private AudioSource _audioSource;
    private string _currentLanguage = "en"; // Idioma por defecto

    private void Awake()
    {
        _document = GetComponent<UIDocument>();

        // Obtener elementos del UI
        _pauseMenu = _document.rootVisualElement.Q<VisualElement>("PauseM");
        _menuContainer = _pauseMenu.Q<VisualElement>("MenuContainer");
        _resumeButton = _menuContainer.Q<Button>("ResumeButton");
        _optionsButton = _menuContainer.Q<Button>("OptionsButton");
        _exitButton = _menuContainer.Q<Button>("ExitButton");

        _optionsMenu = _menuContainer.Q<VisualElement>("OptionsMenu");
        _volumeSlider = _optionsMenu.Q<Slider>("VolumeSlider");
        _volumeLabel = _optionsMenu.Q<Label>("VolumeLabel");
        _volumePercentage = _optionsMenu.Q<Label>("VolumePercentage");
        _englishButton = _optionsMenu.Q<Button>("EnglishButton");
        _spanishButton = _optionsMenu.Q<Button>("SpanishButton");
        _backButton = _optionsMenu.Q<Button>("BackButton"); // Botón Back

        // Inicializar
        _pauseMenu.style.display = DisplayStyle.None;

        // Asignar eventos
        _resumeButton.RegisterCallback<ClickEvent>(evt => ResumeGame());
        _optionsButton.RegisterCallback<ClickEvent>(evt => ShowOptions());
        _exitButton.RegisterCallback<ClickEvent>(evt => ExitGame());
        _volumeSlider.RegisterValueChangedCallback(evt => ChangeVolume(evt.newValue));
        _englishButton.RegisterCallback<ClickEvent>(evt => SetLanguage("en"));
        _spanishButton.RegisterCallback<ClickEvent>(evt => SetLanguage("es"));
        _backButton.RegisterCallback<ClickEvent>(evt => BackToPauseMenu()); // Evento de "Back"

        // Establecer idioma inicial
        SetLanguage(_currentLanguage);
        ChangeVolume(_volumeSlider.value); // Mostrar volumen inicial
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            if (_isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private void PauseGame()
    {
        _isPaused = true;
        _pauseMenu.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
    }

    private void ResumeGame()
    {
        _isPaused = false;
        _pauseMenu.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
    }

    private void ExitGame()
{
    Time.timeScale = 1f; // Asegurarse de reanudar el tiempo
    SceneManager.LoadScene("SampleScene"); // Cargar la escena del menú principal
}

    private void ShowOptions()
    {
        // Mostrar el menú de opciones y ocultar los botones principales
        _optionsMenu.style.display = DisplayStyle.Flex;
        _resumeButton.style.display = DisplayStyle.None;
        _optionsButton.style.display = DisplayStyle.None;
        _exitButton.style.display = DisplayStyle.None;
    }

    private void BackToPauseMenu()
    {
        // Ocultar opciones y mostrar los botones principales
        _optionsMenu.style.display = DisplayStyle.None;
        _resumeButton.style.display = DisplayStyle.Flex;
        _optionsButton.style.display = DisplayStyle.Flex;
        _exitButton.style.display = DisplayStyle.Flex;
    }

    private void ChangeVolume(float value)
    {
        if (_audioSource != null)
            _audioSource.volume = value;

        // Convertir a porcentaje y mostrar
        _volumePercentage.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void SetLanguage(string language)
    {
        _currentLanguage = language;
        _resumeButton.text = language == "es" ? "Reanudar" : "Resume";
        _optionsButton.text = language == "es" ? "Opciones" : "Options";
        _exitButton.text = language == "es" ? "Salir" : "Exit";
        _volumeLabel.text = language == "es" ? "Volumen" : "Volume";
        _englishButton.text = language == "es" ? "Inglés" : "English";
        _spanishButton.text = language == "es" ? "Español" : "Spanish";
        _backButton.text = language == "es" ? "Atrás" : "Back"; // Cambio de texto en el botón "Back"
    }
}
