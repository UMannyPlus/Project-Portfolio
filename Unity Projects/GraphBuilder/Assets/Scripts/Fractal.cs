using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

using static Unity.Mathematics.math;
using float3 = Unity.Mathematics.float3;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

public class Fractal : MonoBehaviour
{
    [SerializeField, Range(3, 8)] private int depth = 4;
    [SerializeField] private Mesh mesh, leafMesh;
    [SerializeField] private Material material;
    [SerializeField] private Gradient gradientA, gradientB;
    [SerializeField] private Color leafColorA, leafColorB;
    [SerializeField, Range(0f, 90f)] private float maxSagAngleA = 15, maxSagAngleB = 25f;
    [SerializeField, Range(0f, 90f)] private float spinSpeedA = 20f, spinSpeedB = 25f;
    [SerializeField, Range(0f, 1f)] private float reverseSpinChance = 0.25f;

    private static quaternion[] _rotations =
    {
        quaternion.identity,
        quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI), 
        quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI) 
    };

    private static readonly int MatricesId = Shader.PropertyToID("_Matrices");
    private static readonly int ColorAId = Shader.PropertyToID("_ColorA");
    private static readonly int ColorBId = Shader.PropertyToID("_ColorB");
    private static readonly int SequenceNumbersId = Shader.PropertyToID("_SequenceNumbers");
    
    private static MaterialPropertyBlock PropertyBlock;

    private NativeArray<FractalPart>[] _parts;
    private NativeArray<float3x4>[] _matrices;
    private ComputeBuffer[] _matricesBuffer;
    private Vector4[] _sequenceNumbers;
    
    private struct FractalPart
    {
        public float3 WorldPosition;
        public quaternion Rotation, WorldRotation;
        public float MaxSagAngle, SpinAngle, SpinVelocity;
    }
    
    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)] 
    private struct UpdateFractalLevelJob : IJobFor
    {
        public float Scale;
        public float DeltaTime;

        [ReadOnly] public NativeArray<FractalPart> Parents;
        public NativeArray<FractalPart> Parts;

        [WriteOnly] public NativeArray<float3x4> Matrices;
        
        public void Execute(int i)
        {
            var parent = Parents[i / 5];
            var part = Parts[i];

            part.SpinAngle += part.SpinVelocity * DeltaTime;

            var upAxis = mul(mul(parent.WorldRotation, part.Rotation), up());
            var sagAxis = cross(up(), upAxis);

            var sagMagnitude = length(sagAxis);
            quaternion baseRotation;
            if (sagMagnitude > 0f)
            {
                sagAxis /= sagMagnitude;
                var sagRotation = quaternion.AxisAngle(sagAxis, part.MaxSagAngle * sagMagnitude);
                baseRotation = mul(sagRotation, parent.WorldRotation);
            }
            else
            {
                baseRotation = parent.WorldRotation;
            }

            //Order matters by the resulting Quaternion represent the rotation obtained by performing the rotation
            //of the second quaternion followed by the rotation of the first quaternion
            part.WorldRotation = mul(baseRotation, mul(part.Rotation, 
                quaternion.RotateY(part.SpinAngle)));
                
            //As the scale is halved at each level, we increase the offset by 150% (1.5f)
            //The parent rotation also affects the direction of the offset
            part.WorldPosition =
                parent.WorldPosition + mul(part.WorldRotation, float3(0f, 1.5f * Scale, 0f));

            Parts[i] = part;
            var r = float3x3(part.WorldRotation) * Scale;
            Matrices[i] = float3x4(r.c0, r.c1, r.c2, part.WorldPosition);
        }
    }

    private void OnEnable()
    {
        _parts = new NativeArray<FractalPart>[depth];
        _matrices = new NativeArray<float3x4>[depth];
        _matricesBuffer = new ComputeBuffer[depth];
        _sequenceNumbers = new Vector4[depth];
        
        const int stride = 12 * 4;
        for (int i = 0, length = 1; i < _parts.Length; i++, length *= 5)
        {
            //First Arg is size, second arg is how long the array is expected to exist
            _parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            _matrices[i] = new NativeArray<float3x4>(length, Allocator.Persistent);
            
            _matricesBuffer[i] = new ComputeBuffer(length, stride);

            _sequenceNumbers[i] = new Vector4(Random.value, Random.value, Random.value, Random.value);
        }
            
        
        _parts[0][0] = CreatePart(0);
        for (int li = 1; li < _parts.Length; li++)
        {
            var levelParts = _parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
                for (int ci = 0; ci < 5; ci++)
                    levelParts[fpi + ci] = CreatePart(ci);
        }

        PropertyBlock ??= new MaterialPropertyBlock();
    }

    private void OnDisable()
    {
        for (int i = 0; i < _matricesBuffer.Length; i++)
        {
            _matricesBuffer[i].Release();
            _parts[i].Dispose();
            _matrices[i].Dispose();
        }
            

        _parts = null;
        _matrices = null;
        _matricesBuffer = null;
        _sequenceNumbers = null;
    }

    private void OnValidate()
    {
        if (_parts != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;

        var rootPart = _parts[0][0];
        
        rootPart.SpinAngle += rootPart.SpinVelocity * deltaTime;
        rootPart.WorldRotation = mul(transform.rotation, mul(rootPart.Rotation, 
            quaternion.RotateY(rootPart.SpinAngle)));
        
        rootPart.WorldPosition = transform.position;

        var objectScale = transform.lossyScale.x;
        
        _parts[0][0] = rootPart;
        var r = float3x3(rootPart.WorldRotation) * objectScale;
        _matrices[0][0] = float3x4(r.c0, r.c1, r.c2, rootPart.WorldPosition);

        var scale = objectScale;

        JobHandle jobHandle = default;
        
        for (int li = 1; li < _parts.Length; li++)
        {
            scale *= 0.5f;

            jobHandle = new UpdateFractalLevelJob
            {
                DeltaTime = deltaTime,
                Scale = scale,
                Parents = _parts[li - 1],
                Parts = _parts[li],
                Matrices = _matrices[li]
            }.ScheduleParallel(_parts[li].Length, 5, jobHandle);
        }
        jobHandle.Complete();

        var bounds = new Bounds(rootPart.WorldPosition, 3f * objectScale * Vector3.one);

        var leafIndex = _matricesBuffer.Length - 1;
        for (int i = 0; i < _matricesBuffer.Length; i++)
        {
            var buffer = _matricesBuffer[i];
            buffer.SetData(_matrices[i]);

            

            //Randomizing the Color for the objects
            Color colorA, colorB;
            Mesh instanceMesh;
            if (i == leafIndex)
            {
                colorA = leafColorA;
                colorB = leafColorB;
                instanceMesh = leafMesh;
            }
            else
            {
                var gradientInterpolator = i / (_matricesBuffer.Length - 2f);
                colorA = gradientA.Evaluate(gradientInterpolator);
                colorB = gradientB.Evaluate(gradientInterpolator);
                instanceMesh = mesh;
            }
            
            PropertyBlock.SetColor(ColorAId, colorA);
            PropertyBlock.SetColor(ColorBId, colorB);
            
            PropertyBlock.SetBuffer(MatricesId, buffer);
            PropertyBlock.SetVector(SequenceNumbersId, _sequenceNumbers[i]);
            
            Graphics.DrawMeshInstancedProcedural(instanceMesh, 0, material, bounds, buffer.count, PropertyBlock);
        }
    }

    private FractalPart CreatePart(int childIndex) => new FractalPart
    {
        MaxSagAngle = radians(Random.Range(maxSagAngleA, maxSagAngleB)),
        Rotation = _rotations[childIndex],
        SpinVelocity = (Random.value < reverseSpinChance ? -1f : 1f) * radians(Random.Range(spinSpeedA, spinSpeedB))
    };

}
