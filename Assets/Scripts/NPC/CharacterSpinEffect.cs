using UnityEngine;

public class CharacterSpinEffect : MonoBehaviour
{
    [SerializeField] Transform tf_Target;

    // Update is called once per frame
    void Update()
    {

        if (tf_Target != null)
        {
            // 목표 방향 = 타겟 위치 - 내 위치
            Vector3 dir = tf_Target.position - transform.position;

            // 방향 벡터가 0이면 회전하지 않음
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion t_Rotation = Quaternion.LookRotation(dir, Vector3.up);
                Vector3 t_Euler = new Vector3(0, t_Rotation.eulerAngles.y, 0);
                transform.eulerAngles = t_Euler;
            }
        }
    }
}
