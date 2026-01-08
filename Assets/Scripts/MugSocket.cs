using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;



// Update EvaluateRecipe to call OnRecipeMatched when a rule matches.
// Keep your existing rules but ensure you call OnRecipeMatched(...) when matched.
// Example rule flow (insert inside EvaluateRecipe where you decide the recipe matches):


public class MugSocket : MonoBehaviour
{
    [Tooltip("Optional filter by tag for the mug GameObject")]
    [SerializeField] private string mugTag = "Mug";
    [SerializeField] private GameObject iceVisual;
    private HashSet<int> countedMugInstanceIds = new HashSet<int>();

    public TrashCan trashCan; // assign in Inspector

    private MugContent currentMugContent;

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(mugTag) && !other.CompareTag(mugTag)) return;

        var content = other.GetComponentInParent<MugContent>();
        if (content == null) return;

        AttachMug(content);
    }

    private void OnTriggerExit(Collider other)
    {
        var content = other.GetComponentInParent<MugContent>();
        if (content == null) return;

        if (content == currentMugContent) DetachMug();
    }

    public void AttachMug(MugContent mug)
    {
        if (currentMugContent == mug) return;
        DetachMug();
        if (mug == null) return;
        currentMugContent = mug;
        currentMugContent.OnContentChanged += HandleContentChanged;

        HandleContentChanged(currentMugContent.ResolvedType, currentMugContent.Fill);
        RefreshAllFromMug();

        if (iceVisual != null)
        {
            iceVisual.transform.SetParent(mug.transform, worldPositionStays: true);
            iceVisual.transform.localScale = Vector3.one; // ensure scale is reasonable
            Debug.Log($"AttachMug: parented iceVisual to mug {mug.gameObject.name}");
        }



        if (currentMugContent.ResolvedType == LiquidType.None || currentMugContent.Fill <= 0f)
            StartCoroutine(DelayedRecheck());
    }



    public void DetachMug()
    {
        if (currentMugContent != null)
        {
            // allow the same mug to be counted again later
            int id = currentMugContent.GetInstanceID();
            if (countedMugInstanceIds.Contains(id))
            {
                countedMugInstanceIds.Remove(id);
                Debug.Log($"DetachMug: removed counted id {id}");
            }

            currentMugContent.OnContentChanged -= HandleContentChanged;

            // do not clear mug-owned visuals here; the mug should own its ice visual
            // If you had socket highlight visuals, clear them separately (not the mug's ice)
            HandleMugRemoved();

            currentMugContent = null;
        }


    }


    private void HandleContentChanged(LiquidType liquid, float fill)
    {
        RefreshAllFromMug(); // makes sure currentMugContent is up to date

        if (liquid == LiquidType.None || fill <= 0f)
        {
            Debug.Log("MugSocket: mug is empty");
            return;
        }

        // Determine temperature from current mug content
        bool hasIce = currentMugContent != null && currentMugContent.HasIce;
        DrinkTemperature temp = hasIce ? DrinkTemperature.Cold : DrinkTemperature.Hot;

        Debug.Log($"MugSocket: Mug present; liquid={liquid} fill={fill:0.00} temp={temp}");

        EvaluateRecipe(liquid, fill, temp);
    }

    private void EvaluateRecipe(LiquidType liquid, float fill, DrinkTemperature temp)
    {
        // Example recipe rule: milk-based drink requires >= 0.5 fill,
        // same recipe whether hot or cold, but we may branch by temp for variations.
        if (liquid == LiquidType.Milk && fill >= 0.5f)
        {
            if (temp == DrinkTemperature.Cold)
            {
                Debug.Log("MugSocket: Milk drink (Cold)");
                ScoreManager.Instance.AddPoints(10);
            }
            else
            {
                Debug.Log("MugSocket: Milk drink (Hot)");
                ScoreManager.Instance.AddPoints(5);
            }


            OnRecipeMatched(liquid, fill, temp);
            return;
        }

        if (liquid == LiquidType.Tea && fill >= 0.5f)
        {
            if (temp == DrinkTemperature.Cold)
            {
                Debug.Log("MugSocket: Tea drink (Cold)");
                ScoreManager.Instance.AddPoints(10);
            }
            else
            {
                Debug.Log("MugSocket: Tea drink (Hot)");
                ScoreManager.Instance.AddPoints(5);
            }


            OnRecipeMatched(liquid, fill, temp);
            return;
        }

        if (liquid == LiquidType.MilkTea && fill >= 0.5f)
        {
            if (temp == DrinkTemperature.Cold)
            {
                Debug.Log("MugSocket: Milk Tea (Cold)");
                ScoreManager.Instance.AddPoints(15);
            }
            else
            {
                Debug.Log("MugSocket: Milk Tea (Hot)");
                ScoreManager.Instance.AddPoints(10);
            }
            OnRecipeMatched(liquid, fill, temp);
            return;
        }

        if (liquid == LiquidType.Coffee && fill >= 0.5f)
        {
            if (temp == DrinkTemperature.Hot)
            {
                Debug.Log("MugSocket: Coffee (Hot)");
                ScoreManager.Instance.AddPoints(20);
            }
            else
            {
                Debug.Log("MugSocket: Coffee (Cold)");
                ScoreManager.Instance.AddPoints(15);
            }
            OnRecipeMatched(liquid, fill, temp);
            return;
        }

        if (liquid == LiquidType.CafeLatte && fill >= 0.5f)
        {
            if (temp == DrinkTemperature.Hot)
            {
                Debug.Log("MugSocket: Cafe Latte (Hot)");
                ScoreManager.Instance.AddPoints(20);
            }
            else
            {
                Debug.Log("MugSocket: Cafe Latte (Cold)");
                ScoreManager.Instance.AddPoints(15);
            }
            OnRecipeMatched(liquid, fill, temp);
            return;
        }


    }

    private void OnRecipeMatched(LiquidType liquid, float fill, DrinkTemperature temp)
    {
        Debug.Log($"OnRecipeMatched invoked: {liquid} fill={fill:0.00} temp={temp}");

        // Guard: ensure we only count the mug once per attach/serve
        if (currentMugContent == null)
        {
            Debug.Log("OnRecipeMatched: no currentMugContent; aborting");
            return;
        }
        int mugId = currentMugContent.GetInstanceID();
        if (countedMugInstanceIds.Contains(mugId))
        {
            Debug.Log($"OnRecipeMatched: mug {mugId} already counted for task");
        }
        else
        {
            countedMugInstanceIds.Add(mugId);

            // Map LiquidType -> DrinkType if they are different enums
            DrinkType mapped = MapLiquidToDrink(liquid); // implement mapping below if needed
            DrinkTemperature mappedTemp = MapTempFromYourSystem(temp);
            // Call TaskManager (safe null-check)
            var tm = Object.FindFirstObjectByType<TaskManager>();
            if (tm != null)
            {
                tm.RegisterPreparedDrink(mapped, mappedTemp, 1);
            }

            else Debug.LogWarning("OnRecipeMatched: TaskManager not found");

            // ✅ Destroy the mug and spawn a new one
            // In OnRecipeMatched
            if (trashCan != null)
            {
                StartCoroutine(trashCan.DestroyAndRespawn(currentMugContent.gameObject, (newMug) =>
                {
                    // Update your recipe script’s reference
                    currentMugContent = newMug.GetComponent<MugContent>();
                }));
            }

        }

        // Existing shared behavior
        Debug.Log($"Recipe matched: {liquid} {temp} fill={fill:0.00}");
    }


    private void HandleMugRemoved()
    {
        Debug.Log("MugSocket: mug removed");
    }

    private IEnumerator DelayedRecheck()
    {
        yield return null; // wait one frame
        if (currentMugContent != null)
            HandleContentChanged(currentMugContent.ResolvedType, currentMugContent.Fill);
    }

    private void RefreshAllFromMug()
    {
        if (currentMugContent == null)
        {
            RefreshIceVisual(false);
            return;
        }

        // Only show ice if the mug actually has content and the HasIce flag is set
        bool isCold = currentMugContent.HasIce && currentMugContent.Fill > 0f;
        RefreshIceVisual(isCold);
    }
    private void RefreshIceVisual(bool hasIce)
    {
        if (iceVisual == null) return;

        // If the ice visual has been parented to a mug (not under the socket), do not toggle it here.
        // This keeps the mug in control of its own ice visual.
        if (!iceVisual.transform.IsChildOf(transform))
        {
            // iceVisual is not a child of the socket (likely parented to the mug) � skip toggling.
            Debug.Log("MugSocket.RefreshIceVisual: iceVisual is owned by mug; skipping socket toggle");
            return;
        }

        if (iceVisual.activeSelf == hasIce) return;
        Debug.Log($"MugSocket.RefreshIceVisual -> {hasIce} (iceVisual activeSelf was {iceVisual.activeSelf})");
        iceVisual.SetActive(hasIce);
        if (hasIce) Debug.Log($"MugSocket.RefreshIceVisual stack:\n{System.Environment.StackTrace}");
    }

    private DrinkType MapLiquidToDrink(LiquidType l)
    {
        switch (l)
        {
            case LiquidType.Tea: return DrinkType.Tea;
            case LiquidType.Milk: return DrinkType.Milk;
            case LiquidType.MilkTea: return DrinkType.MilkTea;
            case LiquidType.Coffee: return DrinkType.Coffee;
            case LiquidType.CafeLatte: return DrinkType.CafeLatte;
            default: return DrinkType.None;
        }
    }

    private DrinkTemperature MapTempFromYourSystem(DrinkTemperature t)
    {
        return t; // direct mapping in this case
    }
}