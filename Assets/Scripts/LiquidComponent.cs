using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class LiquidComponent
{
    public LiquidType type;
    public float amount; // arbitrary units

    public LiquidComponent() { }
    public LiquidComponent(LiquidType t, float a) { type = t; amount = a; }
}

[Serializable]
public class LiquidComposition
{
    public List<LiquidComponent> components = new List<LiquidComponent>();

    public void Clear() => components.Clear();

    public float TotalAmount()
    {
        float sum = 0f;
        foreach (var c in components) sum += c.amount;
        return sum;
    }

    public void Add(LiquidType type, float amount)
    {
        if (amount <= 0f) return;
        var existing = components.Find(c => c.type == type);
        if (existing != null) existing.amount += amount;
        else components.Add(new LiquidComponent(type, amount));
    }

    public void SetSingle(LiquidType type, float amount)
    {
        Clear();
        if (type != LiquidType.None && amount > 0f) Add(type, amount);
    }

    public Dictionary<LiquidType, float> GetRatios()
    {
        var dict = new Dictionary<LiquidType, float>();
        float total = TotalAmount();
        if (total <= 0f) return dict;
        foreach (var c in components) dict[c.type] = c.amount / total;
        return dict;
    }

    public LiquidType[] GetDistinctTypes()
    {
        return components.Select(c => c.type).Distinct().ToArray();
    }

    public override string ToString()
    {
        return string.Join(",", components.Select(c => $"{c.type}:{c.amount:F2}"));
    }
}

public static class LiquidRecipes
{
    private static readonly Dictionary<string, LiquidType> recipes = new Dictionary<string, LiquidType>()
    {
        // Add recipes here. Keys are sorted names of components.
        { MakeKey(new[] { LiquidType.Tea, LiquidType.Milk }), LiquidType.MilkTea },
        { MakeKey(new[] { LiquidType.Coffee, LiquidType.Milk }), LiquidType.CafeLatte },
    };

    private static string MakeKey(IEnumerable<LiquidType> types)
    {
        var sorted = types.Select(t => t.ToString()).OrderBy(s => s);
        return string.Join(",", sorted);
    }

    public static LiquidType Resolve(LiquidComposition composition)
    {
        if (composition == null) return LiquidType.None;
        var types = composition.GetDistinctTypes();
        if (types.Length == 0) return LiquidType.None;
        if (types.Length == 1) return types[0];

        string key = MakeKey(types);
        if (recipes.TryGetValue(key, out var result)) return result;
        return LiquidType.Mixed;
    }
}