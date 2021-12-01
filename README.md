# RJCP Thread Library

The RJCP.Threading library was introduced due to a problem with .NET not having
an implementation of `ITask<T>` which allows for covariant interfaces. This is
to support my `RJCP.Diagnostics.Log` library.

It is based on the information by [Extending the Async Methods in
C#](https://devblogs.microsoft.com/premier-developer/extending-the-async-methods-in-c/).
