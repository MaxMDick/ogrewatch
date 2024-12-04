using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyPlayerStatus : MonoBehaviour
{
    public bool stunned;
    public bool rooted;

    public List<SlowEffect> activeSlows = new List<SlowEffect>();

    private float stunDuration;
    private float rootDuration;
    private float stunTimer;
    private float rootTimer;

    void Update()
    {
        UpdateTimers();
        CleanExpiredSlows();
    }

    private void UpdateTimers()
    {
        // Update timers for stun and root
        if (stunned)
        {
            stunTimer += Time.deltaTime;
            if (stunTimer >= stunDuration)
            {
                stunned = false;
                stunTimer = 0f;
            }
        }

        if (rooted)
        {
            rootTimer += Time.deltaTime;
            if (rootTimer >= rootDuration)
            {
                rooted = false;
                rootTimer = 0f;
            }
        }
    }

    private void CleanExpiredSlows()
    {
        // Remove expired slows
        activeSlows.RemoveAll(slow => slow.IsExpired());
    }

    public void ApplyStun(float duration)
    {
        stunned = true;
        stunDuration = duration;
        stunTimer = 0f;
    }

    public void ApplyRoot(float duration)
    {
        rooted = true;
        rootDuration = duration;
        rootTimer = 0f;
    }

    public void ApplySlow(float duration, float multiplier)
    {
        activeSlows.Add(new SlowEffect(duration, multiplier));
    }

    public bool IsStunned() => stunned;
    public bool IsRooted() => rooted;

    public bool IsSlowed() => activeSlows.Count > 0;

    public float GetEffectiveSlowMultiplier()
    {
        if (!IsSlowed()) return 1f; // No slow effect, so return normal speed

        // Find the strongest (lowest) slow multiplier
        float effectiveMultiplier = 1f;
        foreach (var slow in activeSlows)
        {
            effectiveMultiplier = Mathf.Min(effectiveMultiplier, slow.multiplier);
        }
        return effectiveMultiplier;
    }

    // Nested class to represent each individual slow effect
    [Serializable]
    public class SlowEffect
    {
        public float duration;
        public float multiplier;
        private float timer;

        public SlowEffect(float duration, float multiplier)
        {
            this.duration = duration;
            this.multiplier = multiplier;
            timer = 0f;
        }

        public void UpdateTimer(float deltaTime)
        {
            timer += deltaTime;
        }

        public bool IsExpired()
        {
            timer += Time.deltaTime;
            return timer >= duration;
        }
    }
}
