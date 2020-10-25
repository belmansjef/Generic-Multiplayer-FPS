using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

public enum WeaponType
{
    AssaultRifle,
    SniperRifle
}

public class WeaponController : MonoBehaviour
{
    #region Public Fields

    public bool IsReloading => isReloading;

    #endregion

    #region Serialized Private Fields

    [Header("Weapon statistics:")]
    [SerializeField] private WeaponType weaponType;
    [SerializeField] private int damage;
    [SerializeField] private int magazineSize;
    [SerializeField] private int bulletsPerTap;
    [SerializeField] private int maxPenetration;
    [SerializeField] private float aimMovementSpeed;
    [SerializeField] private float timeBetweenShooting;
    [SerializeField] private float range;
    [SerializeField] private float reloadTime;
    [SerializeField] private float timeBetweenShots;
    [SerializeField] private bool isSemiAutomatic;
    [SerializeField] private float spread;
    [SerializeField] private float innacuracy;

    [Header("Weapon aiming:")]
    [SerializeField] private Vector3 hipfirePosition;
    [SerializeField] private Vector3 adsPosition;

    [Header("Camera recoil:")]
    [SerializeField] private float rotationSpeed = 6f;
    [SerializeField] private float returnSpeed = 25f;
    [Space(5)]
    [SerializeField] private Vector3 recoilRotation = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 recoilRotationADS = new Vector3(0.5f, 0.5f, 1.5f);

    [Header("Weapon recoil:")]
    [SerializeField] private Transform recoilPosition;
    [SerializeField] private Transform rotationPoint;
    [Space(5)]
    [SerializeField] private float positionalRecoilSpeed = 8f;
    [SerializeField] private float rotationalRecoilSpeed = 8f;
    [Space(5)]
    [SerializeField] private float positionalReturnSpeed = 18f;
    [SerializeField] private float rotationalReturnSpeed = 38f;
    [Space(5)]
    [SerializeField] private Vector3 RecoilRotation = new Vector3(10, 5, 7);
    [SerializeField] private Vector3 RecoilKickBack = new Vector3(0.015f, 0f, -0.0f);
    [Space(5)]
    [SerializeField] private Vector3 RecoilRotationAim = new Vector3(10, 4, 6);
    [SerializeField] private Vector3 RecoilKickBackAim = new Vector3(0.015f, 0f, -0.2f);


    [Header("Object references:")]
    [SerializeField] private Camera fpsCam;
    [SerializeField] private Camera scopeCam;

    [SerializeField] private Transform muzzlePosition;
    [SerializeField] private GameObject[] p_MuzzleFlash;
    [SerializeField] private GameObject p_BulletHole;
    [SerializeField] private GameObject weaponModel;

    #endregion

    #region Private Fields

    // Spread
    private float spreadModifier;
    private float innacuracyModifier;

    // Ammo
    private int bulletsLeft;
    private int bulletsShot;

    // Check bools
    private bool isShooting;
    private bool isAiming;
    private bool isReadyToShoot;
    private bool isReloading;

    // Camera recoil
    private Vector3 currentRotation;
    private Vector3 cameraRotation;

    // Weapon recoil
    private Vector3 rotationalRecoil;
    private Vector3 positionalRecoil;
    private Vector3 weaponRotation;

    // Raycasting
    private Vector3 rayOrigin;
    private RaycastHit[] m_RaycastHits;

    private Rigidbody rb;

    // Bug fixing
    private bool isScriptLoaded = false;
    
