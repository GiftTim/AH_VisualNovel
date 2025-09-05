using System.Collections;
using UnityEngine;

public class Danganronpa_CameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform tf_Cam;

    [Header("View Settings")]
    [SerializeField] Vector2 camBoundary = new Vector2(1.5f, 0.8f); // ���� �״��
    [SerializeField] float sightMoveSpeed = 0.02f;                  // ī�޶� �����̵� �ӵ�
    [SerializeField] float sightSensitivity = 0.8f;                 // ī�޶� ȸ�� �ΰ���
    [SerializeField] float lookLimitX = 20f;                        // �¿�(Y) ȸ�� �Ѱ�
    [SerializeField] float lookLimitY = 10f;                        // ����(X) ȸ�� �Ѱ�


    [Header("���� ���º���")]
    float originPosX, originPosY;                                   //  ī�޶� ���� ��ġ
    float currentAngleX, currentAngleY;                             //  ���� �ڵ��� ���� ����


    public float AngleX => currentAngleX;
    public float AngleY => currentAngleY;
   
    void Start()
    {
        if (!tf_Cam) { enabled = false; return; }
        originPosX = tf_Cam.localPosition.x;
        originPosY = tf_Cam.localPosition.y;
    }

    // === ĳ���� �ü� �̵� ===
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

        // �¿� ȸ�� + �¿� �̵�
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

        // ���� ȸ�� + ���� �̵�
        if (p.y > screenHeightHalf - 100 || p.y < -screenHeightHalf + 100)
        {
            currentAngleX += (p.y > 0) ? -sightSensitivity : sightSensitivity; // ���� ����
            currentAngleX = Mathf.Clamp(currentAngleX, -lookLimitY, lookLimitY);

            float t_applySpeedY = (p.y > 0) ? -sightMoveSpeed : sightMoveSpeed; // ���� ����
            tf_Cam.localPosition = new Vector3(
                tf_Cam.localPosition.x,
                tf_Cam.localPosition.y + t_applySpeedY,
                tf_Cam.localPosition.z
            );
        }

        // ī�޶� ȸ�� ����
        tf_Cam.localEulerAngles = new Vector3(currentAngleX, currentAngleY, tf_Cam.localEulerAngles.z);
    }

    // DirectionUI�� ���԰� �ʿ��ϸ� �о���� Getter
    public float LookLimitX => lookLimitX;
    public float LookLimitY => lookLimitY;

    public void ResetAngles()
    {
        currentAngleX = currentAngleY = 0f;
    }

    // === ī�޶� NPC ��ȭ ��� ===
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

            // 1) ��ġ ����
            transform.position = Vector3.MoveTowards(transform.position, targetFrontPos, camSpeed * Time.deltaTime);
            // 2) ȸ�� ����
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(t_Direction), camSpeed * Time.deltaTime);
            yield return null;
        }
    }
}