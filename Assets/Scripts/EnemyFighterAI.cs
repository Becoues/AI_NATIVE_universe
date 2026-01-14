using UnityEngine;

/// <summary>
/// 敌人战机AI - 基础追击和攻击行为
/// 状态机：巡逻 -> 追击 -> 攻击 -> 规避
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HealthSystem))]
[DisallowMultipleComponent]
public class EnemyFighterAI : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target; // 攻击目标（通常是玩家）
    [SerializeField] private float detectionRange = 500f; // 检测范围
    [SerializeField] private bool autoFindPlayer = true; // 自动寻找玩家

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 40f; // 移动速度
    [SerializeField] private float acceleration = 20f;
    [SerializeField] private float turnSpeed = 80f; // 转向速度（度/秒）
    [SerializeField] private float maxAngularVelocity = 200f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 300f; // 攻击距离
    [SerializeField] private float optimalAttackRange = 200f; // 最佳攻击距离
    [SerializeField] private float fireRate = 0.3f; // 射击间隔
    [SerializeField] private float weaponDamage = 8f;
    [SerializeField] private float weaponRange = 400f;
    [SerializeField] private Transform[] firePoints; // 武器发射点
    [SerializeField] private float aimTolerance = 10f; // 瞄准容差角度

    [Header("Behavior")]
    [SerializeField] private float evasionDistance = 50f; // 规避距离阈值
    [SerializeField] private float evasionChance = 0.3f; // 规避概率
    [SerializeField] private float patrolRadius = 100f; // 巡逻半径

    [Header("Visual")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject bulletTracerPrefab;

    // 组件引用
    private Rigidbody rb;
    private HealthSystem healthSystem;

    // AI 状态
    private AIState currentState = AIState.Patrol;
    private Vector3 patrolPoint;
    private float nextFireTime = 0f;
    private float lastEvasionTime = 0f;
    private Vector3 evasionDirection;
    private float stateTimer = 0f;

    private enum AIState
    {
        Patrol,    // 巡逻
        Chase,     // 追击
        Attack,    // 攻击
        Evade      // 规避
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 2f;
        rb.maxAngularVelocity = maxAngularVelocity;

        healthSystem = GetComponent<HealthSystem>();

        // 生成初始巡逻点
        GenerateNewPatrolPoint();
    }

    private void Start()
    {
        // 自动寻找玩家
        if (autoFindPlayer && target == null)
        {
            PlayerShipController player = FindObjectOfType<PlayerShipController>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        // 设置为敌人阵营
        if (healthSystem != null)
        {
            // 确保是敌人队伍
        }
    }

    private void FixedUpdate()
    {
        if (healthSystem != null && healthSystem.IsDead)
            return;

        UpdateAIState();
        ExecuteCurrentState();
    }

    /// <summary>
    /// 更新AI状态机
    /// </summary>
    private void UpdateAIState()
    {
        stateTimer += Time.fixedDeltaTime;

        // 如果没有目标，保持巡逻
        if (target == null)
        {
            currentState = AIState.Patrol;
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 检测目标是否在范围内
        if (distanceToTarget > detectionRange)
        {
            currentState = AIState.Patrol;
            return;
        }

        // 规避检测（距离太近或受到威胁）
        if (distanceToTarget < evasionDistance && Random.value < evasionChance)
        {
            if (Time.time - lastEvasionTime > 3f) // 规避冷却时间
            {
                currentState = AIState.Evade;
                lastEvasionTime = Time.time;
                evasionDirection = Random.onUnitSphere;
                stateTimer = 0f;
                return;
            }
        }

        // 根据距离决定状态
        if (distanceToTarget <= attackRange)
        {
            currentState = AIState.Attack;
        }
        else
        {
            currentState = AIState.Chase;
        }
    }

    /// <summary>
    /// 执行当前状态的行为
    /// </summary>
    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                break;
            case AIState.Chase:
                ChaseTarget();
                break;
            case AIState.Attack:
                AttackTarget();
                break;
            case AIState.Evade:
                Evade();
                break;
        }
    }

    /// <summary>
    /// 巡逻行为
    /// </summary>
    private void Patrol()
    {
        MoveTowards(patrolPoint, moveSpeed * 0.5f);

        // 到达巡逻点，生成新的
        if (Vector3.Distance(transform.position, patrolPoint) < 20f)
        {
            GenerateNewPatrolPoint();
        }
    }

    /// <summary>
    /// 追击目标
    /// </summary>
    private void ChaseTarget()
    {
        if (target == null)
            return;

        // 预判目标位置
        Vector3 predictedPosition = PredictTargetPosition(target, 0.5f);
        MoveTowards(predictedPosition, moveSpeed);
    }

    /// <summary>
    /// 攻击目标
    /// </summary>
    private void AttackTarget()
    {
        if (target == null)
            return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);

        // 保持最佳攻击距离
        if (distanceToTarget < optimalAttackRange * 0.8f)
        {
            // 太近了，后退
            Vector3 retreatPosition = transform.position + (transform.position - target.position).normalized * 50f;
            MoveTowards(retreatPosition, moveSpeed * 0.7f);
        }
        else if (distanceToTarget > optimalAttackRange * 1.2f)
        {
            // 太远了，靠近
            MoveTowards(target.position, moveSpeed * 0.8f);
        }
        else
        {
            // 在最佳距离，盘旋
            Vector3 orbitPosition = target.position + Quaternion.Euler(0, 30f * Time.fixedDeltaTime, 0) *
                                    (transform.position - target.position).normalized * optimalAttackRange;
            MoveTowards(orbitPosition, moveSpeed * 0.6f);
        }

        // 瞄准并射击
        AimAtTarget(target);

        if (IsTargetInSights(target) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate;
        }
    }

    /// <summary>
    /// 规避行为
    /// </summary>
    private void Evade()
    {
        // 规避持续2秒
        if (stateTimer > 2f)
        {
            currentState = AIState.Chase;
            return;
        }

        Vector3 evasionTarget = transform.position + evasionDirection * 100f;
        MoveTowards(evasionTarget, moveSpeed * 1.2f);
    }

    /// <summary>
    /// 移动到目标点
    /// </summary>
    private void MoveTowards(Vector3 targetPosition, float speed)
    {
        // 转向目标
        Vector3 direction = (targetPosition - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        Quaternion newRotation = Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            turnSpeed * Time.fixedDeltaTime
        );
        rb.MoveRotation(newRotation);

        // 向前推进
        rb.AddForce(transform.forward * acceleration, ForceMode.Acceleration);

        // 限制速度
        if (rb.linearVelocity.magnitude > speed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * speed;
        }
    }

    /// <summary>
    /// 瞄准目标
    /// </summary>
    private void AimAtTarget(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        rb.MoveRotation(Quaternion.RotateTowards(
            rb.rotation,
            targetRotation,
            turnSpeed * 1.5f * Time.fixedDeltaTime
        ));
    }

    /// <summary>
    /// 检查目标是否在瞄准线内
    /// </summary>
    private bool IsTargetInSights(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        return angle < aimTolerance;
    }

    /// <summary>
    /// 预判目标位置
    /// </summary>
    private Vector3 PredictTargetPosition(Transform target, float predictionTime)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            return target.position + targetRb.linearVelocity * predictionTime;
        }
        return target.position;
    }

    /// <summary>
    /// 射击
    /// </summary>
    private void Fire()
    {
        if (firePoints == null || firePoints.Length == 0)
        {
            firePoints = new Transform[] { transform };
        }

        foreach (Transform firePoint in firePoints)
        {
            if (firePoint == null)
                continue;

            // Raycast 检测
            Ray ray = new Ray(firePoint.position, firePoint.forward);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, weaponRange))
            {
                // 检查是否击中玩家
                HealthSystem targetHealth = hit.collider.GetComponent<HealthSystem>();
                if (targetHealth != null && targetHealth.Team != healthSystem.Team)
                {
                    targetHealth.TakeDamage(weaponDamage, hit.point, -ray.direction);
                }

                // 子弹轨迹
                if (bulletTracerPrefab != null)
                {
                    CreateBulletTracer(firePoint.position, hit.point);
                }
            }

            // 枪口火光
            if (muzzleFlashPrefab != null)
            {
                GameObject flash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
                Destroy(flash, 0.1f);
            }
        }
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
        Destroy(tracer, 0.2f);
    }

    /// <summary>
    /// 生成新的巡逻点
    /// </summary>
    private void GenerateNewPatrolPoint()
    {
        patrolPoint = transform.position + Random.onUnitSphere * patrolRadius;
    }

    /// <summary>
    /// 设置攻击目标
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    // Gizmos 可视化
    private void OnDrawGizmosSelected()
    {
        // 检测范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 最佳攻击距离
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, optimalAttackRange);

        // 巡逻点
        if (Application.isPlaying && currentState == AIState.Patrol)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, patrolPoint);
            Gizmos.DrawWireSphere(patrolPoint, 5f);
        }

        // 当前目标
        if (target != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
