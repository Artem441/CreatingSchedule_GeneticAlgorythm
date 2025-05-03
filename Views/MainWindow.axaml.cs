using Avalonia.Controls;         // Для Window, Button
using Avalonia.Interactivity;    // Для RoutedEventArgs
using CreatingSchedule.Models;   // Для TeacherAssignment
using CreatingSchedule.ViewModels; // Для MainWindowViewModel

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
}