# TravianBotSharp Copilot Instructions

## Project Overview
TravianBotSharp (TBS) is a Windows-only bot for Travian that automates gameplay via Selenium browser automation. It uses a **handler-based architecture** with task scheduling, command orchestration, and HTML parsing.

## Architecture

### Core Pattern: Handlers + Tasks + Commands
The project uses **Immediate.Handlers** library with a custom constraint hierarchy:

**Request Types** (all inherit from `IConstraint`):
- `ICommand`: Atomic actions, decorated with `[Handler]` attribute
- `ITask`: Scheduled automation workflows, also use `[Handler]` pattern
- Scoped by: `IAccountCommand`, `IVillageCommand`, `IAccountVillageCommand`

**Command Structure** (see [LoginCommand.cs](MainCore/Commands/Features/LoginCommand.cs)):
```csharp
[Handler]
public static partial class LoginCommand {
    public sealed record Command(AccountId AccountId) : IAccountCommand;
    
    private static async ValueTask<Result> HandleAsync(
        Command command,
        IChromeBrowser browser,  // Injected dependencies
        AppDbContext context,
        CancellationToken cancellationToken) { /* ... */ }
}
```

**Task Hierarchy** (see [LoginTask.cs](MainCore/Tasks/LoginTask.cs)):
- `BaseTask` → `AccountTask` → Specific tasks (e.g., `LoginTask`)
- Tasks orchestrate multiple commands
- Handlers are auto-registered via `[Handler]` attribute

