// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// The map mode test project also compiles this folder. It leaves this type out, so that its resolver has few enough types
// to find them all with one method, and both shapes of the generated lookup are run.
#if !FORCE_MAP_MODE

/// <summary>
/// Has members of enough types that the generated resolver spreads its type lookup over several methods.
/// </summary>
[MessagePackObject]
public class HasManyMemberTypes
{
    [Key(0)]
    public Dictionary<bool, bool>? Value0 { get; set; }

    [Key(1)]
    public Dictionary<bool, byte>? Value1 { get; set; }

    [Key(2)]
    public Dictionary<bool, char>? Value2 { get; set; }

    [Key(3)]
    public Dictionary<bool, int>? Value3 { get; set; }

    [Key(4)]
    public Dictionary<bool, long>? Value4 { get; set; }

    [Key(5)]
    public Dictionary<bool, string>? Value5 { get; set; }

    [Key(6)]
    public Dictionary<byte, bool>? Value6 { get; set; }

    [Key(7)]
    public Dictionary<byte, byte>? Value7 { get; set; }

    [Key(8)]
    public Dictionary<byte, char>? Value8 { get; set; }

    [Key(9)]
    public Dictionary<byte, int>? Value9 { get; set; }

    [Key(10)]
    public Dictionary<byte, long>? Value10 { get; set; }

    [Key(11)]
    public Dictionary<byte, string>? Value11 { get; set; }

    [Key(12)]
    public Dictionary<char, bool>? Value12 { get; set; }

    [Key(13)]
    public Dictionary<char, byte>? Value13 { get; set; }

    [Key(14)]
    public Dictionary<char, char>? Value14 { get; set; }

    [Key(15)]
    public Dictionary<char, int>? Value15 { get; set; }

    [Key(16)]
    public Dictionary<char, long>? Value16 { get; set; }

    [Key(17)]
    public Dictionary<char, string>? Value17 { get; set; }

    [Key(18)]
    public Dictionary<int, bool>? Value18 { get; set; }

    [Key(19)]
    public Dictionary<int, byte>? Value19 { get; set; }

    [Key(20)]
    public Dictionary<int, char>? Value20 { get; set; }

    [Key(21)]
    public Dictionary<int, int>? Value21 { get; set; }

    [Key(22)]
    public Dictionary<int, long>? Value22 { get; set; }

    [Key(23)]
    public Dictionary<int, string>? Value23 { get; set; }

    [Key(24)]
    public Dictionary<long, bool>? Value24 { get; set; }

    [Key(25)]
    public Dictionary<long, byte>? Value25 { get; set; }

    [Key(26)]
    public Dictionary<long, char>? Value26 { get; set; }

    [Key(27)]
    public Dictionary<long, int>? Value27 { get; set; }

    [Key(28)]
    public Dictionary<long, long>? Value28 { get; set; }

    [Key(29)]
    public Dictionary<long, string>? Value29 { get; set; }

    [Key(30)]
    public Dictionary<string, bool>? Value30 { get; set; }

    [Key(31)]
    public Dictionary<string, byte>? Value31 { get; set; }

    [Key(32)]
    public Dictionary<string, char>? Value32 { get; set; }

    [Key(33)]
    public Dictionary<string, int>? Value33 { get; set; }

    [Key(34)]
    public Dictionary<string, long>? Value34 { get; set; }

    [Key(35)]
    public Dictionary<string, string>? Value35 { get; set; }
}

#endif
