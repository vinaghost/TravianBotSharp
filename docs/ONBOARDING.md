# TravianBotSharp Onboarding Guide

## Project Overview

TravianBotSharp is a Windows-only Travian automation bot built on .NET 8 with WPF, ReactiveUI, Entity Framework Core, Selenium, and Serilog. The repo is split into a desktop shell, a core automation/runtime layer, and tests that enforce host and architecture rules. The main story is: start the shell, prepare browser and persistence services, sync live game state, schedule tasks, execute automation, and keep everything observable and recoverable.

## Architecture Layers

### Project Overview

Key files:
- [README.md](../README.md)
- [TravBotSharp.sln](../TravBotSharp.sln)
- [MainCore/MainCore.csproj](../MainCore/MainCore.csproj)
- [WPFUI/WPFUI.csproj](../WPFUI/WPFUI.csproj)
- [MainCore.Test/MainCore.Test.csproj](../MainCore.Test/MainCore.Test.csproj)

This layer tells you what the project is and how the solution is assembled.

### Desktop Shell

Key files:
- [WPFUI/App.xaml.cs](../WPFUI/App.xaml.cs)
- [WPFUI/Views/MainWindow.xaml.cs](../WPFUI/Views/MainWindow.xaml.cs)
- [MainCore/UI/ViewModels/MainViewModel.cs](../MainCore/UI/ViewModels/MainViewModel.cs)

This is the WPF entry path and the top-level UI orchestration layer.

### Runtime Core

Key files:
- [MainCore/AppMixins.cs](../MainCore/AppMixins.cs)
- [MainCore/Services/TaskManager.cs](../MainCore/Services/TaskManager.cs)
- [MainCore/Services/RxQueue.cs](../MainCore/Services/RxQueue.cs)
- [MainCore/Services/ChromeManager.cs](../MainCore/Services/ChromeManager.cs)
- [MainCore/Services/ChromeBrowser.cs](../MainCore/Services/ChromeBrowser.cs)
- [MainCore/Services/TimerManager.cs](../MainCore/Services/TimerManager.cs)
- [MainCore/Commands/Update/UpdateAccountInfoCommand.cs](../MainCore/Commands/Update/UpdateAccountInfoCommand.cs)
- [MainCore/Parsers/BuildingTabParser.cs](../MainCore/Parsers/BuildingTabParser.cs)

This layer contains the host setup, scheduling, browser control, parsing, and gameplay automation commands.

### Domain Model

Key files:
- [MainCore/Infrasturecture/Persistence/AppDbContext.cs](../MainCore/Infrasturecture/Persistence/AppDbContext.cs)
- [MainCore/Tasks/Base/BaseTask.cs](../MainCore/Tasks/Base/BaseTask.cs)
- [MainCore/Tasks/Base/AccountTask.cs](../MainCore/Tasks/Base/AccountTask.cs)
- [MainCore/Tasks/Base/VillageTask.cs](../MainCore/Tasks/Base/VillageTask.cs)
- [MainCore/Behaviors/ErrorLoggingBehavior.cs](../MainCore/Behaviors/ErrorLoggingBehavior.cs)
- [MainCore/Behaviors/TaskNameLoggingBehavior.cs](../MainCore/Behaviors/TaskNameLoggingBehavior.cs)
- [MainCore/Behaviors/AccountTaskBehavior.cs](../MainCore/Behaviors/AccountTaskBehavior.cs)
- [MainCore/Behaviors/VillageTaskBehavior.cs](../MainCore/Behaviors/VillageTaskBehavior.cs)
- [MainCore/Entities/](../MainCore/Entities/)

This layer defines the persisted game state, task hierarchy, and cross-cutting execution behaviors.

### Quality Checks

Key files:
- [MainCore.Test/ArchitectureTest.cs](../MainCore.Test/ArchitectureTest.cs)
- [MainCore.Test/HostingTest.cs](../MainCore.Test/HostingTest.cs)
- [MainCore.Test/Commands/Update/UpdateAccountInfoCommandTest.cs](../MainCore.Test/Commands/Update/UpdateAccountInfoCommandTest.cs)
- [MainCore.Test/Parsers/BuildingLayoutParser.Test.cs](../MainCore.Test/Parsers/BuildingLayoutParser.Test.cs)

