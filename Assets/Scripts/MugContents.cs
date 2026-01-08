using UnityEngine;
using System;
using System.Diagnostics;

public class MugContent : MonoBehaviour
{
    public event Action<LiquidType, float> OnContentChanged;

    [SerializeField] private LiquidComposition composition = new LiquidComposition();
    [SerializeField] private LiquidType liquid = LiquidType.None;
    [SerializeField, Range(0f, 1f)] private float fill = 0f;
    private double lastWriteTime = 0.0;

    [Header("Ice Visual (mug-owned)")]
    [SerializeField] private GameObject iceVisualPrefab;         // assign prefab in inspector (optional)
    [SerializeField] private GameObject iceVisualInstance;
    public bool HasIce { get; private set; } = false;
    
    public LiquidType ResolvedType => liquid;
    public float Fill => fill;
    public bool IsEmpty => liquid == LiquidType.None || Mathf.Approximately(fill, 0f);

    private const float maxVolume = 1f; // treat 1.0 as 100% full in these units

    
    private void NotifyChanged()
    {
        OnContentChanged?.Invoke(liquid, fill);
    }

    // Backwards-compatible setter: replaces composition with a single-component amount
    public void SetContent(LiquidType newLiquid, float newFill)
    {
        newFill = Mathf.Clamp01(newFill);
        double now = Time.realtimeSinceStartupAsDouble;
        float newVolume = newFill * maxVolume;
        UnityEngine.Debug.Log($"MugContent.SetContent on {gameObject.name}: newLiquid={newLiquid} newFill={newFill:0.00}");

        if (composition != null && composition.GetDistinctTypes().Length == 1
            && composition.GetDistinctTypes()[0] == newLiquid
            && Mathf.Approximately(fill, newFill)) return;
        composition.SetSingle(newLiquid, newVolume);
        ResolveAndNotify(now);
    }

    // Add volume of a particular liquid type. volume is in same units as maxVolume
    public void AddVolume(LiquidType addType, float volume)
    {
        if (addType == LiquidType.None || volume <= 0f || volume <= Mathf.Epsilon) return;

        // Ensure composition exists
        if (composition == null) composition = new LiquidComposition();

        // If adding would overflow, clamp the incoming volume so total == maxVolume,
        // instead of scaling down all components every time (more predictable).
        float currentTotal = composition.TotalAmount();
        float spaceLeft = Mathf.Clamp(maxVolume - currentTotal, 0f, maxVolume);
        float accepted = Mathf.Min(spaceLeft, volume);

        if (accepted > 0f)
        {
            composition.Add(addType, accepted);
        }
        else
        {
        }
        //Resolve and notify
        double now = Time.realtimeSinceStartupAsDouble;
        ResolveAndNotify(now);
    }

    // Public reset kept as no-op by your design, provide a forced reset overload
    public void ResetContent(string reason = "UHmm", bool force = false)
    {
        if (!force)
        {
            // no-op to preserve content as you implemented
            return;
        }

        composition.Clear();
        liquid = LiquidType.None;
        fill = 0f;
        lastWriteTime = Time.realtimeSinceStartupAsDouble;
        NotifyChanged();
    }

    public void SetIcePrefabFromDispenser(GameObject prefab)
    {
        if (prefab == null) return;
        // only set if mug has no prefab configured yet
        if (iceVisualPrefab == null)
            iceVisualPrefab = prefab;
    }
    public void SetHasIce(bool hasIce)
    {
        HasIce = hasIce;
        UnityEngine.Debug.Log($"SetHasIce: {name} -> {HasIce}");
        if (HasIce)
        {
            if (iceVisualInstance == null && iceVisualPrefab != null)
            {
                iceVisualInstance = Instantiate(iceVisualPrefab, transform);
                iceVisualInstance.transform.localPosition = Vector3.zero;
                iceVisualInstance.transform.localRotation = Quaternion.identity;
                iceVisualInstance.transform.localScale = Vector3.one;
                UnityEngine.Debug.Log($"SetHasIce: instantiated iceVisualInstance for {name}");
            }
            if (iceVisualInstance != null)
            {
                iceVisualInstance.SetActive(true);
                foreach (var r in iceVisualInstance.GetComponentsInChildren<Renderer>(includeInactive: true)) r.enabled = true;
            }
        }
        else if (iceVisualInstance != null)
        {
            iceVisualInstance.SetActive(false);
        }

    }


    public void AddIce(bool value) => SetHasIce(value);

    // Resolve recipe and broadcast values
    public void ResolveAndNotify(double now)
    {
        liquid = LiquidRecipes.Resolve(composition);
        fill = Mathf.Clamp01(composition.TotalAmount() / maxVolume);

        lastWriteTime = now;
        // Debug line for inspection
        // Debug.Log($"MugContent.Resolve on {gameObject.name}: composition={composition} -> liquid={liquid} fill={fill:0.00}");
        NotifyChanged();
    }

    public void ResolveAndNotifyPublic()
    {
        ResolveAndNotify(Time.realtimeSinceStartupAsDouble);
    }

    // Optional helper API for others to read composition details
    public LiquidComposition GetComposition()
    {
        // return a shallow copy to avoid external mutation
        var copy = new LiquidComposition();
        foreach (var c in composition.components) copy.components.Add(new LiquidComponent(c.type, c.amount));
        return copy;
    }
}