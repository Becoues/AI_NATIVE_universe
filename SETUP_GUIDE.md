# 空战游戏系统设置指南

本文档将指导你如何在Unity中设置刚创建的战斗系统，进行白模测试。

---

## 第一步：准备白模对象

### 1.1 创建玩家飞船（白模）

1. 在Hierarchy中创建：`GameObject > 3D Object > Cube`，命名为 `PlayerShip`
2. 缩放调整：`Scale = (2, 0.5, 3)` （扁平的飞机造型）
3. 添加一个子对象作为"机头"：
   - 创建 `Cube`，命名为 `Nose`
   - 位置：`(0, 0, 2)`
   - 缩放：`(0.5, 0.3, 1)`
   - 材质颜色：改为蓝色（玩家标识）

### 1.2 添加组件到玩家飞船

在 `PlayerShip` 上添加以下组件：
```
1. PlayerShipController（已存在）
2. HealthSystem（新增）
   - Max Health: 100
   - Has Shield: ✓
   - Max Shield: 200
   - Team: Player
   - Respawn On Death: ✓

3. WeaponSystem（新增）
   - Gun Damage: 10
   - Gun Fire Rate: 0.1
   - Gun Range: 500
   - Gun Ammo Per Magazine: 100

4. Rigidbody（已存在）
5. Box Collider
   - Is Trigger: OFF
```

### 1.3 创建武器发射点

1. 在 `PlayerShip` 下创建空对象：`GameObject > Create Empty`
2. 命名为 `FirePoint_Left`
3. 位置：`(-1, 0, 1.5)` （左机翼位置）
4. 复制一个命名为 `FirePoint_Right`，位置：`(1, 0, 1.5)`
5. 将这两个发射点拖到 `WeaponSystem` 的 `Fire Points` 数组中

---

## 第二步：创建敌人飞船（白模）

### 2.1 基础结构

1. 创建 `Cube`，命名为 `EnemyFighter`
2. 缩放：`(1.5, 0.4, 2)`
3. 材质颜色：改为红色（敌人标识）

### 2.2 添加组件到敌人

```
1. EnemyFighterAI
   - Detection Range: 500
   - Attack Range: 300
   - Move Speed: 40
   - Turn Speed: 80
   - Weapon Damage: 8

2. HealthSystem
   - Max Health: 50
   - Has Shield: OFF（轻型敌人无护盾）
   - Team: Enemy
   - Respawn On Death: OFF

3. Rigidbody
   - Use Gravity: OFF
   - Drag: 0.5
   - Angular Drag: 2

4. Box Collider
```

### 2.3 创建敌人武器发射点

1. 创建空对象 `FirePoint`，位置 `(0, 0, 1)`
2. 拖到 `EnemyFighterAI` 的 `Fire Points` 数组

### 2.4 保存为Prefab

1. 在 `Assets` 下创建文件夹 `Prefabs`
2. 将 `EnemyFighter` 拖到此文件夹，创建Prefab
3. 删除场景中的敌人实例（让管理器生成）

---

## 第三步：设置敌人生成系统

### 3.1 创建生成管理器

1. 创建空对象：`GameObject > Create Empty`，命名为 `EnemySpawnManager`
2. 添加 `EnemySpawnManager` 组件
3. 配置参数：
   ```
   Enemy Prefab: [拖入 EnemyFighter Prefab]
   Auto Start Waves: ✓
   Initial Enemy Count: 3
   Enemies Per Wave Increase: 2
   Time Between Waves: 10
   First Wave Delay: 5
   ```

### 3.2 创建生成点（可选）

如果想控制敌人生成位置：
1. 创建4个空对象，命名为 `SpawnPoint_1` ~ `SpawnPoint_4`
2. 放置在玩家周围（距离约200-300米）
3. 全部拖到 `EnemySpawnManager` 的 `Spawn Points` 数组
4. 勾选 `Use Spawn Points`

---

## 第四步：创建HUD Canvas

### 4.1 创建UI Canvas

1. `GameObject > UI > Canvas`，命名为 `CombatHUD`
2. Canvas设置：
   - Render Mode: Screen Space - Overlay
   - Canvas Scaler > UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

### 4.2 添加HUD元素

#### 准星（Crosshair）
```
1. UI > Image，命名为 Crosshair
2. Anchor: Center
3. 位置: (0, 0, 0)
4. 大小: (40, 40)
5. 颜色: 白色，半透明
6. 创建一个简单的十字：
   - 子对象 Image (Vertical)，大小 (2, 40)
   - 子对象 Image (Horizontal)，大小 (40, 2)
```

#### 血量条（Health Bar）
```
1. UI > Slider，命名为 HealthBar
2. Anchor: Left Top
3. 位置: (150, -50, 0)
4. 大小: (300, 30)
5. 删除 Handle Slide Area
6. 设置 Fill 颜色为绿色
7. Max Value: 1
8. Value: 1
```

#### 护盾条（Shield Bar）
```
复制 HealthBar，命名为 ShieldBar
位置: (150, -90, 0)
Fill 颜色改为青色
```

