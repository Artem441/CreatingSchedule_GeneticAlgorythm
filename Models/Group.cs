namespace CreatingSchedule.Models;

public class Group
{ 
    public string Name { get; set; }

    public Group(string name)
    {
        Name = name;
    }
    public override string ToString() => Name;
}