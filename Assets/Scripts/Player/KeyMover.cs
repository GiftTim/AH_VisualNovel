using UnityEngine;

public class KeyMover : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Transform playerPosition;

    [Header("Move Settings")]
    [SerializeField] float sightMoveSpeed = 0.02f;

    // === 원래 KeyMoving 그대로 ===
    public void KeyMoving()
    {
        if (!playerPosition) return;

        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) move += playerPosition.forward;
        if (Input.GetKey(KeyCode.S)) move -= playerPosition.forward;
        if (Input.GetKey(KeyCode.A)) move -= playerPosition.right;
        if (Input.GetKey(KeyCode.D)) move += playerPosition.right;

        move.y = 0;

        if (move != Vector3.zero)
        {
            move.Normalize();
            playerPosition.localPosition += move * sightMoveSpeed;
        }
    }


}