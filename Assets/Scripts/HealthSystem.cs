using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 生命值系统 - 适用于玩家和敌人
/// 处理伤害、护盾、死亡和恢复
/// </summary>
[DisallowMultipleComponent]
public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;

    [Header("Shield Settings")]
    [SerializeField] private bool hasShield = true;
    [SerializeField] private float maxShield = 200f;
    [SerializeField] private float currentShield;
    [SerializeField] private float shieldRegenRate = 20f; // 每秒恢复量
    [SerializeField] private float shieldRegenDelay = 5f; // 受击后延迟恢复时间

    [Header("Death Settings")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float destroyDelay = 0.5f;
    [SerializeField] private bool respawnOnDeath = false;
    [SerializeField] private float respawnDelay = 3f;

    [Header("Team")]
    [SerializeField] private TeamType team = TeamType.Player;

    [Header("Events")]
    public UnityEvent<float> OnHealthChanged;
    public UnityEvent<float> OnShieldChanged;
    public UnityEvent<float, Vector3> OnDamaged; // 伤害值，击中点
    public UnityEvent OnDeath;
    public UnityEvent OnRespawn;

    // 私有状态
    private float lastDamageTime;
    private bool isDead = false;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    public enum TeamType
    {
        Player,
        Enemy,
        Neutral
    }

    // 公共属性
    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;
    public float HealthPercentage => currentHealth / maxHealth;

    public float MaxShield => maxShield;
    public float CurrentShield => currentShield;
    public float ShieldPercentage => hasShield ? currentShield / maxShield : 0f;

    public bool IsDead => isDead;
    public TeamType Team => team;
    public bool HasShield => hasShield;

    private void Awake()
    {
        // 初始化生命值
        currentHealth = maxHealth;
        currentShield = hasShield ? maxShield : 0f;

        // 记录出生点
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
    }

    private void Update()
    {
        if (isDead)
            return;

        // 护盾自动恢复
        if (hasShield && currentShield < maxShield)
        {
            // 检查是否超过延迟时间
            if (Time.time - lastDamageTime >= shieldRegenDelay)
            {
                RegenerateShield(shieldRegenRate * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// 受到伤害
    /// </summary>
    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (isDead || damage <= 0)
            return;

        lastDamageTime = Time.time;
        float remainingDamage = damage;

        // 优先扣除护盾
        if (hasShield && currentShield > 0)
        {
            float shieldDamage = Mathf.Min(currentShield, remainingDamage);
            currentShield -= shieldDamage;
            remainingDamage -= shieldDamage;

            OnShieldChanged?.Invoke(ShieldPercentage);
        }

        // 扣除生命值
        if (remainingDamage > 0)
        {
            currentHealth -= remainingDamage;
            currentHealth = Mathf.Max(currentHealth, 0);

            OnHealthChanged?.Invoke(HealthPercentage);
        }

        // 触发受伤事件
        OnDamaged?.Invoke(damage, hitPoint);

        // 检查死亡
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    /// <summary>
    /// 直接伤害（无击中点信息）
    /// </summary>
    public void TakeDamage(float damage)
    {
        TakeDamage(damage, transform.position, Vector3.zero);
    }

    /// <summary>
    /// 恢复生命值
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead || amount <= 0)
            return;

        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(HealthPercentage);
    }

    /// <summary>
    /// 恢复护盾
    /// </summary>
    public void RegenerateShield(float amount)
    {
        if (!hasShield || isDead || amount <= 0)
            return;

        currentShield = Mathf.Min(currentShield + amount, maxShield);
        OnShieldChanged?.Invoke(ShieldPercentage);
    }

    /// <summary>
    /// 完全恢复
    /// </summary>
    public void FullRestore()
    {
        currentHealth = maxHealth;
        currentShield = hasShield ? maxShield : 0f;

        OnHealthChanged?.Invoke(HealthPercentage);
        OnShieldChanged?.Invoke(ShieldPercentage);
    }

    /// <summary>
    /// 死亡处理
    /// </summary>
    private void Die()
    {
        isDead = true;
        OnDeath?.Invoke();

        // 生成爆炸效果
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
        }

        // 禁用碰撞和渲染
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = false;
        }

        // 销毁或重生
        if (respawnOnDeath)
        {
            Invoke(nameof(Respawn), respawnDelay);
        }
        else
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    /// <summary>
    /// 重生
    /// </summary>
    private void Respawn()
    {
        isDead = false;

        // 恢复位置
        transform.position = spawnPosition;
        transform.rotation = spawnRotation;

        // 恢复生命值
        FullRestore();

        // 重新启用碰撞和渲染
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;
        }

        // 重置速度（如果有 Rigidbody）
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        OnRespawn?.Invoke();
    }

    /// <summary>
    /// 立即击毁
    /// </summary>
    public void InstantKill()
    {
        currentHealth = 0;
        currentShield = 0;
        Die();
    }

    /// <summary>
    /// 设置出生点
    /// </summary>
    public void SetSpawnPoint(Vector3 position, Quaternion rotation)
    {
        spawnPosition = position;
        spawnRotation = rotation;
    }

    // 调试可视化
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        // 显示生命值比例
        Gizmos.color = Color.Lerp(Color.red, Color.green, HealthPercentage);
        Gizmos.DrawWireSphere(transform.position + Vector3.up * 2f, 0.5f);

        // 显示护盾状态
        if (hasShield && currentShield > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 1.5f);
        }
    }
}
