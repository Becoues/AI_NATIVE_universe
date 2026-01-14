# 🚀 太阳系空战游戏 - AI NATIVE Universe

一个基于 Unity URP 的太空战斗机游戏原型，具备完整的战斗系统、敌人AI和波次管理。

## ✨ 当前功能

### 核心战斗系统
- ✅ **武器系统** - 机炮射击，Raycast 伤害判定，弹药管理
- ✅ **生命值系统** - 护盾 + 船体双层防护，自动恢复，死亡/重生
- ✅ **敌人AI** - 巡逻、追击、攻击、规避四状态机
- ✅ **波次系统** - 自动生成敌人，难度递增，波次管理
- ✅ **战斗HUD** - 血量、护盾、弹药、准星、波次信息显示
- ✅ **对象池** - 特效和子弹复用，优化性能

### 飞行系统（已有）
- ✅ **6自由度飞行** - 完整的太空飞行控制
- ✅ **摄像机跟随** - 平滑的第三人称视角
- ✅ **太阳系环境** - 行星轨道运动系统

## 🎮 快速开始

### 方法1：自动化设置（3分钟）

1. 打开 Unity 项目
2. 菜单栏：**`Tools > Combat Game > Complete Setup`**
3. 保存 `EnemyFighter` 为 Prefab
4. 在 `EnemySpawnManager` 中分配敌人 Prefab
5. 点击 Play 开始测试！

### 方法2：手动设置

详见 **[完整设置指南](SETUP_GUIDE.md)**

## 📖 文档

| 文档 | 描述 |
|------|------|
| [QUICK_REFERENCE.md](QUICK_REFERENCE.md) | 快速参考卡 - 控制方式、参数调整、问题排查 |
| [SETUP_GUIDE.md](SETUP_GUIDE.md) | 详细设置指南 - 逐步配置教程 |
| README.md | 本文档 - 项目概览 |

## 🎯 控制方式

| 操作 | 按键 |
|------|------|
| 前进/后退 | W / S |
| 左右平移 | A / D |
| 上升 | 空格 |
| 下降 | Shift + 空格 |
| 翻滚 | Q / E |
| 视角 | 鼠标 |
| **射击** | **鼠标左键** 或 **左Ctrl** |
| **装填** | **R** |

## 🏗️ 系统架构

```
战斗系统
├── WeaponSystem.cs           # 武器射击系统
├── HealthSystem.cs           # 生命值和护盾
├── EnemyFighterAI.cs         # 敌人AI行为
├── EnemySpawnManager.cs      # 波次生成管理
├── CombatHUD.cs              # 战斗界面
└── ObjectPool.cs             # 对象池

飞行系统（已有）
├── PlayerShipController.cs   # 玩家飞船控制
├── ChaseCamera.cs            # 摄像机跟随
└── OrbitMotion.cs            # 行星轨道

辅助工具
└── Editor/
    └── CombatGameQuickSetup.cs  # 编辑器快速设置工具
```

## 🔧 技术栈

- **Unity**: 2022.3+ (URP)
- **渲染管线**: Universal Render Pipeline (URP) 17.3.0
- **输入系统**: Unity Input System 1.17.0
- **物理引擎**: Unity Physics (Rigidbody)
- **UI**: TextMeshPro + Unity UI

## 🎨 当前状态

### ✅ 已完成
- [x] 核心战斗循环（射击 → 击中 → 死亡）
- [x] 敌人AI行为（追击、攻击、规避）
- [x] 波次系统（自动生成、难度递增）
- [x] 基础HUD（血量、弹药、波次）
- [x] 白模测试对象
- [x] 自动化设置工具
- [x] 完整文档

### 🚧 待优化
- [ ] 音效系统（射击音、爆炸音、BGM）
- [ ] 粒子特效（Unity Particle System）
- [ ] 3D模型替换（飞船、武器）
- [ ] 导弹武器系统
- [ ] 多种敌人类型（重型、BOSS）
- [ ] 主菜单和暂停界面

