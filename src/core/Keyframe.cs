using System;

namespace SimplyRemadeMI.core;

/// <summary>
/// Easing mode for keyframe interpolation
/// </summary>
public enum EasingMode
{
    Linear,
    EaseInQuadratic,
    EaseOutQuadratic,
    EaseInOutQuadratic,
    Instant
}

/// <summary>
/// Represents a single keyframe with a value and easing mode to the next keyframe
/// </summary>
public class Keyframe
{
    /// <summary>
    /// The value at this keyframe
    /// </summary>
    public float Value { get; set; }
    
    /// <summary>
    /// The easing mode to use when interpolating TO the next keyframe
    /// </summary>
    public EasingMode EasingMode { get; set; }
    
    public Keyframe(float value, EasingMode easingMode = EasingMode.Linear)
    {
        Value = value;
        EasingMode = easingMode;
    }
    
    /// <summary>
    /// Creates a copy of this keyframe
    /// </summary>
    public Keyframe Clone()
    {
        return new Keyframe(Value, EasingMode);
    }
}

/// <summary>
/// Utility class for easing functions
/// </summary>
public static class EasingFunctions
{
    /// <summary>
    /// Apply easing function to interpolation parameter t (0 to 1)
    /// </summary>
    /// <param name="t">Interpolation parameter (0 to 1)</param>
    /// <param name="mode">Easing mode to apply</param>
    /// <returns>Eased interpolation parameter</returns>
    public static float ApplyEasing(float t, EasingMode mode)
    {
        t = Math.Clamp(t, 0f, 1f);
        
        return mode switch
        {
            EasingMode.Linear => t,
            EasingMode.EaseInQuadratic => EaseInQuad(t),
            EasingMode.EaseOutQuadratic => EaseOutQuad(t),
            EasingMode.EaseInOutQuadratic => EaseInOutQuad(t),
            EasingMode.Instant => t < 1f ? 0f : 1f,
            _ => t
        };
    }
    
    /// <summary>
    /// Quadratic ease in: t^2
    /// </summary>
    private static float EaseInQuad(float t)
    {
        return t * t;
    }
    
    /// <summary>
    /// Quadratic ease out: 1 - (1-t)^2
    /// </summary>
    private static float EaseOutQuad(float t)
    {
        return 1f - (1f - t) * (1f - t);
    }
    
    /// <summary>
    /// Quadratic ease in-out: combination of ease in and ease out
    /// </summary>
    private static float EaseInOutQuad(float t)
    {
        if (t < 0.5f)
        {
            return 2f * t * t;
        }
        else
        {
            float f = t - 1f;
            return 1f - 2f * f * f;
        }
    }
    
    /// <summary>
    /// Get display name for easing mode
    /// </summary>
    public static string GetEasingModeName(EasingMode mode)
    {
        return mode switch
        {
            EasingMode.Linear => "Linear",
            EasingMode.EaseInQuadratic => "Ease In (Quadratic)",
            EasingMode.EaseOutQuadratic => "Ease Out (Quadratic)",
            EasingMode.EaseInOutQuadratic => "Ease In And Out (Quadratic)",
            EasingMode.Instant => "Instant",
            _ => "Linear"
        };
    }
    
    /// <summary>
    /// Get all available easing modes
    /// </summary>
    public static EasingMode[] GetAllEasingModes()
    {
        return new[]
        {
            EasingMode.Linear,
            EasingMode.EaseInQuadratic,
            EasingMode.EaseOutQuadratic,
            EasingMode.EaseInOutQuadratic,
            EasingMode.Instant
        };
    }
}