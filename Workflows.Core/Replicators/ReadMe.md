# Replicator Infrastructure

This folder contains the core logic for “replicator” workflows — workflows that dynamically create multiple user tasks per phase (e.g., approval chains, signings).

## How does it work?

<-- paste the "How do BuildPhases and Replicator infrastructure fit together?" section here -->

## Key Concepts

- **ReplicatorPhaseBuilder:** Generates phases/tasks based on workflow context.
- **ReplicatorPhaseConfiguration:** Defines how to create tasks for a given phase.
- **ActivityProvider:** Creates activity instances for each task.
- **ReplicatorManager:** Orchestrates task execution and phase completion.
- **ReplicatorState:** Stores the current progress/status for each phase in the workflow context.

## Getting Started

- To add a new replicator-based workflow, inherit from `ReplicatorActivityWorkflowBase`.
- Override `CreateReplicatorPhaseBuilder` and define your phases/task logic.
- Use the activity provider to implement any phase-specific behavior.

## Further Reading

- [ReplicatorActivityWorkflowBase.cs](./ReplicatorActivityWorkflowBase.cs)
- [ReplicatorPhaseBuilder.cs](./ReplicatorPhaseBuilder.cs)
- [ReplicatorPhaseConfiguration.cs](./ReplicatorPhaseConfiguration.cs)
- [Your domain workflows that use replicator pattern]

