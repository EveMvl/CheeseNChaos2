using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerNew : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;

    private PlayerControls controls;
    private Vector2 moveInput;

    private Camera mainCam;
    private Vector3 targetPosition;
    private bool isMovingToTarget = false;

    private enum ControlMode { Keyboard, Pointer }
    private ControlMode currentMode = ControlMode.Keyboard;

    // Visual debug marker
    public GameObject debugMarkerPrefab;

    private GameObject currentMarker;

    void Awake()
    {
        controls = new PlayerControls();
        mainCam = Camera.main;

        // WASD
        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled  += ctx => moveInput = Vector2.zero;

        // Klik mouse / touchpad
        controls.Player.Click.performed += ctx =>
        {
            Debug.Log("Click detected!");
            if (currentMode == ControlMode.Pointer)
            {
                Vector2 screenPos = Mouse.current.position.ReadValue();
                Ray ray = mainCam.ScreenPointToRay(screenPos);

                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    targetPosition = hit.point;
                    isMovingToTarget = true;
                    Debug.Log("Raycast hit point: " + hit.point);

                    if (debugMarkerPrefab != null)
                    {
                        if (currentMarker != null)
                            Destroy(currentMarker);

                        currentMarker = Instantiate(debugMarkerPrefab, hit.point + Vector3.up * 0.1f, Quaternion.identity);
                        currentMarker.GetComponent<Renderer>().material.color = Color.green; // hit sukses
                    }
                }
                else
                {
                    Debug.Log("Raycast did not hit anything!");
                    if (debugMarkerPrefab != null)
                    {
                        if (currentMarker != null)
                            Destroy(currentMarker);

                        currentMarker = Instantiate(debugMarkerPrefab, ray.origin + ray.direction * 5f, Quaternion.identity);
                        currentMarker.GetComponent<Renderer>().material.color = Color.red; // raycast miss
                    }
                }
            }
        };
    }

    void OnEnable()
    {
        if (controls != null)
            controls.Player.Enable();
    }

    void OnDisable()
    {
        if (controls != null)
            controls.Player.Disable();
    }

    void Update()
    {
        if (currentMode == ControlMode.Keyboard)
            HandleKeyboard();
        else if (currentMode == ControlMode.Pointer)
            HandlePointer();
    }

    private void HandleKeyboard()
    {
        if (moveInput != Vector2.zero)
        {
            Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    private void HandlePointer()
    {
        if (isMovingToTarget)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;

            if ((targetPosition - transform.position).magnitude > 0.1f)
            {
                transform.position += direction * moveSpeed * Time.deltaTime;
                Quaternion targetRot = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
            else
            {
                isMovingToTarget = false;
                Debug.Log("Reached target!");
                if (currentMarker != null)
                {
                    Destroy(currentMarker);
                }
            }
        }
    }

    // Panggil dari UI Button menu
    public void SetControlMode(string mode)
    {
        currentMode = (mode == "Keyboard") ? ControlMode.Keyboard : ControlMode.Pointer;
        Debug.Log("Control mode set to: " + currentMode);
    }
}
