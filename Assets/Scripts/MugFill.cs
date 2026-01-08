using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class MugFill : MonoBehaviour
{
    [Header("Assign per-liquid meshes")]
    [SerializeField] private List<LiquidVisuals> liquidVisuals = new List<LiquidVisuals>();

    [Header("Settings")]
    [SerializeField] private string pourTag = "PourPoint";
    [SerializeField] private float firstThreshold = 2f;
    [SerializeField] private float secondThreshold = 4f;

    private Coroutine fillCoroutine;
    private Pour currentPour;
    private LiquidType currentLiquid = LiquidType.None;
    private float elapsed = 0f;
    private bool isFull = false;

    [SerializeField] private MugContent mugContent;
    private LiquidVisuals activeVisual = null;


    void Start()
    {
        foreach (var v in liquidVisuals)
        {
            if (v.stage1Mesh) v.stage1Mesh.SetActive(false);
            if (v.stage2Mesh) v.stage2Mesh.SetActive(false);
        }
        if (mugContent == null) mugContent = GetComponentInParent<MugContent>();
        //Debug.Log($"MugFill Start: mugContent={(mugContent == null ? "null" : mugContent.gameObject.name)}");

    }

    private void Fill(Collider other)
    {
        if (!string.IsNullOrEmpty(pourTag) && !other.CompareTag(pourTag)) return;

        currentPour = other.GetComponentInParent<Pour>();
        if (currentPour == null)
        {
            //Debug.Log("MugFill: No Pour component on collider or parents");
            return;
        }

        if (currentLiquid != currentPour.liquid && !IsEmpty())
        {
            // If a different liquid starts pouring, do NOT clear existing content.
            // We will combine volumes instead. Keep currentLiquid set to the incoming liquid.
            currentLiquid = currentPour.liquid;
        }

        currentLiquid = currentPour.liquid;

        if (currentPour.IsPouring && !isFull)
        {
            if (fillCoroutine != null) StopCoroutine(fillCoroutine);
            fillCoroutine = StartCoroutine(FillTimer());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Fill(other);

        
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentPour == null) return;

        if (!currentPour.IsPouring && fillCoroutine != null)
        {
            //Debug.Log("MugFill: pouring paused");
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }
        else if (currentPour.IsPouring && fillCoroutine == null && !isFull)
        {
            //Debug.Log("MugFill: pouring resumed");
            fillCoroutine = StartCoroutine(FillTimer());
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (fillCoroutine != null) StopCoroutine(fillCoroutine);
        fillCoroutine = null;
        currentPour = null;
        // optionally reset content when pour source leaves
        UpdateMugContentReset();
    }

    
    private IEnumerator FillTimer()
    {
        if (currentPour == null) yield break;

        // pourRate: normalized volume per second (1.0 means full in secondThreshold seconds)
        float pourRate = 1f / Mathf.Max(0.0001f, secondThreshold);

        // initialize
        UpdateMugContent(); // optionally update once
        activeVisual = GetVisualForResolved();

        while (!isFull)
        {
            if (currentPour != null && !currentPour.IsPouring) { yield return null; continue; }

            // how much normalized volume to add this frame
            float deltaVolume = Time.deltaTime * pourRate;

            // Add the incoming liquid volume into MugContent so composition updates
            if (mugContent != null)
            {
                // If you implemented AddVolume:
                Debug.Log($"Pouring {currentLiquid} IsPouring={currentPour?.IsPouring} delta={deltaVolume:F3}");
                mugContent.AddVolume(currentLiquid, deltaVolume);
                // If you did not implement AddVolume, you must compute a merged fill and call SetContent.
            }

            elapsed += Time.deltaTime;

            // Choose visual based on resolved mug content (or dominant)
            UpdateVisualStage(elapsed);
            yield return null;
        }

        fillCoroutine = null;
    }

    private void UpdateVisualStage(float elapsed)
{
    var resolvedVisual = GetVisualForResolved();

    if (resolvedVisual != activeVisual)
    {
        HideAllVisuals();
        activeVisual = resolvedVisual;
    }

    if (elapsed >= firstThreshold && activeVisual != null && activeVisual.stage1Mesh != null && !activeVisual.stage1Mesh.activeSelf)
    {
        activeVisual.stage1Mesh.SetActive(true);
    }
    UpdateMugContent();

    if (elapsed >= secondThreshold)
    {
        if (activeVisual != null && activeVisual.stage2Mesh != null && !activeVisual.stage2Mesh.activeSelf)
        {
            activeVisual.stage2Mesh.SetActive(true);
            if (activeVisual.stage1Mesh != null && activeVisual.stage1Mesh.activeSelf)
                activeVisual.stage1Mesh.SetActive(false);
        }
        isFull = true;
        UpdateMugContent();

    }
}

    private LiquidVisuals GetVisualFor(LiquidType liquid)
    {
        return liquidVisuals.Find(v => v.liquid == liquid);
    }

    private void HideAllVisuals()
    {
        foreach (var v in liquidVisuals)
        {
            if (v.stage1Mesh) v.stage1Mesh.SetActive(false);
            if (v.stage2Mesh) v.stage2Mesh.SetActive(false);
        }
    }

    private bool IsEmpty()
    {
        return !isFull && Mathf.Approximately(elapsed, 0f);
    }

    // New: push state into MugContent
    private void UpdateMugContent()
    {
        if (mugContent == null) return;

        float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, secondThreshold));

        // If nothing has been poured yet, keep the mug empty
        if (normalized <= 0f)
        {

            return;
        }


        if (mugContent.IsEmpty)
        {
            mugContent.SetContent(currentLiquid, normalized);
            return;
        }


    }

    private void UpdateMugContentReset()
    {
        if (mugContent == null) return;
        mugContent.ResetContent();
    }

    private LiquidVisuals GetVisualForResolved()
    {
        if (mugContent == null) return GetVisualFor(currentLiquid);

        // Try resolved type first (your MugContent exposes Liquid and Fill)
        LiquidType resolved = mugContent.ResolvedType;
        var v = GetVisualFor(resolved);
        if (v != null) return v;

        // If resolved is Mixed or no visual, try dominant component from composition
        // This requires MugContent to expose GetComposition() as suggested earlier.
        var comp = mugContent.GetComposition();
        if (comp != null)
        {
            var ratios = comp.GetRatios();
            if (ratios.Count > 0)
            {
                LiquidType dominant = LiquidType.None;
                float best = -1f;
                foreach (var kv in ratios)
                {
                    if (kv.Value > best) { best = kv.Value; dominant = kv.Key; }
                }
                v = GetVisualFor(dominant);
                if (v != null) return v;
            }
        }

        // Last resort, show the visual for the currently pouring liquid
        return GetVisualFor(currentLiquid);
    }
}