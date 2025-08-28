using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [Header("카메라 할당")]
    [SerializeField] private Camera worldCam;

    [Header("Ray 설정")]
    [SerializeField, Min(0.01f)] private float rayDistance = 100f; // 필요시 인스펙터에서만 조정

    public bool IsHit { get; private set; }
    public RaycastHit HitInfo { get; private set; }
    public string CurrentTag { get; private set; }

    void Update()
    {
        // 1. Raycast의 결과를 담을 임시 지역 변수를 선언합니다.
        RaycastHit tempHitInfo;

        Vector3 MousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);

        // 2. out 매개변수로 속성(HitInfo) 대신 임시 변수(tempHitInfo)를 사용합니다.
        if (Physics.Raycast(worldCam.ScreenPointToRay(MousePosition), out tempHitInfo, rayDistance))
        {
            IsHit = true;

            // 3. 성공했다면, 임시 변수의 값을 속성에 할당합니다.
            HitInfo = tempHitInfo;
            CurrentTag = HitInfo.transform.tag;
        }
        else
        {
            IsHit = false;
            CurrentTag = "";
        }
    }
}