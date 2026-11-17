// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("ImmediateHandler", "IHR0012:Handler method should use CancellationToken", Scope = "module")]
[assembly: SuppressMessage("Minor Code Smell", "S1210:\"Equals\" and the comparison operators should be overridden when implementing \"IComparable\"", Scope = "module")]
[assembly: SuppressMessage("Major Code Smell", "S6966:Awaitable method should be used", Scope = "module")]