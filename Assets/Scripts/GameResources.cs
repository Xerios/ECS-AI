using System;

[Serializable]
public struct GameResources
{
    public static GameResources Current, Max;

    public int Population;
    public int Wood;
    public int Iron;
    public int Faith;

    public bool CanSubstract (GameResources cost)
    {
        return (
               (Population >= cost.Population) &&
               (Wood >= cost.Wood) &&
               (Iron >= cost.Iron) &&
               (Faith >= cost.Faith)
               );
    }

    public void Substract (GameResources cost)
    {
        Wood -= cost.Wood;
        Iron -= cost.Iron;
        Faith -= cost.Faith;
    }
}