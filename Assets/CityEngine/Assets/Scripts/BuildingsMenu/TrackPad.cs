using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TrackPad : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerUpHandler, IDragHandler
{
    private bool isTracking = false;
    private Vector2 mousePosition;
    public Image buildBullseye;
    public Image demolishBullseye;
    public Image backgroundIcon;
    public RectTransform trackpadRect;

    private Image target;


    private void Start()
    {
        // Hide the bullseye at the start
        buildBullseye.gameObject.SetActive(false);
        demolishBullseye.gameObject.SetActive(false);
        backgroundIcon.gameObject.SetActive(true);
        // target = buildBullseye;
    }

    private void OnEnable()
    {
        // if (target != null) target.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // target = null;
        buildBullseye.gameObject.SetActive(false);
        demolishBullseye.gameObject.SetActive(false);
        backgroundIcon.gameObject.SetActive(true);
    }


    // This function is called when the user clicks inside the trackpad
    public void OnPointerDown(PointerEventData eventData)
    {
        isTracking = true;
        target.gameObject.SetActive(true);
        backgroundIcon.gameObject.SetActive(false);

        mousePosition = GetMousePosition(eventData);
        MoveBullseye(mousePosition);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        print("Pointer Exit");

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        print("Pointer Enter");
        isTracking = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTracking = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        bool pointerInBounds = RectTransformUtility.RectangleContainsScreenPoint(trackpadRect, Input.mousePosition, eventData.pressEventCamera);
        isTracking = pointerInBounds;
        if (isTracking)
        {
            mousePosition = GetMousePosition(eventData);
            MoveBullseye(mousePosition);
        }
    }

    // Get the mouse position relative to the trackpad
    private Vector2 GetMousePosition(PointerEventData eventData)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        // Converts the screen space mouse position to the local space of the trackpad RectTransform
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var localMousePosition);

        return localMousePosition;
    }

    public Vector2 GetTargetPosition()
    {
        return mousePosition;
    }

    private void MoveBullseye(Vector2 mousePosition)
    {
        target.rectTransform.localPosition = mousePosition;
    }

    public void SetTarget(TrackpadTargetType targetType)
    {
        if (targetType == TrackpadTargetType.Build)
        {
            target = buildBullseye;
            demolishBullseye.gameObject.SetActive(false);
        }
        else if (targetType == TrackpadTargetType.Demolish)
        {
            target = demolishBullseye;
            buildBullseye.gameObject.SetActive(false);
        }

        target.gameObject.SetActive(true);
    }
}
