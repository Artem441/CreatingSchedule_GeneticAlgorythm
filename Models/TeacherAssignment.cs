namespace CreatingSchedule.Models;

public class TeacherAssignment
{
    public string TeacherName { get; set; }
    public string SubjectName { get; set; }
    public int HoursPerWeek { get; set; }

    public TeacherAssignment(string teacherName, string subjectName, int hoursPerWeek)
    {
        TeacherName = teacherName;
        SubjectName = subjectName;
        HoursPerWeek = hoursPerWeek;
    }

    public override string ToString()
    {
        return $"{TeacherName} - {SubjectName} ({HoursPerWeek} ч/нед)";
    }
}