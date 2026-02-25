using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour
{
    [Header("Base Weapon Settings")]
    public float damage = 20f;
    public float range = 50f;
    public float fireRate = 0.2f;        // ������ ����� ����������
    public int magazineSize = 30;
    public int totalAmmo = 90;
    public float reloadTime = 1.5f;

    [Header("References")]
    public Transform cam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;          // ������ ���������

    [Header("Recoil")]
    public float recoilAmount = 2f;          // ���� ������
    public float recoilSpeed = 10f;         // �������� ���������� ������
    public float recoilRecovery = 6f;          // �������� ��������

    protected int currentAmmo;
    protected int currentTotalAmmo;
    protected bool isReloading;
    protected float nextFireTime;
    protected float currentRecoil;
    protected float targetRecoil;

    protected virtual void Start()
    {
        currentAmmo = magazineSize;
        currentTotalAmmo = totalAmmo;
    }

    protected virtual void Update()
    {
        HandleRecoil();

        if (isReloading) return;

        if (currentAmmo <= 0 || (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize))
        {
            if (currentTotalAmmo > 0)
                StartCoroutine(Reload());
            return;
        }

        if (CanShoot())
            Shoot();
    }

    protected virtual bool CanShoot()
    {
        return Input.GetButton("Fire1") && Time.time >= nextFireTime;
    }

 
    protected virtual void Shoot()
    {
        nextFireTime = Time.time + fireRate;
        currentAmmo--;

        targetRecoil += recoilAmount;

        if (muzzleFlash != null)
            muzzleFlash.Play();

        Ray ray = new Ray(cam.position, cam.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, range))
        {
            Debug.DrawLine(ray.origin, hit.point, Color.green, 0.5f);
            OnHit(hit);
        }
        else
        {
            Debug.DrawLine(ray.origin, ray.origin + ray.direction * range, Color.red, 0.5f);
        }

        OnShoot();
    }


    protected virtual void OnHit(RaycastHit hit)
    {
        IDamageable target = hit.collider.GetComponent<IDamageable>();
        if (target != null)
            target.TakeDamage(damage);

        if (impactEffect != null)
        {
            GameObject fx = Instantiate(impactEffect, hit.point,
                Quaternion.LookRotation(hit.normal));
            Destroy(fx, 1f);
        }
    }


    protected virtual void OnShoot() { }


    protected virtual System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        OnReloadStart();

        yield return new WaitForSeconds(reloadTime);

        int needed = magazineSize - currentAmmo;
        int toLoad = Mathf.Min(needed, currentTotalAmmo);
        currentAmmo += toLoad;
        currentTotalAmmo -= toLoad;

        isReloading = false;
        OnReloadEnd();
    }

    protected virtual void OnReloadStart() { }
    protected virtual void OnReloadEnd() { }

    private void HandleRecoil()
    {
        targetRecoil = Mathf.Lerp(targetRecoil, 0f, recoilRecovery * Time.deltaTime);
        currentRecoil = Mathf.Lerp(currentRecoil, targetRecoil, recoilSpeed * Time.deltaTime);

        if (cam != null)
            cam.localEulerAngles -= new Vector3(currentRecoil * Time.deltaTime, 0f, 0f);
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetTotalAmmo() => currentTotalAmmo;
    public bool GetIsReloading() => isReloading;
}