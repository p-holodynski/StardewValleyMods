using System;

public class ModConfig
{
    public bool clockNotificationsEnabled { get; set; } = true;
    public bool ProfitMarginEnabled { get; set; } = true;
    public float ProfitMarginValue { get; set; } = 0.25f;
    public bool PlantAnySeasonEnabled { get; set; } = true;
    public bool MachineSpeedUpEnabled { get; set; } = true;
    public int MachineSpeedUpChanceValue { get; set; } = 25;
    public int MachineSpeedUpSpeedValue { get; set; } = 25;
    public bool CropGrowEnabled { get; set; } = true;
    public string CropGrowMethod { get; set; } = "Completely";
    public string CropGrowArea { get; set; } = "Individual";
    public int CropGrowChanceValue { get; set; } = 25;
    public bool CropMutateToGiantEnabled { get; set; } = true;
    public int CropMutateToGiantChanceValue { get; set; } = 25;
    public string IridiumClockCustomTexture { get; set; } = "IridiumClock";
    public string RadioactiveClockCustomTexture { get; set; } = "RadioactiveClock";
}