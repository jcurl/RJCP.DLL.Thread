# RJCP Thread Library <!-- omit in toc -->

The RJCP.Threading library was introduced due to a problem with .NET not having
an implementation of `ITask<T>` which allows for covariant interfaces.

It is based on the information by [Extending the Async Methods in
C#](https://devblogs.microsoft.com/premier-developer/extending-the-async-methods-in-c/).

- [1. Features](#1-features)
  - [1.1. Task Interface](#11-task-interface)
    - [1.1.1. Why is this needed](#111-why-is-this-needed)
    - [1.1.2. Introducing an Interface to allow Covariance](#112-introducing-an-interface-to-allow-covariance)
  - [1.2. Task Group](#12-task-group)
  - [1.3. Processes](#13-processes)
    - [1.3.1. Unit Testing](#131-unit-testing)
    - [1.3.2. Executable](#132-executable)
    - [1.3.3. Conclusion](#133-conclusion)
- [2. Release History](#2-release-history)
  - [2.1. Version 0.3.0](#21-version-030)
  - [2.2. Version 0.2.1](#22-version-021)
  - [2.3. Version 0.2.0](#23-version-020)

## 1. Features

### 1.1. Task Interface

#### 1.1.1. Why is this needed

.NET 4.x introduced the `Task<T>` type, which represents operations that can run
on different contexts that return values at some later point in time. In .NET
4.5, they keyword `async` and `await` were introduced that provide an easy to
read asynchronous programming model with the language realizing this in the
background using state machines.

While this model brought significant improvements to the language, it was no
longer possible to define interfaces using covariant types. For example, the
language does not allow:

```csharp
namespace RJCP.Threading.Tasks.Covariance
{
    public interface ILineReader<out T> where T : ILine
    {
        Task<T> GetLineAsync();
    }
}
```

The code above will not compile, unless the `out` keyword is removed. This
prevents code such as the following from being written:

```csharp
namespace RJCP.Threading.Tasks
{
    using System;
    using System.Threading.Tasks;

    public interface ILine {
        string Text { get; }
    }

    public interface ILineReader<T> where T : ILine {
        Task<T> GetLineAsync();
    }

    public class Line : ILine {
        public string Text { get { return "Text"; } }
    }

    public class LineReader : ILineReader<Line> {
        public async Task<Line> GetLineAsync() {
            await Task.Delay(1);
            return new Line();
        }
    }

    public static class LineModule {
        public static async Task PrintLine() {
            // This line has the error:
            //  CS0266: Cannot implicitly convert type 'RJCP.Threading.Tasks.LineReader'
            //  to 'RJCP.Threading.Tasks.ILineReader<RJCP.Threading.Tasks.ILine>'. An
            //  explicit conversion exists (are you missing a cast?)
            ILineReader<ILine> reader = new LineReader();
            ILine line = await reader.GetLineAsync();

            Console.WriteLine("{0}", line.Text);
        }
    }
}
```

The error is raised because the interface `ILineReader` doesn't have a covariant
type `T`. It must be `ILineReader<Line>` for the code to compile. But then if
you have a new class `LineReader2` that has `T : Line2 : ILine`, it can't be
assigned to `reader` as the types are again incompatible.

#### 1.1.2. Introducing an Interface to allow Covariance

However, with the existence of an interface `ITask`, we can now make the type
`T` covariant, and the method works as expected.

```csharp
namespace RJCP.Threading.Tasks {
    using System;
    using System.Threading.Tasks;

    public interface ILine {
        string Text { get; }
    }

    public interface ILineReader<out T> where T : ILine {
        ITask<T> GetLineAsync();
    }

    public class Line : ILine {
        public string Text { get { return "Text"; } }
    }

    public class LineReader : ILineReader<Line> {
        public async ITask<Line> GetLineAsync() {
            await Task.Delay(1);
            return new Line();
        }
    }

    public static class LineModule {
        public static async ITask PrintLine() {
            ILineReader<ILine> reader = new LineReader();
            ILine line = await reader.GetLineAsync();

            Console.WriteLine("{0}", line.Text);
        }
    }
}
```

Where possible, avoid the usage of the `ITask` as it is slower.

### 1.2. Task Group

A `TaskGroup` is a simple collection to reduce boiler-plate code when waiting on
multiple tasks.

Create a `TaskGroup` and `RegisterTask(Task)` to have the task group be able to
wait on the tasks.

### 1.3. Processes

The core of the functionality is in the `RunProcess` task. The easiest way is to
create a `RunProcess` class through one of the static methods:

- `Run(string command, params string[] arguments)`
- `RunFrom(string command, string workDir, params string[] arguments)`
- `RunAsync(string command, params string[] arguments)`
- `RunAsync(string command, string[] arguments, CancellationToken token)`
- `RunFromAsync(string command, string workDir, params string[] arguments)`
- `RunFromAsync(string command, string workDir, string[] arguments, CancellationToken token)`

The static methods return a `RunProcess` or a `Task<RunProcess>` that can be
awaited on. The process runs until completion, and then the result is returned.

You can instantiate the class with one of the constructors, and then call
`Execute` or await with `ExecuteAsync`, which blocks until the process exits.
You can register to delegates before executing the process to get feedback.

#### 1.3.1. Unit Testing

Through object inheritance, it is possible to construct a `RunProcess` object,
and pass it to a method that executes the process, by simulation only.

It works by providing your own class and providing a simulation delegate.

```csharp
internal class GetDirSimProcess : RunProcess
{
    private static int GetDirSim(RunProcess process, string command, string arguments, CancellationToken token)
    {
        GetDirSimProcess p = (GetDirSimProcess)process;

        p.LogStdOut(" Volume in drive C has no label.");
        p.LogStdOut(" Volume Serial Number is 3A2G-7Z2W");
        p.LogStdOut(string.Empty);
        p.LogStdOut($" Directory of {p.WorkingDirectory}");
        p.LogStdOut(string.Empty);
        p.LogStdOut("06/08/2021  18:31    <DIR>          .");
        p.LogStdOut("06/08/2021  18:31    <DIR>          ..");
        p.LogStdOut("25/05/2021  17:59            29,048 testhost.dll");
        p.LogStdOut("25/05/2021  18:00           149,360 testhost.exe");
        p.LogStdOut("              49 File(s)      6,918,578 bytes");
        p.LogStdOut("              17 Dir(s)  830,622,584,832 bytes free");
        return 0;
    }

    public GetDirSimProcess(string command, string workDir, string arguments)
        : base(GetDirSim, command, workDir, arguments) { }
}
```

#### 1.3.2. Executable

A wrapper class `Executable` is written to abstract away `RunProcess`, that one
can build classes based on tool behaviour. Using `RunProcess` simulation for
testing can be used to test multiple different versions of tools in your test
suite, without needing the actual tools (only the inputs and outputs copied
into the test code).

#### 1.3.3. Conclusion

It is then possible to create tools that can automatically detect their
location, and either execute function (like a GIT tool, that embeds in the class
how to find the git binary, and then the command lines needed to execute).

## 2. Release History

### 2.1. Version 0.3.0

Features:

- Process: A wrapper `RunProcess` that asynchronously handles processes and
  their output (DOTNET-1086)
- Executable: Create tools around `RunProcess` with the `Executable` class
  (DOTNET-1088)

### 2.2. Version 0.2.1

Bugfixes:

- TaskGroup: Remove asynchronous behaviour of `TaskCompletionSource` (fails on
  Linux) (DOTNET-970)

Quality:

- Add README.md reference to NuGet package (DOTNET-815)
- Tasks: Fix unnecessary guard around HashSet (DOTNET-833)
- Upgrade from .NET Standard 2.1 to .NET 6.0 (DOTNET-936, DOTNET-941,
  DOTNET-942)

### 2.3. Version 0.2.0

- Initial Version
