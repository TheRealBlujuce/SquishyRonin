using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameLocalization : MonoBehaviour
{
    public enum Language
    {
        English,
        Japanese,
        Spanish
        // Add more languages here if needed
    }

    public Language currentLanguage = Language.English;
    public Dictionary<string, string> localizedTexts = new Dictionary<string, string>();
    private EnglishLocalization englishLocal = new EnglishLocalization();
    private JapaneseLocalization japaneseLocal = new JapaneseLocalization();
    private SpanishLocalization spanishLocal = new SpanishLocalization();

    // Add your localized text data here
    private void Awake()
    {
        SetEnglishLocalization();
        SetJapaneseLocalization();
        SetSpanishLocalization();
    }

    private void Start()
    {
        UpdateLocalizedTexts();
    }

    public void SetLanguage(Language language)
    {
        currentLanguage = language;
        UpdateLocalizedTexts();
    }

    private void UpdateLocalizedTexts()
    {
        TMP_Text[] textObjects = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);

        foreach (TMP_Text textObject in textObjects)
        {
            string objectName = textObject.name;
            string localizedText = GetLocalizedText(objectName);
            textObject.text = localizedText;
        }
    }

    private string GetLocalizedText(string objectName)
    {
        string localizedText = objectName;

        if (localizedTexts.ContainsKey(objectName + "_" + currentLanguage.ToString()))
        {
            localizedText = localizedTexts[objectName + "_" + currentLanguage.ToString()];
        }
        else
        {
            // Fallback to English if the localized text is not found
            if (localizedTexts.ContainsKey(objectName))
            {
                localizedText = localizedTexts[objectName];
            }
        }

        return localizedText;
    }
    public string GetLocalizedTextByValue(string objectName)
    {
        string localizedText = objectName;

        if (localizedTexts.ContainsKey(objectName + "_" + currentLanguage.ToString()))
        {
            localizedText = localizedTexts[objectName + "_" + currentLanguage.ToString()];
        }
        else
        {
            // Fallback to English if the localized text is not found
            if (localizedTexts.ContainsKey(objectName))
            {
                localizedText = localizedTexts[objectName];
            }
        }

        return localizedText;
    }

    private void SetEnglishLocalization()
    {
        // English Localization

        // Menu
        localizedTexts.Add("titleText", englishLocal.titleText);
        localizedTexts.Add("versionText", englishLocal.versionText + Application.version);
        localizedTexts.Add("playText", englishLocal.playText);
        localizedTexts.Add("settingsText", englishLocal.settingsText);
        localizedTexts.Add("creditsText", englishLocal.creditsText);

        // Game UI
        localizedTexts.Add("killsText", englishLocal.killsText);
        localizedTexts.Add("wavesText", englishLocal.wavesText);
        localizedTexts.Add("wavesCountdownText", englishLocal.wavesCountdownText);
        localizedTexts.Add("waveStartText", englishLocal.waveStartText);

        // Game Over
        localizedTexts.Add("deathText", englishLocal.deathText);
        localizedTexts.Add("tryAgainText", englishLocal.tryAgainText);

        // Universal
        localizedTexts.Add("quitText", englishLocal.quitText);

        // Controls

        // Controller
        localizedTexts.Add("controllerText", englishLocal.controllerText);
        localizedTexts.Add("moveController", englishLocal.controllerMove);
        localizedTexts.Add("attackController", englishLocal.controllerAttack);
        localizedTexts.Add("parryController", englishLocal.controllerParry);
        localizedTexts.Add("throwController", englishLocal.controllerThrow);
        localizedTexts.Add("rollController", englishLocal.controllerRoll);

        //Keyboard
        localizedTexts.Add("keyboardText", englishLocal.keyboardText);
        localizedTexts.Add("moveKeyboard", englishLocal.keyboardMove);
        localizedTexts.Add("attackKeyboard", englishLocal.keyboardAttack);
        localizedTexts.Add("parryKeyboard", englishLocal.keyboardParry);
        localizedTexts.Add("throwKeyboard", englishLocal.keyboardThrow);
        localizedTexts.Add("rollKeyboard", englishLocal.keyboardRoll);

    }
    private void SetJapaneseLocalization()
    {
        // Japanese Localization

        // Menu
        localizedTexts.Add("titleText_Japanese", japaneseLocal.titleText);
        localizedTexts.Add("versionText_Japanese", japaneseLocal.versionText + Application.version);
        localizedTexts.Add("playText_Japanese", japaneseLocal.playText);
        localizedTexts.Add("settingsText_Japanese", japaneseLocal.settingsText);
        localizedTexts.Add("creditsText_Japanese", japaneseLocal.creditsText);

        // Game UI
        localizedTexts.Add("killsText_Japanese", japaneseLocal.killsText);
        localizedTexts.Add("wavesText_Japanese", japaneseLocal.wavesText);
        localizedTexts.Add("wavesCountdownText_Japanese", japaneseLocal.wavesCountdownText);
        localizedTexts.Add("waveStartText_Japanese", japaneseLocal.waveStartText);

        // Game Over
        localizedTexts.Add("deathText_Japanese", japaneseLocal.deathText);
        localizedTexts.Add("tryAgainText_Japanese", japaneseLocal.tryAgainText);

        // Universal
        localizedTexts.Add("quitText_Japanese", japaneseLocal.quitText);

        // Controls

        // Controller
        localizedTexts.Add("controllerText_Japanese", japaneseLocal.controllerText);
        localizedTexts.Add("moveController_Japanese", japaneseLocal.controllerMove);
        localizedTexts.Add("attackController_Japanese", japaneseLocal.controllerAttack);
        localizedTexts.Add("parryController_Japanese", japaneseLocal.controllerParry);
        localizedTexts.Add("throwController_Japanese", japaneseLocal.controllerThrow);
        localizedTexts.Add("rollController_Japanese", japaneseLocal.controllerRoll);

        //Keyboard
        localizedTexts.Add("keyboardText_Japanese", japaneseLocal.keyboardText);
        localizedTexts.Add("moveKeyboard_Japanese", japaneseLocal.keyboardMove);
        localizedTexts.Add("attackKeyboard_Japanese", japaneseLocal.keyboardAttack);
        localizedTexts.Add("parryKeyboard_Japanese", japaneseLocal.keyboardParry);
        localizedTexts.Add("throwKeyboard_Japanese", japaneseLocal.keyboardThrow);
        localizedTexts.Add("rollKeyboard_Japanese", japaneseLocal.keyboardRoll);

    }

    private void SetSpanishLocalization()
    {
        // Spanish Localization

        // Menu
        localizedTexts.Add("titleText_Spanish", spanishLocal.titleText);
        localizedTexts.Add("versionText_Spanish", spanishLocal.versionText + Application.version);
        localizedTexts.Add("playText_Spanish", spanishLocal.playText);
        localizedTexts.Add("settingsText_Spanish", spanishLocal.settingsText);
        localizedTexts.Add("creditsText_Spanish", spanishLocal.creditsText);

        // Game UI
        localizedTexts.Add("killsText_Spanish", spanishLocal.killsText);
        localizedTexts.Add("wavesText_Spanish", spanishLocal.wavesText);
        localizedTexts.Add("wavesCountdownText_Spanish", spanishLocal.wavesCountdownText);
        localizedTexts.Add("waveStartText_Spanish", spanishLocal.waveStartText);

        // Game Over
        localizedTexts.Add("deathText_Spanish", spanishLocal.deathText);
        localizedTexts.Add("tryAgainText_Spanish", spanishLocal.tryAgainText);

        // Universal
        localizedTexts.Add("quitText_Spanish", spanishLocal.quitText);

        // Controls

        // Controller
        localizedTexts.Add("controllerText_Spanish", spanishLocal.controllerText);
        localizedTexts.Add("moveController_Spanish", spanishLocal.controllerMove);
        localizedTexts.Add("attackController_Spanish", spanishLocal.controllerAttack);
        localizedTexts.Add("parryController_Spanish", spanishLocal.controllerParry);
        localizedTexts.Add("throwController_Spanish", spanishLocal.controllerThrow);
        localizedTexts.Add("rollController_Spanish", spanishLocal.controllerRoll);

        //Keyboard
        localizedTexts.Add("keyboardText_Spanish", spanishLocal.keyboardText);
        localizedTexts.Add("moveKeyboard_Spanish", spanishLocal.keyboardMove);
        localizedTexts.Add("attackKeyboard_Spanish", spanishLocal.keyboardAttack);
        localizedTexts.Add("parryKeyboard_Spanish", spanishLocal.keyboardParry);
        localizedTexts.Add("throwKeyboard_Spanish", spanishLocal.keyboardThrow);
        localizedTexts.Add("rollKeyboard_Spanish", spanishLocal.keyboardRoll);

    }

}
