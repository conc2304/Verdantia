using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;
using TMPro;

/**
implements a "Hold to Select" functionality for buttons, requiring players to press and hold for a specified duration to trigger an action. 
It also includes features for disabling buttons and providing visual feedback.
**/

public class HoldToSelect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Image progressBar;
    public Image disableIcon; // Icon to show when the button is disabled
    public bool disabled = false; // Whether the button is disabled
    public string disabledMsg = "";
    public TMP_Text disabledText;
    public float holdTime = 1.5f;
    public float delayTime = 0.1f;
    private float delayTimer = 0;
    private float holdTimer;
    private bool isHolding = false;
    public bool hasSelected = false;

    private Coroutine holdCoroutine;
    private Button button;
    public bool disableProgressBar;


    void Start()
    {
        if (progressBar != null) progressBar.fillAmount = 0;
        button = GetComponent<Button>();

        UpdateButtonState(); // Update button appearance based on initial state
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hasSelected && !disabled)
        {
            holdCoroutine = StartCoroutine(HoldSelection());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetHold();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetHold();
    }

    IEnumerator HoldSelection()
    {
        holdTimer = 0;
        delayTimer = 0;
        isHolding = true;

        // Wait for the delay before starting the hold timer
        while (delayTimer < delayTime)
        {
            delayTimer += Time.deltaTime;
            yield return null;
        }

        // While the hold timer is less than the required hold time, keep updating the timer and progress bar
        while (holdTimer < holdTime)
        {
            holdTimer += Time.deltaTime;
            if (!disableProgressBar) progressBar.fillAmount = holdTimer / holdTime; // Update progress bar based on the hold duration
            yield return null;
        }

        // Trigger the selection action if the hold is successful
        OnSelect();
    }

    public void ResetHold()
    {
        if (isHolding)
        {
            delayTimer = 0;
            isHolding = false;
            StopCoroutine(holdCoroutine);
            holdTimer = 0;
            progressBar.fillAmount = 0; // Reset progress bar
        }
    }

    private void OnSelect(Action callback = null)
    {
        if (disabled) return; // Do nothing if the button is disabled

        hasSelected = true;
        isHolding = false;
        progressBar.fillAmount = 1;

        callback?.Invoke();
        button.onClick?.Invoke();
        ResetState();
    }

    public void ResetState()
    {
        isHolding = false;
        hasSelected = false;
        delayTimer = 0;
        disabled = false; // Reset disabled state when resetting
        if (progressBar != null) progressBar.fillAmount = 0; // Start the progress bar as empty
        UpdateButtonState(); // Update the button state based on the disabled status
    }

    // This method enables or disables the button, updating its appearance accordingly
    public void SetDisabled(bool isDisabled, string msgText = "")
    {
        disabled = isDisabled;

        disabledMsg = msgText;
        UpdateButtonState();
    }

    // Update the button's appearance based on whether it's disabled or not
    private void UpdateButtonState()
    {
        if (disabled)
        {
            if (disableIcon != null)
            {

                disabledText.gameObject.SetActive(true);
                disableIcon.gameObject.SetActive(true); // Show the disable icon if present
                disabledText.text = disabledMsg;
            }

        }
        else
        {
            // Enable the button and hide the disable icon
            if (disableIcon != null)
            {
                disabledText.gameObject.SetActive(false);
                disableIcon.gameObject.SetActive(false); // Hide the disable icon if present
            }
        }
    }
}
