using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public Gun gun;
    public Transform gunHolder;
    [SerializeField] private float zoomMultiplier = 3f;
    [SerializeField] private float zoomSpeed = 8f;

    private bool isHoldingShoot = false;
    private Camera playerCamera;
    private float defaultFov;
    private float targetFov;

    private void Awake()
    {
        playerCamera = Camera.main;
        if (playerCamera != null)
        {
            defaultFov = playerCamera.fieldOfView;
            targetFov = defaultFov;
        }
    }

    void OnShoot()
    {
        isHoldingShoot = true;
    }

    void OnShootRelease()
    {
        isHoldingShoot = false;
    }

    void OnReload()
    {
        if (gun != null)
        {
            gun.TryReload();
        }
    }

    void OnAim()
    {
        if (playerCamera != null)
        {
            targetFov = defaultFov / zoomMultiplier;
        }
    }

    void OnAimRelease()
    {
        if (playerCamera != null)
        {
            targetFov = defaultFov;
        }
    }

    void Update()
    {
        if (playerCamera != null)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFov, Time.deltaTime * zoomSpeed);
        }

        if (isHoldingShoot && gun != null)
        {
            gun.Shoot();
        }
    }
    public void OnDrop()
    {
        if (gun != null)
        {
            gun.Drop();
            gun = null;
        }
    }
}
