using System.Collections.ObjectModel;
using CreatingSchedule.Models;
using CreatingSchedule.Services;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;




namespace CreatingSchedule.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly List<Teacher> _teachers = new()
    {
        new Teacher("Ivanov"),
        new Teacher("Sobytilkin"),
        new Teacher("Michael"),
        new Teacher("Anastasia"),
    };

    private readonly List<Classroom> _classrooms = new()
    {
        new Classroom("Aуд. 104"),
        new Classroom("Aуд. 106"),
        new Classroom("Aуд. 108"),
    };

    private readonly List<Subject> _subjects;

    private GeneticSheduler? _scheduler;

    public ObservableCollection<ScheduleEntry> ScheduleEntries { get; set; } = new();

    private int _populationSize = 200;

    public int PopulationSize
    {
        get => _populationSize;
        set => SetProperty(ref _populationSize, value);
    }

    private int _generations = 1000;

    public int Generations
    {
        get => _generations;
        set => SetProperty(ref _generations, value);
    }

    private double _multionRate = 0.3;

    public double MutationRate
    {
        get => _multionRate;
        set => SetProperty(ref _multionRate, value);
    }
    
    public IRelayCommand GenerateScheduleCommand { get; }
    public MainWindowViewModel()
    {
        _subjects = new List<Subject>()
        {
            new Subject("Математический Анализ",5,_teachers[0]),
            new Subject("Физика",4,_teachers[1]),
            new Subject("ОАИП",4,_teachers[2]),
            new Subject("Пргограммирование",3,_teachers[3])
        };
        GenerateScheduleCommand = new RelayCommand(GenerateSchedule);
    }

    private void GenerateSchedule()
    {
        _scheduler = new GeneticSheduler(_subjects,_teachers,_classrooms)
        {
            PopulationSize = PopulationSize,
            Generation = Generations,
            MutationRate = MutationRate
        };

        var bestSchedule = _scheduler.Run();
        
        ScheduleEntries.Clear();
        foreach (var entry in bestSchedule.Entries)
        {
            ScheduleEntries.Add(entry);
        }
    }
    
}