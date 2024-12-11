using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/**
Manages a dynamic loading screen by periodically updating the displayed text with 
random inspirational messages about sustainability and green initiatives. 
It initializes with a default message and cycles through a predefined list of messages at regular intervals while the loading screen is active. 
The behavior stops and resets when the loading screen is disabled.
**/
public class LoadingScreen : MonoBehaviour
{
    public TMP_Text loadingText;
    public string initialMessage = "Loading...";
    public List<string> loadingMessages = new List<string>
    {
        "Planting trees and ideas...",
        "Balancing the ecosystem...",
        "Generating fresh air...",
        "Cultivating sustainable growth...",
        "Encouraging car-free commutes...",
        "Letting the forests regrow...",
        "Pollinating creativity...",
        "Channeling clean water systems...",
        "Breathing life into green spaces...",
        "Supporting local pollinators...",
        "Charging renewable energy...",
        "Rethinking urban design...",
        "Shining light on sustainability...",
        "Laying paths for progress...",
        "Building greener neighborhoods...",
        "Welcoming back the wildlife...",
        "Seeding innovation...",
        "Restoring natural waterways...",
        "Blooming possibilities...",
        "Expanding your green thumb..."
    };

    public float updateInterval = 0.07f; // Time interval in seconds for updating the text

    private Coroutine updateCoroutine;

    private void OnEnable()
    {
        // Start updating the loading text when the GameObject becomes active
        updateCoroutine = StartCoroutine(UpdateLoadingText());

    }

    private void OnDisable()
    {
        // Stop updating the loading text when the GameObject becomes inactive
        if (updateCoroutine != null)
        {
            StopCoroutine(updateCoroutine);
            updateCoroutine = null;
        }
        loadingText.text = initialMessage;
    }

    private IEnumerator UpdateLoadingText()
    {
        while (true)
        {
            // Wait before updating, allow initial message to be first
            yield return new WaitForSeconds(updateInterval);

            string randomMessage = loadingMessages[Random.Range(0, loadingMessages.Count)];

            loadingText.text = randomMessage;
        }
    }
}
