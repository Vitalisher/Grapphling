using UnityEngine;
using UnityEngine.EventSystems;
using YG;

public class PlayerCamera : MonoBehaviour
{
    public float Sensivity = 2f;
    public float rotationX = 0f;
    public float MaxAngleY = 80f;
    public bool isSettingsPanelOpen;

    private Vector2 touchStartPos;
    private bool isTouching = false;

    public virtual void Start()
    {
        LockCursor();
    }

    public virtual void Update()
    {
        if (YG2.isPauseGame == false)
        {
            if (isSettingsPanelOpen)
                return;

            if (Input.touchCount == 0 && !isTouching)
            {
                RotateWithMouse();
            }
        }
    }

    public virtual void RotateWithMouse()
    {
        var horizontal = Input.GetAxis("Mouse X");
        var vertical = Input.GetAxis("Mouse Y");

        transform.parent.Rotate(Vector3.up * horizontal * Sensivity);
        rotationX -= vertical * Sensivity;
        rotationX = Mathf.Clamp(rotationX, -MaxAngleY, MaxAngleY);
        transform.localRotation = Quaternion.Euler(rotationX, 0.0f, 0.0f);
    }

    public void OnTouchDown(BaseEventData eventData)
    {
        if (isSettingsPanelOpen)
            return;

        isTouching = true;
        touchStartPos = ((PointerEventData)eventData).position;
    }

    public void OnTouchDrag(BaseEventData eventData)
    {
        if (isSettingsPanelOpen)
            return;

        Vector2 currentPos = ((PointerEventData)eventData).position;
        Vector2 delta = currentPos - touchStartPos;

        transform.parent.Rotate(Vector3.up * delta.x * Sensivity * 0.1f);
        rotationX -= delta.y * Sensivity * 0.1f;
        rotationX = Mathf.Clamp(rotationX, -MaxAngleY, MaxAngleY);
        transform.localRotation = Quaternion.Euler(rotationX, 0.0f, 0.0f);

        touchStartPos = currentPos;
    }

    public void OnTouchUp(BaseEventData eventData)
    {
        isTouching = false;
    }

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
}
