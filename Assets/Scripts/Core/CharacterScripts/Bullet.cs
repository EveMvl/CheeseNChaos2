using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [Tooltip("Units per second")]
    public float speed = 20f;

    [Tooltip("Seconds before auto-destroy if nothing hit")]
    public float lifetime = 2.5f;

    [Tooltip("Explosion prefab to spawn on impact (optional)")]
    public GameObject explosionPrefab;

    [Tooltip("If true, uses Rigidbody velocity; if false, moves via transform")]
    public bool useRigidbodyVelocity = true;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    void Start()
    {
        Destroy(gameObject, lifetime);

        if (useRigidbodyVelocity && rb.linearVelocity.sqrMagnitude < 0.001f)
            rb.linearVelocity = -transform.up * speed;
    }

    void Update()
    {
        if (!useRigidbodyVelocity)
            transform.Translate(Vector3.down * speed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 💥 Spawn explosion effect
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 2f); // automatically destroys explosion after 2 seconds
        }

        // 🐭 Destroy mice
        if (other.CompareTag("Mouse"))
        {
            Destroy(other.gameObject);
        }

        // 💨 Destroy bullet
        Destroy(gameObject);
    }

}
