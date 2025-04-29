namespace CreatingSchedule.Models;

public class ScheduleEntry
{
    public Subject? Subject { set; get; }
    public Teacher? Teacher { set; get; }
    public Classroom? Classroom { set; get; }
    public int DayOfWeek { set; get; }
    public int TimeSlots { set; get; }

    public ScheduleEntry Clone()
    {
        return new ScheduleEntry
        {
            Subject = this.Subject,
            Teacher = this.Teacher,
            Classroom = this.Classroom,
            DayOfWeek = this.DayOfWeek,
            TimeSlots = this.TimeSlots
        };
    }
}