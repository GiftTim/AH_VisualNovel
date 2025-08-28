using UnityEngine;

public class CharacterSpinEffect : MonoBehaviour
{
    [SerializeField] Transform tf_Target;

    // Update is called once per frame
    void Update()
    {

        if (tf_Target != null)
        {
            // ��ǥ ���� = Ÿ�� ��ġ - �� ��ġ
            Vector3 dir = tf_Target.position - transform.position;

            // ���� ���Ͱ� 0�̸� ȸ������ ����
            if (dir.sqrMagnitude > 0.0001f)
            {
                Quaternion t_Rotation = Quaternion.LookRotation(dir, Vector3.up);
                Vector3 t_Euler = new Vector3(0, t_Rotation.eulerAngles.y, 0);
                transform.eulerAngles = t_Euler;
            }
        }
    }
}
