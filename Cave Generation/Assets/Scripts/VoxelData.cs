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

        var initializer = new Func<Vector3, int>((Vector3 vec) => randomVal(vec));
        
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

    void SetCell(int val, int x, int y, int z)
    {
        dataArray[x, y, z] = val;
    }

    void Initialize(int[,,] array, Vector3 origin, Func<Vector3, int> initializer)
    {
        Vector3 position;

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
    }

    int randomVal(Vector3 x)
    {
        return RNG.Next() % 2;
    }
}