    private List<RaycastHit> SortByDistance(RaycastHit[] _hits)
    {
        return _hits.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).ToList();
    }

    private PhotonView PV;

    #endregion

    #region MonoBehaviour Callbacks

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        rb = GetComponentInParent<Rigidbody>();
        fpsCam = GetComponentInParent<Camera>();

        bulletsLeft = magazineSize;
        isReadyToShoot = true;
    }

    private void Start()
    {
        if (PV.IsMine)
        {
            UIManager.instance.UpdateAmmoUI(bulletsLeft, magazineSize);
            if (weaponType == WeaponType.AssaultRifle)
            {
                UIManager.instance.EnableCrosshair();
            }
            else if (weaponType == WeaponType.SniperRifle)
            {
                UIManager.instance.DisableCrosshair();
            }
        }
        else
        {
            if (scopeCam) scopeCam.enabled = false;
        }

        
        isScriptLoaded = true;
    }

    private void Update()
    {
        if (!PV.IsMine) return;
        if(UIManager.instance.isPaused) return;

        GetInput();

        if (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0 || rb.velocity.magnitude >= 1f || (weaponType == WeaponType.SniperRifle && !isAiming))
        {
            spreadModifier = spread;
            innacuracyModifier = innacuracy;
        }
        else
        {
            spreadModifier = 1f;
            innacuracyModifier = 0f;
        }
    }

    private void FixedUpdate()
    {
        ApplyCameraRecoil();
        ApplyWeaponRecoil();
    }

    #endregion

    #region Public Methods

    public void CancelReload()
    {
        isReloading = false;
        CancelInvoke(nameof(ReloadFinished));
    }

    #endregion

    #region Private Methods

    private void GetInput()
    {
        isShooting = !isSemiAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");
        isAiming = Input.GetButton("Fire2");

        if (Input.GetButtonDown("Fire2"))
            PlayerController.i.movementSpeedMofifier = aimMovementSpeed;
        if (Input.GetButtonUp("Fire2"))
            PlayerController.i.movementSpeedMofifier = 1f;

        if (isAiming) AimDownSights();
        else AimHipFire();

        if(isShooting && bulletsLeft <= 0 && !isReloading) Reload();
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !isReloading) Reload();

        if (isReadyToShoot && isShooting && !isReloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }

    private void Shoot()
    {
        isReadyToShoot = false;

        PV.RPC("PlayMuzzleFlash", RpcTarget.All);

        ShootRay();
        GetCameraRecoil();
        GetWeaponRecoil();

        bulletsLeft--;
        bulletsShot--;

        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke(nameof(Shoot), timeBetweenShots);

        if (bulletsLeft <= 0)
            Reload();

        switch (weaponType)
        {
            case WeaponType.AssaultRifle:
                Invoke(nameof(ResetShot), timeBetweenShooting);
                break;
            case WeaponType.SniperRifle:
                Invoke(nameof(BoltBackward), timeBetweenShooting / 3f);
                break;
        }

        UIManager.instance.UpdateAmmoUI(bulletsLeft, magazineSize);
        PV.RPC("PlayShootSound", RpcTarget.All);
    }

    private void BoltBackward()
    {
        SoundManager.instance.PlaySound(SoundManager.Sound.SniperBoltBackward);
        Invoke(nameof(BoltForward), timeBetweenShooting / 3f);
    }

    private void BoltForward()
    {
        SoundManager.instance.PlaySound(SoundManager.Sound.SniperBoltForward);
        Invoke(nameof(ResetShot), timeBetweenShooting / 3f);
    }
    
    private void ResetShot()
    {
        isReadyToShoot = true;
    }

    private void Reload()
    {
        isReloading = true;
        Invoke(nameof(ReloadFinished), reloadTime);

        switch (weaponType)
        {
            case WeaponType.AssaultRifle:
                SoundManager.instance.PlaySound(SoundManager.Sound.AkReload);
                break;
            case WeaponType.SniperRifle:
                SoundManager.instance.PlaySound(SoundManager.Sound.SniperReload);
                break;
        }
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        isReloading = false;

        UIManager.instance.UpdateAmmoUI(bulletsLeft, magazineSize);
    }

    private void ShootRay()
    {
        m_RaycastHits = new RaycastHit[maxPenetration];
        rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        Vector3 direction = fpsCam.transform.forward + new Vector3(Random.Range(-innacuracyModifier, innacuracyModifier), Random.Range(-innacuracyModifier, innacuracyModifier), 0f);
        
        m_RaycastHits = Physics.RaycastAll(rayOrigin, direction, range);
        List<RaycastHit> hits = SortByDistance(m_RaycastHits);

        HandleDamage(hits);
    }

    private void HandleDamage(List<RaycastHit> _hits)
    {
        int penetrationCount = 0;
        foreach (RaycastHit r in _hits)
        {
            float currentDamage = damage;
            HealthController enemyHealth = r.collider.GetComponent<HealthController>();

            if (enemyHealth)
            {
                currentDamage = CalculateDamage(penetrationCount, currentDamage);
                HitEnemy(currentDamage, PhotonNetwork.LocalPlayer.ActorNumber, r.collider.GetComponent<PhotonView>().ViewID);

                if (currentDamage <= 0)
                {
                    break;
                }
            }
            else
            {
                PV.RPC("InstantiateBulletHole", RpcTarget.All, r.point, r.normal);
            }

            penetrationCount++;
        }
    }

    private float CalculateDamage(int _penetrationCount, float _damage)
    {
        return Mathf.Round(_damage / (1f + (0.125f * _penetrationCount)));
    }

    private void HitEnemy(float _damage, int _sourceID, int _receiverID)
    {
        // Find the hit player and send damage RPC
        PhotonView _receiver = PhotonView.Find(_receiverID);
        if (_receiver)
        {
            _receiver.RPC("TakeDamage", RpcTarget.All, _damage, _sourceID);
        }

        SoundManager.instance.PlaySound(SoundManager.Sound.HitMarker);
        UIManager.instance.ShowHitmarker(0.1f);
    }

    private void KilledEnemy(int _receiverID, int _sourceID)
    {
        if (_sourceID == 6969) return;
        if (PhotonView.Find(_sourceID).IsMine)
        {
            SoundManager.instance.PlaySound(SoundManager.Sound.confirmKill);
        }
    }

    #region Aiming

    private void AimDownSights()
    {
        if (weaponModel.transform.localPosition == adsPosition) return;
        weaponModel.transform.localPosition = Vector3.Slerp(weaponModel.transform.localPosition, adsPosition, 10 * Time.deltaTime);
    }

    private void AimHipFire()
    {
        if (weaponModel.transform.localPosition == hipfirePosition) return;
        weaponModel.transform.localPosition = Vector3.Slerp(weaponModel.transform.localPosition, hipfirePosition, 10 * Time.deltaTime);
    }

    #endregion

    #region Recoil

    private void GetCameraRecoil()
    {
        if (isAiming)
            currentRotation += new Vector3(-recoilRotationADS.x * spreadModifier,Random.Range(-recoilRotationADS.y, recoilRotationADS.y) * spreadModifier,Random.Range(-recoilRotationADS.z, recoilRotationADS.z));
        else
            currentRotation += new Vector3(-recoilRotation.x * spreadModifier,Random.Range(-recoilRotation.y, recoilRotation.y) * spreadModifier, Random.Range(-recoilRotation.z, recoilRotation.z));
    }

    private void ApplyCameraRecoil()
    {
        currentRotation = Vector3.Lerp(currentRotation, Vector3.zero, returnSpeed * Time.fixedDeltaTime);
        cameraRotation = Vector3.Slerp(cameraRotation, currentRotation, rotationSpeed * Time.fixedDeltaTime);
        fpsCam.transform.localRotation = Quaternion.Euler(cameraRotation);
    }

    private void GetWeaponRecoil()
    {
        if (isAiming)
        {
            rotationalRecoil += new Vector3(-RecoilRotationAim.x * spreadModifier,Random.Range(-RecoilRotationAim.y, RecoilRotationAim.y) * spreadModifier, Random.Range(-RecoilRotationAim.z, RecoilRotationAim.z) * spreadModifier);
            positionalRecoil += new Vector3(Random.Range(-RecoilKickBackAim.x, RecoilKickBackAim.x) * spreadModifier, Random.Range(-RecoilKickBackAim.y, RecoilKickBackAim.y) * spreadModifier, RecoilKickBackAim.z * spreadModifier);
        }
        else
        {
            rotationalRecoil += new Vector3(-RecoilRotation.x * spreadModifier, Random.Range(-RecoilRotation.y, RecoilRotation.y) * spreadModifier, Random.Range(-RecoilRotation.z, RecoilRotation.z * spreadModifier));
            positionalRecoil += new Vector3(Random.Range(-RecoilKickBack.x, RecoilKickBack.x) * spreadModifier, Random.Range(-RecoilKickBack.y, RecoilKickBack.y) * spreadModifier, RecoilKickBack.z * spreadModifier);
        }
    }

    private void ApplyWeaponRecoil()
    {
        rotationalRecoil = Vector3.Lerp(rotationalRecoil, Vector3.zero, rotationalReturnSpeed * Time.fixedDeltaTime);
        positionalRecoil = Vector3.Lerp(positionalRecoil, Vector3.zero, positionalReturnSpeed * Time.fixedDeltaTime);
        recoilPosition.localPosition = Vector3.Slerp(recoilPosition.localPosition, positionalRecoil,positionalRecoilSpeed * Time.fixedDeltaTime);

        weaponRotation = Vector3.Slerp(weaponRotation, rotationalRecoil, rotationalRecoilSpeed * Time.fixedDeltaTime);
        rotationPoint.localRotation = Quaternion.Euler(weaponRotation);
    }

    #endregion
    
    private void OnEnable()
    {
        // Reset all recoil when switching weapons
        weaponRotation = Vector3.zero;
        rotationalRecoil = Vector3.zero;
        positionalRecoil = Vector3.zero;
        cameraRotation = Vector3.zero;
        fpsCam.transform.localRotation = Quaternion.Euler(Vector3.zero);
        recoilPosition.localPosition = Vector3.zero;
        rotationPoint.localRotation = Quaternion.Euler(Vector3.zero);

        if (!isScriptLoaded) return;

        if (weaponType == WeaponType.AssaultRifle)
        {
            UIManager.instance.EnableCrosshair();
        }
        else if (weaponType == WeaponType.SniperRifle)
        {
            UIManager.instance.DisableCrosshair();
        }
        
        UIManager.instance.UpdateAmmoUI(bulletsLeft, magazineSize);
    }

    #endregion

    #region RPC Methods

    [PunRPC]
    private void InstantiateBulletHole(Vector3 _point, Vector3 _normal)
    {
        GameObject bulletHoleGo = Instantiate(p_BulletHole, _point, Quaternion.LookRotation(_normal));
        Destroy(bulletHoleGo, 5f);
    }

    [PunRPC]
    private void PlayShootSound()
    {
        SoundManager.Sound sound;

        switch (weaponType)
        {
            case WeaponType.AssaultRifle:
                sound = SoundManager.Sound.AkShot;
                break;
            case WeaponType.SniperRifle:
                sound = SoundManager.Sound.SniperShot;
                break;
            default:
                sound = SoundManager.Sound.AkShot;
                break;
        }

        if (PV.IsMine)
            SoundManager.instance.PlaySound(sound);
        else
            SoundManager.instance.PlaySound(sound, transform.position);
    }

    [PunRPC]
    private void PlayMuzzleFlash()
    {
        GameObject muzzleGo = Instantiate(p_MuzzleFlash[Random.Range(0, p_MuzzleFlash.Length)], muzzlePosition.position,
            Quaternion.Euler(muzzlePosition.eulerAngles));
        muzzleGo.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        muzzleGo.transform.parent = muzzlePosition.transform;
        Destroy(muzzleGo, 0.2f);
    }

    #endregion
}