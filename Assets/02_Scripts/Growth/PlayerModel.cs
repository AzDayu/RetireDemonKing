using System;

[Serializable]
public class PlayerModel
{
    public int Level = 1;
    public long CurrentExp = 0;

    public long Gold = 0;
    public long EnhanceCurrency = 0;
    public int RebirthPoints = 0;

    public int CurrentStage = 1;
    public int MaxStage = 1;
}
