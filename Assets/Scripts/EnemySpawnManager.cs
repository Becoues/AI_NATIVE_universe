using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 敌人生成管理器 - 波次系统
/// 负责生成敌人、管理波次、记录战斗统计
/// </summary>
[DisallowMultipleComponent]
public class EnemySpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab; // 敌人预制体
    [SerializeField] private Transform[] spawnPoints; // 生成点
    [SerializeField] private float spawnRadius = 50f; // 生成半径
    [SerializeField] private bool useSpawnPoints = true; // 使用固定生成点还是随机位置

    [Header("Wave Settings")]
    [SerializeField] private bool autoStartWaves = true;
    [SerializeField] private float timeBetweenWaves = 10f; // 波次间隔
    [SerializeField] private float firstWaveDelay = 3f; // 第一波延迟
    [SerializeField] private int maxWaves = 10; // 最大波次数（0=无限）

    [Header("Wave Progression")]
    [SerializeField] private int initialEnemyCount = 5; // 初始敌人数量
    [SerializeField] private int enemiesPerWaveIncrease = 2; // 每波增加的敌人数
    [SerializeField] private int maxEnemiesPerWave = 20; // 单波最大敌人数
    [SerializeField] private bool increaseWithWaves = true; // 是否随波次递增

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 2f; // 同一波内敌人的生成间隔
    [SerializeField] private bool spawnAllAtOnce = false; // 是否一次性生成全部

    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform; // 玩家位置
    [SerializeField] private bool autoFindPlayer = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;

    // 状态追踪
    private int currentWave = 0;
    private int totalEnemiesSpawned = 0;
    private int totalEnemiesKilled = 0;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawningWave = false;
    private bool allWavesCompleted = false;

    // 公共属性
    public int CurrentWave => currentWave;
    public int ActiveEnemyCount => activeEnemies.Count;
    public bool IsWaveActive => activeEnemies.Count > 0 || isSpawningWave;
    public bool AllWavesCompleted => allWavesCompleted;

    private void Start()
    {
        // 自动查找玩家
        if (autoFindPlayer && playerTransform == null)
        {
            PlayerShipController player = FindObjectOfType<PlayerShipController>();
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        // 验证生成点
        if (useSpawnPoints && (spawnPoints == null || spawnPoints.Length == 0))
        {
            Debug.LogWarning("EnemySpawnManager: No spawn points assigned! Using random positions.");
            useSpawnPoints = false;
        }

        // 自动开始波次
        if (autoStartWaves)
        {
            Invoke(nameof(StartNextWave), firstWaveDelay);
        }
    }

    private void Update()
    {
        // 清理已销毁的敌人引用
        activeEnemies.RemoveAll(enemy => enemy == null);

        // 检查波次完成条件
        if (!isSpawningWave && activeEnemies.Count == 0 && currentWave > 0 && !allWavesCompleted)
        {
            OnWaveCompleted();
        }
    }

    /// <summary>
    /// 开始下一波
    /// </summary>
    public void StartNextWave()
    {
        if (allWavesCompleted || isSpawningWave)
            return;

        // 检查是否达到最大波次
        if (maxWaves > 0 && currentWave >= maxWaves)
        {
            allWavesCompleted = true;
            OnAllWavesCompleted();
            return;
        }

        currentWave++;
        StartCoroutine(SpawnWave());
    }

    /// <summary>
    /// 生成一波敌人
    /// </summary>
    private IEnumerator SpawnWave()
    {
        isSpawningWave = true;

        // 计算本波敌人数量
        int enemyCount = CalculateEnemyCount();

        if (showDebugInfo)
        {
            Debug.Log($"Wave {currentWave} starting! Spawning {enemyCount} enemies.");
        }

        if (spawnAllAtOnce)
        {
            // 一次性生成所有敌人
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy();
            }
        }
        else
        {
            // 间隔生成
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnEnemy();
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        isSpawningWave = false;
    }

    /// <summary>
    /// 生成单个敌人
    /// </summary>
    private void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawnManager: No enemy prefab assigned!");
            return;
        }

        Vector3 spawnPosition = GetSpawnPosition();
        Quaternion spawnRotation = GetSpawnRotation(spawnPosition);

        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, spawnRotation);

        // 设置敌人目标为玩家
        EnemyFighterAI enemyAI = enemy.GetComponent<EnemyFighterAI>();
        if (enemyAI != null && playerTransform != null)
        {
            enemyAI.SetTarget(playerTransform);
        }

        // 注册死亡事件
        HealthSystem enemyHealth = enemy.GetComponent<HealthSystem>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath.AddListener(() => OnEnemyKilled(enemy));
        }

        activeEnemies.Add(enemy);
        totalEnemiesSpawned++;
    }

    /// <summary>
    /// 获取生成位置
    /// </summary>
    private Vector3 GetSpawnPosition()
    {
        if (useSpawnPoints && spawnPoints.Length > 0)
        {
            // 使用预设生成点
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            return spawnPoint.position + Random.insideUnitSphere * spawnRadius * 0.3f;
        }
        else
        {
            // 在玩家周围随机生成
            if (playerTransform != null)
            {
                // 在玩家前方/侧面生成（避免背后偷袭）
                Vector3 randomOffset = Random.onUnitSphere * spawnRadius;
                randomOffset.y = Random.Range(-20f, 20f); // 限制高度变化

                return playerTransform.position + randomOffset;
            }
            else
            {
                // 在世界原点周围生成
                return Random.insideUnitSphere * spawnRadius;
            }
        }
    }

    /// <summary>
    /// 获取生成朝向（面向玩家）
    /// </summary>
    private Quaternion GetSpawnRotation(Vector3 spawnPosition)
    {
        if (playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - spawnPosition).normalized;
            return Quaternion.LookRotation(direction);
        }
        return Quaternion.identity;
    }

    /// <summary>
    /// 计算本波敌人数量
    /// </summary>
    private int CalculateEnemyCount()
    {
        if (!increaseWithWaves)
        {
            return initialEnemyCount;
        }

        int count = initialEnemyCount + (currentWave - 1) * enemiesPerWaveIncrease;
        return Mathf.Min(count, maxEnemiesPerWave);
    }

    /// <summary>
    /// 敌人被击杀回调
    /// </summary>
    private void OnEnemyKilled(GameObject enemy)
    {
        totalEnemiesKilled++;
        activeEnemies.Remove(enemy);

        if (showDebugInfo)
        {
            Debug.Log($"Enemy killed! Remaining: {activeEnemies.Count}");
        }
    }

    /// <summary>
    /// 波次完成
    /// </summary>
    private void OnWaveCompleted()
    {
        if (showDebugInfo)
        {
            Debug.Log($"Wave {currentWave} completed!");
        }

        // 自动开始下一波
        if (autoStartWaves && !allWavesCompleted)
        {
            Invoke(nameof(StartNextWave), timeBetweenWaves);
        }
    }

    /// <summary>
    /// 所有波次完成
    /// </summary>
    private void OnAllWavesCompleted()
    {
        if (showDebugInfo)
        {
            Debug.Log("All waves completed! Victory!");
        }

        // 这里可以触发胜利事件
    }

    /// <summary>
    /// 手动触发下一波（用于UI按钮）
    /// </summary>
    public void TriggerNextWave()
    {
        CancelInvoke(nameof(StartNextWave));
        StartNextWave();
    }

    /// <summary>
    /// 清除所有敌人
    /// </summary>
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        activeEnemies.Clear();
    }

    /// <summary>
    /// 重置管理器
    /// </summary>
    public void ResetManager()
    {
        ClearAllEnemies();
        StopAllCoroutines();
        CancelInvoke();

        currentWave = 0;
        totalEnemiesSpawned = 0;
        totalEnemiesKilled = 0;
        isSpawningWave = false;
        allWavesCompleted = false;

        if (autoStartWaves)
        {
            Invoke(nameof(StartNextWave), firstWaveDelay);
        }
    }

    /// <summary>
    /// 获取统计信息
    /// </summary>
    public string GetStatsString()
    {
        return $"Wave: {currentWave} | Active: {activeEnemies.Count} | Killed: {totalEnemiesKilled} | Total Spawned: {totalEnemiesSpawned}";
    }

    // Gizmos 可视化
    private void OnDrawGizmos()
    {
        if (!useSpawnPoints || spawnPoints == null)
            return;

        Gizmos.color = Color.yellow;
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint != null)
            {
                Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
                Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 20f);
            }
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo)
            return;

        // 简单的调试UI
        GUIStyle style = new GUIStyle();
        style.fontSize = 20;
        style.normal.textColor = Color.white;

        GUI.Label(new Rect(10, 10, 400, 30), GetStatsString(), style);

        if (allWavesCompleted)
        {
            GUI.Label(new Rect(10, 40, 400, 30), "ALL WAVES COMPLETED!", style);
        }
        else if (!IsWaveActive && currentWave > 0)
        {
            GUI.Label(new Rect(10, 40, 400, 30), $"Next wave in {timeBetweenWaves}s", style);
        }
    }
}
