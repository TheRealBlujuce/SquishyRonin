using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    private PlayerInput currentGameInput;
    private GameLocalization localization;
    public GameObject[] menuButtons;
    public float fillSpeed = 1f; // Adjust the speed of the fill animation
    private Image[] buttonBgImages;
    private bool[] isFilling;
    private int selectedButtonIndex = 0;
    private bool canSelect;

    private void Start()
    {
        buttonBgImages = new Image[menuButtons.Length];
        isFilling = new bool[menuButtons.Length];
        localization = FindObjectOfType<GameLocalization>();

        for (int i = 0; i < menuButtons.Length; i++)
        {
            Image bgImage = menuButtons[i].GetComponentInChildren<Image>();
            if (bgImage != null)
            {
                buttonBgImages[i] = bgImage;
                buttonBgImages[i].fillAmount = 0f;
            }
        }

        // Initially, select the first button
        SelectButton(selectedButtonIndex);
    }

    private void Awake()
    {
        currentGameInput = GameController.gameControllerInstance.gameInput;
    }


    private void Update()
    {
        if (GameController.gameControllerInstance.currentGameState == GameController.GameState.MENU)
        {
            if (currentGameInput.MenuMovement.Vertical.ReadValue<Vector2>().y > 0 && canSelect)
            {
                canSelect = false;
                SelectButton(selectedButtonIndex - 1);
            }
            else if (currentGameInput.MenuMovement.Vertical.ReadValue<Vector2>().y < 0 && canSelect)
            {
                canSelect = false;
                SelectButton(selectedButtonIndex + 1);
            }

            // Handle button click when pressing the Enter key
            if (currentGameInput.MenuMovement.Interact.triggered)
            {
                InteractWithSelectedButton();
            }

            selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, menuButtons.Length-1);
        }
    }

    private void SelectButton(int index)
    {
        index = Mathf.Clamp(index, 0, menuButtons.Length-1);
        if (index >= 0 && index < menuButtons.Length)
        {
            // Deselect the previously selected button (if any)
            if (selectedButtonIndex >= 0 && selectedButtonIndex < menuButtons.Length)
            {
                SetButtonSelected(selectedButtonIndex, false);
            }

            // Select the new button
            selectedButtonIndex = index;
            SetButtonSelected(selectedButtonIndex, true);
        }
    }

    private void SetButtonSelected(int index, bool isSelected)
    {
        Image bgImage = buttonBgImages[index];
        float targetFillAmount = isSelected ? 1.0f : 0.0f;

        // Stop any ongoing fill coroutine for this button
        if (isFilling[index])
        {
            StopCoroutine(LerpFillAmount(index,1));
        }

        // Start a new fill coroutine
        StartCoroutine(LerpFillAmount(index, targetFillAmount));
    }

    private IEnumerator LerpFillAmount(int index, float targetFillAmount)
    {
        isFilling[index] = true;
        float startFillAmount = buttonBgImages[index].fillAmount;
        float timeElapsed = 0f;

        while (timeElapsed < fillSpeed)
        {
            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / fillSpeed);
            buttonBgImages[index].fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, t);
            yield return null;
        }

        canSelect = true;
        buttonBgImages[index].fillAmount = targetFillAmount;
        isFilling[index] = false;
    }

    private void InteractWithSelectedButton()
    {
        // Check if there is a selected button
        if (selectedButtonIndex >= 0 && selectedButtonIndex < menuButtons.Length)
        {
            // TODO: Handle button interaction here, for example:
            Debug.Log("Button " + selectedButtonIndex + " clicked!");
            if (selectedButtonIndex == 0)
            {
                GameController.gameControllerInstance.PlayGame();
            }
            if (selectedButtonIndex == 1)
            {
                if (localization.currentLanguage == GameLocalization.Language.English)
                {
                    localization.SetLanguage(GameLocalization.Language.Japanese);
                }
                else
                if (localization.currentLanguage == GameLocalization.Language.Japanese)
                {
                    localization.SetLanguage(GameLocalization.Language.English);
                }
            }
        }
    }
}
