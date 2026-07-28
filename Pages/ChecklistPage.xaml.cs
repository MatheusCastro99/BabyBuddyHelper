using BabyPrepRegistry.Models;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace BabyPrepRegistry.Pages;

public partial class ChecklistPage : ContentPage
{
    
    public ObservableCollection<TaskModel> TaskList { get; set; }
    List<TaskModel> tasks = new();
    public bool isPendingFirst {  get; set; } = false;
    public ChecklistPage()
	{
		InitializeComponent();

        TaskList = new();
		
		tasks.Add(new TaskModel(7, "BabyShower 07", "Get gifts"));
        tasks.Add(new TaskModel(5, "Organize Room 05", "Make Space for the baby!"));
        tasks.Add(new TaskModel(10, "Prepare for baby 10", "Baby about to go Hello World!"));

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

    public void OrganizeByPending(List<TaskModel> currentList)
    {
        var sorted = currentList
            .OrderBy(t => t.IsCompleted)
            .ThenByDescending(t => t.TaskPriority)
            .ToList();

        TaskList.Clear();
        foreach (var item in sorted)
        {
            TaskList.Add(item);
        }
    }

    public void IsPendingFirstHandler(object sender, ToggledEventArgs e)
    {
        if (isPendingFirst)
        {
            OrganizeByPending(tasks);
        }
        else 
        {
            OrganizeByPriority(tasks);
        }
    }

    private async void onAddTaskClicked(object? sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new AddTaskPage(tasks, RefreshTaskList));
        //Debug.WriteLine("Checklist Clicked");
    }

    private void onEditTaskClicked(object? sender, EventArgs e)
    {
        //Debug.WriteLine("Registry Clicked");
    }

    private void RefreshTaskList()
    {
        OrganizeByPriority(tasks);
    }
}