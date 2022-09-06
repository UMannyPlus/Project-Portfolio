using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public class HashVisualization : MonoBehaviour
{
    
    public enum Shape
    {
        Plane,
        Sphere,
        Torus
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor
    {
        [ReadOnly] public NativeArray<float3x4> Positions;
        [WriteOnly] public NativeArray<uint4> Hashes;
        
        public SmallXxHash4 Hash;
        public float3x4 DomainTRS;

        private float4x3 TransformPositions(float3x4 trs, float4x3 p) => float4x3(
            trs.c0.x * p.c0 + trs.c1.x * p.c1 + trs.c2.x * p.c2 + trs.c3.x,
            trs.c0.y * p.c0 + trs.c1.y * p.c1 + trs.c2.y * p.c2 + trs.c3.y,
            trs.c0.z * p.c0 + trs.c1.z * p.c1 + trs.c2.z * p.c2 + trs.c3.z );

        public void Execute(int i)
        {
            var p = TransformPositions(DomainTRS, transpose(Positions[i]));

            var u = (int4)floor(p.c0);
            var v = (int4)floor(p.c1);
            var w = (int4)floor(p.c2);

            Hashes[i] = Hash.Eat(u).Eat(v).Eat(w);
        }
    }

    [SerializeField] private Mesh instanceMesh;
    [SerializeField] private Material material;
    [SerializeField] private Shape shape;
    [SerializeField, Range(1, 512)] private int resolution = 16;
    [SerializeField] private int seed;
    [SerializeField, Range(-0.5f, 0.5f)] private float displacement = 0.1f;
    [SerializeField, Range(0.1f, 10f)] private float instanceScale = 2f;

    [SerializeField] private SpaceTRS domain = new SpaceTRS
    {
        scale = 8f
    };

    private static int _hashesId = Shader.PropertyToID("_Hashes");
    private static int _positionsId = Shader.PropertyToID("_Positions");
    private static int _normalsId = Shader.PropertyToID("_Normals");
    private static int _configId = Shader.PropertyToID("_Config");

    private static Shapes.ScheduleDelegate[] _shapeJobs =
    {
        Shapes.Job<Shapes.Plane>.ScheduleParallel,
        Shapes.Job<Shapes.Sphere>.ScheduleParallel,
        Shapes.Job<Shapes.Torus>.ScheduleParallel
    };

    private NativeArray<uint4> _hashes;
    private NativeArray<float3x4> _positions, _normals;
    private ComputeBuffer _hashesBuffer, _positionsBuffer, _normalsBuffer;
    private MaterialPropertyBlock _propertyBlock;
    private bool _isDirty;
    private Bounds _bounds;

    private void OnEnable()
    {
        _isDirty = true;
        var length = resolution * resolution;
        length = length / 4 + (length & 1);
        
        _hashes = new NativeArray<uint4>(length, Allocator.Persistent);
        _positions = new NativeArray<float3x4>(length, Allocator.Persistent);
        _normals = new NativeArray<float3x4>(length, Allocator.Persistent);
        
        _hashesBuffer = new ComputeBuffer(length * 4, 4);
        _positionsBuffer = new ComputeBuffer(length * 4, 3 * 4);
        _normalsBuffer = new ComputeBuffer(length * 4, 3 * 4);

        _propertyBlock ??= new MaterialPropertyBlock();
        _propertyBlock.SetBuffer(_hashesId, _hashesBuffer);
        _propertyBlock.SetBuffer(_positionsId, _positionsBuffer);
        _propertyBlock.SetBuffer(_normalsId, _normalsBuffer);
        _propertyBlock.SetVector(_configId, new Vector4(resolution, instanceScale / resolution, displacement));
    }

    private void OnDisable()
    {
        _hashes.Dispose();
        _positions.Dispose();
        _normals.Dispose();
        
        _hashesBuffer.Release();
        _positionsBuffer.Release();
        _normalsBuffer.Release();
        
        _hashesBuffer = null;
        _positionsBuffer = null;
        _normalsBuffer = null;
    }

    private void OnValidate()
    {
        if (_hashesBuffer != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        if (_isDirty || transform.hasChanged)
        {
            _isDirty = false;
            transform.hasChanged = false;

            var handle = _shapeJobs[(int)shape](_positions, _normals, resolution, transform.localToWorldMatrix, default);
            
            new HashJob
            {
                Positions = _positions,
                Hashes = _hashes,
                Hash = SmallXxHash.Seed(seed),
                DomainTRS = domain.Matrix
            }.ScheduleParallel(_hashes.Length, resolution, handle).Complete();
            
            _hashesBuffer.SetData(_hashes.Reinterpret<uint>(4 * 4));
            _positionsBuffer.SetData(_positions.Reinterpret<float3>(3 * 4 * 4));
            _normalsBuffer.SetData(_normals.Reinterpret<float3>(3 * 4 * 4));

            _bounds = new Bounds(
                transform.position,
                float3(2f * cmax(abs(transform.lossyScale)) + displacement));
        }
        
        Graphics.DrawMeshInstancedProcedural( instanceMesh, 0, material, _bounds, resolution * resolution, 
            _propertyBlock);
    }
}
