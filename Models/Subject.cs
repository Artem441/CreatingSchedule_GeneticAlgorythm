namespace CreatingSchedule.Models;
public class Subject
{
    public string? Name { get; set; }
    public Teacher? AssingedTeacher { get; set; }
    public int HoursPerWeek { get; set; }

    public Subject(string name, int hours, Teacher teacher)
    {
        Name = name;
        AssingedTeacher = teacher;
        HoursPerWeek = hours;
    }
}
