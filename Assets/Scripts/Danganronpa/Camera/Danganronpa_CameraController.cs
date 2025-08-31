using System.Collections;
using UnityEngine;

public class Danganronpa_CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform tf_Cam;

    [Header("View Settings")]
    [SerializeField] Vector2 camBoundary = new Vector2(1.5f, 0.8f); // 원본 그대로
    [SerializeField] float sightMoveSpeed = 0.02f;                  // 카메라 평행이동 속도
    [SerializeField] float sightSensitivity = 0.8f;                 // 카메라 회전 민감도
    [SerializeField] float lookLimitX = 20f;                        // 좌우(Y) 회전 한계
    [SerializeField] float lookLimitY = 10f;                        // 상하(X) 회전 한계


    [Header("내부 상태변수")]
    float originPosX, originPosY;                                   //  카메라 원래 위치
    float currentAngleX, currentAngleY;                             //  원래 코드의 누적 각도


    public float AngleX => currentAngleX;
    public float AngleY => currentAngleY;
   
    void Start()
    {
        if (!tf_Cam) { enabled = false; return; }
        originPosX = tf_Cam.localPosition.x;
        originPosY = tf_Cam.localPosition.y;
    }

    // === 캐릭터 시선 이동 ===
    public void CameraLimit()
    {
        if (!tf_Cam) return;

        if (tf_Cam.localPosition.x >= camBoundary.x)
            tf_Cam.localPosition = new Vector3(camBoundary.x, tf_Cam.localPosition.y, tf_Cam.localPosition.z);
        else if (tf_Cam.localPosition.x <= -camBoundary.x)
            tf_Cam.localPosition = new Vector3(-camBoundary.x, tf_Cam.localPosition.y, tf_Cam.localPosition.z);

        if (tf_Cam.localPosition.y >= 1 + camBoundary.y)
            tf_Cam.localPosition = new Vector3(tf_Cam.localPosition.x, originPosY + camBoundary.y, tf_Cam.localPosition.z);
        else if (tf_Cam.localPosition.y <= 1 - camBoundary.y)
            tf_Cam.localPosition = new Vector3(tf_Cam.localPosition.x, originPosY - camBoundary.y, tf_Cam.localPosition.z);
    }

    public void ViewMoving(Vector2 pointerLocal, float screenWidthHalf, float screenHeightHalf)
    {
        if (!tf_Cam) return;

        var p = pointerLocal;

        // 좌우 회전 + 좌우 이동
        if (p.x > screenWidthHalf - 100 || p.x < -screenWidthHalf + 100)
        {
            currentAngleY += (p.x > 0) ? sightSensitivity : -sightSensitivity;
            currentAngleY = Mathf.Clamp(currentAngleY, -lookLimitX, lookLimitX);

            float t_applySpeed = (p.x > 0) ? sightMoveSpeed : -sightMoveSpeed;
            tf_Cam.localPosition = new Vector3(
                tf_Cam.localPosition.x + t_applySpeed,
                tf_Cam.localPosition.y,
                tf_Cam.localPosition.z
            );
        }

        // 상하 회전 + 상하 이동
        if (p.y > screenHeightHalf - 100 || p.y < -screenHeightHalf + 100)
        {
            currentAngleX += (p.y > 0) ? -sightSensitivity : sightSensitivity; // 방향 반전
            currentAngleX = Mathf.Clamp(currentAngleX, -lookLimitY, lookLimitY);

            float t_applySpeedY = (p.y > 0) ? -sightMoveSpeed : sightMoveSpeed; // 방향 반전
            tf_Cam.localPosition = new Vector3(
                tf_Cam.localPosition.x,
                tf_Cam.localPosition.y + t_applySpeedY,
                tf_Cam.localPosition.z
            );
        }

        // 카메라 회전 적용
        tf_Cam.localEulerAngles = new Vector3(currentAngleX, currentAngleY, tf_Cam.localEulerAngles.z);
    }

    // DirectionUI가 리밋값 필요하면 읽어가도록 Getter
    public float LookLimitX => lookLimitX;
    public float LookLimitY => lookLimitY;

    public void ResetAngles()
    {
        currentAngleX = currentAngleY = 0f;
    }

    // === 카메라 NPC 대화 모드 ===
    public void CameraTargetting(Transform target, float camSpeed = 0.05f)
    {
        StopAllCoroutines();
        StartCoroutine(CameraTargettingCoroutine(target, camSpeed));
    }

    IEnumerator CameraTargettingCoroutine(Transform target, float camSpeed = 0.05f)
    {
        Vector3 targetPos = target.position;
        Vector3 targetFrontPos = targetPos + target.forward;
        Vector3 t_Direction = (targetPos - targetFrontPos).normalized;

        while(transform.position != targetFrontPos||Quaternion.Angle(transform.rotation, Quaternion.LookRotation(t_Direction))>=0.5f)
        {

            // 1) 위치 보간
            transform.position = Vector3.MoveTowards(transform.position, targetFrontPos, camSpeed * Time.deltaTime);
            // 2) 회전 보간
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(t_Direction), camSpeed * Time.deltaTime);
            yield return null;
        }
    }
}