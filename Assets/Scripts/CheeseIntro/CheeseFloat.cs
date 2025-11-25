using UnityEngine;

public class CheeseFloat : MonoBehaviour
{
    public float hoverHeight = 0.3f;   // how much it moves up/down
    public float hoverSpeed = 2f;      // how fast it hovers
    public float rotateSpeed = 50f;    // rotation speed (degrees/sec)

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Hover up/down
        float newY = startPos.y + Mathf.Sin(Time.time * hoverSpeed) * hoverHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Rotate slowly
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
    }
}