### 🎯 未来计划
- [ ] 关卡设计和任务系统
- [ ] 武器升级系统
- [ ] 更多特效和后处理
- [ ] 移动平台适配
- [ ] 多人联机（可选）

## 📦 项目结构

```
AI_NATIVE_universe/
├── Assets/
│   ├── Scripts/
│   │   ├── WeaponSystem.cs
│   │   ├── HealthSystem.cs
│   │   ├── EnemyFighterAI.cs
│   │   ├── EnemySpawnManager.cs
│   │   ├── CombatHUD.cs
│   │   ├── ObjectPool.cs
│   │   ├── PlayerShipController.cs
│   │   ├── ChaseCamera.cs
│   │   ├── OrbitMotion.cs
│   │   ├── CelestialBodyAppearance.cs
│   │   └── Editor/
│   │       └── CombatGameQuickSetup.cs
│   │
│   ├── Prefabs/          # 特效和敌人预制体
│   ├── Scenes/           # 游戏场景
│   └── Settings/         # URP渲染设置
│
├── SETUP_GUIDE.md        # 详细设置指南
├── QUICK_REFERENCE.md    # 快速参考卡
└── README.md             # 本文档
```

## 🐛 常见问题

### Q: 射击没反应？
**A:** 检查 WeaponSystem 的 `Fire Points` 是否分配

### Q: 敌人不攻击？
**A:** 检查 `Detection Range` 是否够大（推荐500）

### Q: HUD 不显示？
**A:** 确认 CombatHUD 的所有引用都已连接

### Q: 游戏太卡？
**A:** 减少初始敌人数量，或降低特效质量

更多问题查看 [QUICK_REFERENCE.md](QUICK_REFERENCE.md)

## 🎨 素材需求

当前使用白模测试，后续优化需要：

### 3D模型
- 玩家飞船模型 (.fbx, 3K-5K 面)
- 敌人战机模型 x3 (.fbx)
- 导弹/武器模型

### 材质贴图
- 飞船贴图 (Albedo, Normal, Metallic, Emission)
- 2048x2048 分辨率

### 音效
- 机炮射击音 (.wav)
- 爆炸音效 x3 (.wav)
- 引擎音（循环）(.ogg)
- 战斗BGM (.ogg)

### UI素材
- 科幻字体
- UI图标集
- 准星图片

详见 [SETUP_GUIDE.md](SETUP_GUIDE.md) 的素材清单部分

## 📊 性能优化

当前实现已做优化：
- ✅ Raycast 射击（无物理子弹）
- ✅ 对象池复用特效
- ✅ 轻量级AI状态机
- ✅ 条件更新（仅需要时运行）

如遇性能问题：
1. 减少同时存在的敌人数量
2. 降低粒子发射率
3. 使用简单碰撞体
4. 调整摄像机渲染距离

## 🤝 贡献

这是一个原型项目，欢迎改进和扩展！

建议改进方向：
- 更丰富的敌人AI行为
- 更多武器类型和特效
- 关卡设计和剧情
- UI/UX优化
- 音效和配乐

## 📜 版本历史

### v1.0.0 (2026-01-14)
- ✨ 初始版本
- ✅ 完整战斗系统原型
- ✅ 白模测试通过
- ✅ 自动化设置工具
- ✅ 完整文档

## 📄 许可

本项目为教育和原型开发目的创建。

## 🎓 学习资源

- [Unity 官方文档](https://docs.unity3d.com/)
- [URP 渲染管线指南](https://docs.unity3d.com/Packages/com.unity.render-pipelines.universal@latest)
- [Input System 教程](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest)

---

**开发提示**:
1. 先用白模测试玩法手感
2. 确认战斗循环好玩后再加美术
3. 音效比视觉更重要！
4. 频繁测试和调整参数

祝开发顺利！🚀✨
