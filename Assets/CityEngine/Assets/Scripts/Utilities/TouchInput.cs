using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Button))]
public class TouchInputButtonTrigger : MonoBehaviour, IPointerClickHandler
{
    private Button button;

    private void Awake()
    {
        // Cache the Button component
        button = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Trigger the button's onClick actions
        button.onClick.Invoke();
    }

    private void Update()
    {
        // Handle touch input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                // Check if the touch is over this button
                if (RectTransformUtility.RectangleContainsScreenPoint(
                        GetComponent<RectTransform>(),
                        touch.position,
                        Camera.main))
                {
                    button.onClick.Invoke();
                }
            }
        }
    }
}
