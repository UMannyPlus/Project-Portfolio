using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Graph : MonoBehaviour
{
    [SerializeField] private Transform pointPrefab;
    [SerializeField, Range(10, 100)] private int resolution = 10;
    [SerializeField] private FunctionLibrary.FunctionName function;
    [SerializeField, Min(0f)] private float functionDuration = 1f, transitionDuration = 1f;
    [SerializeField] private TransitionMode transitionMode;

    private Transform[] _points;
    private float _duration;
    private bool _transitioning;
    private FunctionLibrary.FunctionName _transitionFunction;

    private enum TransitionMode
    {
        Cycle,
        Random
    }
    private void Awake()
    {
        var step = 2f / resolution;
        var scale = Vector3.one * step;

        _points = new Transform[resolution * resolution];
        for (int i = 0; i < _points.Length; i++)
        {
            var point = _points[i] = Instantiate(pointPrefab, this.transform);
            point.localScale = scale;
        }
    }

    private void Update()
    {
        _duration += Time.deltaTime;
        if (_transitioning)
        {
            if (_duration >= transitionDuration)
            {
                _duration -= transitionDuration;
                _transitioning = false;
            }
        }
        else if (_duration >= functionDuration)
        {
            _duration -= functionDuration;
            _transitioning = true;
            _transitionFunction = function;
            PickNextFunction();
        }

        if (_transitioning)
        {
            UpdateFunctionTransition();
        }
        else
        {
            UpdateFunction();
        }
    }

    private void UpdateFunction()
    {
        var time = Time.time;
        var step = 2f / resolution;
        var f = FunctionLibrary.GetFunction(function);

        var v = 0.5f * step - 1;
        for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }

            float u = (x + 0.5f) * step - 1f;
            
            _points[i].localPosition = f(u, v, time);
        }
    }
    
    private void UpdateFunctionTransition()
    {
        FunctionLibrary.Function from = FunctionLibrary.GetFunction(_transitionFunction),
            to = FunctionLibrary.GetFunction(function);
        var progress = _duration / transitionDuration;
        var time = Time.time;
        var step = 2f / resolution;
        var f = FunctionLibrary.GetFunction(function);

        var v = 0.5f * step - 1;
        for (int i = 0, x = 0, z = 0; i < _points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (z + 0.5f) * step - 1f;
            }

            var u = (x + 0.5f) * step - 1f;
            
            _points[i].localPosition = FunctionLibrary.Morph(u, v, time, from, to, progress);
        }
    }

    private void PickNextFunction()
    {
        function = transitionMode == TransitionMode.Cycle
            ? FunctionLibrary.GetNextFunctionName(function)
            : FunctionLibrary.GetRandomFunctionNameOtherThan(function);
    }
}
