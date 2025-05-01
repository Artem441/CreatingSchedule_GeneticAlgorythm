using System;
using System.Collections.Generic;
using System.Linq;

namespace CreatingSchedule.Models;

public class Schedule
{
    public List<ScheduleEntry> Entries { get; set; } = new List<ScheduleEntry>();

    public double CalculateFitness()
    {
        double fitness = 100;

        fitness -= CheckTeacherConflicts() * 2;
        fitness -= CheckClassroomConflicts() * 2;
        fitness -= CheckTeacherOverwork() * 2;
        fitness -= CheckStudentOverwork() * 2;
        fitness -= CheckWindows() * 2;
        fitness -= CheckTeacherDailyLoadLimit() * 2;
        fitness -= CheckGroupDailyLoadLimit() * 2;
        
        
        return Math.Max(0,fitness);
    }

    private int CheckTeacherConflicts() => Entries.GroupBy(e => new { e.Teacher, e.DayOfWeek, e.TimeSlot }).Count(g => g.Count() > 1);
    private int CheckClassroomConflicts() => Entries.GroupBy(e => new { e.Classroom, e.DayOfWeek, e.TimeSlot }).Count(g => g.Count() > 1);
    private int CheckTeacherOverwork() => Entries.GroupBy(e => new { e.Teacher, e.DayOfWeek, e.TimeSlot }).Count(g => g.Count() > 1);
    private int CheckStudentOverwork() => Entries.GroupBy(e => new { e.Group, e.DayOfWeek, e.TimeSlot }).Count(g => g.Count() > 1);
    private int CheckGroupDailyLoadLimit() => Entries.GroupBy(e => new {e.Group,e.DayOfWeek}).Count(g => g.Count() > 5);
    private int CheckTeacherDailyLoadLimit() => Entries.GroupBy(e => new {e.Teacher,e.DayOfWeek}).Count(g => g.Count() > 5);

    
    private int CheckWindows()
    {
        int penalty = 0;
        
        var grouped = Entries.GroupBy(e => e.DayOfWeek);

        foreach (var day in grouped)
        {
            var slots = day.Select(e => e.TimeSlot).OrderBy(t => t).ToList();
            for (int i = 1; i < slots.Count; i++)
            {
                if (slots[i] - slots[i - 1] > 1)
                {
                    penalty++; // штраф за перерыв между парами(окна)
                }
            }
        }
        return penalty;
    }

    private int CheckDistribution()
    {
        int score = 0;
        
        var perDay = Entries.GroupBy(e => e.DayOfWeek).Select(g => g.Count()).ToList();
        double avg = perDay.Average();
        foreach (var count in perDay)
        {
            if (count >= avg - 1 && count <= avg + 1)
            {
                score += 1; // Плюс за равномерное распределение нагрузки
            }
        }
        return score;
    }

    public Schedule Clone()
    {
        var clone = new Schedule();
        clone.Entries = this.Entries.Select(e => e.Clone()).ToList();
        return clone;
    }
}