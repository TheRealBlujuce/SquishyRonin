using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AnimateMenu : MonoBehaviour
{
    [SerializeField] private bool isAnimating = false;
    public GameObject tryAgainButton;
    public GameObject quitButton;
    public GameObject background;
    public TextMeshProUGUI deathText;
    public float fadeInDuration = 2f; // Adjust the total duration for sliding down and fade-in
    private ButtonSelector buttonsSelector;
    private GameLocalization localization;

    private void Start()
    {
        isAnimating = false;
        buttonsSelector = GetComponentInParent<ButtonSelector>();
        localization = FindObjectOfType<GameLocalization>();

        deathText.text = localization.GetLocalizedTextByValue("deathText");

    }

    private IEnumerator Animate()
    {
        isAnimating = true;

        //Fade In BG
        yield return FadeInBG();

        yield return null;
        
        //Fade In Death Text
        yield return FadeInDeathText();

        yield return null;

        //Fade In Buttons
        yield return FadeInButtons();

        yield return null;

        isAnimating = false;

        buttonsSelector.SelectDefaultButton();

        yield break;

    }


    private IEnumerator FadeInBG()
    {
        Vector3 backgroundTargetDest = new Vector3(0f, 0f);
        RectTransform backgroundTransform = background.GetComponent<RectTransform>();
        Color targetColor = background.GetComponent<Image>().color;
        Color baseColor = new Color(1f, 1f, 1f);
        targetColor.a = 0f;
        background.GetComponent<Image>().color = targetColor;

        float startTime = Time.time;
        while (Time.time - startTime < fadeInDuration)
        {
            // Calculate the time elapsed since the start of the animation
            float elapsed = Time.time - startTime;

            // Calculate the progress (a value between 0 and 1) for sliding down and fade-in
            float progress = Mathf.Clamp01(elapsed / fadeInDuration);

            // Move the background down
            float slideDownY = Mathf.Lerp(764f, backgroundTargetDest.y, progress);
            backgroundTransform.localPosition = new Vector3(0f, slideDownY, 0f);

            // Fade the background in
            float alpha = Mathf.Lerp(0f, 0.5f, progress);
            baseColor.a = alpha;
            targetColor.a = alpha;
            background.GetComponent<Image>().color = Color.Lerp(baseColor, targetColor, progress);

            yield return null;
        }

        // Ensure the background reaches the exact destination and final opacity
        backgroundTransform.localPosition = backgroundTargetDest;
        targetColor.a = 0.5f;
        background.GetComponent<Image>().color = targetColor;
    }

    private IEnumerator FadeInDeathText()
    {
        Color textTargetColor = deathText.color; // Use deathText's initial color as the target color
        textTargetColor.a = 0f;
        deathText.color = textTargetColor;

        float startTime = Time.time;
        while (Time.time - startTime < fadeInDuration)
        {
            // Calculate the time elapsed since the start of the animation
            float elapsed = Time.time - startTime;

            // Calculate the progress (a value between 0 and 1) for the fade-in
            float progress = Mathf.Clamp01(elapsed / fadeInDuration);

            // Fade the death text in
            float alpha = Mathf.Lerp(0f, 1f, progress);
            textTargetColor.a = alpha;
            deathText.color = Color.Lerp(deathText.color, textTargetColor, progress);

            yield return null;
        }

        // Ensure the death text reaches the exact destination and final opacity
        textTargetColor.a = 1f;
        deathText.color = textTargetColor;
    }


     private IEnumerator FadeInButtons()
    {
        // Get the Image and TextMeshProUGUI components of both buttons
        Image image1 = tryAgainButton.GetComponentInChildren<Image>();
        TextMeshProUGUI text1 = tryAgainButton.GetComponentInChildren<TextMeshProUGUI>();

        Image image2 = quitButton.GetComponentInChildren<Image>();
        TextMeshProUGUI text2 = quitButton.GetComponentInChildren<TextMeshProUGUI>();

        // Set the initial colors of the buttons
        Color initialColor = Color.white;
        initialColor.a = 0f;
        image1.color = initialColor;
        text1.color = initialColor;
        image2.color = initialColor;
        text2.color = initialColor;

        float startTime = Time.time;
        while (Time.time - startTime < fadeInDuration)
        {
            // Calculate the time elapsed since the start of the animation
            float elapsed = Time.time - startTime;

            // Calculate the progress (a value between 0 and 1) for the fade-in
            float progress = Mathf.Clamp01(elapsed / fadeInDuration);

            // Fade in the buttons
            Color targetColor = Color.white;
            targetColor.a = progress; // Set the alpha value based on the progress

            image1.color = targetColor;
            text1.color = targetColor;
            image2.color = targetColor;
            text2.color = targetColor;

            yield return null;
        }

        // Ensure the buttons reach the exact destination and final opacity
        Color finalColor = Color.white;
        finalColor.a = 1f;
        image1.color = finalColor;
        text1.color = finalColor;
        image2.color = finalColor;
        text2.color = finalColor;
    }

    public void StartMenuAnimation()
    {
        if (!isAnimating)
        {
            StartCoroutine(Animate());
        }
    }

    public bool CheckIsMenuAnimating()
    {
        return isAnimating;
    }


}
