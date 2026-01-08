using System;

[Serializable]
public class DrinkTask
{
    public DrinkType drink;
    public DrinkTemperature temperature = DrinkTemperature.Any;
    public int amountRequired = 1;
    [NonSerialized] public int amountCompleted = 0;

    public bool IsComplete => amountCompleted >= amountRequired;
    public string DisplayText => $"{amountCompleted}/{amountRequired} x {drink} ({temperature})";
}