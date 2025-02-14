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
    private Label _volumeLabel, _volumePercentage, _pauseTitleLabel; // Añadido _pauseTitleLabel
    private Button _englishButton, _catalanButton, _backButton; 
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
        _catalanButton = _optionsMenu.Q<Button>("CatalanButton");
        _backButton = _optionsMenu.Q<Button>("BackButton");

        _pauseTitleLabel = _menuContainer.Q<Label>("PauseTitle"); // Obtener el Label "Pausa"

        // Inicializar
        _pauseMenu.style.display = DisplayStyle.None;

        // Asignar eventos
        _resumeButton.RegisterCallback<ClickEvent>(evt => ResumeGame());
        _optionsButton.RegisterCallback<ClickEvent>(evt => ShowOptions());
        _exitButton.RegisterCallback<ClickEvent>(evt => ExitGame());
        _volumeSlider.RegisterValueChangedCallback(evt => ChangeVolume(evt.newValue));
        _englishButton.RegisterCallback<ClickEvent>(evt => SetLanguage("en"));
        _catalanButton.RegisterCallback<ClickEvent>(evt => SetLanguage("ca"));
        _backButton.RegisterCallback<ClickEvent>(evt => BackToPauseMenu());

        // Establecer idioma inicial
        SetLanguage(_currentLanguage);
        ChangeVolume(_volumeSlider.value); // Mostrar volumen inicial
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !SiEscenaActiva("GameOver") && !SiEscenaActiva("LevelComplete")) 
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
        SceneManager.LoadScene("MainMenuUnseen"); // Cargar la escena del menú principal
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
        _resumeButton.text = language == "ca" ? "Repren" : "Resume";
        _optionsButton.text = language == "ca" ? "Opcions" : "Options";
        _exitButton.text = language == "ca" ? "Sortir" : "Exit";
        _volumeLabel.text = language == "ca" ? "Volum" : "Volume";
        _englishButton.text = language == "ca" ? "Angles" : "English";
        _catalanButton.text = language == "ca" ? "Catala" : "Catalan";
        _backButton.text = language == "ca" ? "Enrere" : "Back";

        // Cambiar el texto del título de pausa
        _pauseTitleLabel.text = language == "ca" ? "Pausa" : "Pause"; // Cambiar el texto
    }

    public static bool SiEscenaActiva(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        return scene.IsValid() && scene.isLoaded;
    }
}
