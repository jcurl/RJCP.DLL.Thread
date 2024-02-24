# RJCP Thread Library <!-- omit in toc -->

The RJCP.Threading library was introduced due to a problem with .NET not having
an implementation of `ITask<T>` which allows for covariant interfaces.

It is based on the information by [Extending the Async Methods in
C#](https://devblogs.microsoft.com/premier-developer/extending-the-async-methods-in-c/).

- [1. Why is this Library needed](#1-why-is-this-library-needed)
- [Features](#features)
  - [2. Introducing an Interface to allow Covariance](#2-introducing-an-interface-to-allow-covariance)
  - [Task Group](#task-group)
- [2. Release History](#2-release-history)
  - [2.1. Version 0.2.1](#21-version-021)
  - [2.2. Version 0.2.0](#22-version-020)

## 1. Why is this Library needed

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

## Features

### 2. Introducing an Interface to allow Covariance

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

### Task Group

A `TaskGroup` is a simple collection to reduce boiler-plate code when waiting on
multiple tasks.

Create a `TaskGroup` and `RegisterTask(Task)` to have the task group be able to
wait on the tasks.

## 2. Release History

### 2.1. Version 0.2.1

Bugfixes:

- TaskGroup: Remove asynchronous behaviour of `TaskCompletionSource` (fails on
  Linux) (DOTNET-970)

Quality:

- Add README.md reference to NuGet package (DOTNET-815)
- Tasks: Fix unnecessary guard around HashSet (DOTNET-833)
- Upgrade from .NET Standard 2.1 to .NET 6.0 (DOTNET-936, DOTNET-941,
  DOTNET-942)

### 2.2. Version 0.2.0

- Initial Version
