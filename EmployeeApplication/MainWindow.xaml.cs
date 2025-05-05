using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HotelDBLibrary;
using HotelDBLibrary.Models;

using HotelTask = HotelDBLibrary.Models.Task;
using Task = System.Threading.Tasks.Task;

namespace EmployeeApplication;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    
    private readonly HotelDbContext _context;
    private readonly TaskManager _taskManager;
    
    private readonly List<string> _roles = new List<string> {TaskManager.TypeCleaning, TaskManager.TypeService, TaskManager.TypeMaintenance};
    private readonly List<string> _statuses = new List<string> {TaskManager.StatusPending, TaskManager.StatusInProgress, TaskManager.StatusCompleted};
    
    public MainWindow()
    {
        InitializeComponent();

        try
        {
            _context = new HotelDbContext();
            _taskManager = new TaskManager(_context);
        }
        catch (Exception e)
        {
            MessageBox.Show($"Failed to initialize database connection: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            throw;
        }
        
        RoleComboBox.ItemsSource = _roles;
        StatusComboBox.ItemsSource = _statuses;
    }

    private async void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (RoleComboBox.SelectedItem is string selectedRole)
        {
            StatusTextBlock.Text = $"Loading tasks for {selectedRole}...";
            TasksListView.ItemsSource = null;
            ClearEditArea();
            EditTaskGrid.IsEnabled = false;

            try
            {
                List<HotelTask> tasks = await _taskManager.GetTasksByTypeAsync(selectedRole);
                
                TasksListView.ItemsSource = tasks;
                StatusTextBlock.Text = $"Loaded {tasks.Count} tasks for {selectedRole}. Select a task to edit.";
                TaskReloadButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = $"Failed to load tasks for {selectedRole}.";
            }
        }
        else
        {
            TasksListView.ItemsSource = null;
            ClearEditArea();
            EditTaskGrid.IsEnabled = false;
            StatusTextBlock.Text = "Please select a valid role.";
        }
    }

    private void ClearEditArea()
    {
        SelectedTaskTextBlock.Text = "(no task selected)";
        StatusComboBox.SelectedItem = "";
        NotesTextBox.Text = "";
    }

    private void TasksListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TasksListView.SelectedItem is HotelTask selectedTask)
        {
            EditTaskGrid.IsEnabled = true;
            SelectedTaskTextBlock.Text = $"Room {selectedTask.RoomId} - {selectedTask.Task1}";
            StatusComboBox.SelectedItem = selectedTask.Status;
            StatusTextBlock.Text = "Select a task to edit";
        }
        else
        {
            ClearEditArea();
            EditTaskGrid.IsEnabled = false;
            StatusTextBlock.Text = "Select a task to edit";
        }
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        if (TasksListView.SelectedItem is HotelTask selectedTask && StatusComboBox.SelectedItem is string newStatus)
        {
            string? newNotes = NotesTextBox.Text;

            StatusTextBlock.Text = $"Saving changes for task with id: {selectedTask.Id}";
            SaveButton.IsEnabled = false;

            try
            {
                bool success = await _taskManager.UpdateTaskStatusAndNotesAsync(selectedTask.Id, newStatus, newNotes);

                if (success)
                {
                    StatusTextBlock.Text = $"Task with id: {selectedTask.Id} updated successfully.";

                    await ReladTasksForCurrentRole();
                }
                else
                {
                    StatusTextBlock.Text = $"Failed to update task with id: {selectedTask.Id}.";
                    MessageBox.Show("Failed to save changes", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show($"Error saving changes: {exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = $"Error saving changes for task with id: {selectedTask.Id}.";
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }
        else
        {
            MessageBox.Show("Please select a task and status before saving", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ReladTasksForCurrentRole()
    {
        if (RoleComboBox.SelectedItem is string selectedRole)
        {
            StatusTextBlock.Text = $"Reloading tasks for {selectedRole}...";
            TasksListView.ItemsSource = null;
            ClearEditArea();
            EditTaskGrid.IsEnabled = false;
            TaskReloadButton.IsEnabled = false;

            try
            {
                List<HotelTask> tasks = await _taskManager.GetTasksByTypeAsync(selectedRole);
                TasksListView.ItemsSource = tasks;
                StatusTextBlock.Text = $"Reloaded {tasks.Count} tasks for {selectedRole}.";
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error reloading tasks: {e.Message}", "Error", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                StatusTextBlock.Text = $"Failed to reload tasks for {selectedRole}.";
            }
            finally
            {
                TaskReloadButton.IsEnabled = true;
            }
        }
    }

    private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        await ReladTasksForCurrentRole();
    }
}