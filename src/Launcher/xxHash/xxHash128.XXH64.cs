#pragma warning disable
// ReSharper disable all

namespace XXHash;

using System.Runtime.CompilerServices;

public static partial class xxHash128
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static ulong XXH64_avalanche(ulong hash)
    {
        hash ^= hash >> 33;
        hash *= XXH_PRIME64_2;
        hash ^= hash >> 29;
        hash *= XXH_PRIME64_3;
        hash ^= hash >> 32;
        return hash;
    }
}
