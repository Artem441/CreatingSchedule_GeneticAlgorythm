using System;
using System.Collections.Generic;
using System.Linq;

namespace CreatingSchedule.Models
{
    public class Schedule
    {
        public List<ScheduleEntry> Entries { get; set; } = new List<ScheduleEntry>();

        public double CalculateFitness(List<Subject> subjects, List<Group> groups)
        {
            double fitness = 100.0;

            // Штрафы за конфликты и нарушения
            fitness -= CheckTeacherOverwork() * 2.0; // Увеличен штраф за накладки учителей
            fitness -= CheckStudentOverwork() * 2.0; // Увеличен штраф за накладки групп
            fitness -= CheckWindows() * 0.5;
            fitness -= CheckTeacherDailyLoadLimit() * 1.0;
            fitness -= CheckGroupDailyLoadLimit() * 1.0;
            fitness -= CheckTeacherSubjectAssignment() * 1.0;

            // Штраф за недостающие или лишние уроки
            foreach (var group in groups)
            {
                foreach (var subject in subjects.Where(s => s.AssingedTeacher != null))
                {
                    var requiredHours = subject.HoursPerWeek;
                    var actualHours = Entries.Count(e => e.Group.Name == group.Name && e.Subject.Name == subject.Name);
                    if (actualHours < requiredHours)
                    {
                        fitness -= (requiredHours - actualHours) * 2.0;
                    }
                    else if (actualHours > requiredHours)
                    {
                        fitness -= (actualHours - requiredHours) * 1.0;
                    }
                }
            }

            return Math.Max(0, fitness);
        }

        private int CheckTeacherDailyLoadLimit()
        {
            var grouped = new Dictionary<(Teacher, int), int>();
            foreach (var entry in Entries)
            {
                var key = (entry.Teacher, entry.DayOfWeek);
                grouped[key] = grouped.GetValueOrDefault(key, 0) + 1;
            }

            return grouped.Count(item => item.Value > 5);
        }

        private int CheckTeacherOverwork()
        {
            var grouped = new Dictionary<(Teacher, int, int), int>();
            foreach (var entry in Entries)
            {
                var key = (entry.Teacher, entry.DayOfWeek, entry.TimeSlot);
                grouped[key] = grouped.GetValueOrDefault(key, 0) + 1;
            }

            return grouped.Sum(item => item.Value > 1 ? item.Value - 1 : 0);
        }

        private int CheckStudentOverwork()
        {
            var grouped = new Dictionary<(Group, int, int), int>();
            foreach (var entry in Entries)
            {
                var key = (entry.Group, entry.DayOfWeek, entry.TimeSlot);
                grouped[key] = grouped.GetValueOrDefault(key, 0) + 1;
            }

            return grouped.Sum(item => item.Value > 1 ? item.Value - 1 : 0);
        }

        private int CheckGroupDailyLoadLimit()
        {
            var grouped = new Dictionary<(Group, int), int>();
            foreach (var entry in Entries)
            {
                var key = (entry.Group, entry.DayOfWeek);
                grouped[key] = grouped.GetValueOrDefault(key, 0) + 1;
            }

            return grouped.Count(pair => pair.Value > 5);
        }

        private int CheckWindows()
        {
            int totalPenalty = 0;
            var scheduleByGroupAndDay = new Dictionary<(Group, int), List<ScheduleEntry>>();

            foreach (var entry in Entries)
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
                var timeSlots = pair.Value.Select(e => e.TimeSlot).Distinct().ToList();
                timeSlots.Sort();

                if (timeSlots.Count < 2)
                    continue;

                for (int i = 1; i < timeSlots.Count; i++)
                {
                    int windowSize = timeSlots[i] - timeSlots[i - 1] - 1;
                    if (windowSize > 0)
                    {
                        totalPenalty += windowSize;
                    }
                }
            }

            return totalPenalty;
        }

        private int CheckDistribution()
        {
            var perDay = Entries.GroupBy(e => e.DayOfWeek).Select(g => g.Count()).ToList();
            if (!perDay.Any())
                return 0;

            double avg = perDay.Average();
            return perDay.Count(count => count >= avg - 1 && count <= avg + 1);
        }

        private int CheckTeacherSubjectAssignment()
        {
            return Entries.Count(entry => entry.Teacher != entry.Subject.AssingedTeacher);
        }

