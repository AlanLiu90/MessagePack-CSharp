// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma warning disable SA1402 // File may only contain a single type

namespace MessagePack.SourceGenerator.Transforms;

public partial class FormatterTemplate : IFormatterTemplate
{
    public FormatterTemplate(AnalyzerOptions options, ObjectSerializationInfo info)
    {
        this.Options = options;
        this.Info = info;
    }

    public AnalyzerOptions Options { get; }

    public QualifiedNamedTypeName ResolverName => this.Options.Generator.Resolver.Name;

    public ObjectSerializationInfo Info { get; }

    ResolverRegisterInfo IFormatterTemplate.Info => this.Info;

    public string FileName => $"{this.Info.FileNameHint}.g.cs";
}

public partial class StringKeyFormatterTemplate : IFormatterTemplate
{
    public StringKeyFormatterTemplate(AnalyzerOptions options, ObjectSerializationInfo info)
    {
        this.Options = options;
        this.Info = info;
    }

    public AnalyzerOptions Options { get; }

    public QualifiedNamedTypeName ResolverName => this.Options.Generator.Resolver.Name;

    public ObjectSerializationInfo Info { get; }

    ResolverRegisterInfo IFormatterTemplate.Info => this.Info;

    public string FileName => $"{this.Info.FileNameHint}.g.cs";
}

public partial class ResolverTemplate
{
    public ResolverTemplate(AnalyzerOptions options, IReadOnlyList<ResolverRegisterInfo> registerInfos)
    {
        this.Options = options;
        this.AllRegisterRegistrations = registerInfos;
    }

    public AnalyzerOptions Options { get; init; }

    public QualifiedNamedTypeName ResolverName => this.Options.Generator.Resolver.Name;

    public IReadOnlyList<ResolverRegisterInfo> AllRegisterRegistrations { get; }

    public IEnumerable<ResolverRegisterInfo> OpenGenericRegistrations => this.AllRegisterRegistrations.Where(r => r.IsUnboundGenericType);

    public IEnumerable<ResolverRegisterInfo> ConstructedTypeRegistrations => this.AllRegisterRegistrations.Where(r => !r.IsUnboundGenericType);

    /// <summary>
    /// The most key hashes, on average, that one generated method switches over. See <see cref="SplitByKeyHash"/>.
    /// </summary>
    private const int MaxAverageKeyHashesPerMethod = 32;

    public string FileName => $"{this.ResolverName.GetQualifiedName(Qualifiers.Namespace, GenericParameterStyle.ArityOnly)}.g.cs";

    /// <summary>
    /// Groups registrations, by their index, under the hash of the runtime key of their data type (see <see cref="RuntimeTypeKey"/>).
    /// More than one index in a group means a hash collision, which the generated code resolves by comparing types.
    /// </summary>
    /// <param name="registrations">The closed registrations.</param>
    /// <param name="unkeyed">Receives the indexes of registrations whose runtime key cannot be predicted.</param>
    private static IReadOnlyList<IGrouping<ulong, int>> GroupByRuntimeTypeKeyHash(IReadOnlyList<ResolverRegisterInfo> registrations, out IReadOnlyList<int> unkeyed)
    {
        List<(ulong Hash, int Index)> keyed = new();
        List<int> unkeyedList = new();
        for (int i = 0; i < registrations.Count; i++)
        {
            if (RuntimeTypeKey.TryGetKey(registrations[i].DataType, out string key))
            {
                keyed.Add((RuntimeTypeKey.Hash(key), i));
            }
            else
            {
                unkeyedList.Add(i);
            }
        }

        unkeyed = unkeyedList;
        return keyed.GroupBy(k => k.Hash, k => k.Index).OrderBy(g => g.Key).ToList();
    }

    /// <summary>
    /// Splits the key hashes into buckets by <c>hash % count</c>, each of which gets a generated method that switches over its hashes.
    /// </summary>
    /// <param name="keyedRegistrations">The result of <see cref="GroupByRuntimeTypeKeyHash"/>.</param>
    /// <returns>The buckets, by index. Some may be empty. A single bucket means the hashes need no splitting.</returns>
    /// <remarks>
    /// A JIT compiles a whole method the first time it runs, so one switch over every key hash makes the first lookup
    /// pay for all of them. Mono's JIT also takes quadratic time over the large sparse switch that the C# compiler
    /// turns into a binary search, because each of its many branch-only blocks restarts a pass over the method.
    /// With one method per bucket, a lookup compiles a few dozen cases instead.
    /// </remarks>
    private static IReadOnlyList<IReadOnlyList<IGrouping<ulong, int>>> SplitByKeyHash(IReadOnlyList<IGrouping<ulong, int>> keyedRegistrations)
    {
        int bucketCount = Math.Max(1, (keyedRegistrations.Count + MaxAverageKeyHashesPerMethod - 1) / MaxAverageKeyHashesPerMethod);
        List<IGrouping<ulong, int>>[] buckets = new List<IGrouping<ulong, int>>[bucketCount];
        for (int i = 0; i < bucketCount; i++)
        {
            buckets[i] = new();
        }

        foreach (IGrouping<ulong, int> group in keyedRegistrations)
        {
            buckets[(int)(group.Key % (ulong)bucketCount)].Add(group);
        }

        return buckets;
    }
}

public partial class EnumTemplate : IFormatterTemplate
{
    public EnumTemplate(AnalyzerOptions options, EnumSerializationInfo info)
    {
        this.Options = options;
        this.Info = info;
    }

    public AnalyzerOptions Options { get; }

    public QualifiedNamedTypeName ResolverName => this.Options.Generator.Resolver.Name;

    public EnumSerializationInfo Info { get; }

    ResolverRegisterInfo IFormatterTemplate.Info => this.Info;

    public string FileName => $"{this.Info.FileNameHint}.g.cs";
}

public partial class UnionTemplate : IFormatterTemplate
{
    public UnionTemplate(AnalyzerOptions options, UnionSerializationInfo info)
    {
        this.Options = options;
        this.Info = info;
    }

    public AnalyzerOptions Options { get; }

    public QualifiedNamedTypeName ResolverName => this.Options.Generator.Resolver.Name;

    public UnionSerializationInfo Info { get; }

    ResolverRegisterInfo IFormatterTemplate.Info => this.Info;

    public string FileName => $"{this.Info.FileNameHint}.g.cs";
}

public partial class CompositeResolverTemplate : IFormatterTemplate
{
    public string FileName => $"{this.ResolverName.Name}.g.cs";

    public required QualifiedNamedTypeName ResolverName { get; init; }

    ResolverRegisterInfo IFormatterTemplate.Info => throw new NotImplementedException();

    public required string[] ResolverInstanceExpressions { get; init; }

    public required string[] FormatterInstanceExpressions { get; init; }
}
