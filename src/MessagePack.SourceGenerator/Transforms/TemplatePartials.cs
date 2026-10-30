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
