
using System.Collections.ObjectModel;
using CreatingSchedule.Models;
using CreatingSchedule.Services;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia.Interactivity;


namespace CreatingSchedule.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public ObservableCollection<Teacher> Teachers { get; set; } = new();
    public ObservableCollection<Classroom> Classrooms { get; set; } = new();
    public ObservableCollection<Subject> Subjects { get; set; } = new();
    public ObservableCollection<Group> Groups { get; set; } = new();
    public ObservableCollection<string> GroupNames => new(Groups.Select(g => g.Name));
    public ObservableCollection<ScheduleEntry> ScheduleForSelectedGroup { get; set; } = new(); // для бизнесс логики
    
    public ObservableCollection<ScheduleEntry> Entries { get; set; } = new(); // для табличного отображения в UI
    

    public ObservableCollection<TeacherAssignment> Assignments { get; set; } = new();
    
    
    [ObservableProperty]
    private string newTeacherName = string.Empty;
    
    [ObservableProperty]
    private string newSubjectName = string.Empty;
    
    [ObservableProperty]
    private string newClassroomName = string.Empty;
    
    [ObservableProperty]
    private string selectedGroup = string.Empty;
    
    [ObservableProperty]
    private string newAssignmentTeacher = string.Empty;

    [ObservableProperty]
    private string newAssignmentSubject = string.Empty;

    [ObservableProperty]
    private bool _hasSchedule;

    [ObservableProperty] private int newAssignmentHours = 2;
    
    
    private GeneticSheduler? _generatedSchedule;
    private Schedule? _finalSchedule;

    [ObservableProperty]
    private int populationSize = 300;
    
    [ObservableProperty] 
    private int generations = 2000;
    
    [ObservableProperty] 
    private double mutationRate = 0.3;
    public ICommand RemoveTeachingAssignmentCommand { get; }
    
    public ICommand RemoveClassroomCommand { get; }
    public MainWindowViewModel()
    {
        RemoveTeachingAssignmentCommand = new RelayCommand<TeacherAssignment>(assignment =>
        {
            if (assignment != null)
            {
                Assignments.Remove(assignment);
                SyncFromAssignments();
            }
        });
        RemoveClassroomCommand = new RelayCommand<Classroom>(Classroom =>
        {
            if (Classrooms != null)
            {
                Classrooms.Remove(Classroom);
            }
        });
        // Инициализация по умолчанию
        Groups.Add(new Group($"Group {453501}"));
        Groups.Add(new Group($"Group {453502}"));
        Groups.Add(new Group($"Group {453503}"));
        Groups.Add(new Group($"Group {453504}"));
        Groups.Add(new Group($"Group {453505}"));
        
        var teacher = new Teacher("Зоя Николавена"); 
        Teachers.Add(teacher);
        Subjects.Add(new Subject("МА", 3, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"Мат.Анализ",3));
        teacher = new Teacher("Олег Иванович");
        Teachers.Add(teacher);
        Subjects.Add(new Subject("ОВА", 2, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"ОВА",2));
        teacher = new Teacher("Сан-Саныч");
        Teachers.Add(teacher);
        Subjects.Add(new Subject("ФизК", 2, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"ФизК",2));
        teacher = new Teacher("Егор Геннадьевич");
        Teachers.Add(teacher);
        Subjects.Add(new Subject("ОАиП", 1, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"ОАиП",1));
        teacher = new Teacher("Игорь Иванович");
        Teachers.Add(teacher);
        Subjects.Add(new Subject("Программирование", 2, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"Программирование",2));
        teacher = new Teacher("Наталья Евгеньевна");
        Teachers.Add(teacher);
        Subjects.Add(new Subject("БелЯз", 2, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"БелЯз",2));
        teacher = new Teacher("Александр Васильевич");
        Teachers.Add(teacher);
        Subjects.Add(new Subject("Физика", 2, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"Физика",2));
        teacher = new Teacher("Татьяна Владимировна");
        Teachers.Add(teacher);
        Subjects.Add(new Subject("ИнЯз", 2, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"ИнЯз",2));
        teacher = new Teacher("Наталья Геннадьевна");
        Teachers.Add(teacher);
        Subjects.Add(new Subject("ДМ", 2, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"ДМ",2));
        teacher = new Teacher("Виталий Васильевичк");
        Teachers.Add(teacher);
        Subjects.Add(new Subject("К.Ч", 1, teacher));
        Assignments.Add(new TeacherAssignment(teacher.Name,"К.Ч",1));

        for (int i = 1; i <= 10; i++)
        {
            Classrooms.Add(new Classroom($"Classroom {i}"));
        }
        SelectedGroup = Groups.First().Name;
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

        _finalSchedule = _generatedSchedule.Run();
        HasSchedule = _finalSchedule != null && _finalSchedule.Entries.Any();
        UpdateScheduleForSelectedGroup(); // Заполняем расписание для выбранной группы
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
        _finalSchedule = null;
        HasSchedule = false;
    }

    partial void OnSelectedGroupChanged(string value)
    {
        if (_finalSchedule != null)
        {
            UpdateScheduleForSelectedGroup();
        }
    }

    private void UpdateScheduleForGroup()
    {
        if (_finalSchedule == null || string.IsNullOrWhiteSpace(SelectedGroup)) return;
        
        var entries = _finalSchedule
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
    public ObservableCollection<TimeSlotScheduleViewModel> TimeSlotSchedules { get; } = new();
    
    public void UpdateScheduleForSelectedGroup()
    {
        TimeSlotSchedules.Clear();

        if (_finalSchedule == null || string.IsNullOrEmpty(SelectedGroup))
        {
            return;
        }
        
        var slotTimeSlots = new Dictionary<int, string>
        {
            { 0, "09:00\n-\n10.20" },
            { 1, "10:35\n-\n11.55" },
            { 2, "12:25\n-\n13.45" },
            { 3, "14:00\n-\n15.20" },
            { 4, "15:50\n-\n17.10" }
        };

        var currentGroup = Groups.FirstOrDefault(g => g.Name == SelectedGroup);
        if (currentGroup == null) return;
        
        
        foreach (var slot in slotTimeSlots)
        {
            var slotModel = new TimeSlotScheduleViewModel
            {
                TimeTable = slot.Value,
                Monday = new List<ScheduleEntry>(),
                Tuesday = new List<ScheduleEntry>(),
                Wednesday = new List<ScheduleEntry>(),
                Thursday = new List<ScheduleEntry>(),
                Friday = new List<ScheduleEntry>(),
            };
            
            var entriesInSlot = _finalSchedule.Entries.Where(e => e.Group.Name == SelectedGroup && e.TimeSlot == slot.Key).ToList();
            foreach (var entry in entriesInSlot)
            {
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
    }

    [RelayCommand]
    private void AddTeacherAssignment()
    {
        if (!string.IsNullOrWhiteSpace(NewAssignmentTeacher) && !string.IsNullOrWhiteSpace(NewAssignmentSubject) &&
            NewAssignmentHours > 0)
        {
            Assignments.Add(new TeacherAssignment(NewAssignmentTeacher,NewAssignmentSubject,NewAssignmentHours));
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
            
            var subject = new Subject(assign.SubjectName, assign.HoursPerWeek, teacher);
            Subjects.Add(subject);
        }
    }
    //[RelayCommand]
   // private void RemoveClassroom(Classroom classroom)
    //{
      //  Classrooms.Remove(classroom);
    //}
    
}