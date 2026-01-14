using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 编辑器工具 - 快速创建空战游戏测试对象
/// 使用方法：在 Unity 菜单栏选择 Tools > Combat Game > Quick Setup
/// </summary>
public class CombatGameQuickSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Combat Game/1. Create Player Ship (White Box)")]
    public static void CreatePlayerShip()
    {
        // 创建主体
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "PlayerShip";
        player.transform.position = Vector3.zero;
        player.transform.localScale = new Vector3(2f, 0.5f, 3f);

        // 设置材质颜色
        Renderer renderer = player.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.2f, 0.5f, 1f); // 蓝色
        renderer.material = mat;

        // 创建机头
        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = "Nose";
        nose.transform.SetParent(player.transform);
        nose.transform.localPosition = new Vector3(0, 0, 2f);
        nose.transform.localScale = new Vector3(0.25f, 0.6f, 0.5f);

        Renderer noseRenderer = nose.GetComponent<Renderer>();
        noseRenderer.material = mat;

        // 添加组件
        PlayerShipController controller = player.AddComponent<PlayerShipController>();
        HealthSystem health = player.AddComponent<HealthSystem>();
        WeaponSystem weapon = player.AddComponent<WeaponSystem>();
        Rigidbody rb = player.GetComponent<Rigidbody>();

        // 配置 Rigidbody
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearDamping = 0.2f;
            rb.angularDamping = 2f;
        }

        // 配置 HealthSystem
        // 使用反射设置私有字段（因为是SerializeField）
        SetPrivateField(health, "maxHealth", 100f);
        SetPrivateField(health, "hasShield", true);
        SetPrivateField(health, "maxShield", 200f);
        SetPrivateField(health, "team", HealthSystem.TeamType.Player);
        SetPrivateField(health, "respawnOnDeath", true);

        // 创建发射点
        GameObject firePointLeft = new GameObject("FirePoint_Left");
        firePointLeft.transform.SetParent(player.transform);
        firePointLeft.transform.localPosition = new Vector3(-0.8f, 0, 1.2f);

        GameObject firePointRight = new GameObject("FirePoint_Right");
        firePointRight.transform.SetParent(player.transform);
        firePointRight.transform.localPosition = new Vector3(0.8f, 0, 1.2f);

        // 分配发射点到武器系统
        SetPrivateField(weapon, "firePoints", new Transform[] { firePointLeft.transform, firePointRight.transform });

        // 设置图层
        player.layer = LayerMask.NameToLayer("Player");
        if (player.layer == 0) // 如果图层不存在
        {
            Debug.LogWarning("Player layer not found. Please create 'Player' layer manually.");
        }

        Selection.activeGameObject = player;
        Debug.Log("Player ship created! Don't forget to assign explosion prefab to HealthSystem.");
    }

    [MenuItem("Tools/Combat Game/2. Create Enemy Fighter (White Box)")]
    public static void CreateEnemyFighter()
    {
        // 创建主体
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
        enemy.name = "EnemyFighter";
        enemy.transform.position = new Vector3(50, 0, 50);
        enemy.transform.localScale = new Vector3(1.5f, 0.4f, 2f);

        // 设置材质颜色
        Renderer renderer = enemy.GetComponent<Renderer>();
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(1f, 0.2f, 0.2f); // 红色
        renderer.material = mat;

        // 添加组件
        EnemyFighterAI ai = enemy.AddComponent<EnemyFighterAI>();
        HealthSystem health = enemy.AddComponent<HealthSystem>();
        Rigidbody rb = enemy.GetComponent<Rigidbody>();

        // 配置 Rigidbody
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 2f;
        }

        // 配置 HealthSystem
        SetPrivateField(health, "maxHealth", 50f);
        SetPrivateField(health, "hasShield", false);
        SetPrivateField(health, "team", HealthSystem.TeamType.Enemy);
        SetPrivateField(health, "respawnOnDeath", false);

        // 创建发射点
        GameObject firePoint = new GameObject("FirePoint");
        firePoint.transform.SetParent(enemy.transform);
        firePoint.transform.localPosition = new Vector3(0, 0, 1.2f);

        SetPrivateField(ai, "firePoints", new Transform[] { firePoint.transform });

        // 设置图层
        enemy.layer = LayerMask.NameToLayer("Enemy");
        if (enemy.layer == 0)
        {
            Debug.LogWarning("Enemy layer not found. Please create 'Enemy' layer manually.");
        }

        Selection.activeGameObject = enemy;
        Debug.Log("Enemy fighter created! Save as Prefab before testing.");
    }

    [MenuItem("Tools/Combat Game/3. Create Simple Effects")]
    public static void CreateSimpleEffects()
    {
        // 创建 Prefabs 文件夹
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }

        // 枪口火光
        GameObject muzzleFlash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        muzzleFlash.name = "MuzzleFlash";
        muzzleFlash.transform.localScale = Vector3.one * 0.3f;

        Renderer mfRenderer = muzzleFlash.GetComponent<Renderer>();
        Material mfMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mfMat.color = Color.yellow;
        mfMat.SetColor("_EmissionColor", Color.yellow * 2f);
        mfMat.EnableKeyword("_EMISSION");
        mfRenderer.material = mfMat;

        PooledObject mfPooled = muzzleFlash.AddComponent<PooledObject>();
        mfPooled.SetReturnDelay(0.1f);

        PrefabUtility.SaveAsPrefabAsset(muzzleFlash, "Assets/Prefabs/MuzzleFlash.prefab");
        DestroyImmediate(muzzleFlash);

        // 子弹轨迹
        GameObject tracer = new GameObject("BulletTracer");
        LineRenderer line = tracer.AddComponent<LineRenderer>();
        line.startWidth = 0.05f;
        line.endWidth = 0.05f;
        line.positionCount = 2;
        line.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        line.material.color = Color.yellow;
        line.material.SetColor("_EmissionColor", Color.yellow * 2f);
        line.material.EnableKeyword("_EMISSION");

        PooledObject tracerPooled = tracer.AddComponent<PooledObject>();
        tracerPooled.SetReturnDelay(0.2f);

        PrefabUtility.SaveAsPrefabAsset(tracer, "Assets/Prefabs/BulletTracer.prefab");
        DestroyImmediate(tracer);

        // 击中特效
        GameObject hitEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hitEffect.name = "HitEffect";
        hitEffect.transform.localScale = Vector3.one * 0.5f;

        Renderer heRenderer = hitEffect.GetComponent<Renderer>();
        Material heMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        heMat.color = Color.white;
        heMat.SetColor("_EmissionColor", Color.white * 3f);
        heMat.EnableKeyword("_EMISSION");
        heRenderer.material = heMat;

        PooledObject hePooled = hitEffect.AddComponent<PooledObject>();
        hePooled.SetReturnDelay(0.5f);

        PrefabUtility.SaveAsPrefabAsset(hitEffect, "Assets/Prefabs/HitEffect.prefab");
        DestroyImmediate(hitEffect);

        // 爆炸效果
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.name = "Explosion";
        explosion.transform.localScale = Vector3.one * 5f;

        Renderer exRenderer = explosion.GetComponent<Renderer>();
        Material exMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        exMat.color = new Color(1f, 0.5f, 0f);
        exMat.SetColor("_EmissionColor", new Color(1f, 0.5f, 0f) * 5f);
        exMat.EnableKeyword("_EMISSION");
        exRenderer.material = exMat;

        Light exLight = explosion.AddComponent<Light>();
        exLight.range = 50f;
        exLight.intensity = 8f;
        exLight.color = new Color(1f, 0.5f, 0f);

        PrefabUtility.SaveAsPrefabAsset(explosion, "Assets/Prefabs/Explosion.prefab");
        DestroyImmediate(explosion);

        AssetDatabase.Refresh();
        Debug.Log("Simple effects created in Assets/Prefabs/");
    }

    [MenuItem("Tools/Combat Game/4. Create Spawn Manager")]
    public static void CreateSpawnManager()
    {
        GameObject manager = new GameObject("EnemySpawnManager");
        EnemySpawnManager spawnManager = manager.AddComponent<EnemySpawnManager>();

        // 创建生成点
        for (int i = 0; i < 4; i++)
        {
            GameObject spawnPoint = new GameObject($"SpawnPoint_{i + 1}");
            spawnPoint.transform.SetParent(manager.transform);

            float angle = i * 90f * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * 200f;
            spawnPoint.transform.position = pos;
        }

        Selection.activeGameObject = manager;
        Debug.Log("Spawn manager created! Assign enemy prefab in Inspector.");
    }

    [MenuItem("Tools/Combat Game/5. Setup Camera")]
    public static void SetupCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            mainCam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();
        }

        ChaseCamera chaseCamera = mainCam.GetComponent<ChaseCamera>();
        if (chaseCamera == null)
        {
            chaseCamera = mainCam.AddComponent<ChaseCamera>();
        }

        mainCam.fieldOfView = 70f;
        mainCam.farClipPlane = 2000f;

        // 尝试自动找到玩家
        PlayerShipController player = FindObjectOfType<PlayerShipController>();
        if (player != null)
        {
            SetPrivateField(chaseCamera, "target", player.transform);
        }

        Selection.activeGameObject = mainCam.gameObject;
        Debug.Log("Camera setup complete!");
    }

    [MenuItem("Tools/Combat Game/Complete Setup")]
    public static void CompleteSetup()
    {
        Debug.Log("Starting complete setup...");

        CreatePlayerShip();
        CreateEnemyFighter();
        CreateSimpleEffects();
        CreateSpawnManager();
        SetupCamera();

        Debug.Log("Complete setup finished! Check the scene and save enemy as prefab.");
    }

    // 辅助方法：设置私有字段
    private static void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            Debug.LogWarning($"Field '{fieldName}' not found in {obj.GetType().Name}");
        }
    }
#endif
}
