using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Rendering;

using static Unity.Mathematics.math;
using float4x3 = Unity.Mathematics.float4x3;

public class Shapes 
{
    public interface IShape
    {
        Point4 GetPoint4(int i, float resolution, float invResolution);
    }

    public delegate JobHandle ScheduleDelegate(NativeArray<float3x4> positions, NativeArray<float3x4> normals,
        int resolution, float4x4 trs, JobHandle dependency);
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Job<S> : IJobFor where S: struct, IShape
    {
        [WriteOnly] private NativeArray<float3x4> _positions, _normals;
        public float3x4 positionTRS, normalTRS;
        public float Resolution, InvResolution;

        public void Execute(int i)
        {
            var p = default(S).GetPoint4(i, Resolution, InvResolution);
            
            float4x3 TransformVectors (float3x4 trs, float4x3 p, float w = 1f) => float4x3(
                trs.c0.x * p.c0 + trs.c1.x * p.c1 + trs.c2.x * p.c2 + trs.c3.x * w,
                trs.c0.y * p.c0 + trs.c1.y * p.c1 + trs.c2.y * p.c2 + trs.c3.y * w,
                trs.c0.z * p.c0 + trs.c1.z * p.c1 + trs.c2.z * p.c2 + trs.c3.z * w
            );
            
            

            _positions[i] = transpose(TransformVectors(positionTRS, p.Positions));

            var n = transpose(TransformVectors(normalTRS, p.Normals, 0f));
            
            _normals[i] = float3x4(normalize(n.c0), normalize(n.c1), normalize(n.c2), normalize(n.c3));
        }

        public static JobHandle ScheduleParallel(NativeArray<float3x4> positions, NativeArray<float3x4> normals, 
            int resolution, float4x4 trs, JobHandle dependency)
        {
            var tim = transpose(inverse(trs));
            return new Job<S>
            {
                _positions = positions,
                _normals = normals,
                Resolution = resolution,
                InvResolution = 1f / resolution,
                positionTRS = float3x4(trs.c0.xyz, trs.c1.xyz, trs.c2.xyz, trs.c3.xyz),
                normalTRS = float3x4(tim.c0.xyz, tim.c1.xyz, tim.c2.xyz, tim.c3.xyz)
            }.ScheduleParallel(positions.Length, resolution, dependency);
        }
    }

    public struct Point4
    {
        public float4x3 Positions, Normals;
    }
    
    public struct Plane : IShape
    {
        public Point4 GetPoint4(int i, float resolution, float invResolution)
        {
            var uv = IndexTo4UV(i, resolution, invResolution);
            return new Point4
            {
                Positions = float4x3(uv.c0 - 0.5f, 0f, uv.c1 - 0.5f),
                Normals = float4x3(0f, 1f, 0f)
            };
        }
    }
    
    public struct Sphere : IShape
    {
        public Point4 GetPoint4(int i, float resolution, float invResolution)
        {
            var uv = IndexTo4UV(i, resolution, invResolution);

            
            //Creation of a octahedron that will be used as the base of the sphere
            Point4 p;
            p.Positions.c0 = uv.c0 - 0.5f;
            p.Positions.c1 = uv.c1 - 0.5f;
            p.Positions.c2 = 0.5f - abs(p.Positions.c0) - abs(p.Positions.c1);

            var offset = max(-p.Positions.c2, 0f);
            p.Positions.c0 += @select(-offset, offset, p.Positions.c0 < 0f);
            p.Positions.c1 += @select(-offset, offset, p.Positions.c1 < 0f);
            
            //Transforming into a sphere by Pythagorean theorem and rsqrt()
            var scale = 0.5f * rsqrt(
                p.Positions.c0 * p.Positions.c0 +
                p.Positions.c1 * p.Positions.c1 +
                p.Positions.c2 * p.Positions.c2);

            p.Positions.c0 *= scale;
            p.Positions.c1 *= scale;
            p.Positions.c2 *= scale;
            
            p.Normals = p.Positions;
            return p;
        }
    }
    
    public struct Torus : IShape
    {
        public Point4 GetPoint4(int i, float resolution, float invResolution)
        {
            var uv = IndexTo4UV(i, resolution, invResolution);

            const float r1 = 0.375f;
            const float r2 = 0.125f;
            var s = r1 + r2 * cos(2f * PI * uv.c1);

            Point4 p;
            p.Positions.c0 = s * sin(2f * PI * uv.c0);
            p.Positions.c1 = r2 * sin(2f * PI * uv.c1);
            p.Positions.c2 = s * cos(2f * PI * uv.c0);
            p.Normals = p.Positions;

            return p;
        }
    }

    private static float4x2 IndexTo4UV(int i, float resolution, float invResolution)
    {
        float4x2 uv;
        var i4 = 4f * i + float4(0f, 1f, 2f, 3f);
        uv.c1 = floor(invResolution * i4 + 0.00001f);
        uv.c0 = invResolution * (i4 - resolution * uv.c1 + 0.5f);
        uv.c1 = invResolution * (uv.c1 + 0.5f);
        return uv;
    }
}
