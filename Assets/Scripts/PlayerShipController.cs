using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class PlayerShipController : MonoBehaviour
{
    [Header("Thrust")]
    [SerializeField] private float forwardAcceleration = 30f;
    [SerializeField] private float strafeAcceleration = 20f;
    [SerializeField] private float verticalAcceleration = 20f;
    [SerializeField] private float boostMultiplier = 2.5f;
    [SerializeField] private float maxSpeed = 60f;
    [SerializeField] private float drag = 0.2f;

    [Header("Rotation")]
    [SerializeField] private float yawSpeed = 90f;
    [SerializeField] private float pitchSpeed = 90f;
    [SerializeField] private float rollSpeed = 110f;

    [Header("Camera Input")]
    [SerializeField] private float mouseSensitivity = 2.5f;
    [SerializeField] private bool useMouseLook = true;

    [Header("Auto-align")]
    [SerializeField] private bool alignToVelocity = true;
    [SerializeField] private float alignLerp = 6f;

    private Rigidbody body;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
        body.linearDamping = drag;
        body.angularDamping = 2f;
        body.interpolation = RigidbodyInterpolation.Interpolate;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        bool shiftHeld = IsKeyPressed(Key.LeftShift) || IsKeyPressed(Key.RightShift);
        bool spaceHeld = IsKeyPressed(Key.Space);
        float boost = shiftHeld && !spaceHeld ? boostMultiplier : 1f;
        float forwardInput = ReadAxis(Key.W, Key.S);
        float strafeInput = ReadAxis(Key.D, Key.A);

        float ascend = spaceHeld && !shiftHeld ? 1f : 0f;          // Space = up
        float descend = spaceHeld && shiftHeld ? 1f : 0f;          // Shift + Space = down
        float verticalInput = ascend - descend;

        Vector3 thrust = new Vector3(
            strafeInput * strafeAcceleration,
            verticalInput * verticalAcceleration,
            forwardInput * forwardAcceleration
        );

        body.AddRelativeForce(thrust * boost, ForceMode.Acceleration);

        if (body.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            body.linearVelocity = body.linearVelocity.normalized * maxSpeed;
        }
    }

    private void HandleRotation()
    {
        float yawInput = 0f;
        float pitchInput = 0f;

        if (useMouseLook)
        {
            var mouse = Mouse.current;
            if (mouse != null)
            {
                Vector2 delta = mouse.delta.ReadValue() * mouseSensitivity;
                yawInput = delta.x;
                pitchInput = -delta.y;
            }
        }

        if (!useMouseLook)
        {
            yawInput = ReadAxis(Key.D, Key.A);
            pitchInput = ReadAxis(Key.W, Key.S);
        }

        float rollInput = 0f;
        if (IsKeyPressed(Key.Q)) rollInput += 1f;
        if (IsKeyPressed(Key.E)) rollInput -= 1f;

        Vector3 localAngles = new Vector3(
            pitchInput * pitchSpeed,
            yawInput * yawSpeed,
            rollInput * rollSpeed
        );

        Quaternion desired = body.rotation * Quaternion.Euler(localAngles * Time.fixedDeltaTime);

        if (alignToVelocity)
        {
            Vector3 v = body.linearVelocity;
            if (v.sqrMagnitude > 0.1f)
            {
                Quaternion target = Quaternion.LookRotation(v.normalized, transform.up);
                desired = Quaternion.Slerp(desired, target, alignLerp * Time.fixedDeltaTime);
            }
        }

        body.MoveRotation(desired);
    }

    private static bool IsKeyPressed(Key key)
    {
        var kb = Keyboard.current;
        return kb != null && kb[key].isPressed;
    }

    private static float ReadAxis(Key positive, Key negative)
    {
        float value = 0f;
        if (IsKeyPressed(positive)) value += 1f;
        if (IsKeyPressed(negative)) value -= 1f;
        return value;
    }
}
