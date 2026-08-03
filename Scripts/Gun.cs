using UnityEngine;
using System.Collections;

public class Gun : MonoBehaviour
{
    public float reloadTime = 1f;
    public float fireRate = 0.15f;
    public int magSize = 20;
    public float recoilDistance = 0.1f;
    public float recoilSpeed = 15f;
    public GameObject bullet;
    public Transform bulletSpawnPoint;
    public GameObject weaponFlash;
    public GameObject droppedWeapon;
    private int currentAmmo;
    private bool isReloading = false;
    private float nextTimeToFire = 0f;
    private Quaternion initialRotation;
    private Vector3 initialPosition;
    private Vector3 reloadRotationOffset = new Vector3(66, 50, 50);
    public AudioClip shootingSFX;
    void Start()
    {
        currentAmmo = magSize;
        initialRotation = transform.localRotation;
        initialPosition = transform.localPosition;
        UIManager.Instance.ammoText.text = currentAmmo + " / " + magSize;

    }

    public void Shoot()
    {
        if (isReloading)return;
        if(Time.time < nextTimeToFire)return;
        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }
        nextTimeToFire = Time.time + fireRate;
        currentAmmo--;
        UIManager.Instance.ammoText.text = currentAmmo + " / " + magSize;
        AudioManager.Instance.PlaySFX(shootingSFX, 0.25f);
        Quaternion adjustedRotation = bulletSpawnPoint.rotation * Quaternion.Euler(-1f,-1f,0f);
        Instantiate(bullet, bulletSpawnPoint.position, adjustedRotation);
        Instantiate(weaponFlash, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        StopCoroutine(nameof(Recoil));
        StartCoroutine(nameof(Recoil));
    }
IEnumerator Reload()
    {
        isReloading = true;
        Quaternion targetRotation = Quaternion.Euler(initialRotation.eulerAngles + reloadRotationOffset);
        float halfReloadTime = reloadTime / 2f;
        float t = 0f;
        while (t < halfReloadTime)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t / halfReloadTime);            
            yield return null;
        }
        t = 0f;
        while (t < halfReloadTime)
        {
            t += Time.deltaTime;
            transform.localRotation = Quaternion.Slerp(targetRotation, initialRotation, t / halfReloadTime);            
            yield return null;
        }
        currentAmmo = magSize;
        UIManager.Instance.ammoText.text = currentAmmo + " / " + magSize;
        isReloading = false;
    }
    public void TryReload()
    {
        if(isReloading) return;
        if(currentAmmo == magSize) return;
        StartCoroutine(Reload());
    }
    private IEnumerator Recoil()
    {
        Vector3 recoilTarget = initialPosition + new Vector3(recoilDistance, 0f, 0f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(initialPosition, recoilTarget, t);            
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * recoilSpeed;
            transform.localPosition = Vector3.Lerp(recoilTarget, initialPosition, t);            
            yield return null;
        }
        transform.localPosition = initialPosition;
    }
    public void Drop()
    {
        UIManager.Instance.ammoText.text = "";
        Instantiate(droppedWeapon, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}
