namespace CreatingSchedule.Models;

public class ScheduleEntry
{
    public Group Group { get; set; }
    public Subject? Subject { set; get; }
    public Teacher? Teacher { set; get; }
    public Classroom? Classroom { set; get; }
    public int DayOfWeek { set; get; }
    public int TimeSlot { set; get; }

    public ScheduleEntry Clone()
    {
        return new ScheduleEntry
        {
            Subject = this.Subject,
            Teacher = this.Teacher,
            Classroom = this.Classroom,
            DayOfWeek = this.DayOfWeek,
            TimeSlot = this.TimeSlot,
            Group = this.Group
        };
    }

    public string GetTimeSlotString() => TimeSlot switch
    {
        0 => "9:00–10:20",
        1 => "10:35–11:55",
        2 => "12:10–13:30",
        3 => "13:45–15:05",
        4 => "15:20–16:40",
        5 => "16:55–18:15",
        _ => "?"
    };
}