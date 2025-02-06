using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuEvents : MonoBehaviour
{
    private UIDocument _document;
    private Button _startButton;
    private Button _createRoomButton;
    private Button _joinRoomButton;
    private Button _exitButton;
    private VisualElement _container;
    private TextField _codeInputField; // Campo para el código de la sala
    private Label _generatedCodeLabel; // Mostrar código generado
    private List<Button> _menuButtons = new List<Button>();
    private AudioSource _audioSource;

    private string roomCode;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _document = GetComponent<UIDocument>();

        // Obtener referencias a los elementos UI
        _startButton = _document.rootVisualElement.Q<Button>("StartGameButton");
        _createRoomButton = _document.rootVisualElement.Q<Button>("CreateR");
        _joinRoomButton = _document.rootVisualElement.Q<Button>("JoinR");
        _exitButton = _document.rootVisualElement.Q<Button>("Exit");
        _container = _document.rootVisualElement.Q<VisualElement>("Container");
        _codeInputField = _document.rootVisualElement.Q<TextField>("CodeInputField");
        _generatedCodeLabel = _document.rootVisualElement.Q<Label>("GeneratedCodeLabel");

        // Ocultar los botones y el campo de código al inicio
        _createRoomButton.style.display = DisplayStyle.None;
        _joinRoomButton.style.display = DisplayStyle.None;
        _codeInputField.style.display = DisplayStyle.None;
        _generatedCodeLabel.style.display = DisplayStyle.None;

        // Registrar eventos
        _startButton.RegisterCallback<ClickEvent>(OnStartButtonClick);
        _joinRoomButton.RegisterCallback<ClickEvent>(OnJoinRoomClick);
        _createRoomButton.RegisterCallback<ClickEvent>(OnCreateRoomClick);
        _exitButton.RegisterCallback<ClickEvent>(OnExitButtonClick);

        // Obtener todos los botones del menú y registrar el sonido
        _menuButtons = _document.rootVisualElement.Query<Button>().ToList();
        for (int i = 0; i < _menuButtons.Count; i++)
        {
            _menuButtons[i].RegisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }

    private void OnDisable()
    {
        _startButton.UnregisterCallback<ClickEvent>(OnStartButtonClick);
        _joinRoomButton.UnregisterCallback<ClickEvent>(OnJoinRoomClick);
        _createRoomButton.UnregisterCallback<ClickEvent>(OnCreateRoomClick);
        _exitButton.UnregisterCallback<ClickEvent>(OnExitButtonClick);

        for (int i = 0; i < _menuButtons.Count; i++)
        {
            _menuButtons[i].UnregisterCallback<ClickEvent>(OnAllButtonsClick);
        }
    }

    private void OnStartButtonClick(ClickEvent evt)
    {
        Debug.Log("You pressed the Start Button");

        // Ocultar el menú principal
        _container.style.display = DisplayStyle.None;

        // Mostrar los botones "Create Room" y "Join Room"
        _createRoomButton.style.display = DisplayStyle.Flex;
        _joinRoomButton.style.display = DisplayStyle.Flex;
    }

    private void OnJoinRoomClick(ClickEvent evt)
    {
        Debug.Log("Join Room button clicked");

        // Mostrar el campo de entrada de código
        _codeInputField.style.display = DisplayStyle.Flex;
        _generatedCodeLabel.style.display = DisplayStyle.None; // Ocultar el código generado
    }

    private void OnCreateRoomClick(ClickEvent evt)
    {
        Debug.Log("Create Room button clicked");

        // Generar un código aleatorio
        roomCode = GenerateRoomCode();
        _generatedCodeLabel.text = "Room Code: " + roomCode;

        // Mostrar el código generado
        _generatedCodeLabel.style.display = DisplayStyle.Flex;
    }

    private void OnExitButtonClick(ClickEvent evt)
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }

    private void OnAllButtonsClick(ClickEvent evt)
    {
        _audioSource.Play();
    }

    // Método para generar un código aleatorio
    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        string code = "";
        for (int i = 0; i < 6; i++) // Código de 6 caracteres
        {
            code += chars[Random.Range(0, chars.Length)];
        }
        return code;
    }
}
