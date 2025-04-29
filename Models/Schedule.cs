using System.Collections.Generic;
using System.Linq;

namespace CreatingSchedule.Models;

public class Schedule
{
    public List<ScheduleEntry> Entries { get; set; } = new List<ScheduleEntry>();

    public double CalculateFitness()
    {
        double fitness = 0;

        fitness += CheckConflicts();
        fitness += CheckWindows();
        fitness += CheckDistribution();
        
        return fitness;             
    }

    private int CheckConflicts()
    {
        int penalty = 0;

        foreach (var group in Entries.GroupBy(e => new { e.DayOfWeek, e.TimeSlots }))
        {
            var teachers = group.Select(e => e.Teacher).Distinct().ToList();
            if (teachers.Count < group.Count())
            {
                penalty -= 10; // наказание если у учителя несколько пар одновременно
            }
        }
        return penalty;
    }

    private int CheckWindows()
    {
        int penalty = 0;
        
        var grouped = Entries.GroupBy(e => e.DayOfWeek);

        foreach (var day in grouped)
        {
            var slots = day.Select(e => e.TimeSlots).OrderBy(t => t).ToList();
            for (int i = 1; i < slots.Count; i++)
            {
                if (slots[i] - slots[i - 1] > 1)
                {
                    penalty -= 5; // штраф за перерыв между парами(окна)
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
                score += 2; // Плюс за равномерное распределение нагрузки
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