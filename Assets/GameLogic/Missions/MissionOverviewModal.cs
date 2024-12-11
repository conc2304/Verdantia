using UnityEngine;
using TMPro;

/**
Container for UI elements in a mission overview modal. 
It holds references to text components and a container for key metrics, 
which can be dynamically populated to display information such as the mission's title, objective, time limit, difficulty, brief, and key metrics. 
It serves as a structure for organizing and displaying mission details in the UI.
**/
public class MissionOverviewModal : MonoBehaviour
{

    public TMP_Text Title;
    public TMP_Text ObjectiveText;
    public TMP_Text TimeLimitText;
    public TMP_Text DifficultyText;
    public TMP_Text MissionBriefText;
    public TMP_Text KeyMetricsText;
    public GameObject KeyMetricsContainer;
}
