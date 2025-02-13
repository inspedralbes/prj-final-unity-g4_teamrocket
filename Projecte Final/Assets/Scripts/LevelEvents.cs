using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // Para cambiar de escena

public class LevelEvents : MonoBehaviour
{
    private VisualElement root;
    private VisualElement background;
    private Label titleText;
    private Label forNowText;
    private VisualElement buttonContainer;
    private Button exitButton;

    private float fadeDuration = 2.0f;
    private float textGrowDuration = 2.0f;
    private float forNowDelay = 0.2f;
    private float forNowGrowDuration = 1.5f;
    private float buttonDelay = 0.5f; // Pequeña pausa antes de mostrar el botón

    void Start()
{
    var uiDocument = GetComponent<UIDocument>();
    root = uiDocument.rootVisualElement;
    titleText = root.Q<Label>("TitleText");
    forNowText = root.Q<Label>("ForNowText");
    buttonContainer = root.Q<VisualElement>("ButtonContainer");
    exitButton = root.Q<Button>("ExitButton");

    // Ocultamos todo al inicio
    titleText.style.opacity = 0;
    forNowText.style.opacity = 0;
    buttonContainer.style.opacity = 0;

    // Creamos un fondo negro
    background = new VisualElement();
    background.style.position = Position.Absolute;
    background.style.width = Length.Percent(100);
    background.style.height = Length.Percent(100);
    background.style.backgroundColor = new Color(0, 0, 0, 0);
    root.Insert(0, background);

    // Asignamos el evento al botón
    exitButton.clicked += () => SceneManager.LoadScene("SampleScene");

    // Establecemos las clases de estilo para el botón Exit
    exitButton.AddToClassList("ExitButton"); // Asegúrate de que se aplica el estilo

    StartCoroutine(PlayLevelCompleteSequence());
}


    IEnumerator PlayLevelCompleteSequence()
    {
        yield return StartCoroutine(FadeToBlack());

        titleText.style.opacity = 1;
        yield return StartCoroutine(AnimateTitleText());

        yield return new WaitForSeconds(forNowDelay);
        forNowText.style.opacity = 1;
        yield return StartCoroutine(AnimateForNowText());

        yield return new WaitForSeconds(buttonDelay);
        buttonContainer.style.opacity = 1;
    }

    IEnumerator FadeToBlack()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = elapsedTime / fadeDuration;
            background.style.backgroundColor = new Color(0, 0, 0, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        background.style.backgroundColor = Color.black;
    }

    IEnumerator AnimateTitleText()
    {
        float elapsedTime = 0f;
        float startSize = 10f;
        float endSize = 70f;

        while (elapsedTime < textGrowDuration)
        {
            float size = Mathf.Lerp(startSize, endSize, elapsedTime / textGrowDuration);
            titleText.style.fontSize = size;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        titleText.style.fontSize = endSize;
    }

    IEnumerator AnimateForNowText()
    {
        float elapsedTime = 0f;
        float startSize = 10f;
        float endSize = 45f;

        while (elapsedTime < forNowGrowDuration)
        {
            float size = Mathf.Lerp(startSize, endSize, elapsedTime / forNowGrowDuration);
            forNowText.style.fontSize = size;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        forNowText.style.fontSize = endSize;
    }
}
