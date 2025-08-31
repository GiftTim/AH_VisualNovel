using UnityEngine;

public class Danganronpa_MousePointer
{
    private RectTransform tf_MousePointer;
    private float screenWidthHalf, screenHeightHalf;


    public Danganronpa_MousePointer(RectTransform target)
    {
        tf_MousePointer = target;
        RecalcScreenHalf(); // 최초 계산
    }

    // 외부에서 읽을 수 있는 프로퍼티 추가
    public float ScreenWidthHalf => screenWidthHalf;
    public float ScreenHeightHalf => screenHeightHalf;

    public void RecalcScreenHalf()
    {
        screenWidthHalf = Screen.width * 0.5f;
        screenHeightHalf = Screen.height * 0.5f;
    }

    public Vector2 PointerLocal =>
        tf_MousePointer ? (Vector2)tf_MousePointer.localPosition : Vector2.zero;

    public void UpdatePositionFromMouse()
    {
        if (!tf_MousePointer) return;

        Vector2 mousePosition = Input.mousePosition;
        Vector2 pointerPosition = new Vector2(
            mousePosition.x - screenWidthHalf,
            mousePosition.y - screenHeightHalf
        );

        float clampedX = Mathf.Clamp(pointerPosition.x, -screenWidthHalf + 50, screenWidthHalf - 50);
        float clampedY = Mathf.Clamp(pointerPosition.y, -screenHeightHalf + 50, screenHeightHalf - 50);

        tf_MousePointer.localPosition = new Vector2(clampedX, clampedY);
    }
}

/*
using UnityEngine;

public class Danganronpa_MousePointer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] RectTransform tf_MousePointer;

    // 화면 절반 크기
    float screenWidthHalf, screenHeightHalf;

    public Vector2 PointerLocal => tf_MousePointer ? (Vector2)tf_MousePointer.localPosition : Vector2.zero;
    public float ScreenWidthHalf => screenWidthHalf;
    public float ScreenHeightHalf => screenHeightHalf;

    void Start()
    {
        screenWidthHalf = Screen.width * 0.5f;
        screenHeightHalf = Screen.height * 0.5f;
    }

    // === 원래 MoveMousePointer 그대로 ===
    public void MouseMoving()
    {
        if (!tf_MousePointer) return;

        Vector2 mousePosition = Input.mousePosition;
        Vector2 pointerPosition = new Vector2(
            mousePosition.x - screenWidthHalf,
            mousePosition.y - screenHeightHalf
        );

        float clampedX = Mathf.Clamp(pointerPosition.x, -screenWidthHalf + 50, screenWidthHalf - 50);
        float clampedY = Mathf.Clamp(pointerPosition.y, -screenHeightHalf + 50, screenHeightHalf - 50);

        tf_MousePointer.localPosition = new Vector2(clampedX, clampedY);
    }
}
*/