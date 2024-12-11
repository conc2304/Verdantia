using UnityEngine;
using TMPro;
using UnityEngine.UI;

//  Manages the UI display for a mission, including its image, title, difficulty, and selection state, toggling border colors when selected.
public class MissionItem : MonoBehaviour
{
    public Image image;
    public TMP_Text title;
    public TMP_Text difficulty;
    public bool isSelected;
    public Image border;
    public Color borderColorDefault;
    public Color borderColorSelected;


    public void SetSelected(bool selected)
    {
        isSelected = selected;
        border.color = isSelected ? borderColorSelected : borderColorDefault;
    }
}
