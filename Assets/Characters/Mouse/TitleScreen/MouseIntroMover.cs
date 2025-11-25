using UnityEngine;

public class MouseIntroMover : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float moveDuration = 3f; // seconds to move before stopping

    private float timer = 0f;
    private bool isMoving = true;

    [HideInInspector]
    public bool introDone = false; // flag untuk player controller

    void Update()
    {
        if (isMoving)
        {
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

            timer += Time.deltaTime;

            if (timer >= moveDuration)
            {
                isMoving = false; // stop moving
                introDone = true;  // auto-move selesai
            }
        }
    }
}
