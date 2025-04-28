using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Services;

namespace SimpleWorkflow;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.AddDbContext<WorkflowDbContext>(opts => opts.UseInMemoryDatabase("MinimizedDb"), 
                    ServiceLifetime.Singleton);
                services.AddDbContextFactory<WorkflowDbContext>(opts => opts.UseInMemoryDatabase("MinimizedDb"),
                    ServiceLifetime.Scoped);

                services.AddScoped<ITaskService, TaskService>();
                services.AddScoped<IHistoryService, HistoryService>();
                services.AddScoped<IWorkflowService, WorkflowService>();

                services.AddScoped<IWorkflowMachine, SimpleWorkflow>();
                services.AddSingleton<IWorkflowDefinition, HelloWorkflowDefinition>();
            })
            .Build();

        await host.StartAsync();

        using (IServiceScope scope = host.Services.CreateScope())
        {
            await RunDemoAsync(scope.ServiceProvider);
        }

        await host.StopAsync();
    }

    private static async Task RunDemoAsync(IServiceProvider sp)
    {
        // 1. Load services
        var ts    = sp.GetRequiredService<ITaskService>();
        var hs    = sp.GetRequiredService<IHistoryService>();
        var ws    = sp.GetRequiredService<IWorkflowService>();
        var logger  = sp.GetRequiredService<ILogger<WorkflowBase<SimpleWorkflowContext>>>();
        var definition = sp.GetRequiredService<IWorkflowDefinition>();

        // 2. Populate data
        (int documentId, WorkflowTemplate template) = await DbSeed.EnsureInitialDataAsync(sp, definition);

        // 3. Create state machine
        var workflow = new SimpleWorkflow(ws, ts, hs, logger);

        // 4. Start workflow
        var startPayload = new StartWorkflowPayload
        {
            WorkflowBaseId     = new Guid(template.WorkflowBaseId),
            Originator         = "console-user",
            DocumentId         = new DocumentId(documentId),
            WorkflowTemplateId = new WorkflowTemplateId(template.Id),
            Storage            = new Dictionary<string, string?>
            {
                ["ReplicatorType"] = "1", // Jadamisi
                ["AssociationName"] = template.TemplateName,
                ["Description"] = "Just a simple task",
                ["DueDays"] =  "1",
                ["AssignedTo"] = string.Join(";#", new List<string?>{ "Mr.Musk" })
            }
        };
        
        Result<Unit> startResult = await workflow.StartAsync(startPayload, CancellationToken.None);
        if (!startResult.Succeeded)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"⚠️ Start failed: {startResult.Error}");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        // 5. End workflow
        await workflow.TriggerTransitionAsync(WorkflowTrigger.AllTasksCompleted, CancellationToken.None);
    }
}

/// <summary>
/// Each workflow must have a definition.
/// </summary>
internal sealed class HelloWorkflowDefinition : IWorkflowDefinition
{
    public Type Type            => typeof(SimpleWorkflow);
    public Type ContextType     => typeof(SimpleWorkflowContext);
    public string Name          => Type.Name;
    public string DisplayName   => "Simple Workflow";
    public string Id            => "6ad97055-1b6a-4e4f-92d7-097203cb24aa";

    public WorkflowDefinition GetDefinition() => new()
    {
        Id   = Id,
        Name = Name,
        DisplayName = DisplayName,
        Actions = ["Complete", "Reject"]
    };

    // no replicator phases for this tiny workflow
    public ReplicatorConfiguration? ReplicatorConfiguration => null;
}

internal static class DbSeed
{
    public static async Task<(int documentId, WorkflowTemplate template)> EnsureInitialDataAsync(IServiceProvider sp, IWorkflowDefinition def)
    {
        var db = sp.GetRequiredService<WorkflowDbContext>();

        var library = new Library { Name = "Test Library", Created = DateTime.UtcNow };
        db.Libraries.Add(library);

        var document = new Uptime.Workflows.Core.Data.Document
        {
            Title       = "Contract",
            CreatedBy   = "John Doe",
            Library     = library,
            Created     = DateTime.UtcNow
        };
        db.Documents.Add(document);

        var template = new WorkflowTemplate
        {
            TemplateName        = "Contract Approval",
            WorkflowName        = def.Name,
            WorkflowBaseId      = def.Id,
            Library             = library,
            AssociationDataJson = "{}",
            Created             = DateTime.UtcNow,
            Modified            = DateTime.UtcNow
        };
        db.WorkflowTemplates.Add(template);
        await db.SaveChangesAsync();

        return (document.Id, template);
    }
}

/// <summary>
/// Workflow context is a data object that contains all the data needed to run a workflow after reHydration.
/// Context data is stored into database.
/// </summary>
internal class SimpleWorkflowContext : WorkflowContext
{
    public UserTaskActivityData? ActivityData { get; set; }

    /// <summary>
    /// Each workflow context may contains different data objects.
    /// For that we need to provide particular deserialization implementation.
    /// </summary>
    /// <param name="json"></param>
    public override void Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<SimpleWorkflowContext>(json);
        if (obj != null)
        {
            Storage = obj.Storage;
        }
    }
}

/// Each workflow must have:
/// 1. workflow definition implementation
/// 2. state machine configuration implementation
/// The services for communicating with the workflow data layer are implemented in the core library but are replaceable.
internal sealed class SimpleWorkflow(IWorkflowService workflowService, ITaskService taskService, IHistoryService historyService, ILogger<WorkflowBase<SimpleWorkflowContext>> logger)
    : WorkflowBase<SimpleWorkflowContext>(workflowService, taskService, historyService, logger)
{
    protected override IWorkflowDefinition WorkflowDefinition => new HelloWorkflowDefinition();

    protected override void ConfigureStateMachineAsync(CancellationToken _)
    {
        Machine.Configure(BaseState.NotStarted)
            .Permit(WorkflowTrigger.Start, BaseState.InProgress);
        Machine.Configure(BaseState.InProgress)
            .Permit(WorkflowTrigger.AllTasksCompleted, BaseState.Completed);
    }

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload payload, CancellationToken __)
    {
        if (payload.Storage.TryGetValue("AssignedTo", out string? assignedTo))
        {
            string? taskDescription = payload.Storage.GetValue("Description");
            string? dueDays = payload.Storage.GetValue("DueDays");

            _ = int.TryParse(dueDays, out int days);

            WorkflowContext.ActivityData = new UserTaskActivityData
            {
                AssignedBy = payload.Originator,
                AssignedTo = assignedTo!,
                TaskDescription = taskDescription,
                DueDate = DateTime.Now.AddDays(days)
            };
        }

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"'{WorkflowDefinition.DisplayName}' started");
        Console.WriteLine($"Task created to '{WorkflowContext.ActivityData!.AssignedTo}' by {WorkflowContext.ActivityData!.AssignedBy}");
        Console.WriteLine($"Description: '{WorkflowContext.ActivityData!.TaskDescription}', DeadLine: {WorkflowContext.ActivityData.DueDate}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"{WorkflowDefinition.DisplayName} completed");
        Console.ForegroundColor = ConsoleColor.White;

        return Task.CompletedTask;
    }
}