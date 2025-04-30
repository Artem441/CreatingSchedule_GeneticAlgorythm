
using System.Collections.ObjectModel;
using CreatingSchedule.Models;
using CreatingSchedule.Services;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;




namespace CreatingSchedule.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Teacher> Teachers { get; set; } = new();
    public ObservableCollection<Classroom> Classrooms { get; set; } = new();
    public ObservableCollection<Subject> Subjects { get; set; } = new();
    public ObservableCollection<Group> Groups { get; set; } = new();
    public ObservableCollection<string> GroupNames => new(Groups.Select(g => g.Name));
    public ObservableCollection<ScheduleEntry> ScheduleForSelectedGroup { get; set; } = new();
    
    [ObservableProperty]
    private string newTeacherName = string.Empty;
    
    [ObservableProperty]
    private string newSubjectName = string.Empty;
    
    [ObservableProperty]
    private string newClassroomName = string.Empty;
    
    [ObservableProperty]
    private string selectedGroup = string.Empty;
    
    
    private GeneticSheduler? _generatedSchedule;

    [ObservableProperty]
    private int populationSize = 300;
    
    [ObservableProperty] 
    private int generations = 2000;
    
    [ObservableProperty] 
    private double mutationRate = 0.3;

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

        for (int i = 1; i <= 10; i++)
        {
            Classrooms.Add(new Classroom($"Classroom {i}"));
        }
        SelectedGroup = Groups.First().Name;
        GenerateSchedule();
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

        _generatedSchedule.Run(); // Прогоняем только 1 раз, кешируем данные
        UpdateScheduleForGroup(); // Заполняем расписание для выбранной группы
    }

    [RelayCommand]
    private void AddTeacherSubject()
    {
        if (!string.IsNullOrWhiteSpace(NewTeacherName) && !string.IsNullOrWhiteSpace(NewSubjectName))
        {
            var teacher = new Teacher(NewTeacherName);
            Teachers.Add(teacher);
            Subjects.Add(new Subject(NewSubjectName,2,teacher));
            NewTeacherName = string.Empty;
            NewSubjectName = string.Empty;
        }
    }

    [RelayCommand]
    private void AddClassroom()
    {
        if (!string.IsNullOrWhiteSpace(NewClassroomName))
        {
            Classrooms.Add(new Classroom(NewClassroomName));
            NewClassroomName = string.Empty;
        }
    }

    [RelayCommand]
    private void Reset()
    {
        ScheduleForSelectedGroup.Clear();
        _generatedSchedule = null;
    }

    partial void OnSelectedGroupChanged(string value)
    {
        UpdateScheduleForGroup();
    }

    private void UpdateScheduleForGroup()
    {
        if (_generatedSchedule == null || string.IsNullOrWhiteSpace(SelectedGroup)) return;
        
        var entries = _generatedSchedule
            .Run()
            .Entries
            .Where(e => e.Group.Name == SelectedGroup)
            .OrderBy(e => e.DayOfWeek)
            .ThenBy(e => e.TimeSlot);
        
        ScheduleForSelectedGroup.Clear();
        foreach (var entry in entries)
        {
            ScheduleForSelectedGroup.Add(entry);
        }
    }
}