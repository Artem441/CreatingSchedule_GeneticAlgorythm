
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CreatingSchedule.Models;
using CreatingSchedule.Services;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;

namespace CreatingSchedule.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    // Коллекции для хранения данных
    public ObservableCollection<Teacher> Teachers { get; } = new();
    public ObservableCollection<Classroom> Classrooms { get; } = new();
    public ObservableCollection<Subject> Subjects { get; } = new();
    public ObservableCollection<Group> Groups { get; } = new();
    public ObservableCollection<string> GroupNames => new(Groups.Select(g => g.Name));
    public ObservableCollection<ScheduleEntry> ScheduleForSelectedGroup { get; } = new();
    public ObservableCollection<ScheduleEntry> Entries { get; } = new();
    public ObservableCollection<TeacherAssignment> Assignments { get; } = new();
    public ObservableCollection<TimeSlotScheduleViewModel> TimeSlotSchedules { get; } = new();

    // Свойства для ввода данных
    [ObservableProperty] private string _newTeacherName = string.Empty;
    [ObservableProperty] private string _newSubjectName = string.Empty;
    [ObservableProperty] private string _newClassroomName = string.Empty;
    [ObservableProperty] private string _selectedGroup = string.Empty;
    [ObservableProperty] private string _newAssignmentTeacher = string.Empty;
    [ObservableProperty] private string _newAssignmentSubject = string.Empty;
    [ObservableProperty] private int _newAssignmentHours = 2;
    [ObservableProperty] private bool _hasSchedule;
    [ObservableProperty] private int _populationSize = 450;
    [ObservableProperty] private int _generations = 300;
    [ObservableProperty] private double _mutationRate = 0.5;

    private GeneticSheduler? _generatedSchedule;
    private Schedule? _finalSchedule;

    // Команды
    public ICommand RemoveTeachingAssignmentCommand { get; }
    public ICommand RemoveClassroomCommand { get; }

    public MainWindowViewModel()
    {
        // Инициализация команд
        RemoveTeachingAssignmentCommand = new RelayCommand<TeacherAssignment>(assignment =>
        {
            if (assignment != null)
            {
                Assignments.Remove(assignment);
                SyncFromAssignments();
            }
        });

        RemoveClassroomCommand = new RelayCommand<Classroom>(classroom =>
        {
            if (classroom != null)
            {
                Classrooms.Remove(classroom);
            }
        });

        // Инициализация тестовых данных
        InitializeDefaultData();
        SelectedGroup = Groups.First().Name;
    }

    private void InitializeDefaultData()
    {
        // Добавление групп
        for (int i = 1; i <= 4; i++)
        {
            Groups.Add(new Group($"Group {453500 + i}"));
        }

        // Добавление учителей и предметов
        var defaultAssignments = new[]
        {
            ("Зоя Николавена", "МА", 3),
            ("Олег Иванович", "ОВА", 2),
            ("Сан-Саныч", "ФизК", 2),
            ("Егор Геннадьевич", "ОАиП", 1),
            ("Игорь Иванович", "Программирование", 2),
            ("Наталья Евгеньевна", "БелЯз", 2),
            ("Александр Васильевич", "Физика", 2),
            ("Татьяна Владимировна", "ИнЯз", 2),
            ("Наталья Геннадьевна", "ДМ", 2),
            ("Виталий Васильевичк", "К.Ч", 1)
        };

        foreach (var (teacherName, subjectName, hours) in defaultAssignments)
        {
            var teacher = new Teacher(teacherName);
            Teachers.Add(teacher);
            Subjects.Add(new Subject(subjectName, hours, teacher));
            Assignments.Add(new TeacherAssignment(teacherName, subjectName, hours));
        }

        // Добавление аудиторий
        for (int i = 1; i <= 15; i++)
        {
            Classrooms.Add(new Classroom($"Classroom {i}"));
        }
    }

    [RelayCommand]
    private void GenerateSchedule()
    {
        _generatedSchedule = new GeneticSheduler(Subjects.ToList(), Teachers.ToList(), Classrooms.ToList(), Groups.ToList())
        {
            PopulationSize = PopulationSize,
            Generations = Generations, 
            MutationRate = MutationRate
        };

        _finalSchedule = _generatedSchedule.Run();
        HasSchedule = _finalSchedule != null && _finalSchedule.Entries.Any();
        UpdateScheduleForSelectedGroup();
    }

    [RelayCommand]
    private void AddTeacherSubject()
    {
        if (!string.IsNullOrWhiteSpace(NewTeacherName) && !string.IsNullOrWhiteSpace(NewSubjectName))
        {
            var teacher = new Teacher(NewTeacherName);
            Teachers.Add(teacher);
            Subjects.Add(new Subject(NewSubjectName, 2, teacher));
            Assignments.Add(new TeacherAssignment(NewTeacherName, NewSubjectName, 2));
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
        Entries.Clear();
        TimeSlotSchedules.Clear();
        _generatedSchedule = null;
        _finalSchedule = null;
        HasSchedule = false;
    }

    [RelayCommand]
    private void AddTeacherAssignment()
    {
        if (!string.IsNullOrWhiteSpace(NewAssignmentTeacher) && !string.IsNullOrWhiteSpace(NewAssignmentSubject) && NewAssignmentHours > 0)
        {
            Assignments.Add(new TeacherAssignment(NewAssignmentTeacher, NewAssignmentSubject, NewAssignmentHours));
            NewAssignmentTeacher = string.Empty;
            NewAssignmentSubject = string.Empty;
            NewAssignmentHours = 2;
            SyncFromAssignments();
        }
    }

    [RelayCommand]
    private void ClearTeachingAssignment()
    {
        Assignments.Clear();
        SyncFromAssignments();
    }

    [RelayCommand]
    private void ClearClassroom()
    {
        Classrooms.Clear();
    }

    private void SyncFromAssignments()
    {
        Teachers.Clear();
        Subjects.Clear();

        foreach (var assign in Assignments)
        {
            var teacher = new Teacher(assign.TeacherName);
            Teachers.Add(teacher);
            Subjects.Add(new Subject(assign.SubjectName, assign.HoursPerWeek, teacher));
        }
    }

    partial void OnSelectedGroupChanged(string value)
    {
        if (_finalSchedule != null)
        {
            UpdateScheduleForSelectedGroup();
        }
    }

    public void UpdateScheduleForSelectedGroup()
    {
        TimeSlotSchedules.Clear();
        ScheduleForSelectedGroup.Clear();

        if (_finalSchedule == null || string.IsNullOrEmpty(SelectedGroup))
            return;

        var currentGroup = Groups.FirstOrDefault(g => g.Name == SelectedGroup);
        if (currentGroup == null)
            return;

        var slotTimeSlots = new Dictionary<int, string>
        {
            { 0, "09:00\n-\n10:20" },
            { 1, "10:35\n-\n11:55" },
            { 2, "12:25\n-\n13:45" },
            { 3, "14:00\n-\n15:20" },
            { 4, "15:50\n-\n17:10" }
        };

        foreach (var slot in slotTimeSlots)
        {
            var slotModel = new TimeSlotScheduleViewModel
            {
                TimeTable = slot.Value,
                Monday = new List<ScheduleEntry>(),
                Tuesday = new List<ScheduleEntry>(),
                Wednesday = new List<ScheduleEntry>(),
                Thursday = new List<ScheduleEntry>(),
                Friday = new List<ScheduleEntry>()
            };

            var entriesInSlot = _finalSchedule.Entries
                .Where(e => e.Group.Name == SelectedGroup && e.TimeSlot == slot.Key)
                .ToList();

            foreach (var entry in entriesInSlot)
            {
                ScheduleForSelectedGroup.Add(entry);
                switch (entry.DayOfWeek)
                {
                    case 0: slotModel.Monday.Add(entry); break;
                    case 1: slotModel.Tuesday.Add(entry); break;
                    case 2: slotModel.Wednesday.Add(entry); break;
                    case 3: slotModel.Thursday.Add(entry); break;
                    case 4: slotModel.Friday.Add(entry); break;
                }
            }
            TimeSlotSchedules.Add(slotModel);
        }

        // Логирование для отладки
        var totalHours = Subjects.Sum(s => s.HoursPerWeek);
        var scheduledHours = ScheduleForSelectedGroup.Count;
        //Console.WriteLine($"Группа {SelectedGroup}: Запланировано {scheduledHours} из {totalHours} пар");
    }
}