# 🔫 武器系统无法射击 - 故障排查指南

## 快速解决方案

### 方案1：使用备用射击键（最简单）

如果鼠标左键不工作，尝试以下按键：

```
✅ F键 - 射击（新增的备用键）
✅ 左Ctrl键 - 射击
✅ 鼠标左键 - 射击（如果鼠标工作）
```

### 方案2：启用调试模式（推荐）

1. 在Unity中选择 `PlayerShip`
2. 找到 `Weapon System` 组件
3. 勾选 `Show Debug Info`
4. 点击 Play，按射击键查看Console输出

如果看到 "Fire: F Key" 或 "Firing! Ammo: 100/100"，说明武器系统正常。

### 方案3：添加输入调试器

1. 选择 `PlayerShip`
2. 添加组件：`Input Debugger`
3. 勾选 `Show On Screen`
4. 点击 Play，左侧会显示所有输入状态

---

## 常见问题诊断

### 问题1: 没有WeaponSystem组件

**症状**: 按任何键都没反应

**解决**:
```
1. 选中玩家飞船
2. Add Component > Weapon System
3. 配置参数（参考SETUP_GUIDE.md）
```

### 问题2: 没有FirePoints

**症状**: Console显示 "Firing!" 但没有子弹

**解决**:
```
1. 选中玩家飞船
2. 创建子对象 GameObject > Create Empty
3. 命名为 FirePoint_Left，位置 (-1, 0, 1.5)
4. 创建 FirePoint_Right，位置 (1, 0, 1.5)
5. 将两个发射点拖到 Weapon System > Fire Points 数组
```

### 问题3: 鼠标被锁定到飞行控制

**症状**: 鼠标移动改变飞行方向，但左键无法射击

**原因**: `PlayerShipController.useMouseLook = true` 占用了鼠标

**解决方案A（推荐）**: 使用键盘射击
```
按 F键 或 左Ctrl 射击
不影响鼠标飞行控制
```

**解决方案B**: 关闭鼠标飞行
```
1. 选中玩家飞船
2. Player Ship Controller > Use Mouse Look 取消勾选
3. 现在可以用鼠标左键射击
4. 使用 WASD 控制飞行方向
```

### 问题4: 输入系统未启用

**症状**: Console显示 "Mouse.current is null" 或 "Keyboard.current is null"

**解决**:
```
1. Edit > Project Settings > Player
2. Active Input Handling: 改为 "Input System Package (New)"
   或 "Both"
3. 重启Unity编辑器
```

### 问题5: 子弹轨迹看不见

**症状**: 能射击（弹药减少）但看不到子弹

**解决**:
```
1. 检查是否分配了特效Prefab：
   Weapon System > Bullet Tracer Prefab

2. 如果没有，运行：
   Tools > Combat Game > Create Simple Effects

3. 手动分配：
   Assets/Prefabs/BulletTracer 拖到 Bullet Tracer Prefab
```

### 问题6: 弹药耗尽

**症状**: 之前能射击，现在不行了

**解决**:
```
按 R键 装填
等待2秒后继续射击
```

---

## 完整诊断流程

### 步骤1: 检查输入设备

```csharp
// 在PlayerShip上添加 InputDebugger 组件
// 查看左侧显示的输入状态
```

**预期结果**:
- Keyboard: Available ✅
- Mouse: Available ✅

**如果显示 "NOT FOUND"**:
- 检查 Project Settings > Player > Active Input Handling
- 确保是 "Input System Package (New)" 或 "Both"

### 步骤2: 检查按键响应

按住 **F键** 或 **左Ctrl**，观察：

**InputDebugger应该显示**:
```
F: True
```
或
```
L-Ctrl: True
```

**如果按键不响应**:
- Unity可能失去焦点，点击Game窗口
- 检查是否在Editor模式（不是Build模式）

### 步骤3: 检查武器系统

在 WeaponSystem 组件中启用 `Show Debug Info`

按射击键，Console应该显示：
```
Fire: F Key
Firing! Ammo: 99/100
```

