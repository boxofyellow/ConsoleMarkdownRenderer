# Migration Guide: v0.8.x → v0.9.0

Version 0.9.0 introduces two related changes: `MarkdownDisplayer` now implements `IDisposable`, and the static `Displayer` facade no longer holds a long-lived internal instance.

## Why

Previously, `MarkdownDisplayer` created an internal `HttpClient` lazily and never disposed it.  While the single static instance in `Displayer` made this effectively harmless, the addition of `IHttpClientFactory` support means callers can now create multiple `MarkdownDisplayer` instances.  Each self-managed `HttpClient` must be disposed to release socket resources.

## What Changed

### `MarkdownDisplayer` implements `IDisposable`

`MarkdownDisplayer` now implements `IDisposable`.  When the instance manages its own `HttpClient` (i.e. no `IHttpClientFactory` was supplied), calling `Dispose()` releases that client.  When a factory was supplied the caller owns the client lifetime and `Dispose()` is a no-op for the HTTP resources.

**Before (v0.8.x)**

```csharp
var displayer = new MarkdownDisplayer();
await displayer.DisplayMarkdownAsync(uri);
// HttpClient was never released
```

**After (v0.9.0)**

```csharp
using var displayer = new MarkdownDisplayer();
await displayer.DisplayMarkdownAsync(uri);
// HttpClient is disposed when the using block exits
```

For DI scenarios the lifetime is controlled by the container, so register `MarkdownDisplayer` as a scoped or transient service and let the container dispose it:

```csharp
services.AddScoped<IMarkdownDisplayer, MarkdownDisplayer>();
```

### `Displayer` static facade — no more singleton

`Displayer.DisplayMarkdownAsync` previously delegated to a single static `MarkdownDisplayer` instance.  It now creates a fresh instance per call and disposes it before returning.

**Behavioral impact:** If your application called `Displayer.DisplayMarkdownAsync` many times in a tight loop the connection pool now cycles once per call rather than being reused across calls.  For typical documentation-display use cases this has no observable effect.  If you need connection reuse across calls, use `IMarkdownDisplayer` directly and manage the lifetime yourself.

```csharp
// This is unchanged at the call site — no migration required
await Displayer.DisplayMarkdownAsync(uri);
```

## `new MarkdownDisplayer().DefaultPipeline` — consider making a static helper

Several places in the code construct a throwaway `MarkdownDisplayer` solely to access `DefaultPipeline`:

```csharp
var pipeline = new MarkdownDisplayer().DefaultPipeline;
```

Since `DefaultPipeline` does not depend on any instance state (no HTTP client is involved), this pattern leaks a disposable.  Although harmless in practice (the client is only created lazily on first HTTP use), it is cleaner to wrap the pipeline in a static helper or to reuse a single instance.  These call sites have been enumerated below for the library maintainer to decide the best fix:

- `ConsoleMarkdownRenderer.Tests/RendererTests.cs` line 63
- `ConsoleMarkdownRenderer.Tests/RendererTests.cs` line 271
- `ConsoleMarkdownRenderer.Tests/RendererTests.cs` line 306
