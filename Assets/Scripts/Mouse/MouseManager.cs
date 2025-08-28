using UnityEngine;

public class MouseManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CameraController cameraController;
    [SerializeField] KeyMover keyMover;
    private MousePointer mousePointer;

    [Header("Arrow UI")] // DirectionArrowEffect의 필드 이식
    [SerializeField] GameObject CamUp; [SerializeField] GameObject NotCamUp;
    [SerializeField] GameObject CamDown; [SerializeField] GameObject NotCamDown;
    [SerializeField] GameObject CamLeft; [SerializeField] GameObject NotCamLeft;
    [SerializeField] GameObject CamRight; [SerializeField] GameObject NotCamRight;

    [Header("Mouse Pointer Prefab")]
    [SerializeField] private RectTransform tf_MousePointer;

    private bool isMouseMovementEnabled = true;


    private void Awake()
    {
        mousePointer = new MousePointer(tf_MousePointer);
    }

    void OnRectTransformDimensionsChange()
    {
        // 해상도/윈도우 리사이즈 대응
        mousePointer?.RecalcScreenHalf();
    }

    void Update()
    {
        if (!isMouseMovementEnabled) return;

        // 1) 포인터 UI 위치
        mousePointer?.UpdatePositionFromMouse();

        // 2) 카메라 회전/이동
        if (cameraController) cameraController.ViewMoving(mousePointer.PointerLocal, mousePointer.ScreenWidthHalf, mousePointer.ScreenHeightHalf);

        // 3) 카메라 한계 클램프
        if (cameraController) cameraController.CameraLimit();

        // 4) 방향 화살표 갱신 (DirectionArrowEffect.DirectionEffect 로직 이식)
        UpdateDirectionArrows(cameraController);

        // 5) WASD 이동
        if (keyMover) keyMover.KeyMoving();
    }

    /// <summary>마우스 움직임/관련 UI 비활성</summary>
    public void DisableMovement()
    {
        isMouseMovementEnabled = false;
        HideAllArrows(); // 기존 DirectionArrowEffect.HideAllArrows 대체
    }

    /// <summary>마우스 움직임/관련 UI 활성</summary>
    public void EnableMovement()
    {
        isMouseMovementEnabled = true;
    }


    // ====== 아래부터 DirectionArrowEffect 통합 ======
    public void HideAllArrows()
    {
        if (CamUp) CamUp.SetActive(false); if (NotCamUp) NotCamUp.SetActive(false);
        if (CamDown) CamDown.SetActive(false); if (NotCamDown) NotCamDown.SetActive(false);
        if (CamLeft) CamLeft.SetActive(false); if (NotCamLeft) NotCamLeft.SetActive(false);
        if (CamRight) CamRight.SetActive(false); if (NotCamRight) NotCamRight.SetActive(false);
    }

    private void UpdateDirectionArrows(CameraController cv)
    {
        if (!cv) return;

        float currentAngleX = cv.AngleX;
        float currentAngleY = cv.AngleY;
        float lookLimitX = cv.LookLimitX;
        float lookLimitY = cv.LookLimitY;

        HideAllArrows();

        // 좌/우
        if (currentAngleY <= -lookLimitX)
        {
            if (CamLeft) CamLeft.SetActive(true);
            if (NotCamLeft) NotCamLeft.SetActive(true);
        }
        else if (currentAngleY < 0)
        {
            if (CamLeft) CamLeft.SetActive(true);
        }

        if (currentAngleY >= lookLimitX)
        {
            if (CamRight) CamRight.SetActive(true);
            if (NotCamRight) NotCamRight.SetActive(true);
        }
        else if (currentAngleY > 0)
        {
            if (CamRight) CamRight.SetActive(true);
        }

        // 상/하 (조건 반전)
        if (currentAngleX <= -lookLimitY)
        {
            if (CamUp) CamUp.SetActive(true);
            if (NotCamUp) NotCamUp.SetActive(true);
        }
        else if (currentAngleX < 0)
        {
            if (CamUp) CamUp.SetActive(true);
        }

        if (currentAngleX >= lookLimitY)
        {
            if (CamDown) CamDown.SetActive(true);
            if (NotCamDown) NotCamDown.SetActive(true);
        }
        else if (currentAngleX > 0)
        {
            if (CamDown) CamDown.SetActive(true);
        }
    }

}