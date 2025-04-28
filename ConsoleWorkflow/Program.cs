using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SigningWorkflow;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Services;
using static SigningWorkflow.Constants;
using Unit = Uptime.Workflows.Core.Common.Unit;

namespace ConsoleWorkflow;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
                builder.AddFilter("SigningWorkflow", LogLevel.Information);
                builder.AddFilter("Uptime.Workflows",                   LogLevel.Information);
                builder.AddFilter("Microsoft.EntityFrameworkCore",      LogLevel.Warning); 
            })
            .ConfigureServices((ctx, services) =>
            {
                services.AddDbContext<WorkflowDbContext>(opts => opts.UseInMemoryDatabase("MinimizedDb"), 
                    ServiceLifetime.Singleton);
                services.AddDbContextFactory<WorkflowDbContext>(opts => opts.UseInMemoryDatabase("MinimizedDb"),
                    ServiceLifetime.Scoped);

                services.AddScoped<ITaskService, TaskService>();
                services.AddScoped<IHistoryService, HistoryService>();
                services.AddScoped<IWorkflowService, WorkflowService>();

                services.AddScoped<SigningWorkflow.SigningWorkflow>();
                services.AddSingleton<IWorkflowDefinition, SigningWorkflowDefinition>();

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
        #region 1. Load services
        PrintMessage("Preparing services");

        var db = sp.GetRequiredService<WorkflowDbContext>();
        var definition = sp.GetRequiredService<IWorkflowDefinition>();
        var workflow = sp.GetRequiredService<SigningWorkflow.SigningWorkflow>(); 

        #endregion

        #region 2. Populate data
        PrintMessage("Populating data");

        (int documentId, WorkflowTemplate template) = await DbSeed.EnsureInitialDataAsync(db, definition);

        #endregion

        #region 3. Start workflow
        PrintMessage("Starting workflow");

        var startPayload = new StartWorkflowPayload
        {
            WorkflowBaseId     = new Guid(template.WorkflowBaseId),
            Originator         = "Console-user",
            DocumentId         = new DocumentId(documentId),
            WorkflowTemplateId = new WorkflowTemplateId(template.Id),
            Storage            = new Dictionary<string, string?>
            {
                [WorkflowStorageKeys.AssociationName] = template.TemplateName,
                [TaskStorageKeys.TaskDescription] = "Sign contract",
                [TaskStorageKeys.TaskDueDays] =  "1",
                [TaskStorageKeys.TaskSigners] = string.Join(";#", new List<string?>{ "Mr.Musk" })
            }
        };
        
        Result<Unit> startResult = await workflow.StartAsync(startPayload, CancellationToken.None);
        if (!startResult.Succeeded)
        {
            PrintMessage($"⚠️ Start failed: {startResult.Error}", ConsoleColor.Red);
            return;
        }

        #endregion

        #region 4. Simulate task completion
        PrintMessage("Altering task");

        WorkflowTask workflowTask = await db.WorkflowTasks
            .Include(t => t.Workflow)
            .ThenInclude(w => w.WorkflowTemplate)
            .FirstAsync();

        Result<Unit> rehydrateResult = workflow.RehydrateAsync(workflowTask.Workflow, CancellationToken.None);
        if (!rehydrateResult.Succeeded)
        {
            PrintMessage($"⚠️ Rehydration failed: {rehydrateResult.Error}", ConsoleColor.Red);
            return;
        }

        var taskPayload = new Dictionary<string, string?> 
        {
            { TaskStorageKeys.TaskComment, "I signed the contract" },
            { TaskStorageKeys.TaskEditor, "Client Test" },
            { TaskStorageKeys.TaskResult, nameof(WorkflowEventType.TaskCompleted) }
        };

        await workflow.AlterTaskAsync(workflowTask, taskPayload, CancellationToken.None);

        #endregion
    }

    private static void PrintMessage(string message, ConsoleColor color = ConsoleColor.DarkYellow)
    {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ForegroundColor = ConsoleColor.White;
    }
}

internal static class DbSeed
{
    public static async Task<(int documentId, WorkflowTemplate template)> EnsureInitialDataAsync(WorkflowDbContext db, IWorkflowDefinition def)
    {
        var library = new Library { Name = "Test Library", Created = DateTime.UtcNow };
        db.Libraries.Add(library);

        var document = new Document
        {
            Title       = "Contract",
            CreatedBy   = "John Doe",
            Library     = library,
            Created     = DateTime.UtcNow
        };
        db.Documents.Add(document);

        var template = new WorkflowTemplate
        {
            TemplateName        = "Contract Signing",
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