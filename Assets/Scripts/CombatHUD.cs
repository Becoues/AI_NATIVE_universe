using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 战斗HUD - 显示玩家状态、准星、弹药等
/// </summary>
[DisallowMultipleComponent]
public class CombatHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HealthSystem playerHealth;
    [SerializeField] private WeaponSystem playerWeapon;
    [SerializeField] private EnemySpawnManager spawnManager;

    [Header("Health UI")]
    [SerializeField] private Image healthBar;
    [SerializeField] private Image shieldBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private Color healthFullColor = Color.green;
    [SerializeField] private Color healthLowColor = Color.red;

    [Header("Weapon UI")]
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI reloadText;
    [SerializeField] private Image crosshair;

    [Header("Wave UI")]
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private TextMeshProUGUI enemyCountText;

    [Header("Speed UI")]
    [SerializeField] private TextMeshProUGUI speedText;
    [SerializeField] private Rigidbody playerRigidbody;

    [Header("Crosshair Settings")]
    [SerializeField] private Color crosshairNormalColor = Color.white;
    [SerializeField] private Color crosshairEnemyColor = Color.red;
    [SerializeField] private float crosshairCheckRange = 500f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Damage Indicators")]
    [SerializeField] private Image damageVignette;
    [SerializeField] private float vignetteFadeSpeed = 2f;

    private bool autoFindReferences = true;
    private float currentVignetteAlpha = 0f;

    private void Start()
    {
        // 自动查找引用
        if (autoFindReferences)
        {
            FindReferences();
        }

        // 初始化UI
        if (reloadText != null)
        {
            reloadText.gameObject.SetActive(false);
        }

        if (damageVignette != null)
        {
            Color vignetteColor = damageVignette.color;
            vignetteColor.a = 0f;
            damageVignette.color = vignetteColor;
        }

        // 注册事件
        if (playerHealth != null)
        {
            playerHealth.OnDamaged.AddListener(OnPlayerDamaged);
        }
    }

    private void FindReferences()
    {
        // 查找玩家组件
        if (playerHealth == null)
        {
            PlayerShipController player = FindObjectOfType<PlayerShipController>();
            if (player != null)
            {
                playerHealth = player.GetComponent<HealthSystem>();
                playerWeapon = player.GetComponent<WeaponSystem>();
                playerRigidbody = player.GetComponent<Rigidbody>();
            }
        }

        // 查找生成管理器
        if (spawnManager == null)
        {
            spawnManager = FindObjectOfType<EnemySpawnManager>();
        }
    }

    private void Update()
    {
        UpdateHealthUI();
        UpdateWeaponUI();
        UpdateWaveUI();
        UpdateSpeedUI();
        UpdateCrosshair();
        UpdateDamageVignette();
    }

    /// <summary>
    /// 更新生命值UI
    /// </summary>
    private void UpdateHealthUI()
    {
        if (playerHealth == null)
            return;

        // 血量条
        if (healthBar != null)
        {
            healthBar.fillAmount = playerHealth.HealthPercentage;

            // 根据生命值改变颜色
            healthBar.color = Color.Lerp(healthLowColor, healthFullColor, playerHealth.HealthPercentage);
        }

        // 护盾条
        if (shieldBar != null)
        {
            shieldBar.fillAmount = playerHealth.ShieldPercentage;
        }

        // 血量文本
        if (healthText != null)
        {
            healthText.text = $"HP: {Mathf.Ceil(playerHealth.CurrentHealth)}/{playerHealth.MaxHealth}";

            if (playerHealth.HasShield)
            {
                healthText.text += $"\nShield: {Mathf.Ceil(playerHealth.CurrentShield)}/{playerHealth.MaxShield}";
            }
        }
    }

    /// <summary>
    /// 更新武器UI
    /// </summary>
    private void UpdateWeaponUI()
    {
        if (playerWeapon == null)
            return;

        // 弹药显示
        if (ammoText != null)
        {
            ammoText.text = $"{playerWeapon.CurrentAmmo} / {playerWeapon.MaxAmmo}";

            // 低弹药警告
            if (playerWeapon.CurrentAmmo <= playerWeapon.MaxAmmo * 0.3f)
            {
                ammoText.color = Color.yellow;
            }
            else
            {
                ammoText.color = Color.white;
            }
        }

        // 装填提示
        if (reloadText != null)
        {
            reloadText.gameObject.SetActive(playerWeapon.IsReloading);
        }
    }

    /// <summary>
    /// 更新波次UI
    /// </summary>
    private void UpdateWaveUI()
    {
        if (spawnManager == null)
            return;

        if (waveText != null)
        {
            waveText.text = $"Wave: {spawnManager.CurrentWave}";
        }

        if (enemyCountText != null)
        {
            enemyCountText.text = $"Enemies: {spawnManager.ActiveEnemyCount}";
        }
    }

    /// <summary>
    /// 更新速度UI
    /// </summary>
    private void UpdateSpeedUI()
    {
        if (playerRigidbody == null || speedText == null)
            return;

        float speed = playerRigidbody.linearVelocity.magnitude;
        speedText.text = $"Speed: {Mathf.RoundToInt(speed)} m/s";
    }

    /// <summary>
    /// 更新准星状态
    /// </summary>
    private void UpdateCrosshair()
    {
        if (crosshair == null)
            return;

        // 屏幕中心射线检测
        Ray ray = Camera.main != null
            ? Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
            : new Ray();

        RaycastHit hit;
        bool hitEnemy = Physics.Raycast(ray, out hit, crosshairCheckRange, enemyLayer);

        // 改变准星颜色
        if (hitEnemy)
        {
            HealthSystem targetHealth = hit.collider.GetComponent<HealthSystem>();
            if (targetHealth != null && targetHealth.Team != HealthSystem.TeamType.Player)
            {
                crosshair.color = crosshairEnemyColor;
                return;
            }
        }

        crosshair.color = crosshairNormalColor;
    }

    /// <summary>
    /// 更新伤害晕影
    /// </summary>
    private void UpdateDamageVignette()
    {
        if (damageVignette == null)
            return;

        // 逐渐淡出
        if (currentVignetteAlpha > 0)
        {
            currentVignetteAlpha -= vignetteFadeSpeed * Time.deltaTime;
            currentVignetteAlpha = Mathf.Max(currentVignetteAlpha, 0);

            Color vignetteColor = damageVignette.color;
            vignetteColor.a = currentVignetteAlpha;
            damageVignette.color = vignetteColor;
        }
    }

    /// <summary>
    /// 玩家受伤回调
    /// </summary>
    private void OnPlayerDamaged(float damage, Vector3 hitPoint)
    {
        // 触发伤害晕影
        if (damageVignette != null)
        {
            currentVignetteAlpha = Mathf.Min(currentVignetteAlpha + 0.3f, 0.8f);
        }
    }

    /// <summary>
    /// 设置玩家引用
    /// </summary>
    public void SetPlayerReferences(HealthSystem health, WeaponSystem weapon, Rigidbody rb)
    {
        playerHealth = health;
        playerWeapon = weapon;
        playerRigidbody = rb;

        // 重新注册事件
        if (playerHealth != null)
        {
            playerHealth.OnDamaged.AddListener(OnPlayerDamaged);
        }
    }

    /// <summary>
    /// 设置生成管理器引用
    /// </summary>
    public void SetSpawnManager(EnemySpawnManager manager)
    {
        spawnManager = manager;
    }
}
