using System;

public class Danganronpa_MouseSpinEffect
{
    public float SpinSpeed { get; set; }
    public UnityEngine.Vector3 SpinDir { get; set; }

    public Danganronpa_MouseSpinEffect(float spinSpeed, UnityEngine.Vector3 spinDir)
    {
        SpinSpeed = spinSpeed;
        SpinDir = spinDir;
    }

    // ȸ�� ���� ��� (Transform�� ���� �������� ����)
    public UnityEngine.Vector3 GetRotationDelta(float deltaTime)
    {
        return SpinDir * SpinSpeed * deltaTime;
    }
}