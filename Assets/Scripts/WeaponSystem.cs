using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 武器系统 - 处理玩家飞船的射击功能
/// 支持多种武器类型：机炮、激光、导弹
/// </summary>
[DisallowMultipleComponent]
public class WeaponSystem : MonoBehaviour
{
    [Header("Machine Gun Settings")]
    [SerializeField] private float gunDamage = 10f;
    [SerializeField] private float gunFireRate = 0.1f; // 每0.1秒发射一次 = 600发/分
    [SerializeField] private float gunRange = 500f;
    [SerializeField] private int gunAmmoPerMagazine = 100;
    [SerializeField] private float gunReloadTime = 2f;

    [Header("Fire Points")]
    [SerializeField] private Transform[] firePoints; // 武器发射点（支持多枪管）

    [Header("Visual Effects")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject bulletTracerPrefab;
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private LineRenderer laserSight; // 激光瞄准线（可选）

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip gunFireSound;
    [SerializeField] private AudioClip emptyClickSound;
    [SerializeField] private AudioClip reloadSound;

    [Header("Weapon Type")]
    [SerializeField] private WeaponType currentWeapon = WeaponType.MachineGun;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;

    // 私有状态
    private float nextFireTime = 0f;
    private int currentAmmo;
    private bool isReloading = false;
    private int currentFirePointIndex = 0;

    public enum WeaponType
    {
        MachineGun,
        Laser,
        Missile
    }

    // 公共属性
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => gunAmmoPerMagazine;
    public bool IsReloading => isReloading;
    public WeaponType CurrentWeaponType => currentWeapon;

    private void Awake()
    {
        currentAmmo = gunAmmoPerMagazine;

        // 自动查找 AudioSource
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0.7f; // 70% 3D音效
    }

    private void Update()
    {
        HandleInput();

        // 如果没有提供发射点，使用自身位置
        if (firePoints == null || firePoints.Length == 0)
        {
            firePoints = new Transform[] { transform };
        }
    }

    private void HandleInput()
    {
        // 射击输入（多种方式）
        bool firePressed = false;

        // 方式1: 鼠标左键
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            firePressed = true;
            if (showDebugInfo) Debug.Log("Fire: Mouse Left Button");
        }

        // 方式2: 左Ctrl键
        if (!firePressed && Keyboard.current != null && Keyboard.current.leftCtrlKey.isPressed)
        {
            firePressed = true;
            if (showDebugInfo) Debug.Log("Fire: Left Ctrl");
        }

        // 方式3: F键（备用射击键，避免与移动键冲突）
        if (!firePressed && Keyboard.current != null && Keyboard.current.fKey.isPressed)
        {
            firePressed = true;
            if (showDebugInfo) Debug.Log("Fire: F Key");
        }

        if (firePressed && !isReloading)
        {
            TryFire();
        }

        // 手动装填（R键）
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
        {
            if (!isReloading && currentAmmo < gunAmmoPerMagazine)
            {
                if (showDebugInfo) Debug.Log("Reloading...");
                StartReload();
            }
        }
    }

    private void TryFire()
    {
        if (Time.time < nextFireTime)
            return;

        if (currentAmmo <= 0)
        {
            // 没有弹药，自动装填
            if (!isReloading)
            {
                PlaySound(emptyClickSound);
                StartReload();
            }
            return;
        }

        Fire();
    }

    private void Fire()
    {
        if (showDebugInfo)
        {
            Debug.Log($"Firing! Ammo: {currentAmmo}/{gunAmmoPerMagazine}");
        }

        switch (currentWeapon)
        {
            case WeaponType.MachineGun:
                FireMachineGun();
                break;
            case WeaponType.Laser:
                FireLaser();
                break;
            case WeaponType.Missile:
                FireMissile();
                break;
        }

        nextFireTime = Time.time + gunFireRate;
        currentAmmo--;

        // 播放射击音效
        PlaySound(gunFireSound);
    }

    private void FireMachineGun()
    {
        // 轮流使用多个发射点（如果有多个枪管）
        Transform firePoint = firePoints[currentFirePointIndex];
        currentFirePointIndex = (currentFirePointIndex + 1) % firePoints.Length;

        // Raycast 检测
        Ray ray = new Ray(firePoint.position, firePoint.forward);
        RaycastHit hit;

        bool didHit = Physics.Raycast(ray, out hit, gunRange);

        // 生成枪口火光
        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(flash, 0.1f);
        }

        if (didHit)
        {
            // 检查是否击中了可伤害对象
            HealthSystem targetHealth = hit.collider.GetComponent<HealthSystem>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(gunDamage, hit.point, -ray.direction);
            }

            // 生成击中特效
            if (hitEffectPrefab != null)
            {
                GameObject hitEffect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(hitEffect, 1f);
            }

            // 生成子弹轨迹（从枪口到击中点）
            if (bulletTracerPrefab != null)
            {
                CreateBulletTracer(firePoint.position, hit.point);
            }

            // 调试可视化
            Debug.DrawLine(firePoint.position, hit.point, Color.yellow, 0.1f);
        }
        else
        {
            // 没有击中，显示最大射程的轨迹
            Vector3 endPoint = firePoint.position + firePoint.forward * gunRange;
            if (bulletTracerPrefab != null)
            {
                CreateBulletTracer(firePoint.position, endPoint);
            }

            Debug.DrawRay(firePoint.position, firePoint.forward * gunRange, Color.red, 0.1f);
        }
    }

    private void FireLaser()
    {
        // TODO: 激光武器实现（持续射线）
        Debug.Log("Laser weapon not implemented yet");
    }

    private void FireMissile()
    {
        // TODO: 导弹武器实现
        Debug.Log("Missile weapon not implemented yet");
    }

    private void CreateBulletTracer(Vector3 start, Vector3 end)
    {
        GameObject tracer = Instantiate(bulletTracerPrefab);
        LineRenderer line = tracer.GetComponent<LineRenderer>();

        if (line != null)
        {
            line.SetPosition(0, start);
            line.SetPosition(1, end);
        }
        else
        {
            // 如果没有 LineRenderer，简单移动对象
            tracer.transform.position = start;
            tracer.transform.LookAt(end);
        }

        Destroy(tracer, 0.2f);
    }

    private void StartReload()
    {
        if (isReloading)
            return;

        isReloading = true;
        PlaySound(reloadSound);
        Invoke(nameof(FinishReload), gunReloadTime);
    }

    private void FinishReload()
    {
        currentAmmo = gunAmmoPerMagazine;
        isReloading = false;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // 武器切换（未来扩展）
    public void SwitchWeapon(WeaponType newWeapon)
    {
        currentWeapon = newWeapon;
        // 可以在这里添加切换动画和音效
    }

    // 外部调用：添加弹��
    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, gunAmmoPerMagazine);
    }

    private void OnDrawGizmosSelected()
    {
        // 在编辑器中可视化射程
        if (firePoints != null && firePoints.Length > 0)
        {
            Gizmos.color = Color.red;
            foreach (Transform firePoint in firePoints)
            {
                if (firePoint != null)
                {
                    Gizmos.DrawRay(firePoint.position, firePoint.forward * gunRange);
                }
            }
        }
    }
}
