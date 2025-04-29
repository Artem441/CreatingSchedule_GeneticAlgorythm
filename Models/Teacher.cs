using System.Collections.Generic;

namespace CreatingSchedule.Models;

public class Teacher
{
    public string? Name { get; set; }

    public Teacher(string name)
    {
        Name = name;
    }
}