#### 弹药文本（Ammo Text）
```
1. UI > Text - TextMeshPro，命名为 AmmoText
2. Anchor: Right Bottom
3. 位置: (-150, 100, 0)
4. 字体大小: 36
5. 对齐: Right
6. 默认文本: "100 / 100"
```

#### 装填提示（Reload Text）
```
复制 AmmoText，命名为 ReloadText
位置: (-150, 50, 0)
颜色: 黄色
文本: "RELOADING..."
初始状态: 不激活（Inactive）
```

#### 波次信息（Wave Text）
```
UI > Text - TextMeshPro，命名为 WaveText
Anchor: Center Top
位置: (0, -50, 0)
字体大小: 48
对齐: Center
文本: "Wave: 1"
```

#### 敌人数量（Enemy Count Text）
```
复制 WaveText，命名为 EnemyCountText
位置: (0, -100, 0)
字体大小: 32
文本: "Enemies: 0"
```

#### 速度表（Speed Text）
```
UI > Text - TextMeshPro，命名为 SpeedText
Anchor: Left Bottom
位置: (150, 50, 0)
字体大小: 28
文本: "Speed: 0 m/s"
```

#### 伤害晕影（Damage Vignette）
```
1. UI > Image，命名为 DamageVignette
2. Anchor: Stretch（四个角都拉满）
3. 颜色: 红色，Alpha = 0
4. Raycast Target: OFF（不接收点击）
5. 可选：导入一个渐变纹理作为Sprite
```

### 4.3 添加 CombatHUD 脚本

1. 在 `Canvas (CombatHUD)` 上添加 `CombatHUD` 组件
2. 拖拽连接所有UI元素到对应字段：
   ```
   Health Bar: [拖入 HealthBar 的 Fill Image]
   Shield Bar: [拖入 ShieldBar 的 Fill Image]
   Health Text: [拖入血量文本]
   Ammo Text: [拖入弹药文本]
   Reload Text: [拖入装填文本]
   Crosshair: [拖入准星Image]
   Wave Text: [拖入波次文本]
   Enemy Count Text: [拖入敌人数量文本]
   Speed Text: [拖入速度文本]
   Damage Vignette: [拖入晕影Image]

   Player Health: [拖入玩家的 HealthSystem]
   Player Weapon: [拖入玩家的 WeaponSystem]
   Player Rigidbody: [拖入玩家的 Rigidbody]
   Spawn Manager: [拖入 EnemySpawnManager]
   ```

---

## 第五步：摄像机设置

### 5.1 配置 ChaseCamera

1. 在 Main Camera 上添加（如果没有）`ChaseCamera` 组件
2. 设置：
   ```
   Target: [拖入 PlayerShip Transform]
   Local Offset: (0, 2.5, -10)  # 向上2.5米，向后10米
   Follow Smoothing: 6
   Look Smoothing: 10
   ```

### 5.2 调整摄像机参数

```
Field of View: 70（提升速度感）
Near Clipping: 0.3
Far Clipping: 2000（看到远处敌人）
```

---

## 第六步：创建简单特效（临时白模）

### 6.1 枪口火光（Muzzle Flash）

1. 创建 `Sphere`，命名为 `MuzzleFlash`
2. 缩放：`(0.3, 0.3, 0.3)`
3. 材质：自发光，黄色/橙色
4. 添加 `PooledObject` 组件，设置 `Return Delay = 0.1`
5. 保存为Prefab：`Prefabs/MuzzleFlash`
6. 删除场景实例

### 6.2 子弹轨迹（Bullet Tracer）

1. 创建空对象 `BulletTracer`
2. 添加 `Line Renderer` 组件：
   ```
   Width: 0.05
   Color: 黄色发光
   Positions: 2个点
   Material: Default-Particle（或自发光材质）
   ```
3. 添加 `PooledObject` 组件，`Return Delay = 0.2`
4. 保存为Prefab：`Prefabs/BulletTracer`

### 6.3 击中特效（Hit Effect）

1. 创建 `Sphere`，命名为 `HitEffect`
2. 缩放：`(0.5, 0.5, 0.5)`
3. 材质：白色发光
4. 添加简单动画（可选）：缩放从1到0
5. 添加 `PooledObject`，`Return Delay = 0.5`
6. 保存为Prefab：`Prefabs/HitEffect`

### 6.4 爆炸特效（Explosion）

1. 创建 `Sphere`，命名为 `Explosion`
2. 缩放：`(5, 5, 5)`
3. 材质：橙色/红色自发光
4. 添加 `Light` 组件（爆炸闪光）：
   ```
   Range: 50
   Intensity: 8
   Color: 橙色
   ```
5. 添加简单脚本让它扩大后消失（或用动画）
6. 保存为Prefab：`Prefabs/Explosion`

### 6.5 将特效连接到武器系统

在 `PlayerShip` 的 `WeaponSystem` 组件中：
```
Muzzle Flash Prefab: [Prefabs/MuzzleFlash]
Bullet Tracer Prefab: [Prefabs/BulletTracer]
Hit Effect Prefab: [Prefabs/HitEffect]
```

