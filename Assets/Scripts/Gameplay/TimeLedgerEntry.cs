namespace ChronoBlade.Gameplay
{
    // One entry in TimeEconomyController's rolling ledger — consumed by the debug HUD (spec 06).
    public readonly struct TimeLedgerEntry
    {
        public readonly float Delta;
        public readonly string Reason;
        public readonly float ResultingTime;

        public TimeLedgerEntry(float delta, string reason, float resultingTime)
        {
            Delta = delta;
            Reason = reason;
            ResultingTime = resultingTime;
        }
    }
}
