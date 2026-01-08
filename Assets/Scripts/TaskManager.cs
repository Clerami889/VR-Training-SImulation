using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    public int tasksCount = 2;
    public int unitsPerTask = 1;
    public bool allowDuplicates = false;
    public bool includeTemperature = true; // toggle to include temp in randomization
    public bool allowAnyTemperature = false; // allow "Any" as a valid target
    public List<DrinkTask> tasks = new List<DrinkTask>();
    public int level = 1;

    public event Action<List<DrinkTask>> OnTasksCreated;
    public event Action<DrinkTask> OnTaskUpdated;

    private System.Random rng;

    private void Awake()
    {
        rng = new System.Random();
        CreateRandomTasks();
    }

    private void Update()
    {
        if (AllComplete())
        {
            GenerateNewTask();
        }
    }

    public void CreateRandomTasks()
    {
        tasks.Clear();
        var values = Enum.GetValues(typeof(DrinkType)).Cast<DrinkType>().Where(d => d != DrinkType.None).ToList();
        var candidates = new List<DrinkType>(values);

        for (int i = 0; i < tasksCount; i++)
        {
            if (candidates.Count == 0 && !allowDuplicates) break;
            DrinkType pick = allowDuplicates ? candidates[rng.Next(candidates.Count)] : candidates[rng.Next(candidates.Count)];
            if (!allowDuplicates) candidates.Remove(pick);

            DrinkTemperature temp = DrinkTemperature.Any;
            if (includeTemperature)
            {
                // Choose Hot or Cold or Any depending on settings
                if (allowAnyTemperature && rng.NextDouble() < 0.15) temp = DrinkTemperature.Any; // small chance of Any
                else temp = rng.NextDouble() < 0.5 ? DrinkTemperature.Hot : DrinkTemperature.Cold;
            }

            var t = new DrinkTask { drink = pick, amountRequired = unitsPerTask, temperature = temp };
            tasks.Add(t);
            Debug.Log($"TaskManager: Added task {t.drink} ({t.temperature})");
        }

        Debug.Log($"TaskManager: Created {tasks.Count} tasks");
        OnTasksCreated?.Invoke(tasks);
    }

    public void RegisterPreparedDrink(DrinkType prepared, DrinkTemperature preparedTemp, int units = 1)
    {
        Debug.Log($"TaskManager.RegisterPreparedDrink called: {prepared} temp={preparedTemp} units={units}");
        foreach (var t in tasks)
        {
            if (t.IsComplete) continue;
            bool typeMatches = t.drink == prepared;
            bool tempMatches = (t.temperature == DrinkTemperature.Any) || (t.temperature == preparedTemp);
            if (typeMatches && tempMatches)
            {
                t.amountCompleted = Mathf.Min(t.amountRequired, t.amountCompleted + units);
                Debug.Log($"TaskManager: updated {t.drink} ({t.temperature}) -> {t.amountCompleted}/{t.amountRequired}");
                OnTaskUpdated?.Invoke(t);
                return;
            }
        }
        Debug.Log($"TaskManager: no matching incomplete task for {prepared} ({preparedTemp})");
    }


    public bool AllComplete() => tasks.All(t => t.IsComplete);

    public void GenerateNewTask()
    {
        if (level >= 2 && level < 4)
        {
            level++;
            unitsPerTask++;
            // increase difficulty
            CreateRandomTasks();
            Debug.Log($"New batch created! Level {level}, each task requires {unitsPerTask} drinks.");
        }
        else if (level >= 4)
        {
            level++;
            unitsPerTask++;
            tasksCount++;
            CreateRandomTasks();
        }

    }
}