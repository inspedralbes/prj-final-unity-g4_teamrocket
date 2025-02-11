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

        // Obtener el objeto de GameObject que contiene el ParticleSystem
        var bloodRainGameObject = GameObject.Find("GeneradorSangre");
        if (bloodRainGameObject != null)
        {
            bloodRain = bloodRainGameObject.GetComponent<ParticleSystem>();
        }

        // Asegurarse de que el ParticleSystem se inicie desde el principio
        if (bloodRain != null)
        {
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
    }

    private IEnumerator FadeInGameOver()
    {
        // Animación para mostrar "Game Over" con un fondo negro y hacer aparecer el texto y los botones
        // Fondo negro
        gameOverElement.style.opacity = 1;
        float fadeDuration = 1.5f;
        float timeElapsed = 0;

        while (timeElapsed < fadeDuration)
        {
            gameOverElement.style.opacity = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        gameOverElement.style.opacity = 1;

        // Texto Game Over
        timeElapsed = 0;
        fadeDuration = 2f;
        while (timeElapsed < fadeDuration)
        {
            gameOverLabel.style.opacity = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        gameOverLabel.style.opacity = 1;

        // Botón Try Again
        timeElapsed = 0;
        fadeDuration = 3f;
        while (timeElapsed < fadeDuration)
        {
            tryAgainButton.style.opacity = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        tryAgainButton.style.opacity = 1;

        // Botón Exit
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

    private void OnExitButtonClicked()
    {
        // Cargar la escena "Sample Scene"
        SceneManager.LoadScene("Sample Scene");
    }
}
