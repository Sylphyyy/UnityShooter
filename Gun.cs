using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Item
{
    const float defaultFOV = 75f; 
    [SerializeField] Camera cam;
    [SerializeField] GameObject bulletImpactPrefab;
    GunInfo gun;
    float lastShot;

    bool scoped;
    float baseCameraFOV;

    float lastShotTime;

    PhotonView PV;

    public override void AltUseRepeating()
    {
        scoped = true;
    }

    void Update()
    {
        if (!itemGameObject.activeInHierarchy)
            return;

        ManageScoping();

        if (Input.GetMouseButtonDown(0) && CanShoot())
        {
            Shoot();
            lastShotTime = Time.time;
        }
    }

    private bool CanShoot()
    {
        return Time.time > lastShotTime + shootCooldown;
    }

    void ManageScoping()
    {
        float scopedFOV = baseCameraFOV * gun.scopeZoomMult;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, scoped ? scopedFOV : baseCameraFOV, gun.scopeInSpeed * Time.deltaTime);

        scoped = false;
    }


    void Awake()
    {
        PV = GetComponent<PhotonView>();
        gun = itemInfo as GunInfo;

        if (!PV.IsMine)
            return;
        baseCameraFOV = cam.fieldOfView;
    }

    public override void Use()
    {
        Shoot();
    }

    public override void UseRepeating()
    {
        if (!(gun.automatic))
            return;
        Shoot();
    }

    protected virtual void Shoot()
    {
        if (Time.time < lastShot + gun.firerate)
            return;

        for (int i = 0; i < gun.pelletsPerAttack; i++)
        {
            Vector2 spread = Random.insideUnitCircle * gun.spread;
            Ray ray = cam.ViewportPointToRay(new Vector2(0.5f, 0.5f) + spread / (cam.fieldOfView / defaultFOV));
            ray.origin = cam.transform.position;
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                hit.collider.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
                PV.RPC("RPC_Shoot", RpcTarget.All, hit.point, hit.normal);
            }
        }
        lastShot = Time.time;
    }

    [PunRPC]
    protected virtual void RPC_Shoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        Collider[] colliders = Physics.OverlapSphere(hitPosition, 0.3f);
        if (colliders.Length != 0)
        {
            GameObject bulletImpactObj = Instantiate(bulletImpactPrefab, hitPosition + hitNormal * 0.001f, Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation);
            Destroy(bulletImpactObj, 10f);
            bulletImpactObj.transform.SetParent(colliders[0].transform);
        }
    }
}
