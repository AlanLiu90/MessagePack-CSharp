// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text;
using Microsoft.CodeAnalysis;

namespace MessagePack.SourceGenerator.Transforms;

/// <summary>
/// Predicts, at compile time, the key that the generated resolver computes from a <see cref="System.Type"/> at runtime.
/// </summary>
/// <remarks>
/// The key is the namespace-qualified CLR name of the type: <c>+</c> between nested types, a <c>`N</c> arity suffix,
/// generic arguments as <c>[A,B]</c> and arrays as <c>[]</c> or <c>[,]</c>. At runtime it is built only from
/// <see cref="System.Type.FullName"/> of non-generic types and generic type definitions, which every runtime formats the same way.
/// Looking formatters up by this key, instead of by a table of <c>typeof(...)</c> values, means the runtime only has to
/// load the type that is actually requested.
/// </remarks>
internal static class RuntimeTypeKey
{
    /// <summary>
    /// Gets the runtime key for a closed type.
    /// </summary>
    /// <returns><see langword="false"/> when the runtime name cannot be predicted from source, such as for types that contain type parameters.</returns>
    internal static bool TryGetKey(QualifiedTypeName type, out string key)
    {
        StringBuilder builder = new();
        bool success = TryAppendKey(type, builder);
        key = builder.ToString();
        return success;
    }

    /// <summary>
    /// 64-bit FNV-1a over UTF-16 code units. The generated <c>GetTypeKeyHash</c> method must compute the same value.
    /// </summary>
    internal static ulong Hash(string key)
    {
        ulong hash = 14695981039346656037UL;
        foreach (char ch in key)
        {
            hash = (hash ^ ch) * 1099511628211UL;
        }

        return hash;
    }

    private static bool TryAppendKey(QualifiedTypeName type, StringBuilder builder)
    {
        switch (type)
        {
            case QualifiedArrayTypeName array:
                if (!TryAppendKey(array.ElementType, builder))
                {
                    return false;
                }

                builder.Append('[').Append(',', array.ArrayRank - 1).Append(']');
                return true;

            case QualifiedNamedTypeName named when named.Kind != TypeKind.TypeParameter:
                List<QualifiedTypeName> typeArguments = new();
                if (!TryAppendDefinitionName(named, builder, typeArguments))
                {
                    return false;
                }

                if (typeArguments.Count > 0)
                {
                    builder.Append('[');
                    for (int i = 0; i < typeArguments.Count; i++)
                    {
                        if (i > 0)
                        {
                            builder.Append(',');
                        }

                        if (!TryAppendKey(typeArguments[i], builder))
                        {
                            return false;
                        }
                    }

                    builder.Append(']');
                }

                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// Appends what <see cref="System.Type.FullName"/> returns for the type, or for its generic type definition,
    /// and collects the type arguments of the type and its containing types (outermost first), as <see cref="System.Type.GetGenericArguments"/> returns them.
    /// </summary>
    private static bool TryAppendDefinitionName(QualifiedNamedTypeName named, StringBuilder builder, List<QualifiedTypeName> typeArguments)
    {
        switch (named.Container)
        {
            case NestingTypeContainer { NestingType: { } nesting }:
                if (!TryAppendDefinitionName(nesting, builder, typeArguments))
                {
                    return false;
                }

                builder.Append('+');
                break;
            case NamespaceTypeContainer { Namespace: { } ns }:
                if (ns.Contains("::"))
                {
                    // An extern alias is not part of the runtime name.
                    return false;
                }

                builder.Append(ns).Append('.');
                break;
        }

        builder.Append(named.Name);
        if (named.Arity > 0)
        {
            builder.Append('`').Append(named.Arity);
        }

        if (named.TypeArguments.Length != named.Arity)
        {
            return false;
        }

        typeArguments.AddRange(named.TypeArguments);
        return true;
    }
}
