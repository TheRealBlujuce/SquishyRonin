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

    [SerializeField] private GameObject controlsMenu;

    private void Start()
    {
        buttonBgImages = new Image[menuButtons.Length];
        isFilling = new bool[menuButtons.Length];
        localization = FindFirstObjectByType<GameLocalization>();

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



    private void Update()
    {

        if (currentGameInput == null) {currentGameInput = GameController.gameControllerInstance.gameInput;}

        if (GameController.gameControllerInstance.currentGameState == GameController.GameState.MENU)
        {
            if (currentGameInput.MenuMovement.Vertical.ReadValue<Vector2>().y > 0 && canSelect && selectedButtonIndex != 0)
            {
                canSelect = false;
                SelectButton(selectedButtonIndex - 1);
            }
            else if (currentGameInput.MenuMovement.Vertical.ReadValue<Vector2>().y < 0 && canSelect && selectedButtonIndex != 3)
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
        Vector3 targetScale = isSelected ? new Vector3(1.15f, 1.15f, 1.15f) : new Vector3(1f, 1f, 1f);

        // Stop any ongoing fill coroutine for this button
        if (isFilling[index])
        {
            StopCoroutine(LerpFillAmount(index, 1f, new Vector3(1.15f, 1.15f, 1.15f)));
        }

        // Start a new fill coroutine
        StartCoroutine(LerpFillAmount(index, targetFillAmount, targetScale));

        if(selectedButtonIndex == 2) { controlsMenu.SetActive(true); } else { controlsMenu.SetActive(false); }
    }

    private IEnumerator LerpFillAmount(int index, float targetFillAmount, Vector3 targetScale)
    {
        isFilling[index] = true;
        float startFillAmount = buttonBgImages[index].fillAmount;
        float timeElapsed = 0f;

        while (timeElapsed < fillSpeed)
        {
            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / fillSpeed);
            buttonBgImages[index].fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, t);
            menuButtons[index].transform.localScale = Vector3.Lerp(menuButtons[index].transform.localScale, targetScale, t * 0.2f);
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
            // Debug.Log("Button " + selectedButtonIndex + " clicked!");
            if (selectedButtonIndex == 0)
            {
                GameController.gameControllerInstance.PlayGame();
            }
            if (selectedButtonIndex == 1)
            {
                // switch game localization
                switch(GameController.gameControllerInstance.currentLocalization)
                {
                    case 0:
                        GameController.gameControllerInstance.currentLocalization = 1;
                        localization.SetLanguage(GameLocalization.Language.Japanese);
                        break;
                    case 1:
                        GameController.gameControllerInstance.currentLocalization = 2;
                        localization.SetLanguage(GameLocalization.Language.Spanish);
                        break;
                    case 2:
                        GameController.gameControllerInstance.currentLocalization = 0;
                        localization.SetLanguage(GameLocalization.Language.English);
                        break;
                }
            }
            if(selectedButtonIndex == 3)
            {
                Application.Quit();
            }
        }
    }
}
