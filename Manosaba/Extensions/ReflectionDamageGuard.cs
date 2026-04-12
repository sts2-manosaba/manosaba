using System;
using System.Threading;
using System.Threading.Tasks;

namespace Manosaba.Extensions;

internal static class ReflectionDamageGuard
{
    private static readonly AsyncLocal<int> _depth = new();

    public static bool IsActive => _depth.Value > 0;

    public static async Task Run(Func<Task> action)
    {
        _depth.Value++;
        try
        {
            await action();
        }
        finally
        {
            _depth.Value--;
        }
    }
}

