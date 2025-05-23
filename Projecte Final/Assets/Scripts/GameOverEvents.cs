using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameOverEvents : MonoBehaviour
{
    public VisualElement gameOverElement; // Elemento de Game Over
    public Label gameOverLabel; // Etiqueta GameOver
    public Button tryAgainButton; // Botón Try Again
    public Button exitButton; // Botón Exit
    public ParticleSystem bloodRain; // Referencia al ParticleSystem de lluvia de sangre

    private void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

        gameOverElement = root.Q<VisualElement>("GameO");
        gameOverLabel = root.Q<Label>("GameOverLabel");
        tryAgainButton = root.Q<Button>("TryAgainButton");
        exitButton = root.Q<Button>("ExitButton");

        // Obtener el objeto que contiene el ParticleSystem
        var bloodRainGameObject = GameObject.Find("GeneradorSangre");
        if (bloodRainGameObject != null)
        {
            bloodRain = bloodRainGameObject.GetComponent<ParticleSystem>();
            bloodRain.Play(); // Reproducir la lluvia de sangre inmediatamente
        }

        // Configuración inicial
        gameOverElement.style.opacity = 0;
        gameOverLabel.style.opacity = 0;
        tryAgainButton.style.opacity = 0;
        exitButton.style.opacity = 0;

        // Llamada a la animación de aparición
        StartCoroutine(FadeInGameOver());

        // Agregar el evento al botón "Exit"
        exitButton.clicked += OnExitButtonClicked;
        tryAgainButton.clicked += OntryAgainButton;
    }

    private IEnumerator FadeInGameOver()
{
    // Aparecer el fondo primero
    gameOverElement.style.opacity = 1;
    yield return new WaitForSeconds(0.5f); // Pequeña pausa para enfatizar el efecto

    // Mostrar el texto "Game Over" primero
    float fadeDuration = 2f;
    float timeElapsed = 0;
    while (timeElapsed < fadeDuration)
    {
        gameOverLabel.style.opacity = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
        timeElapsed += Time.deltaTime;
        yield return null;
    }
    gameOverLabel.style.opacity = 1;

    // Pequeña pausa antes de los botones
    yield return new WaitForSeconds(0.5f);

    // Mostrar el botón "Try Again" con una animación suave
    timeElapsed = 0;
    fadeDuration = 3f; // Aseguramos que tenga una animación progresiva
    while (timeElapsed < fadeDuration)
    {
        tryAgainButton.style.opacity = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
        timeElapsed += Time.deltaTime;
        yield return null;
    }
    tryAgainButton.style.opacity = 1;

    // Pequeña pausa antes del botón "Exit"
    yield return new WaitForSeconds(0.5f);

    // Mostrar el botón "Exit"
    timeElapsed = 0;
    fadeDuration = 3.5f;
    while (timeElapsed < fadeDuration)
    {
        exitButton.style.opacity = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
        timeElapsed += Time.deltaTime;
        yield return null;
    }
    exitButton.style.opacity = 1;
}

    private void OntryAgainButton(){
        SceneManager.LoadScene("Lobby");
    }

    private void OnExitButtonClicked()
    {
        // Cargar la escena "Sample Scene"
        SceneManager.LoadScene("MainMenuUnseen");
    }
}
