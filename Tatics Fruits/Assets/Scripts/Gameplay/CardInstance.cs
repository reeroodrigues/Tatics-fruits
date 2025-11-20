namespace New_GameplayCore
{
    public readonly struct CardInstance
    {
        public readonly string RollId;
        public readonly CardTypeSo Type;
        public readonly int Value;
        public CardInstance(string rollId, CardTypeSo type, int value)
        { RollId = rollId; Type = type; Value = value; }
    }

    public readonly struct PairResult
    {
        public readonly CardInstance A;
        public readonly CardInstance B;
        public readonly int PointsAdded;
        public readonly int ComboCountAfter;
        public readonly int TimeBonusSeconds;
        public PairResult(CardInstance a, CardInstance b, int pointsAdded, int comboAfter, int timeBonus)
        { A = a; B = b; PointsAdded = pointsAdded; ComboCountAfter = comboAfter; TimeBonusSeconds = timeBonus; }
    }
}