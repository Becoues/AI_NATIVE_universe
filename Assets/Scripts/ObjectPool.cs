using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 通用对象池 - 用于复用子弹、粒子特效等频繁创建销毁的对象
/// </summary>
public class ObjectPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 100;
    [SerializeField] private bool expandPool = true; // 池满时是否扩展

    private Queue<GameObject> availableObjects = new Queue<GameObject>();
    private List<GameObject> allObjects = new List<GameObject>();

    // 单例字典（支持多个对象池）
    private static Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

    private void Awake()
    {
        InitializePool();
    }

    /// <summary>
    /// 初始化对象池
    /// </summary>
    private void InitializePool()
    {
        if (prefab == null)
        {
            Debug.LogError("ObjectPool: No prefab assigned!");
            return;
        }

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewObject();
        }
    }

    /// <summary>
    /// 创建新对象
    /// </summary>
    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);

        allObjects.Add(obj);
        availableObjects.Enqueue(obj);

        // 添加自动回收组件
        PooledObject pooledObj = obj.GetComponent<PooledObject>();
        if (pooledObj == null)
        {
            pooledObj = obj.AddComponent<PooledObject>();
        }
        pooledObj.SetPool(this);

        return obj;
    }

    /// <summary>
    /// 从池中获取对象
    /// </summary>
    public GameObject Get()
    {
        GameObject obj = null;

        // 尝试从可用队列获取
        while (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();

            // 检查对象是否被意外销毁
            if (obj != null)
            {
                obj.SetActive(true);
                return obj;
            }
        }

        // 池已空，尝试扩展
        if (expandPool && allObjects.Count < maxPoolSize)
        {
            obj = CreateNewObject();
            obj.SetActive(true);
            return obj;
        }

        Debug.LogWarning($"ObjectPool: Pool exhausted for {prefab.name}");
        return null;
    }

    /// <summary>
    /// 从池中获取对象（指定位置和旋转）
    /// </summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = Get();
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        return obj;
    }

    /// <summary>
    /// 归还对象到池
    /// </summary>
    public void Return(GameObject obj)
    {
        if (obj == null)
            return;

        obj.SetActive(false);
        obj.transform.SetParent(transform);

        if (!availableObjects.Contains(obj))
        {
            availableObjects.Enqueue(obj);
        }
    }

    /// <summary>
    /// 清空池
    /// </summary>
    public void Clear()
    {
        foreach (GameObject obj in allObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        availableObjects.Clear();
        allObjects.Clear();
    }

    /// <summary>
    /// 获取池状态
    /// </summary>
    public string GetPoolStats()
    {
        return $"{prefab.name} Pool - Active: {allObjects.Count - availableObjects.Count}, Available: {availableObjects.Count}, Total: {allObjects.Count}";
    }

    // ========== 静态便捷方法 ==========

    /// <summary>
    /// 创建新对象池
    /// </summary>
    public static ObjectPool CreatePool(GameObject prefab, int initialSize = 20)
    {
        string poolName = $"Pool_{prefab.name}";

        // 检查是否已存在
        if (pools.ContainsKey(poolName))
        {
            return pools[poolName];
        }

        // 创建新池
        GameObject poolObject = new GameObject(poolName);
        ObjectPool pool = poolObject.AddComponent<ObjectPool>();
        pool.prefab = prefab;
        pool.initialPoolSize = initialSize;
        pool.InitializePool();

        pools[poolName] = pool;
        return pool;
    }

    /// <summary>
    /// 获取或创建对象池
    /// </summary>
    public static ObjectPool GetPool(GameObject prefab)
    {
        string poolName = $"Pool_{prefab.name}";

        if (pools.ContainsKey(poolName))
        {
            return pools[poolName];
        }

        return CreatePool(prefab);
    }

    private void OnDestroy()
    {
        // 从字典中移除
        string poolName = $"Pool_{prefab.name}";
        if (pools.ContainsKey(poolName))
        {
            pools.Remove(poolName);
        }
    }
}

/// <summary>
/// 池化对象组件 - 自动管理对象的生命周期
/// </summary>
public class PooledObject : MonoBehaviour
{
    private ObjectPool pool;
    private float lifetime = 0f;
    private float returnDelay = -1f; // 负数表示不自动归还

    public void SetPool(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    /// <summary>
    /// 设置自动归还延迟
    /// </summary>
    public void SetReturnDelay(float delay)
    {
        returnDelay = delay;
        lifetime = 0f;
    }

    private void OnEnable()
    {
        lifetime = 0f;
    }

    private void Update()
    {
        if (returnDelay > 0)
        {
            lifetime += Time.deltaTime;

            if (lifetime >= returnDelay)
            {
                ReturnToPool();
            }
        }
    }

    /// <summary>
    /// 归还到池
    /// </summary>
    public void ReturnToPool()
    {
        if (pool != null)
        {
            pool.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