These tests protect startup, architecture, and parser behavior.

## Key Concepts

- Host-driven startup: [WPFUI/App.xaml.cs](../WPFUI/App.xaml.cs) creates the application host, wires the shell, and starts the app.
- Composition root: [MainCore/AppMixins.cs](../MainCore/AppMixins.cs) configures logging, EF Core, Splat integration, and behaviors.
- Reactive orchestration: [MainCore/UI/ViewModels/MainViewModel.cs](../MainCore/UI/ViewModels/MainViewModel.cs) coordinates browser setup, database hydration, and layout loading.
- Hierarchical tasks: [MainCore/Tasks/Base/BaseTask.cs](../MainCore/Tasks/Base/BaseTask.cs) is the base contract, with account and village specializations layered on top.
- Queue-based automation: [MainCore/Services/TaskManager.cs](../MainCore/Services/TaskManager.cs) orders tasks and publishes status changes through [MainCore/Services/RxQueue.cs](../MainCore/Services/RxQueue.cs).
- Persistence first: [MainCore/Infrasturecture/Persistence/AppDbContext.cs](../MainCore/Infrasturecture/Persistence/AppDbContext.cs) stores the bot’s account, village, building, queue, farm, and setting state.
- Guardrails through tests: [MainCore.Test/ArchitectureTest.cs](../MainCore.Test/ArchitectureTest.cs) and [MainCore.Test/HostingTest.cs](../MainCore.Test/HostingTest.cs) lock in architectural and host-level invariants.

## Guided Tour

1. Start with [README.md](../README.md) and [TravBotSharp.sln](../TravBotSharp.sln) to understand the project’s purpose and structure.
2. Follow the startup path in [WPFUI/App.xaml.cs](../WPFUI/App.xaml.cs), [MainCore/AppMixins.cs](../MainCore/AppMixins.cs), and [WPFUI/Views/MainWindow.xaml.cs](../WPFUI/Views/MainWindow.xaml.cs).
3. Inspect runtime orchestration in [MainCore/UI/ViewModels/MainViewModel.cs](../MainCore/UI/ViewModels/MainViewModel.cs), [MainCore/Services/ChromeManager.cs](../MainCore/Services/ChromeManager.cs), [MainCore/Services/TaskManager.cs](../MainCore/Services/TaskManager.cs), and [MainCore/Infrasturecture/Persistence/AppDbContext.cs](../MainCore/Infrasturecture/Persistence/AppDbContext.cs).
4. Trace live game synchronization in [MainCore/Commands/Update/UpdateAccountInfoCommand.cs](../MainCore/Commands/Update/UpdateAccountInfoCommand.cs) and [MainCore/Parsers/BuildingTabParser.cs](../MainCore/Parsers/BuildingTabParser.cs).
5. Study the domain model in [MainCore/Tasks/Base/BaseTask.cs](../MainCore/Tasks/Base/BaseTask.cs), [MainCore/Tasks/Base/AccountTask.cs](../MainCore/Tasks/Base/AccountTask.cs), [MainCore/Tasks/Base/VillageTask.cs](../MainCore/Tasks/Base/VillageTask.cs), and the files under [MainCore/Entities/](../MainCore/Entities/).
6. Finish with the tests in [MainCore.Test/ArchitectureTest.cs](../MainCore.Test/ArchitectureTest.cs) and [MainCore.Test/HostingTest.cs](../MainCore.Test/HostingTest.cs).

## File Map

### Project and Shell

- [README.md](../README.md) explains the bot’s purpose and supported automation features.
- [TravBotSharp.sln](../TravBotSharp.sln) is the solution root for the three projects.
- [WPFUI/App.xaml.cs](../WPFUI/App.xaml.cs) is the application bootstrap and host entry point.
- [WPFUI/Views/MainWindow.xaml.cs](../WPFUI/Views/MainWindow.xaml.cs) manages the shell window lifecycle.
- [MainCore/AppMixins.cs](../MainCore/AppMixins.cs) is the composition root for services, logging, and persistence.
- [MainCore/UI/ViewModels/MainViewModel.cs](../MainCore/UI/ViewModels/MainViewModel.cs) coordinates startup, database hydration, and shutdown.

