using Avalonia.Controls;         
using Avalonia.Interactivity;    
using CreatingSchedule.Models;   
using CreatingSchedule.ViewModels;

namespace CreatingSchedule.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnRemoveAssignmentClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && 
            button.DataContext is TeacherAssignment assignment &&
            DataContext is MainWindowViewModel vm)
        {
            vm.RemoveTeachingAssignmentCommand.Execute(assignment);
        }
    }

    private void OnRemoveClassroomClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is Classroom classroom && DataContext is MainWindowViewModel vm)
        {
            vm.RemoveClassroomCommand.Execute(classroom);
        }
    }
}