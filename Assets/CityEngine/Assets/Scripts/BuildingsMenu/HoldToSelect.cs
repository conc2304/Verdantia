using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class HoldToSelect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Image progressBar;
    public float holdTime = 2f;
    private float holdTimer;
    private bool isHolding = false;
    public bool hasSelected = false;

    private Coroutine holdCoroutine;
    private Button button; // Reference to the Button component


    void Start()
    {
        if (progressBar != null) progressBar.fillAmount = 0; // Start the progress bar as empty
        button = GetComponent<Button>();  // Get the Button component
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        print("OnPointerDown");
        if (!hasSelected)
        {
            // Start the hold coroutine when the pointer is down
            holdCoroutine = StartCoroutine(HoldSelection());
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        print("OnPointerUp");
        // Reset the hold if the user releases the button before selecting the item
        ResetHold();
        print("Reset Select");

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Reset the hold if the pointer exits the button area
        ResetHold();
        print("Reset Select");
    }

    IEnumerator HoldSelection()
    {
        isHolding = true;
        holdTimer = 0;

        // While the hold timer is less than the required hold time, keep updating the timer and progress bar
        while (holdTimer < holdTime)
        {
            holdTimer += Time.deltaTime;
            progressBar.fillAmount = holdTimer / holdTime; // Update progress bar based on the hold duration
            yield return null;
        }

        // Trigger the selection action if the hold is successful
        OnSelect();
    }

    public void ResetHold()
    {
        print("Reset Hold");

        if (isHolding)
        {
            isHolding = false;
            StopCoroutine(holdCoroutine);
            holdTimer = 0;
            progressBar.fillAmount = 0; // Reset progress bar
        }
    }

    private void OnSelect(Action callback = null)
    {
        print("OnSelect");
        hasSelected = true;
        isHolding = false;
        progressBar.fillAmount = 1; // Set progress bar to full

        Debug.Log($"it has been selected!");

        if (callback != null)
        {
            callback.Invoke(); // Call the callback function
        }

        // Manually trigger the ClickCheck function
        // ResetHold();
        button.onClick.Invoke();  // Invoke the event listener on the button (which should trigger ClickCheck)
    }

    public void ResetState()
    {
        isHolding = false;
        hasSelected = false;
        if (progressBar != null) progressBar.fillAmount = 0; // Start the progress bar as empty

    }
}

