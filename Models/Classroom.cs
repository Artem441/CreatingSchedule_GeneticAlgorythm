namespace CreatingSchedule.Models;

public class Classroom
{
    public string? Name { get; set; }

    public Classroom(string name)
    {
        Name = name;
    }
}