**如果没有输出**:
- WeaponSystem组件未正确添加
- 脚本编译错误（查看Console）

### 步骤4: 检查子弹发射

如果能看到 "Firing!" 但没有子弹：

1. **检查发射点**:
   ```
   Weapon System > Fire Points
   Size: 至少1
   Element 0: FirePoint_Left (或任何Transform)
   ```

2. **检查特效Prefab**:
   ```
   Bullet Tracer Prefab: [已分配]
   Muzzle Flash Prefab: [已分配]
   ```

3. **在Scene视图查看**:
   - 选中玩家飞船
   - 按射击键
   - 应该能看到黄色的 Debug.DrawRay 线条

---

## 验证检查清单

运行游戏前确认：

- [ ] PlayerShip 有 `WeaponSystem` 组件
- [ ] PlayerShip 有 `HealthSystem` 组件
- [ ] PlayerShip 有 `Rigidbody` 组件
- [ ] WeaponSystem 的 `Fire Points` 已分配（至少1个）
- [ ] 特效Prefab已创建（运行 Tools > Combat Game > Create Simple Effects）
- [ ] Input System 已启用（Project Settings）
- [ ] 场景中有摄像机

运行游戏时测试：

- [ ] 按 **F键** 能射击
- [ ] 按 **左Ctrl** 能射击
- [ ] 弹药显示在HUD（如果有HUD）
- [ ] 能看到黄色子弹轨迹
- [ ] Console无报错

---

## 最简单的测试方法

如果一切都不确定，从头开始：

### 1分钟快速测试

```bash
1. Unity菜单：Tools > Combat Game > Complete Setup
2. 保存EnemyFighter为Prefab
3. 删除场景中的敌人实例
4. EnemySpawnManager分配敌人Prefab
5. 点击Play
6. 等待5秒（敌人生成）
7. 按住 F键 射击
```

如果这样还不行，查看Console的错误信息。

---

## 控制方式速查

### 飞行控制
- W/S - 前进/后退
- A/D - 左右平移
- Q/E - 翻滚
- 空格 - 上升
- Shift+空格 - 下降
- 鼠标 - 视角（如果启用）

### 战斗控制
- **F键** - 射击（主要）✅
- **左Ctrl** - 射击（备用）✅
- **鼠标左键** - 射击（如果鼠标未用于飞行）
- **R键** - 手动装填

---

## 技术细节

### 输入检测顺序

WeaponSystem按以下顺序检测输入：

```csharp
1. Mouse.current.leftButton.isPressed
2. Keyboard.current.leftCtrlKey.isPressed
3. Keyboard.current.fKey.isPressed  // 新增
```

只要任何一个为true，就会触发射击。

### 为什么F键是最佳选择？

- ✅ 不与WASD移动键冲突
- ✅ 右手食指容易触及
- ✅ 不干扰鼠标飞行控制
- ✅ 类似FPS游戏的"使用"键，直觉

### 射速限制

默认射速：每0.1秒一发（600发/分）

如果按住射击键没有连发：
```
检查 Gun Fire Rate 参数
推荐值：0.1 (快速) 到 0.3 (中速)
```

---

## 还是不行？

### 终极调试方法

1. 在 WeaponSystem.cs 的 `Fire()` 方法开头添加：
   ```csharp
   Debug.LogError("WEAPON FIRED!");
   ```

2. 点击Play，按F键

3. 如果看到红色错误"WEAPON FIRED!"，说明：
   - ✅ 输入系统正常
   - ✅ WeaponSystem正常
   - ❌ 问题在子弹生成或显示

4. 如果没有看到任何输出：
   - ❌ 输入系统有问题
   - 添加 InputDebugger 组件继续诊断

---

## 获取帮助

如果以上都不能解决：

1. 截图 Console 的错误信息
2. 截图 WeaponSystem Inspector 设置
3. 确认 Unity 版本（应该是 6.x）
4. 检查 InputDebugger 的显示内容

---

**更新时间**: 2026-01-14
**版本**: v1.0.2
**适用于**: Unity 6.0+ with Input System
