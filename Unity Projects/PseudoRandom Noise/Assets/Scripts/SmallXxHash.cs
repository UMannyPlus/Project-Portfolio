using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;

public readonly struct SmallXxHash {
    private const uint PrimeA = 0b10011110001101110111100110110001;
    private const uint PrimeB = 0b10000101111010111100101001110111;
    private const uint PrimeC = 0b11000010101100101010111000111101;
    private const uint PrimeD = 0b00100111110101001110101100101111;
    private const uint PrimeE = 0b00010110010101100110011110110001;

    private readonly uint _accumulator;

    public SmallXxHash(uint accumulator)
    {
        this._accumulator = accumulator;
    }

    public SmallXxHash Eat(int data) =>
        RotateLeft(_accumulator + (uint)data * PrimeC, 17) * PrimeD;
    
    //variant method
    public SmallXxHash Eat(byte data) =>
        RotateLeft(_accumulator + data * PrimeE, 11) * PrimeA;

    public static SmallXxHash Seed(int seed) =>
        (uint)seed + PrimeE;

    public static implicit operator uint(SmallXxHash hash)
    {
        var avalanche = hash._accumulator;
        avalanche ^= avalanche >> 15;
        avalanche *= PrimeB;
        avalanche ^= avalanche >> 13;
        avalanche *= PrimeC;
        avalanche ^= avalanche >> 16;

        return avalanche;
    }

    public static implicit operator SmallXxHash(uint accumulator) =>
        new SmallXxHash(accumulator);

    public static implicit operator SmallXxHash4(SmallXxHash hash) =>
        new SmallXxHash4(hash._accumulator);

    private static uint RotateLeft(uint data, int steps) => 
        (data << steps) | (data >> 32 - steps);
}

public readonly struct SmallXxHash4 {
    
    private const uint PrimeB = 0b10000101111010111100101001110111;
    private const uint PrimeC = 0b11000010101100101010111000111101;
    private const uint PrimeD = 0b00100111110101001110101100101111;
    private const uint PrimeE = 0b00010110010101100110011110110001;

    private readonly uint4 _accumulator;

    public SmallXxHash4(uint4 accumulator)
    {
        this._accumulator = accumulator;
    }

    public SmallXxHash4 Eat(int4 data) =>
        RotateLeft(_accumulator + (uint4)data * PrimeC, 17) * PrimeD;

    public static SmallXxHash4 Seed(int4 seed) =>
        (uint4)seed + PrimeE;

    public static implicit operator uint4(SmallXxHash4 hash)
    {
        var avalanche = hash._accumulator;
        avalanche ^= avalanche >> 15;
        avalanche *= PrimeB;
        avalanche ^= avalanche >> 13;
        avalanche *= PrimeC;
        avalanche ^= avalanche >> 16;

        return avalanche;
    }

    public static implicit operator SmallXxHash4(uint4 accumulator) =>
        new SmallXxHash4(accumulator);

    private static uint4 RotateLeft(uint4 data, int steps) => 
        (data << steps) | (data >> 32 - steps);
}