### Behavior Pipeline (Middleware-like Decorators)
Applied in assembly attribute (see [AppMixins.cs](MainCore/AppMixins.cs#L7-L16)):
1. `AccountDataLoggingBehavior`: Adds account context to logs
2. `TaskNameLoggingBehavior`: Logs task execution
3. `CommandLoggingBehavior`: Logs command execution
4. `ErrorLoggingBehavior`: Logs errors
5. `AccountTaskBehavior`: Checks login state, updates account/village info
6. `VillageTaskBehavior`: Ensures village context

Behaviors enforce pre-conditions before task/command execution (see [AccountTaskBehavior.cs](MainCore/Behaviors/AccountTaskBehavior.cs#L26-L50)).

### Error Handling: FluentResults
- Returns `Result` or `Result<T>` from handlers
- Custom error types: `Stop`, `Skip`, `Retry`, `Cancel`, `MissingBuilding`, `LackOfFreeCrop`, etc.
- `Stop`: Fatal error, halt bot
- `Skip`: Reschedule task for later
- `Retry`: Repeat immediately

## Key Dependencies & Patterns

| Component | Purpose | Key Detail |
|-----------|---------|-----------|
| **Selenium** | Browser automation | Wrapped by `IChromeBrowser` service |
| **HtmlAgilityPack** | HTML parsing | Used by `MainCore.Parsers` module |
| **EF Core + SQLite** | Data persistence | Configured in [AppMixins.cs#L36-L42](MainCore/AppMixins.cs#L36-L42) |
| **Specifications** (Ardalis) | Query builders | See [VillagesSpec.cs](MainCore/Specifications/VillagesSpec.cs) |
| **FluentValidation** | Request validation | Auto-registered in [AppMixins.cs#L50](MainCore/AppMixins.cs#L50) |
| **Serilog** | Structured logging | Account-enriched logs, daily rotation to `./logs/` |
| **ReactiveUI** | Reactive patterns | Used in UI layer |

## Critical Conventions

### Naming & Sealing (Enforced by ArchUnitNET tests)
- **Commands**: Must be `sealed record Command(...)` ending with "Command" suffix
- **Tasks**: Must be sealed `class Task : AccountTask/VillageTask`, naming flexible
- **Handlers**: Must be `private static async ValueTask<Result> HandleAsync(...)`
- **Specifications**: Inherit from `Specification<TEntity, TKey>`

### Architecture Tests ([ArchitectureTest.cs](MainCore.Test/ArchitectureTest.cs))
Enforces naming conventions via reflection. Before adding new commands/tasks:
- Commands must be sealed records
- Tasks must be sealed classes
- Handlers must be static methods named `HandleAsync`

### Data Transfer
- **DTOs**: Light objects for client communication (see [AccountDetailDto.cs](MainCore/DTO/AccountDetailDto.cs))
- **Entities**: Database models (see [MainCore/Entities/](MainCore/Entities/))
- **Mapping**: Use Riok.Mapperly for DTO ↔ Entity conversion

### Parsers: HTML Extraction
All Travian HTML parsing in [MainCore/Parsers/](MainCore/Parsers/). Examples:
- `LoginParser.IsIngamePage()`: Checks for server time element
- `LoginParser.GetUsernameInput()`: Finds login form
- Pattern: Static methods return `HtmlNode?`, use HtmlAgilityPack DOM traversal

## Data Flow

```
UI Request → Task Scheduled
    ↓
Task.HandleAsync() executes
    ↓
Behaviors check pre-conditions (login, village context)
    ↓
Task orchestrates Commands
    ↓
Commands interact with:
  - IChromeBrowser: Selenium-backed browser
  - AppDbContext: SQLite access
  - Parsers: Extract data from HTML
    ↓
Result (success/error) bubbles up
    ↓
Behaviors log and update state
```

## Development Workflows

### Running the Application
- **Build**: `dotnet build` (Windows only via RuntimeIdentifier)
- **Debug**: F5 in Visual Studio (starts WPFUI project)
- **Test**: `dotnet test MainCore.Test` (runs ArchUnitNET architecture + unit tests)

### Adding New Features

**Step 1: Create Command**
```csharp
// MainCore/Commands/Features/MyFeature/MyCommand.cs
[Handler]
public static partial class MyCommand {
    public sealed record Command(AccountId AccountId) : IAccountCommand;
    
    private static async ValueTask<Result> HandleAsync(
        Command command,
        IChromeBrowser browser,
        AppDbContext context,
        CancellationToken cancellationToken) {
        // Implement using browser.Click, browser.Input, browser.GetElement
        // Parse response with HtmlAgilityPack or Parsers
        return Result.Ok(); // or Result.Fail(error)
    }
}
```

**Step 2: Create Task** (if needed as automation workflow)
```csharp
// MainCore/Tasks/MyTask.cs
[Handler]
public static partial class MyTask {
    public sealed class Task(AccountId accountId) : AccountTask(accountId) {
        protected override string TaskName => "My Task";
    }
    
    private static async ValueTask<Result> HandleAsync(
        Task task,
        MyCommand.Handler myCommand,
        IChromeBrowser browser,
        CancellationToken cancellationToken) {
        var result = await myCommand.HandleAsync(new(task.AccountId), cancellationToken);
        if (result.IsFailed) return result;
        return Result.Ok();
    }
}
```

**Step 3: Update Tests**
- Add validator tests for input validation
- Ensure command/task naming follows conventions (ArchUnitNET will verify)

### Database Queries
Use Specifications pattern via [Ardalis.Specification](https://github.com/ardalis/Specification):
```csharp
var spec = new VillagesSpec(accountId);
var villages = await context.ApplySpecification(spec).ToListAsync();
```

### Adding Parsers
- Static methods in `MainCore.Parsers.*`
- Use HtmlAgilityPack: `doc.GetElementbyId()`, `Descendants()`, class/attribute selectors
- Return `HtmlNode?` for optional elements
- Pattern: `GetXxx()` for element finders, `IsXxx()` for boolean checks

## Common Gotchas

1. **Account Context Missing**: Ensure `IDataService` is initialized with `AccountId` before creating `IChromeBrowser` (see [AppMixins.cs#L56-L61](MainCore/AppMixins.cs#L56-L61))
2. **Login State**: `AccountTaskBehavior` auto-injects `LoginTask` if not in-game; check with `LoginParser.IsIngamePage()`
3. **Storage SQLite Connection**: Shared cache mode allows concurrent access; no multi-process restrictions
4. **Result Error Propagation**: Always use `if (result.IsFailed) return result;` pattern—don't ignore failures
5. **HTML Parsing Fragility**: Travian UI changes break parsers; add fallback selectors in parsers

## Key Files Reference

| File | Purpose |
|------|---------|
| [AppMixins.cs](MainCore/AppMixins.cs) | DI registration, Serilog setup, DbContext config |
| [MainCore.csproj](MainCore/MainCore.csproj) | Dependencies, build configuration |
| [Constraints/](MainCore/Constraints/) | Interfaces defining Request hierarchy |
| [Behaviors/](MainCore/Behaviors/) | Pre/post-execution logic for tasks/commands |
| [Services/](MainCore/Services/) | Chrome, Timer, Delay, Logger, Settings, Data services |
| [Parsers/](MainCore/Parsers/) | Travian HTML extraction logic |
| [Specifications/](MainCore/Specifications/) | EF query builders |
| [Errors/](MainCore/Errors/) | Custom error types for Result |

## Testing Strategy
- **Unit Tests**: Validator tests, integration tests with fake DbContext ([FakeDbContextFactory.cs](MainCore.Test/FakeDbContextFactory.cs))
- **Architecture Tests**: Enforce naming/sealing conventions via ArchUnitNET ([ArchitectureTest.cs](MainCore.Test/ArchitectureTest.cs))
- **Integration**: Small, test-specific scenarios; full end-to-end testing via manual bot runs

---
Last updated: 2026-02-28 | Architecture: Handler Pattern + Task Orchestration + HTML Parsing
