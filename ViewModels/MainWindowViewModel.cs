using System;
using System.Collections.ObjectModel;
using CreatingSchedule.Models;
using CreatingSchedule.Services;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;




namespace CreatingSchedule.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Teacher> Teachers { get; set; } = new();
    public ObservableCollection<Classroom> Classrooms { get; set; } = new();
    public ObservableCollection<Subject> Subjects { get; set; } = new();
    public ObservableCollection<Group> Groups { get; set; } = new();
    public ObservableCollection<string> GroupNames { get; set; } = new();
    public ObservableCollection<ScheduleEntry> ScheduleForSelectesGroup { get; set; } = new();
    
    private GeneticSheduler? _generatedSchedule;

    [ObservableProperty] private int populationSize = 300;
    [ObservableProperty] private int generations = 2000;
    [ObservableProperty] private double mutationRate = 0.3;
    [ObservableProperty] private Group? selectedGroup;

    public MainWindowViewModel()
    {
        // Инициализация по умолчанию(вроде если в меню ничего не указать)
        for (int i = 1; i <= 10; i++)
        {
            var teacher = new Teacher($"Teacher {i}");
            Teachers.Add(teacher);
            Subjects.Add(new Subject($"Subject {i}", 2, teacher));
        }

        for (int i = 1; i <= 5; i++)
        {
            Groups.Add(new Group($"Group {i}"));
        }

        Classrooms.Add(new Classroom("104"));
        Classrooms.Add(new Classroom("106"));
        Classrooms.Add(new Classroom("108"));
        Classrooms.Add(new Classroom("320"));
    }

    [RelayCommand]
    private void GenerateSchedule()
    {
        _generatedSchedule = new GeneticSheduler(Subjects.ToList(), Teachers.ToList(), Classrooms.ToList(), Groups.ToList())
        {
            PopulationSize = PopulationSize,
            Generation = Generations,
            MutationRate = MutationRate
        };

        var bestSchedule = _generatedSchedule.Run();
        
        ScheduleForSelectesGroup.Clear();
        foreach (var entry in bestSchedule.Entries) 
        {
            ScheduleForSelectesGroup.Add(entry);
        }
    }
}