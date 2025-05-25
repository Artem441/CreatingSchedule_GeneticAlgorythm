using System;
using System.Collections.Generic;
using System.Linq;
using CreatingSchedule.Models;

namespace CreatingSchedule.Services
{
    public class GeneticSheduler
    {
        private readonly List<Subject> _subjects;
        private readonly List<Teacher> _teachers;
        private readonly List<Classroom> _classrooms;
        private readonly List<Group> _groups;
        private readonly Random _random = new Random();

        public int PopulationSize { get; set; } = 450;
        public int Generations { get; set; } = 300; // Восстановлено до 300 для большей точности
        public double MutationRate { get; set; } = 0.5;
        public int ElitismCount { get; set; } = 40;

        public GeneticSheduler(List<Subject> subjects, List<Teacher> teachers, List<Classroom> classrooms, List<Group> groups)
        {
            _subjects = subjects ?? throw new ArgumentNullException(nameof(subjects));
            _teachers = teachers ?? throw new ArgumentNullException(nameof(teachers));
            _classrooms = classrooms ?? throw new ArgumentNullException(nameof(classrooms));
            _groups = groups ?? throw new ArgumentNullException(nameof(groups));
        }

        public Schedule Run()
        {
            var population = InitializePopulation();

            for (int generation = 0; generation < Generations; generation++)
            {
                population = Evolve(population);
                var bestFitness = population.Max(s => s.CalculateFitness(_subjects, _groups));
                Console.WriteLine($"Поколение {generation}: Лучший фитнес = {bestFitness:F2}");

                if (bestFitness > 99.5)
                    break;
            }

            var bestSchedule = population.OrderByDescending(s => s.CalculateFitness(_subjects, _groups)).First();
            bestSchedule.RepairSchedule(_subjects, _teachers, _classrooms, _groups, _random);
            return bestSchedule;
        }

        private List<Schedule> InitializePopulation()
        {
            var population = new List<Schedule>(PopulationSize);

            for (int i = 0; i < PopulationSize; i++)
            {
                var schedule = new Schedule();
                foreach (var group in _groups)
                {
                    foreach (var subject in _subjects.Where(s => s.AssingedTeacher != null))
                    {
                        for (int j = 0; j < subject.HoursPerWeek; j++)
                        {
                            schedule.Entries.Add(CreateRandomEntry(subject, group));
                        }
                    }
                }
                schedule.RepairSchedule(_subjects, _teachers, _classrooms, _groups, _random);
                population.Add(schedule);
            }

            return population;
        }

        private ScheduleEntry CreateRandomEntry(Subject subject, Group group)
        {
            return new ScheduleEntry
            {
                Subject = subject,
                Teacher = subject.AssingedTeacher,
                Classroom = _classrooms[_random.Next(_classrooms.Count)],
                Group = group,
                DayOfWeek = _random.Next(5),
                TimeSlot = _random.Next(5)
            };
        }

        private List<Schedule> Evolve(List<Schedule> population)
        {
            var ordered = population.OrderByDescending(s => s.CalculateFitness(_subjects, _groups)).ToList();
            var nextGeneration = new List<Schedule>(PopulationSize)
            {
                Capacity = PopulationSize
            };
            nextGeneration.AddRange(ordered.Take(ElitismCount).Select(s => s.Clone()));

            while (nextGeneration.Count < PopulationSize)
            {
                var parent1 = TournamentSelection(ordered);
                var parent2 = TournamentSelection(ordered);
                var child = Crossover(parent1, parent2);

                if (_random.NextDouble() < MutationRate)
                {
                    Mutate(child);
                }

                child.RepairSchedule(_subjects, _teachers, _classrooms, _groups, _random);
                nextGeneration.Add(child);
            }

            return nextGeneration;
        }

        private Schedule TournamentSelection(List<Schedule> population)
        {
            const int tournamentSize = 5;
            var tournament = new List<Schedule>(tournamentSize);
            for (int i = 0; i < tournamentSize; i++)
            {
                tournament.Add(population[_random.Next(population.Count)]);
            }
            return tournament.OrderByDescending(s => s.CalculateFitness(_subjects, _groups)).First();
        }

        private Schedule Crossover(Schedule p1, Schedule p2)
        {
            var child = new Schedule();
            var groups = _groups.ToList();

            foreach (var group in groups)
            {
                var entries1 = p1.Entries.Where(e => e.Group.Name == group.Name).ToList();
                var entries2 = p2.Entries.Where(e => e.Group.Name == group.Name).ToList();

                // Собираем уроки для группы, избегая конфликтов
                var childEntries = new List<ScheduleEntry>();
                var usedSlots = new HashSet<(int, int)>();

                // Добавляем уроки из первого родителя
                foreach (var entry in entries1.OrderBy(_ => _random.Next()))
                {
                    if (!usedSlots.Contains((entry.DayOfWeek, entry.TimeSlot)))
                    {
                        childEntries.Add(entry.Clone());
                        usedSlots.Add((entry.DayOfWeek, entry.TimeSlot));
                    }
                }

                // Добавляем уроки из второго родителя, если слот свободен
                foreach (var entry in entries2.OrderBy(_ => _random.Next()))
                {
                    if (!usedSlots.Contains((entry.DayOfWeek, entry.TimeSlot)))
                    {
                        childEntries.Add(entry.Clone());
                        usedSlots.Add((entry.DayOfWeek, entry.TimeSlot));
                    }
                }

                child.Entries.AddRange(childEntries);
            }

            return child;
        }

        private void Mutate(Schedule schedule)
        {
            if (schedule.Entries.Count == 0)
                return;

            var entry = schedule.Entries[_random.Next(schedule.Entries.Count)];
            int mutationType = _random.Next(4);

            switch (mutationType)
            {
                case 0:
                    entry.DayOfWeek = _random.Next(5);
                    entry.TimeSlot = _random.Next(5);
                    break;
                case 1:
                    entry.Classroom = _classrooms[_random.Next(_classrooms.Count)];
                    break;
                case 2:
                    var subject = _subjects.Where(s => s.AssingedTeacher?.Name == entry.Teacher.Name)
                                          .OrderBy(_ => _random.Next())
                                          .FirstOrDefault();
                    if (subject != null)
                    {
                        entry.Subject = subject;
                        entry.Teacher = subject.AssingedTeacher;
                    }
                    break;
                case 3: // Новая мутация: перемещение конфликтующего урока
                    var conflicts = schedule.Entries
                        .GroupBy(e => (e.Group, e.DayOfWeek, e.TimeSlot))
                        .Where(g => g.Count() > 1)
                        .SelectMany(g => g.Skip(1))
                        .ToList();

                    if (conflicts.Any())
                    {
                        var conflictEntry = conflicts[_random.Next(conflicts.Count)];
                        bool hasConflict;
                        int attempts = 0;
                        const int maxAttempts = 50;

                        do
                        {
                            conflictEntry.DayOfWeek = _random.Next(5);
                            conflictEntry.TimeSlot = _random.Next(5);
                            conflictEntry.Classroom = _classrooms[_random.Next(_classrooms.Count)];

                            hasConflict = schedule.Entries.Any(e =>
                                e != conflictEntry &&
                                e.DayOfWeek == conflictEntry.DayOfWeek &&
                                e.TimeSlot == conflictEntry.TimeSlot &&
                                (e.Teacher.Name == conflictEntry.Teacher.Name ||
                                 e.Classroom.Name == conflictEntry.Classroom.Name ||
                                 e.Group.Name == conflictEntry.Group.Name));

                            attempts++;
                        } while (hasConflict && attempts < maxAttempts);
                    }
                    break;
            }
        }
    }
}