在 `EnemyFighter Prefab` 的 `EnemyFighterAI` 中：
```
Muzzle Flash Prefab: [Prefabs/MuzzleFlash]
Bullet Tracer Prefab: [Prefabs/BulletTracer]
```

在敌人的 `HealthSystem` 中：
```
Explosion Prefab: [Prefabs/Explosion]
```

---

## 第七步：碰撞层设置（重要！）

### 7.1 创建图层

1. 打开 `Edit > Project Settings > Tags and Layers`
2. 添加图层：
   ```
   Layer 8: Player
   Layer 9: Enemy
   Layer 10: PlayerWeapon
   Layer 11: EnemyWeapon
   ```

### 7.2 分配图层

```
PlayerShip: Layer = Player
EnemyFighter Prefab: Layer = Enemy
```

### 7.3 配置碰撞矩阵

1. `Edit > Project Settings > Physics`
2. 在 Layer Collision Matrix 中：
   - ✓ Player 可以碰撞 Enemy
   - ✓ Enemy 可以碰撞 Player
   - ✗ Player 不碰撞 Player
   - ✗ Enemy 不碰撞 Enemy

---

## 第八步：测试场景布置

### 8.1 场景检查清单

确保场景中有：
- ✓ PlayerShip（带所有组件）
- ✓ Main Camera（带 ChaseCamera）
- ✓ EnemySpawnManager
- ✓ Canvas (CombatHUD)
- ✓ Directional Light

### 8.2 保存场景

1. `File > Save As`
2. 命名为 `CombatTestScene`
3. 保存在 `Scenes` 文件夹

---

## 第九步：播放测试！

### 9.1 运行游戏

1. 点击 Play 按钮
2. 5秒后第一波敌人应该生成
3. 使用控制：
   - **WASD** - 移动
   - **鼠标** - 视角
   - **左Ctrl** 或 **鼠标左键** - 射击
   - **空格** - 上升
   - **Shift+空格** - 下降
   - **Q/E** - 翻滚
   - **R** - 装填

### 9.2 检查事项

- [ ] 玩家飞船可以正常飞行
- [ ] 按住射击键有子弹轨迹
- [ ] 击中敌人后敌人血量下降
- [ ] 敌人死亡后有爆炸效果
- [ ] 敌人会追击和攻击玩家
- [ ] HUD正确显示血量、弹药、波次
- [ ] 玩家受伤时有红色晕影
- [ ] 第一波清完后自动开始第二波

---

## 常见问题排查

### Q1: 点击射击没反应
**A:** 检查：
- WeaponSystem 的 FirePoints 是否已分配
- Console有没有报错
- 是否在装填中（等2秒）

### Q2: 敌人不攻击
**A:** 检查：
- EnemyFighterAI 的 Target 是否自动找到玩家
- Detection Range 是否足够大（建议500）
- Console 是否有错误

### Q3: HUD不显示数据
**A:** 检查：
- CombatHUD 的引用是否全部连接
- Canvas 的 Render Mode 是否正确
- EventSystem 是否存在（创建Canvas时自动生成）

### Q4: 子弹轨迹不显示
**A:** 检查：
- BulletTracer Prefab 的 LineRenderer 材质是否可见
- 是否正确拖到 WeaponSystem 的字段
- 射程是否足够（Gun Range）

### Q5: 游戏太卡
**A:** 优化：
- 减少初始波次敌人数量
- 启用对象池（已实现）
- 减少粒子数量

---

## 下一步改进方向

完成白模测试后，可以：

1. **添加音效**
   - 导入免费音效包
   - 配置 AudioSource

2. **添加粒子效果**
   - 使用 Unity Particle System
   - 替换白模特效

3. **导入3D模型**
   - 替换 Cube 为真实飞船模型
   - 添加材质和贴图

4. **扩展武器系统**
   - 添加导弹
   - 添加激光武器

5. **UI美化**
   - 科幻风格UI素材
   - 添加动画效果

---

## 脚本引用总览

以下是所有新增的脚本文件：

```
Assets/Scripts/
├── WeaponSystem.cs           # 武器射击系统
├── HealthSystem.cs           # 生命值和伤害系统
├── EnemyFighterAI.cs         # 敌人AI
├── EnemySpawnManager.cs      # 波次生成管理器
├── CombatHUD.cs              # 战斗HUD
├── ObjectPool.cs             # 对象池系统
├── PlayerShipController.cs   # (已存在) 飞船控制
├── ChaseCamera.cs            # (已存在) 摄像机跟随
├── OrbitMotion.cs            # (已存在) 轨道运动
└── CelestialBodyAppearance.cs # (已存在) 天体外观
```

---

## 联系和反馈

如果遇到问题：
1. 检查 Unity Console 的错误信息
2. 确认所有引用都已正确连接
3. 对比本文档的设置步骤
4. 检查组件的参数设置

祝你测试顺利！🚀
