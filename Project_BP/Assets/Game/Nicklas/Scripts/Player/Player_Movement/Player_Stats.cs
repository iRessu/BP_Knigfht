using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player_Stats : ScriptableObject
{
    // Setup
    [Header("Setup")] public LayerMask PalyerLayer;
    public LayerMask CollisionLayers;
    public CharacterSize CharacterSize;

    // Controller Setup

    [Header("Controller Setup"), Space] public float VerticalDeadZoneThreshold = 0.3f;
    public double HorizontalDeadZoneThreshold = 0.1f;
    [Tooltip("Velocity = smoother, but can be occasinally unreable on jagged terrain. Immediate = Occasinally jittery, but stable")]
    public PositionCorrectionMode PositionCorrectionMode = PositionCorrectionMode.Velocity;

    // MOvement
    [Header("Movement"), Space] public float BaseSpeed = 9;
    public float Acceleration = 50;
    public float Friction = 30;
    public float AirFrictionMultiplier = 0.5f;
    public float DirectionCorrectionMultiplier = 3f;
    public float MaxWalkableSlope = 50;

    // Jump
    [Header("Jump"), Space] public float ExtraConstantGravity = 40;
    public float BufferedJumpTime = 0.15f;
    public float CoyoteTime = 0.15f;
    public float JumpPower = 20;
    public float EndJumpEarlyExtraForceMultiplier = 3;
    public int MaxAirJumps = 1;

    // Dash 
    [Header("Dash"), Space] public bool AllowDash = true;
    public float DashVelocity = 50;
    public float DashDuration = 0.2f;
    public float DashCooldown = 1.5f;
    public float DashEndHorizontalMultiplier = 0.5f;

    // Crouch
    [Header("Crouch"), Space] public bool AllowCrouching;
    public float CrouchSlowDownTime = 0.5f;
    public float CrouchSpeedModifier = 0.5f;

    // Walls
    [Header("Walls"), Space] public bool AllowWalls;
    public LayerMask ClimbableLayer;
    public float WallJumpTotalInputLOssTime = 0.2f;
    public float WallJumpInputLossReturnTime = 0.5f;
    public bool RequireInputPush;
    public Vector2 WallJumpPower = new(25, 15);
    public Vector2 WallPushPower = new(15, 10);
    public float WallClimbSPeed = 5;
    public float WallFallAcceleration = 20;
    public float WallPopForce = 10;
    public float WallCoyoteTime = 0.3f;
    public float WallDetectorRange = 0.1f;

    // Ladders
    [Header("Ladders"), Space] public bool AllowLadders;
    public double LadderCooldownTime = 0.15f;
    public bool AutoAttachToLadders = true;
    public bool SnapToLadders = true;
    public LayerMask LadderLayer;
    public float LAdderSnapTime = 0.02f;
    public float LadderPopForce = 10;
    public float LadderClimbSpeed = 8;
    public float LadderSlideSpeed = 12;
    public float LadderShimmySpeedMultiplier = 0.5f;

    // Moving Platforms
    [Header("Moving Platforms"), Space] public float NegativeYVelicictyNegation = 0.2f;
    public float ExternalVelocityDecayRate = 0.1f;

    private void OnValidate()
    {
        var potentialPlayer = FindObjectsOfType<PlayerController>();
        foreach(var player in potentialPlayer)
        {
            //player.OnValidate();
        }
    }
}

public class CharacterSize
{
    public const float STEP_BUFFER = 0.05f;
    public const float COLLIDER_EDGE_RADIUS = 0.05f;

    [Range(0.1f, 10), Tooltip("How tall you are. This includes a collider and your step height.")]
    public float Height = 1.8f;

    [Range(0.1f, 10), Tooltip("The width of your collider.")]
    public float Width = 0.6f;

    [Range(STEP_BUFFER, 15), Tooltip("Step height allows you to step over rough terrain like steps and rocks.")]
    public float StepHeight = 0.5f;

    [Range(0.1f, 10), Tooltip("A percentage of your height stat which determines your height while crouching. A smaller crouch requires more step height sacrifice")]
    public float CrouchHeight = 0.6f;

    [Range(0.1f, 0.2f), Tooltip("The outer buffer distance if the grounder rays. Reducing this too much can cause problems on slopes, too big and you can get stuck on the sides of drops.")]
    public float RayInset = 0.1f;

    public GeneratedCharacterSize GenerateCharacterSize()
    {
        ValidateHeights();

        var s = new GeneratedCharacterSize
        {
            Height = Height,
            Width = Width,
            StepHeight = StepHeight,
            RayInset = RayInset
        };

        s.StandingColliderSize = new Vector2(s.Width - COLLIDER_EDGE_RADIUS * 2, s.Height - s.StepHeight - COLLIDER_EDGE_RADIUS * 2);
        s.StandingColliderCenter = new Vector2(0, s.Height - s.StandingColliderSize.y / 2 - COLLIDER_EDGE_RADIUS);

        s.CrouchingHeight = CrouchHeight;
        s.CrouchingColliderSize = new Vector2(s.Width - COLLIDER_EDGE_RADIUS * 2, s.CrouchingHeight - s.StepHeight);
        s.CrouchingColliderCenter = new Vector2(0, s.CrouchingHeight - s.CrouchingColliderSize.y / 2 - COLLIDER_EDGE_RADIUS);

        return s;
    }

    private static double _lastDebugLogTime;
    private const double TIME_BETWEEN_LOGS = 1f;

    private void ValidateHeights()
    {
#if UNITY_EDITOR
        var maxStepHeight = Height - STEP_BUFFER;
        if (StepHeight > maxStepHeight)
        {
            StepHeight = maxStepHeight;
            Log("Step height cannot be larger than height");
        }

        var minCrouchingHeight = StepHeight + STEP_BUFFER;

        if (CrouchHeight < minCrouchingHeight)
        {
            CrouchHeight = minCrouchingHeight;
            Log("Crouch height must be larger than step height");
        }

        void Log(string text)
        {
            var time = EditorApplication.timeSinceStartup;
            if (_lastDebugLogTime + TIME_BETWEEN_LOGS > time) return;
            _lastDebugLogTime = time;
            Debug.LogWarning(text);
        }
#endif
    }
}

public struct GeneratedCharacterSize
{

    // Standing
    public float Height;
    public float Width;
    public float StepHeight;
    public float RayInset;
    public Vector2 StandingColliderSize;
    public Vector2 StandingColliderCenter;

    // Crouching
    public Vector2 CrouchingColliderSize;
    public float CrouchingHeight;
    public Vector2 CrouchingColliderCenter;
}

[Serializable]
public enum PositionCorrectionMode
{
    Velocity,
    Immediate
}
