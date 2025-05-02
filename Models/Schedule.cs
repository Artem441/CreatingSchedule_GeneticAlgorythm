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

        //fitness -= CheckTeacherConflicts() * 2;
       // fitness -= CheckClassroomConflicts() * 2;
        fitness -= CheckTeacherOverwork() * 2;
        fitness -= CheckStudentOverwork() * 2;
        fitness -= CheckWindows();
        fitness -= CheckTeacherDailyLoadLimit() * 2;
        fitness -= CheckGroupDailyLoadLimit() * 2;
        
        
        return Math.Max(0,fitness);
    }

   // private int CheckTeacherConflicts() => Entries.GroupBy(e => new { e.Teacher, e.DayOfWeek, e.TimeSlot }).Count(g => g.Count() > 1);
   // private int CheckClassroomConflicts() => Entries.GroupBy(e => new { e.Classroom, e.DayOfWeek, e.TimeSlot }).Count(g => g.Count() > 1);
    

    private int CheckTeacherDailyLoadLimit()
    {
        var grouped = new Dictionary<(Teacher, int),int>();
        foreach (var entry in Entries)
        {
            var key = (entry.Teacher, entry.DayOfWeek);
            if (!grouped.ContainsKey(key))
            {
                grouped[key] = 1;
            }
            else
            {
                grouped[key]++;
            }
        }

        int overloadCount = 0;

        foreach (var item in grouped)
        {
            if (item.Value > 5)
            {
                overloadCount++;
            }
        }

        return overloadCount;
    }
    
    private int CheckTeacherOverwork()
    {
        var grouped = new Dictionary<(Teacher, int, int),int>();
        foreach (var entry in Entries)
        {
            var key = (entry.Teacher, entry.DayOfWeek, entry.TimeSlot);
            if (!grouped.ContainsKey(key))
            {
                grouped[key] = 1;
            }
            else
            {
                grouped[key]++;
            }
        }

        int overloadCount = 0;
        
        foreach (var item in grouped)
        {
            if (item.Value > 1)
            {
                overloadCount++;
            }
        }
        return overloadCount;
    }
    private int CheckStudentOverwork()
    {
        var grouped = new Dictionary<(Group, int, int), int>();
        foreach (var entry in Entries)
        {
            var key = (entry.Group, entry.DayOfWeek, entry.TimeSlot);
            if (!grouped.ContainsKey(key))
            {
                grouped[key] = 1;
            }
            else
            {
                grouped[key]++;
            }
        }

        int overloadCount = 0;
            
        foreach (var item in grouped)
        {
            if (item.Value > 1)
            {
                overloadCount++;
            }
        }
        return overloadCount;
    }
    
    private int CheckGroupDailyLoadLimit()
    {
        var grouped = new Dictionary<(Group, int),int>(); // тут в качестве ключа словаря создается кортеж
        foreach (var entry in Entries)
        {
            var key = (entry.Group, entry.DayOfWeek);
            if (!grouped.ContainsKey(key))
            {
                grouped[key] = 1;
            }
            else
            {
                grouped[key] += 1;
            }
        }
        
        int overloadCount = 0;
        foreach (var pair in grouped)
        {
            if (pair.Value > 5)
            {
                overloadCount++;
            }
        }
        return overloadCount;
    }
    private int CheckWindows()
    {
        int totalPenalty = 0;
        
        Dictionary<(Group, int),List<ScheduleEntry>> scheduleByGroupAndDay = new Dictionary<(Group, int),List<ScheduleEntry>>();

        foreach (ScheduleEntry entry in Entries)
        {
            var key = (entry.Group, entry.DayOfWeek);

            if (!scheduleByGroupAndDay.ContainsKey(key))
            {
                scheduleByGroupAndDay[key] = new List<ScheduleEntry>();
            }
            
            scheduleByGroupAndDay[key].Add(entry);
        }

        foreach (var pair in scheduleByGroupAndDay)
        {
            List<ScheduleEntry> daySchedule = pair.Value;

            List<int> timeSlots = new List<int>();

            foreach (ScheduleEntry entry in daySchedule)
            {
                timeSlots.Add(entry.TimeSlot);
            }
            
            timeSlots.Sort();

            if (timeSlots.Count < 2)
            {
                continue;
            }

            for (int i = 1; i < timeSlots.Count; i++)
            {
                int currentSlot = timeSlots[i];
                int PreviousSlot = timeSlots[i - 1];

                if (currentSlot - PreviousSlot > 1)
                {
                    int windowSize = currentSlot - PreviousSlot;
                    totalPenalty += windowSize;
                }
                
            }   
        }
        return totalPenalty;
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