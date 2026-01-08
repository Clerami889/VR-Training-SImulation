using TMPro;
using UnityEngine;

public class TaskLine : MonoBehaviour
{
    public DrinkType drinkType;
    public DrinkTemperature temperature;
    [SerializeField] private TextMeshProUGUI textField;

    public void Initialize(DrinkTask task)
    {
        drinkType = task.drink;
        temperature = task.temperature;
        if (textField == null) textField = GetComponentInChildren<TextMeshProUGUI>();
        Refresh(task);
    }

    public void Refresh(DrinkTask task)
    {
        if (textField == null) textField = GetComponentInChildren<TextMeshProUGUI>();
        textField.text = $"{task.amountRequired} x {task.drink}  ({task.amountCompleted}/{task.amountRequired})  [{task.temperature}]";
        textField.color = task.IsComplete ? Color.green : Color.white;
        textField.ForceMeshUpdate(); // ensure TMP updates vertex colors
    }
}