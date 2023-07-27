using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameLocalization : MonoBehaviour
{
    public enum Language
    {
        English,
        Japanese
        // Add more languages here if needed
    }

    public Language currentLanguage = Language.English;
    public Dictionary<string, string> localizedTexts = new Dictionary<string, string>();
    private EnglishLocalization englishLocal = new EnglishLocalization();
    private JapaneseLocalization japaneseLocal = new JapaneseLocalization();

    // Add your localized text data here
    private void Awake()
    {
        SetEnglishLocalization();
        SetJapaneseLocalization();

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
        TMP_Text[] textObjects = FindObjectsOfType<TMP_Text>();

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

    }

}
