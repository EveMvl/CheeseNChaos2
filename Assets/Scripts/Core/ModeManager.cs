using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

public class ModeManager : MonoBehaviour
{
    public enum Mode { Cheese, Gun }
    public Mode currentMode = Mode.Cheese;

    [Header("Prefabs & References")]
    public GameObject cheesePrefab;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public Image cheeseIndicator;
    public Image gunIndicator;

    [Header("Gun Settings")]
    public int maxAmmo = 10;
    public float reloadTime = 2f;
    public float bulletForce = 500f;
    private int currentAmmo;
    private bool isReloading = false;

    [Header("Cheese Settings")]
    public float cheeseCooldown = 2f;
    private bool canDropCheese = true;

    [Header("Audio Settings")]
    public AudioSource audioSource;       // Drag object SFX ke sini
    public AudioClip gunModeSFX;          // Drag clip suara gun mode ke sini

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        currentAmmo = maxAmmo;

        if (cheeseIndicator != null) cheeseIndicator.enabled = false;
        if (gunIndicator != null) gunIndicator.enabled = false;
    }

    void Update()
    {
        HandleInput();
        UpdateCheeseIndicator();
        UpdateGunIndicator();
    }

    void HandleInput()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
            ToggleMode();

        if (Keyboard.current.fKey.wasPressedThisFrame)
            ToggleFirePoint();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (currentMode == Mode.Cheese)
                TryDropCheese();
            else if (currentMode == Mode.Gun)
                TryShoot();
        }
    }

    void ToggleMode()
    {
        // switch antara Cheese <-> Gun
        currentMode = currentMode == Mode.Cheese ? Mode.Gun : Mode.Cheese;
        Debug.Log($"Switched to {currentMode} Mode");

        // nyalain indikator sesuai mode
        if (cheeseIndicator != null)
            cheeseIndicator.enabled = currentMode == Mode.Cheese;

        if (gunIndicator != null)
            gunIndicator.enabled = currentMode == Mode.Gun;

        // 🎧 Mainin SFX cuma kalau switch ke Gun Mode
        if (currentMode == Mode.Gun && audioSource != null && gunModeSFX != null)
        {
            audioSource.PlayOneShot(gunModeSFX);
        }
    }

    void ToggleFirePoint()
    {
        if (firePoint != null)
        {
            firePoint = null;
            Debug.Log("🔄 FirePoint disabled — using main camera instead.");
        }
        else
        {
            firePoint = GameObject.Find("GunMuzzle")?.transform;
            Debug.Log("🔄 FirePoint set to 'GunMuzzle' (if exists).");
        }
    }

    void TryDropCheese()
    {
        if (!canDropCheese) return;
        StartCoroutine(DropCheeseCooldown());
    }

    IEnumerator DropCheeseCooldown()
    {
        Vector3? targetPos = GetMouseWorldPosition();
        if (targetPos != null)
        {
            Vector3 spawnPoint = targetPos.Value;
            float yOffset = 0.05f;

            if (cheesePrefab != null)
            {
                Collider c = cheesePrefab.GetComponent<Collider>();
                if (c != null)
                    yOffset = Mathf.Max(c.bounds.extents.y, 0.05f);
                else
                {
                    Renderer r = cheesePrefab.GetComponent<Renderer>();
                    if (r != null)
                        yOffset = Mathf.Max(r.bounds.extents.y, 0.05f);
                }
            }

            Vector3 testOrigin = spawnPoint + Vector3.up * 1.0f;
            if (Physics.Raycast(testOrigin, Vector3.down, out RaycastHit hit, 3f))
            {
                spawnPoint = hit.point + Vector3.up * (yOffset + 0.34f);
            }
            else
            {
                spawnPoint += Vector3.up * (yOffset + 0.1f);
            }

            Quaternion rot = Quaternion.Euler(-90f, Random.Range(0f, 360f), 0f);
            GameObject cheese = Instantiate(cheesePrefab, spawnPoint, rot);
            CheeseManager.RegisterCheese(cheese.transform);
            Debug.Log($"🧀 Spawned cheese at {spawnPoint}");
        }

        canDropCheese = false;
        yield return new WaitForSeconds(cheeseCooldown);
        canDropCheese = true;
    }

    void TryShoot()
    {
        if (isReloading) return;
        Vector3? targetPos = GetMouseWorldPosition();
        if (targetPos == null) return;
        Shoot(targetPos.Value);
    }

    void Shoot(Vector3 targetPos)
    {
        if (cam == null)
        {
            Debug.LogWarning("ModeManager: Camera not found.");
            return;
        }

        Vector3 firePos = firePoint != null ? firePoint.position : cam.transform.position;
        Vector3 dir = (targetPos - firePos).normalized;
        if (dir.sqrMagnitude < 0.0001f)
            dir = cam.transform.forward;

        Vector3 up = -dir;
        Vector3 right = Vector3.Cross(Vector3.up, up).normalized;
        if (right.sqrMagnitude < 0.001f)
            right = Vector3.Cross(Vector3.forward, up).normalized;
        Vector3 forward = Vector3.Cross(up, right);
        Quaternion rot = Quaternion.LookRotation(forward, up);

        GameObject bullet = Instantiate(bulletPrefab, firePos, rot);

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float bulletSpeed = 20f;
            Bullet bulletComp = bullet.GetComponent<Bullet>();
            if (bulletComp != null && bulletComp.speed > 0f)
                bulletSpeed = bulletComp.speed;

            rb.linearVelocity = dir * bulletSpeed;
            rb.useGravity = false;
        }

        Debug.DrawRay(firePos, dir * 5f, Color.cyan, 2f);
    }

    Vector3? GetMouseWorldPosition()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (Vector3.Dot(hit.normal, Vector3.up) > 0.7f)
                return hit.point;
        }
        return null;
    }

    void UpdateCheeseIndicator()
    {
        if (cheeseIndicator == null) return;
        bool shouldShow = currentMode == Mode.Cheese;
        cheeseIndicator.enabled = shouldShow;
        if (!shouldShow) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 worldPos = ray.GetPoint(2.5f);
        float hover = Mathf.Sin(Time.time * 1.5f) * 0.01f;
        float pulse = 1f + Mathf.Sin(Time.time * 1.5f) * 0.05f;

        cheeseIndicator.transform.position = worldPos + cam.transform.up * hover;
        cheeseIndicator.transform.rotation = cam.transform.rotation;
        cheeseIndicator.transform.localScale = Vector3.one * 0.057f * pulse;
    }

    void UpdateGunIndicator()
    {
        if (gunIndicator == null) return;
        bool shouldShow = currentMode == Mode.Gun;
        gunIndicator.enabled = shouldShow;
        if (!shouldShow) return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Vector3 worldPos = ray.GetPoint(3f);
        float hover = Mathf.Sin(Time.time * 1.5f) * 0.01f;
        float pulse = 1f + Mathf.Sin(Time.time * 1.5f) * 0.05f;

        gunIndicator.transform.position = worldPos + cam.transform.up * hover;
        gunIndicator.transform.rotation = cam.transform.rotation;
        gunIndicator.transform.localScale = Vector3.one * 0.115f * pulse;
    }
}
