using System;
using System.Collections.Generic;
using System.Linq;
using CreatingSchedule.Models;

namespace CreatingSchedule.Services;

public class GeneticSheduler
{
    private readonly List<Subject> _subjects;
    private readonly List<Teacher> _teachers;
    private readonly List<Classroom> _classrooms;
    private readonly Random _random = new Random();
    
    // settings of Genetic Algorithm
    public int PopulationSize { get; set; } = 750;
    public int Generation { get; set; } = 3000;
    public double MutationRate { get; set; } = 0.3;
    public int ElitismCount { get; set; } = 75;

    public GeneticSheduler(List<Subject> subjects, List<Teacher> teachers, List<Classroom> classrooms)
    {
        _subjects = subjects;
        _teachers = teachers;
        _classrooms = classrooms;
    }

    public Schedule Run()
    {
        var population = InitializePopulation();

        for (int generation = 0; generation < Generation; generation++)
        {
            population = Evolve(population);

            var bestFitness = population.Max(s => s.CalculateFitness());
            Console.WriteLine($"Поколение {generation}: Лучший фитнес = {bestFitness:F2}");

            if (bestFitness > 90) // достижение цели(оптимальное расписание из поставленных условий)
            {
                break;
            }
        }
        return population.OrderByDescending(s => s.CalculateFitness()).First();
    }

    private List<Schedule> InitializePopulation()   
    {
        var population = new List<Schedule>();

        for (int i = 0; i < PopulationSize; i++)
        {
            var schedule = new Schedule();

            foreach (var subject in _subjects)
            {
                for (int j = 0; j < subject.HoursPerWeek; j++)
                {
                    schedule.Entries.Add(new ScheduleEntry
                    {
                        Subject = subject,
                        Teacher = subject.AssingedTeacher,
                        Classroom = _classrooms[_random.Next(_classrooms.Count)],
                        DayOfWeek = _random.Next(5),
                        TimeSlots = _random.Next(4)
                    });
                }
            }
            population.Add(schedule);
        }
        return population;
    }

    private List<Schedule> Evolve(List<Schedule> population)
    {
        var ordered = population.OrderByDescending(s => s.CalculateFitness()).ToList();
        
        var nextGeneration = new List<Schedule>();
        nextGeneration.AddRange(ordered.Take(ElitismCount).Select(s => s.Clone()));

        while (nextGeneration.Count < PopulationSize)
        {
            var parent1 = TournamentSelection(nextGeneration);// исправил с ordered
            var parent2 = TournamentSelection(nextGeneration);
            var child = Crossover(parent1, parent2);

            if (_random.NextDouble() < MutationRate)
            {
                Mutate(child);
            }
            nextGeneration.Add(child);
        }
        return nextGeneration;
    }

    private Schedule TournamentSelection(List<Schedule> population)
    {
        var tournament = new List<Schedule>();

        for (int i = 0; i < 5; i++)
        {
            tournament.Add(population[_random.Next(population.Count)]);
        }
        return tournament.OrderByDescending(s => s.CalculateFitness()).First();
    }

    private Schedule Crossover(Schedule p1, Schedule p2)
    {
        var child = new Schedule();
        for (int i = 0; i < p1.Entries.Count; i++)
        {
            child.Entries.Add(_random.NextDouble() < 0.5 ? p1.Entries[i] : p2.Entries[i]);
        }
        return child;
    }

    private void Mutate(Schedule schedule)
    {
        var entry = schedule.Entries[_random.Next((schedule.Entries.Count))];

        int mutationType = _random.Next(3);

        switch (mutationType)
        {
            case 0:
                entry.DayOfWeek = _random.Next(5);
                entry.TimeSlots = _random.Next(4);
                break;
            case 1:
                entry.Classroom = _classrooms[_random.Next(_classrooms.Count)];
                break;
            case 2:
                entry.Teacher = _teachers[_random.Next(_teachers.Count)];
                break;
        }
    }
}