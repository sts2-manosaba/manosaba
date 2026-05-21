using System.Reflection;
using HarmonyLib;

namespace Manosaba.Patches;

internal static class LobbyDifficultyUiPatchUtil
{
    private const BindingFlags CleanUpLobbyFlags =
        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;

    /// <summary>
    /// Resolves private <c>CleanUpLobby</c> across game versions (v0.107: bool only; beta: bool + NetError).
    /// </summary>
    public static MethodBase? ResolveCleanUpLobbyMethod(Type screenType)
    {
        MethodInfo? single = AccessTools.DeclaredMethod(screenType, "CleanUpLobby", [typeof(bool)]);
        if (single != null)
        {
            return single;
        }

        foreach (MethodInfo method in screenType.GetMethods(CleanUpLobbyFlags))
        {
            if (method.Name != "CleanUpLobby")
            {
                continue;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length >= 1 && parameters[0].ParameterType == typeof(bool))
            {
                return method;
            }
        }

        return AccessTools.Method(screenType, "CleanUpLobby", [typeof(bool)]);
    }
}
