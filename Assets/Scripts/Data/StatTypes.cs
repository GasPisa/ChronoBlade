namespace ChronoBlade.Data
{
    // The three RPG stats, all paid for and earned in time (no separate currency).
    public enum StatType
    {
        Vigor,   // max time capacity
        Siega,   // time-per-kill multiplier
        Premura, // move / attack speed multiplier
    }

    public enum ModifierType
    {
        Flat,
        Percent,
    }
}