        public void RepairSchedule(List<Subject> subjects, List<Teacher> teachers, List<Classroom> classrooms, List<Group> groups, Random random)
        {
            // Удаляем существующие конфликты
            RemoveConflicts();

            foreach (var group in groups)
            {
                foreach (var subject in subjects.Where(s => s.AssingedTeacher != null))
                {
                    var requiredHours = subject.HoursPerWeek;
                    var actualHours = Entries.Count(e => e.Group.Name == group.Name && e.Subject.Name == subject.Name);

                    // Добавление недостающих уроков
                    while (actualHours < requiredHours)
                    {
                        var entry = new ScheduleEntry
                        {
                            Subject = subject,
                            Teacher = subject.AssingedTeacher,
                            Classroom = classrooms[random.Next(classrooms.Count)],
                            Group = group,
                            DayOfWeek = random.Next(5),
                            TimeSlot = random.Next(5)
                        };

                        // Проверка конфликтов
                        bool hasConflict;
                        int attempts = 0;
                        const int maxAttempts = 100; // Увеличено число попыток

                        do
                        {
                            hasConflict = Entries.Any(existing =>
                                existing.DayOfWeek == entry.DayOfWeek &&
                                existing.TimeSlot == entry.TimeSlot &&
                                (existing.Teacher.Name == entry.Teacher.Name ||
                                 existing.Classroom.Name == entry.Classroom.Name ||
                                 existing.Group.Name == entry.Group.Name));

                            if (hasConflict)
                            {
                                entry.DayOfWeek = random.Next(5);
                                entry.TimeSlot = random.Next(5);
                                entry.Classroom = classrooms[random.Next(classrooms.Count)];
                                attempts++;
                            }
                        } while (hasConflict && attempts < maxAttempts);

                        if (!hasConflict)
                        {
                            Entries.Add(entry);
                            actualHours++;
                        }
                        else
                        {
                            // Попробуем переместить существующий конфликтный урок
                            var conflictEntry = Entries.FirstOrDefault(e =>
                                e.DayOfWeek == entry.DayOfWeek &&
                                e.TimeSlot == entry.TimeSlot &&
                                (e.Teacher.Name == entry.Teacher.Name ||
                                 e.Classroom.Name == entry.Classroom.Name ||
                                 e.Group.Name == entry.Group.Name));

                            if (conflictEntry != null)
                            {
                                // Перемещаем конфликтный урок
                                bool moved = false;
                                attempts = 0;
                                while (attempts < maxAttempts)
                                {
                                    conflictEntry.DayOfWeek = random.Next(5);
                                    conflictEntry.TimeSlot = random.Next(5);
                                    conflictEntry.Classroom = classrooms[random.Next(classrooms.Count)];

                                    hasConflict = Entries.Any(e =>
                                        e != conflictEntry &&
                                        e.DayOfWeek == conflictEntry.DayOfWeek &&
                                        e.TimeSlot == conflictEntry.TimeSlot &&
                                        (e.Teacher.Name == conflictEntry.Teacher.Name ||
                                         e.Classroom.Name == conflictEntry.Classroom.Name ||
                                         e.Group.Name == conflictEntry.Group.Name));

                                    if (!hasConflict)
                                    {
                                        moved = true;
                                        Entries.Add(entry);
                                        actualHours++;
                                        break;
                                    }
                                    attempts++;
                                }
                            }
                        }
                    }
                    while (actualHours > requiredHours)
                    {
                        var extraEntry = Entries.FirstOrDefault(e => e.Group.Name == group.Name && e.Subject.Name == subject.Name);
                        if (extraEntry != null)
                        {
                            Entries.Remove(extraEntry);
                            actualHours--;
                        }
                    }
                }
            }
        }

        private void RemoveConflicts()
        {
            var grouped = new Dictionary<(Group, int, int), List<ScheduleEntry>>();
            foreach (var entry in Entries)
            {
                var key = (entry.Group, entry.DayOfWeek, entry.TimeSlot);
                if (!grouped.ContainsKey(key))
                {
                    grouped[key] = new List<ScheduleEntry>();
                }
                grouped[key].Add(entry);
            }

            foreach (var group in grouped)
            {
                if (group.Value.Count > 1)
                {
                    // Оставляем только первый урок, остальные удаляем
                    for (int i = 1; i < group.Value.Count; i++)
                    {
                        Entries.Remove(group.Value[i]);
                    }
                }
            }
        }

        public Schedule Clone()
        {
            return new Schedule
            {
                Entries = Entries.Select(e => e.Clone()).ToList()
            };
        }
    }
}