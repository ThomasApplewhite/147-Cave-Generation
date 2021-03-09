//using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script is based off of one made by Board to Bits

public class VoxelData
{

    private int[,,] dataArray;

    private System.Random RNG;

    public VoxelData(int size, Vector3 originPosition)
    {
        dataArray = new int[size, size, size];

        RNG = new System.Random();

        var initializer = new Func<Vector3, int>((Vector3 vec) => AllZeros(vec));
        
        Initialize(dataArray, originPosition, initializer);
    }

    public int dataWidth
    {
        get {return dataArray.GetLength(0);}
    }

    public int dataHeight{
        get {return dataArray.GetLength(1);}
    }

    public int dataDepth
    {
        get {return dataArray.GetLength(2);}
    }

    public int GetCell(int x, int y, int z)
    {
        return dataArray[x, y, z];
    }

    public void SetCell(int val, int x, int y, int z)
    {
        dataArray[x, y, z] = val;
    }

    void Initialize(int[,,] array, Vector3 origin, Func<Vector3, int> initializer)
    {
        Vector3 position;

        //Set each value to whatever the initializer says it should be
        for(int x = 0; x < dataWidth; ++x)
        {
            for(int y = 0; y < dataHeight; ++y)
            {
                for(int z = 0; z < dataDepth; ++z)
                {
                    array[x, y, z] = initializer.Invoke(new Vector3(x, y, z));
                }
            }
        }

        //then set the origin to an open space
        array[(int)origin.x, (int)origin.y, (int)origin.z] = 0;
    }

    int AllZeros(Vector3 x)
    {
        return 0;
    }

    int FlatPerlinNoise(Vector3 v)
    {
        float val = Mathf.PerlinNoise(v.x, v.z);
        Debug.Log($"Value {val} for vector {v}");
        return val >= 0.5f ? 1 : 0;
    }

    int RandomVal(Vector3 x)
    {
        return RNG.Next() % 2;
    }

    int AllOnes(Vector3 x)
    {
        return 1;
    }

    int PerlinNoise(Vector3 v)
    {
        float val = PerlinNoise3D(v.x, v.y, v.z);
        Debug.Log($"Value {val} for vector {v}");
        return val >= 0f ? 1 : 0;
    }

    public static float PerlinNoise3D(float x, float y, float z)
    {
        y += 1;
        z += 2;
        float xy = _perlin3DFixed(x, y);
        float xz = _perlin3DFixed(x, z);
        float yz = _perlin3DFixed(y, z);
        float yx = _perlin3DFixed(y, x);
        float zx = _perlin3DFixed(z, x);
        float zy = _perlin3DFixed(z, y);
        return xy * xz * yz * yx * zx * zy;
    }
 
    static float _perlin3DFixed(float a, float b)
    {
        return Mathf.Sin(Mathf.PI * Mathf.PerlinNoise(a, b));
    }
}
