namespace IntelliPM.Domain.ValueObjects;

public record StoryPoints
{
    public int Value { get; init; }

    public StoryPoints(int value)
    {
        if (value < 0 || value > 100)
            throw new ArgumentException("Story points must be between 0 and 100", nameof(value));
        
        Value = value;
    }
}

