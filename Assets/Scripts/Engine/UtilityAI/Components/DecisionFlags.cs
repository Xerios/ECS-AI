namespace UtilityAI
{
    [System.Flags]
    public enum DecisionFlags: byte {
        NONE = 0,
        OVERRIDE = 1 << 1,
        DOUBLE_WEIGHT = 1 << 2,
    }
}