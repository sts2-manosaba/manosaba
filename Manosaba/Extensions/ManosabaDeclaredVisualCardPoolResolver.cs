using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Models;

namespace Manosaba.Extensions;

/// <summary>
/// Resolves <see cref="CardModel.VisualCardPool"/> from the card type's <c>[Pool(typeof(TCardPool))]</c> attribute so
/// shared canonical cards (e.g. Common) are not stuck with whichever character pool <see cref="CardModel.Pool"/> picked first.
/// </summary>
internal static class ManosabaDeclaredVisualCardPoolResolver
{
    private static readonly ConcurrentDictionary<Type, CardPoolModel?> Cache = new();

    private static readonly MethodInfo ModelDbCardPoolOpenGeneric = typeof(ModelDb)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m.Name == nameof(ModelDb.CardPool) && m.IsGenericMethodDefinition && m.GetParameters().Length == 0);

    internal static CardPoolModel? TryResolveDeclaredVisualPool(Type cardConcreteType)
    {
        return Cache.GetOrAdd(cardConcreteType, ResolveUncached);
    }

    private static CardPoolModel? ResolveUncached(Type cardType)
    {
        Type? poolType = ExtractDeclaredPoolType(cardType);
        if (poolType == null || !typeof(CardPoolModel).IsAssignableFrom(poolType))
            return null;

        try
        {
            MethodInfo closed = ModelDbCardPoolOpenGeneric.MakeGenericMethod(poolType);
            return (CardPoolModel)closed.Invoke(null, null)!;
        }
        catch
        {
            return null;
        }
    }

    private static Type? ExtractDeclaredPoolType(Type cardType)
    {
        foreach (CustomAttributeData cad in cardType.GetCustomAttributesData())
        {
            if (!cad.AttributeType.Name.EndsWith("PoolAttribute", StringComparison.Ordinal)
                && !cad.AttributeType.Name.Equals("Pool", StringComparison.Ordinal))
                continue;

            if (cad.ConstructorArguments.Count == 0)
                continue;

            if (cad.ConstructorArguments[0].Value is Type t)
                return t;
        }

        return null;
    }
}