### Runtime and Automation

- [MainCore/Services/TaskManager.cs](../MainCore/Services/TaskManager.cs) owns task queues and ordering.
- [MainCore/Services/RxQueue.cs](../MainCore/Services/RxQueue.cs) publishes task and status notifications.
- [MainCore/Services/ChromeManager.cs](../MainCore/Services/ChromeManager.cs) manages browser setup and lifecycle.
- [MainCore/Services/ChromeBrowser.cs](../MainCore/Services/ChromeBrowser.cs) is the browser wrapper used for Travian interaction.
- [MainCore/Services/TimerManager.cs](../MainCore/Services/TimerManager.cs) coordinates scheduled execution.
- [MainCore/Commands/Update/UpdateAccountInfoCommand.cs](../MainCore/Commands/Update/UpdateAccountInfoCommand.cs) refreshes account information from the live game.
- [MainCore/Parsers/BuildingTabParser.cs](../MainCore/Parsers/BuildingTabParser.cs) parses Travian building-page state.

### Domain and Persistence

- [MainCore/Infrasturecture/Persistence/AppDbContext.cs](../MainCore/Infrasturecture/Persistence/AppDbContext.cs) is the local data model and EF Core context.
- [MainCore/Tasks/Base/BaseTask.cs](../MainCore/Tasks/Base/BaseTask.cs) defines the core task contract.
- [MainCore/Tasks/Base/AccountTask.cs](../MainCore/Tasks/Base/AccountTask.cs) and [MainCore/Tasks/Base/VillageTask.cs](../MainCore/Tasks/Base/VillageTask.cs) specialize task scope.
- [MainCore/Behaviors/ErrorLoggingBehavior.cs](../MainCore/Behaviors/ErrorLoggingBehavior.cs), [MainCore/Behaviors/TaskNameLoggingBehavior.cs](../MainCore/Behaviors/TaskNameLoggingBehavior.cs), [MainCore/Behaviors/AccountTaskBehavior.cs](../MainCore/Behaviors/AccountTaskBehavior.cs), and [MainCore/Behaviors/VillageTaskBehavior.cs](../MainCore/Behaviors/VillageTaskBehavior.cs) shape execution behavior and logging.
- The entity files under [MainCore/Entities/](../MainCore/Entities/) model accounts, villages, buildings, jobs, settings, farms, and hero items.

### Tests and Safety Nets

- [MainCore.Test/ArchitectureTest.cs](../MainCore.Test/ArchitectureTest.cs) enforces naming and dependency constraints.
- [MainCore.Test/HostingTest.cs](../MainCore.Test/HostingTest.cs) checks that the DI host can build.
- [MainCore.Test/Commands/Update/UpdateAccountInfoCommandTest.cs](../MainCore.Test/Commands/Update/UpdateAccountInfoCommandTest.cs) covers command behavior.
- [MainCore.Test/Parsers/BuildingLayoutParser.Test.cs](../MainCore.Test/Parsers/BuildingLayoutParser.Test.cs) covers parser behavior.

## Complexity Hotspots

The graph does not include numeric complexity values, so these are inferred hotspots based on responsibility and coupling:

- [MainCore/AppMixins.cs](../MainCore/AppMixins.cs): central DI, logging, EF Core, and behavior composition.
- [MainCore/UI/ViewModels/MainViewModel.cs](../MainCore/UI/ViewModels/MainViewModel.cs): startup orchestration spans browser setup, database work, and UI hydration.
- [MainCore/Services/TaskManager.cs](../MainCore/Services/TaskManager.cs): queue ordering, cancellation, status tracking, and notification emission all meet here.
- [MainCore/Infrasturecture/Persistence/AppDbContext.cs](../MainCore/Infrasturecture/Persistence/AppDbContext.cs): wide schema surface with account, village, queue, and settings defaults.
- [MainCore/Tasks/Base/VillageTask.cs](../MainCore/Tasks/Base/VillageTask.cs): inherits account behavior, adds village scoping, and touches persistence.
- [MainCore/Behaviors/](../MainCore/Behaviors/): cross-cutting behavior logic is easy to misread because it is wired through handlers rather than direct calls.