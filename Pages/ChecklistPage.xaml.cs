using BabyPrepRegistry.Models;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BabyPrepRegistry.Pages;

public partial class ChecklistPage : ContentPage
{
    
    public ObservableCollection<TaskModel> TaskList { get; set; }
    public ChecklistPage()
	{
		InitializeComponent();

        TaskList = new();
		List<TaskModel> tasks = new();
		tasks.Add(new TaskModel(03, "BabyShower", "Get gifts"));
        tasks.Add(new TaskModel(15, "Organize Room", "Make Space for the baby!"));
        tasks.Add(new TaskModel(09, "Prepare for baby", "Baby about to go Hello World!"));

        OrganizeByPriority(tasks);

        BindingContext = this;
    }

    public void OrganizeByPriority(List<TaskModel> currentList)
    {
        var sorted = currentList.OrderByDescending(t => t.TaskPriority).ToList();
        TaskList.Clear();
        foreach (var item in sorted)
        {
            TaskList.Add(item);
        }
    }

    private async void onAddTaskClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddTaskPage());
        //Debug.WriteLine("Checklist Clicked");
    }

    private void onEditTaskClicked(object? sender, EventArgs e)
    {
        //Debug.WriteLine("Registry Clicked");
    }
}