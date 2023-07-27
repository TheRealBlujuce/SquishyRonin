using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonSelector : MonoBehaviour
{
    private PlayerInput currentGameInput;
    public GameObject[] buttons;
    public float fillSpeed = 1f;
    private Image[] buttonBgImages;
    private bool[] isFilling;
    private int selectedButtonIndex = 0;
    private bool canSelect = false;

    [SerializeField] AnimateMenu gameOverScreen;
    private GameLocalization localization;

    private void Awake()
    {
        currentGameInput = GameController.gameControllerInstance.gameInput;
        buttonBgImages = new Image[buttons.Length];
        isFilling = new bool[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            Image bgImage = buttons[i].GetComponentInChildren<Image>();
            if (bgImage != null)
            {
                buttonBgImages[i] = bgImage;
                buttonBgImages[i].fillAmount = 0f;
            }
        }
    }

    private void Start()
    {
        localization = FindObjectOfType<GameLocalization>();

        buttons[0].GetComponentInChildren<TextMeshProUGUI>().text = localization.GetLocalizedTextByValue("tryAgainText");
        buttons[1].GetComponentInChildren<TextMeshProUGUI>().text = localization.GetLocalizedTextByValue("quitText");

        // Initially, select the first button
        // SelectButton(selectedButtonIndex);
    }

    private void Update()
    {
        if (GameController.gameControllerInstance.currentGameState == GameController.GameState.GAMEOVER)
        {
            // Handle button selection with arrow keys only if there is no selected button
            if (currentGameInput.MenuMovement.Horizontal.ReadValue<Vector2>().x < 0 && canSelect)
            {
                canSelect = false;
                SelectButton(selectedButtonIndex - 1);
            }
            else if (currentGameInput.MenuMovement.Horizontal.ReadValue<Vector2>().x > 0 && canSelect)
            {
                canSelect = false;
                SelectButton(selectedButtonIndex + 1);
            }

            // Handle button click when pressing the Enter key
            if (currentGameInput.MenuMovement.Interact.triggered)
            {
                InteractWithSelectedButton();
            }

            selectedButtonIndex = Mathf.Clamp(selectedButtonIndex, 0, buttons.Length-1);
        }
    }

    private void SelectButton(int index)
    {
        index = Mathf.Clamp(index, 0, buttons.Length-1);
        if (index >= 0 && index < buttons.Length)
        {
            // Deselect the previously selected button (if any)
            if (selectedButtonIndex >= 0 && selectedButtonIndex < buttons.Length)
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
            StopCoroutine(LerpFillAmount(index, 1));
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

        buttonBgImages[index].fillAmount = targetFillAmount;
        isFilling[index] = false;
        canSelect = true;


        
    }

    private void InteractWithSelectedButton()
    {
        // Check if there is a selected button
        if (selectedButtonIndex >= 0 && selectedButtonIndex < buttons.Length)
        {
            // TODO: Handle button interaction here, for example:
            // Debug.Log("Button " + selectedButtonIndex + " clicked!");
            // You can use the selectedButtonIndex to call the respective button function
            if (selectedButtonIndex == 0)
            {
                OnTryAgainButtonClicked();
            }
            else if (selectedButtonIndex == 1)
            {
                OnQuitButtonClicked();
            }
        }
    }

    // Called when the Try Again button is clicked
    public void OnTryAgainButtonClicked()
    {
        // Restart the level (you can replace this with your level restart logic)
        //Debug.Log("Restarting the level...");
        // Put your level restart code here
        GameController.gameControllerInstance.RestartLevel();
    }

    // Called when the Quit button is clicked
    public void OnQuitButtonClicked()
    {
        // Go back to the main menu (you can replace this with your menu scene loading logic)
        //Debug.Log("Going back to the main menu...");
        // Put your menu scene loading code here
        GameController.gameControllerInstance.ReturnToMenu();
    }

    public void SelectDefaultButton()
    {
        selectedButtonIndex = 0;
        SelectButton(selectedButtonIndex);
    }
}
