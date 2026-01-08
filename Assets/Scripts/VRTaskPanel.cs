using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VRTaskPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private RectTransform listParent;
    [SerializeField] private GameObject linePrefab;

    private List<GameObject> lines = new List<GameObject>();

    private void OnEnable()
    {
        var manager = Object.FindAnyObjectByType<TaskManager>();
        if (manager != null)
        {
            manager.OnTasksCreated += ShowTasks;
            manager.OnTaskUpdated += UpdateOneTask;
        }
    }

    private void OnDisable()
    {
        var manager = Object.FindAnyObjectByType<TaskManager>();
        if (manager != null)
        {
            manager.OnTasksCreated -= ShowTasks;
            manager.OnTaskUpdated -= UpdateOneTask;
        }
    }

    public void ShowTasks(List<DrinkTask> tasks)
    {
        ClearLines();
        title.text = "Tasks";
        for (int i = 0; i < tasks.Count; i++)
        {
            var go = Instantiate(linePrefab, listParent);
            go.name = $"TaskLine_{i}";
            var tl = go.GetComponent<TaskLine>();
            if (tl == null) tl = go.AddComponent<TaskLine>();
            tl.Initialize(tasks[i]);
            lines.Add(go);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(listParent);
    }

    public void UpdateOneTask(DrinkTask updated)
    {
        foreach (var go in lines)
        {
            var tl = go.GetComponent<TaskLine>();
            if (tl != null && tl.drinkType == updated.drink && tl.temperature == updated.temperature)
            {
                tl.Refresh(updated);
                return;
            }
        }
        // if not found, optionally add the task line
        Debug.Log($"VRTaskPanel: UpdateOneTask did not find line for {updated.drink}");
    }

    private void ClearLines()
    {
        foreach (var l in lines) Destroy(l);
        lines.Clear();
    }
}