using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 输入调试器 - 帮助检测输入问题
/// 在玩家飞船上添加此组件来查看输入状态
/// </summary>
public class InputDebugger : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private bool showOnScreen = true;
    [SerializeField] private bool logToConsole = false;

    private string debugText = "";

    private void Update()
    {
        if (!showOnScreen && !logToConsole)
            return;

        // 检查输入设备状态
        debugText = "=== Input Debug ===\n";

        // 键盘状态
        if (Keyboard.current != null)
        {
            debugText += "Keyboard: Available\n";
            debugText += $"  W: {Keyboard.current.wKey.isPressed}\n";
            debugText += $"  A: {Keyboard.current.aKey.isPressed}\n";
            debugText += $"  S: {Keyboard.current.sKey.isPressed}\n";
            debugText += $"  D: {Keyboard.current.dKey.isPressed}\n";
            debugText += $"  F: {Keyboard.current.fKey.isPressed}\n";
            debugText += $"  L-Ctrl: {Keyboard.current.leftCtrlKey.isPressed}\n";
            debugText += $"  Space: {Keyboard.current.spaceKey.isPressed}\n";
            debugText += $"  Shift: {Keyboard.current.leftShiftKey.isPressed}\n";
        }
        else
        {
            debugText += "Keyboard: NOT FOUND!\n";
        }

        debugText += "\n";

        // 鼠标状态
        if (Mouse.current != null)
        {
            debugText += "Mouse: Available\n";
            debugText += $"  Left Button: {Mouse.current.leftButton.isPressed}\n";
            debugText += $"  Right Button: {Mouse.current.rightButton.isPressed}\n";
            debugText += $"  Position: {Mouse.current.position.ReadValue()}\n";
            debugText += $"  Delta: {Mouse.current.delta.ReadValue()}\n";
        }
        else
        {
            debugText += "Mouse: NOT FOUND!\n";
        }

        debugText += "\n";

        // 组件状态
        WeaponSystem weapon = GetComponent<WeaponSystem>();
        if (weapon != null)
        {
            debugText += "Weapon System:\n";
            debugText += $"  Ammo: {weapon.CurrentAmmo}/{weapon.MaxAmmo}\n";
            debugText += $"  Reloading: {weapon.IsReloading}\n";
        }

        HealthSystem health = GetComponent<HealthSystem>();
        if (health != null)
        {
            debugText += "\nHealth System:\n";
            debugText += $"  HP: {health.CurrentHealth:F0}/{health.MaxHealth}\n";
            debugText += $"  Shield: {health.CurrentShield:F0}/{health.MaxShield}\n";
        }

        if (logToConsole && Time.frameCount % 60 == 0) // 每秒打印一次
        {
            Debug.Log(debugText);
        }
    }

    private void OnGUI()
    {
        if (!showOnScreen)
            return;

        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.white;
        style.normal.background = MakeBackgroundTexture();

        GUI.Label(new Rect(10, 200, 300, 400), debugText, style);
    }

    private Texture2D MakeBackgroundTexture()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, new Color(0, 0, 0, 0.7f));
        texture.Apply();
        return texture;
    }
}
