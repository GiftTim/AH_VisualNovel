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

    // 회전 벡터 계산 (Transform에 직접 접근하지 않음)
    public UnityEngine.Vector3 GetRotationDelta(float deltaTime)
    {
        return SpinDir * SpinSpeed * deltaTime;
    }
}