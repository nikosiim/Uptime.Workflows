using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Uptime.Workflows.Core;
using Uptime.Workflows.Core.Common;
using Uptime.Workflows.Core.Data;
using Uptime.Workflows.Core.Entities;
using Uptime.Workflows.Core.Enums;
using Uptime.Workflows.Core.Interfaces;
using Workflow.Minimized.Stateless;
using System.Text.Json;

namespace Workflow.Minimized;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((ctx, services) =>
            {
                services.AddDbContext<WorkflowDbContext>(opts => opts.UseInMemoryDatabase("MinimizedDb"), ServiceLifetime.Singleton);
                services.AddTransient(typeof(IStateMachineFactory<,>), typeof(StatelessStateMachineFactory<,>));
                services.AddScoped<IWorkflowMachine, HelloWorkflow>();
                services.AddSingleton<IWorkflowDefinition, HelloWorkflowDefinition>();
                services.AddScoped<IWorkflowRepository, Uptime.Application.Common.WorkflowRepository>();
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
        var machine = sp.GetRequiredService<IStateMachineFactory<BaseState, WorkflowTrigger>>();
        var repo    = sp.GetRequiredService<IWorkflowRepository>();
        var logger  = sp.GetRequiredService<ILogger<WorkflowBase<HelloContext>>>();
        var definition = sp.GetRequiredService<IWorkflowDefinition>();

        (int documentId, WorkflowTemplate template) = await DbSeed.EnsureInitialDataAsync(sp, definition);

        var hello = new HelloWorkflow(machine, repo, logger);

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
                ["TaskDescription"] = "Please approve",
                ["TaskDueDate"] =  DateTime.UtcNow.AddDays(1).ToUniversalTime().ToString("o", CultureInfo.InvariantCulture),
                ["TaskExecutors"] = string.Join(";#", new List<string?>{ "Mr.Musk" }),
                ["TaskSigners"] = string.Join(";#", new List<string?>{ "Mr.Trump" })
            }
        };

        Result<Unit> startResult = await hello.StartAsync(startPayload, CancellationToken.None);
        if (!startResult.Succeeded)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"⚠️ Start failed: {startResult.Error}");
            Console.ForegroundColor = ConsoleColor.White;
            return;
        }

        await hello.TriggerTransitionAsync(WorkflowTrigger.AllTasksCompleted, CancellationToken.None);
    }
}

internal sealed class HelloWorkflowDefinition : IWorkflowDefinition
{
    public Type Type         => typeof(HelloWorkflow);
    public Type ContextType  => typeof(HelloContext);
    public string Name       => Type.Name;
    public string DisplayName=> "Hello-World Workflow";
    public string Id         => "6ad97055-1b6a-4e4f-92d7-097203cb24aa";

    public WorkflowDefinition GetDefinition() => new()
    {
        Id   = Id,
        Name = Name,
        DisplayName = DisplayName
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

internal class HelloContext : WorkflowContext
{
    public UserTaskActivityData? HelloTask { get; set; }
    
    public override void Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<HelloContext>(json);
        if (obj != null)
        {
            Storage = obj.Storage;
        }
    }
}

internal sealed class HelloWorkflow(IStateMachineFactory<BaseState, WorkflowTrigger> machine, IWorkflowRepository repo, ILogger<WorkflowBase<HelloContext>> log)
    : WorkflowBase<HelloContext>(machine, repo, log)
{
    protected override IWorkflowDefinition WorkflowDefinition => new HelloWorkflowDefinition();

    protected override void ConfigureStateMachineAsync(CancellationToken _)
    {
        Machine.Configure(BaseState.NotStarted)
            .Permit(WorkflowTrigger.Start, BaseState.InProgress);
        Machine.Configure(BaseState.InProgress)
            .Permit(WorkflowTrigger.AllTasksCompleted, BaseState.Completed);
    }

    protected override void OnWorkflowActivatedAsync(IWorkflowPayload _, CancellationToken __)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Workflow {WorkflowDefinition.DisplayName} Activated");
        Console.ForegroundColor = ConsoleColor.White;
    }

    protected override Task OnWorkflowCompletedAsync(CancellationToken cancellationToken)
    {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine($"Workflow {WorkflowDefinition.DisplayName} Completed");
        Console.ForegroundColor = ConsoleColor.White;

        return Task.CompletedTask;
    }
}