using UnityEngine;
using UnityEngine.InputSystem;

public class MouseHeadFollow : MonoBehaviour
{
    [Header("Head Settings")]
    public Transform head;                // the head bone or object
    public Animator mouseAnimator;        // full mouse animator
    public float followDelay = 2f;        // delay before head starts following
    public float rotationSpeed = 5f;      // rotation speed
    public Vector3 rotationOffset = new Vector3(-20f, 0f, 0f); // adjustable input offset
    public Quaternion restingRotation = Quaternion.identity;   // rotation when not following

    private bool canFollow = false;

    void Start()
    {
        Invoke(nameof(EnableFollow), followDelay);

        // Store initial rotation as resting position if not set
        if (restingRotation == Quaternion.identity && head != null)
            restingRotation = head.rotation;
    }

    void EnableFollow()
    {
        canFollow = true;
        if (mouseAnimator != null)
            mouseAnimator.enabled = false;
    }

    void Update()
    {
        if (!canFollow || head == null) return;

        // Get mouse position
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Check if cursor is on the right half of the screen
        float screenWidth = Screen.width;

        Quaternion targetRot = restingRotation;

        if (mousePos.x >= screenWidth / 2)
        {
            // Convert to world position
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, 5f));

            // Direction from head to cursor
            Vector3 direction = mouseWorld - head.position;
            if (direction.sqrMagnitude > 0.001f)
            {
                targetRot = Quaternion.LookRotation(direction) * Quaternion.Euler(rotationOffset);
            }
        }

        // Smoothly rotate towards target rotation (following or resting)
        head.rotation = Quaternion.Slerp(head.rotation, targetRot, rotationSpeed * Time.deltaTime);
    }
}
