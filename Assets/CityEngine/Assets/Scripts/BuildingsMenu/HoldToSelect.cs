using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class HoldToSelect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public Image progressBar;
    public float holdTime = 1.5f;
    public float delayTime = 0.1f;
    private float delayTimer = 0;
    private float holdTimer;
    private bool isHolding = false;
    public bool hasSelected = false;

    private Coroutine holdCoroutine;
    private Button button;


    void Start()
    {
        if (progressBar != null) progressBar.fillAmount = 0;
        button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!hasSelected)
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
            progressBar.fillAmount = holdTimer / holdTime; // Update progress bar based on the hold duration
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
        hasSelected = true;
        isHolding = false;
        progressBar.fillAmount = 1;

        callback?.Invoke();
        button.onClick?.Invoke();
    }

    public void ResetState()
    {
        isHolding = false;
        hasSelected = false;
        delayTimer = 0;
        if (progressBar != null) progressBar.fillAmount = 0; // Start the progress bar as empty
    }
}

