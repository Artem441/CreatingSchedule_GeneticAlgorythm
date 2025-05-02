using CreatingSchedule.Models;
using System.Collections.Generic;

namespace CreatingSchedule.ViewModels;

public class TimeSlotScheduleViewModel
{
    public string TimeTable { get; set; }
    public List<ScheduleEntry> Monday { get; set; }
    public List<ScheduleEntry> Tuesday { get; set; }
    public List<ScheduleEntry> Wednesday { get; set; }
    public List<ScheduleEntry> Thursday { get; set; }
    public List<ScheduleEntry> Friday { get; set; }
}