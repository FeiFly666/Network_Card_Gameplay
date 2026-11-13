using System.Text;
using System.Text.Json;

namespace GameServer.Monitoring;

internal sealed class MonitorChannel
{
    internal const string SnapshotCommand = "monitor snapshot";
    private const string LaunchArgument = "--monitor";
    private const string SnapshotPrefix = "GAMESERVER_MONITOR ";
    private static readonly TimeSpan SnapshotTimeout = TimeSpan.FromSeconds(3);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private TaskCompletionSource<ServerSnapshot>? _pendingSnapshot;

    internal static MonitorChannel? Create(string[] args)
    {
        if (!args.Contains(LaunchArgument, StringComparer.Ordinal))
            return null;

        Console.InputEncoding = new UTF8Encoding(false);
        Console.OutputEncoding = new UTF8Encoding(false);
        return new MonitorChannel();
    }

    // Called by the console command loop, which accepts one request at a time.
    internal void WriteSnapshot()
    {
        var request = new TaskCompletionSource<ServerSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        Volatile.Write(ref _pendingSnapshot, request);

        try
        {
            var snapshot = request.Task.WaitAsync(SnapshotTimeout).GetAwaiter().GetResult();
            Console.WriteLine(SnapshotPrefix + JsonSerializer.Serialize(snapshot, JsonOptions));
        }
        catch (TimeoutException)
        {
            Console.Error.WriteLine("[monitor] Snapshot request timed out.");
        }
        catch (Exception error)
        {
            Console.Error.WriteLine($"[monitor] Snapshot request failed: {error.Message}");
        }
        finally
        {
            Interlocked.CompareExchange(ref _pendingSnapshot, null, request);
        }
    }

    // The game loop copies live state; the console thread serializes the copy.
    internal void CompletePendingSnapshot(Func<ServerSnapshot> capture)
    {
        var request = Interlocked.Exchange(ref _pendingSnapshot, null);
        if (request is null)
            return;

        try
        {
            request.SetResult(capture());
        }
        catch (Exception error)
        {
            request.SetException(error);
        }
    }
}
