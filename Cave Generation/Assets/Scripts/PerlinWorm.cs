﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinWorm
{
    public int duration{ get; set; }

    public int clearRange{ get; set; }

    //these angles are in degrees, btw.

    public float wormPitchMax{ get; set; } = 135f;

    public float wormYawMax{ get; set; } = 135f;

    public float wormRollMax{ get; set; } = 135f;

    //that is, forward-backward
    private float wormRoll;

    //that is, up down
    private float wormPitch;

    //that is, left-right
    private float wormYaw;

    private int mapSize;

    private Vector3 internalForward = new Vector3(0f, 0f, 0f);

    public PerlinWorm(int duration, int clearRange, Vector3 origin, float x=0, float y=0, float z=0)
    {
        this.duration = duration;
        this.clearRange = clearRange;

        //initialize with random roll pitch yaw to add some variety
        this.wormRoll = PerlinNoise2D(x + origin.x, y + origin.y);
        this.wormPitch = PerlinNoise2D(y + origin.y, z + origin.z);
        this.wormYaw = PerlinNoise2D(x + origin.x, z + origin.z);
    }

    public void Wormify(Dictionary<Vector3Int, Chunk> world, Vector3Int currChunkCoord, Vector3 pos, Vector3 offset, int time=0)
    {
        //Step 1: Clear where we currently are.
        RadialAdd(world[currChunkCoord].data, (int)(PerlinNoise3D(pos.x * 100, pos.y * 100, pos.z * 100) * clearRange/2)+ clearRange, pos);

        //Step 2: Check if we've done enough clears. If not, contiune
        if(time < duration)
        {
            //Step 3: Adjust Pitch and Yaw with their noise values
            var newRoll = wormRoll + (PerlinNoise3D(pos.x + (currChunkCoord * world[currChunkCoord].data.dataDepth).x, 
                    pos.y + (currChunkCoord * world[currChunkCoord].data.dataDepth).y, 
                    pos.z + (currChunkCoord * world[currChunkCoord].data.dataDepth).z) * 180);
            var newPitch = wormPitch + (PerlinNoise3D(pos.z + (currChunkCoord * world[currChunkCoord].data.dataDepth).z, 
                    pos.x + (currChunkCoord * world[currChunkCoord].data.dataDepth).x, 
                    pos.y + (currChunkCoord * world[currChunkCoord].data.dataDepth).y) * 180);
            var newYaw = wormYaw + (PerlinNoise3D(pos.y + (currChunkCoord * world[currChunkCoord].data.dataDepth).y, 
                    pos.x + (currChunkCoord * world[currChunkCoord].data.dataDepth).x, 
                    pos.z + (currChunkCoord * world[currChunkCoord].data.dataDepth).z) * 180);

            wormRoll = newRoll;
            wormPitch = newPitch;
            wormYaw = newYaw;

            //Step 4: Calculate a new position based on the new Yaw and Pitch that is "1" away i.e. normal
            internalForward = new Vector3(wormRoll, wormPitch, wormYaw).normalized;

            /*from my research and poor recollection of AMS 10, it is sufficient to add the original position
            (treating it as a vector from origin to pos) to the forward vector to create a vector from origin
            to the new pos, which is a vector3 representation of position.
            Luckily for us, Vector Addition is a defined operator in Unity.*/
            VoxelData voxelMap = world[currChunkCoord].data;
            Debug.DrawRay(pos + (currChunkCoord * world[currChunkCoord].data.dataDepth), internalForward, Color.black, 100.0f, false);
            var newPos = pos + internalForward;
            //check if we exit the chunk, if so, move to next chunk
            if(newPos.x > voxelMap.dataWidth - 1 || newPos.y > voxelMap.dataHeight - 1 || newPos.z > voxelMap.dataDepth - 1|| newPos.x < 0 || newPos.y < 0 || newPos.z < 0)
            {
                if(newPos.x > voxelMap.dataWidth - 1)
                {
                    currChunkCoord.x += 1;
                    newPos.x = newPos.x - (voxelMap.dataWidth - 1);
                }
                else if(newPos.x < 0)
                {
                    currChunkCoord.x -= 1;
                    newPos.x = (voxelMap.dataWidth - 1) + newPos.x;
                }

                if(newPos.y > voxelMap.dataHeight - 1)
                {
                    currChunkCoord.y += 1;
                    newPos.y = newPos.y - (voxelMap.dataHeight - 1);
                }
                else if(newPos.y < 0)
                {
                    currChunkCoord.y -= 1;
                    newPos.y = (voxelMap.dataHeight - 1) + newPos.y;
                }

                if(newPos.z > voxelMap.dataDepth - 1)
                {
                    currChunkCoord.z += 1;
                    newPos.z = newPos.z - (voxelMap.dataDepth - 1);
                }
                else if(newPos.z < 0)
                {
                    currChunkCoord.z -= 1;
                    newPos.z = (voxelMap.dataDepth - 1) + newPos.z;
                }
                
            }

            //Step 5: Re[B]ursion: Repeat clearing step if the position is in an existing chunk
            //and hasn't reached its desination yet, otherwise, return
            if(world.ContainsKey(currChunkCoord))
            {
                Wormify(world, currChunkCoord, newPos, (currChunkCoord * world[currChunkCoord].data.dataDepth), time + 1);
            }
            else
            {
                return;
            }
        }
    }

    public void WalkableWorms(Dictionary<Vector3Int, Chunk> world, Vector3Int currChunkCoord, Vector3Int destChunkCoord, Vector3 pos, Vector3 offset, int time=0)
    {
        //Step 1: Clear where we currently are, with the clear range modified by plus 
        //or minus noise times half of the clear range
        RadialAdd(world[currChunkCoord].data, (int)(PerlinNoise3D(pos.x * 100, pos.y * 100, pos.z * 100) * clearRange/2)+ clearRange, pos);

        //Step 2: Check if we've done enough clears. If not, contiune
        if(time < duration)
        {
            //Step 3: Calculate the direction we want the worm to go in
            Vector3 destinationCenter = new Vector3(((destChunkCoord.x - currChunkCoord.x) * world[destChunkCoord].data.dataDepth) + world[destChunkCoord].data.dataWidth/2, ((destChunkCoord.y-currChunkCoord.y) * world[destChunkCoord].data.dataDepth) + world[destChunkCoord].data.dataHeight/2, ((destChunkCoord.z-currChunkCoord.z) * world[destChunkCoord].data.dataDepth) + world[destChunkCoord].data.dataDepth/2);
            Vector3 desiredDirection = (destinationCenter) - pos;
            desiredDirection = desiredDirection.normalized;
            
            //Step 4: Adjust Pitch and Yaw with their noise values multiplied by twice 
            //the number of degrees you want to be able to rotate (180 = + or minus 90)
            var newRoll = wormRoll + (PerlinNoise3D(pos.x + (currChunkCoord * world[currChunkCoord].data.dataDepth).x, 
                    pos.y + (currChunkCoord * world[currChunkCoord].data.dataDepth).y, 
                    pos.z + (currChunkCoord * world[currChunkCoord].data.dataDepth).z) * 180);
            var newPitch = wormPitch + (PerlinNoise3D(pos.z + (currChunkCoord * world[currChunkCoord].data.dataDepth).z, 
                    pos.x + (currChunkCoord * world[currChunkCoord].data.dataDepth).x, 
                    pos.y + (currChunkCoord * world[currChunkCoord].data.dataDepth).y) * 180);
            var newYaw = wormYaw + (PerlinNoise3D(pos.y + (currChunkCoord * world[currChunkCoord].data.dataDepth).y, 
                    pos.x + (currChunkCoord * world[currChunkCoord].data.dataDepth).x, 
                    pos.z + (currChunkCoord * world[currChunkCoord].data.dataDepth).z) * 180);

            wormRoll = newRoll;
            wormPitch = newPitch;
            wormYaw = newYaw;

            //Step 5: Calculate a new position based on the desired direction, modified 
            //by the yaw/pitch/roll that is "1" away i.e. normal
            internalForward = (((new Vector3(wormRoll, wormPitch, wormYaw) * (1f/1000f)) + desiredDirection)/2).normalized;

            /*from my research and poor recollection of AMS 10, it is sufficient to add the original position
            (treating it as a vector from origin to pos) to the forward vector to create a vector from origin
            to the new pos, which is a vector3 representation of position.
            Luckily for us, Vector Addition is a defined operator in Unity.*/
            VoxelData voxelMap = world[currChunkCoord].data;
            Debug.DrawRay(pos + (currChunkCoord * world[currChunkCoord].data.dataDepth), internalForward, Color.yellow, 100.0f, false);
            var newPos = pos + internalForward;
            //check if worm has exited chunk. if yes, move to new chunk
            if(newPos.x > voxelMap.dataWidth - 1 || newPos.y > voxelMap.dataHeight - 1 || newPos.z > voxelMap.dataDepth - 1|| newPos.x < 0 || newPos.y < 0 || newPos.z < 0)
            {
                if(newPos.x > voxelMap.dataWidth - 1)
                {
                    currChunkCoord.x += 1;
                    newPos.x = newPos.x - (voxelMap.dataWidth - 1);
                }
                else if(newPos.x < 0)
                {
                    currChunkCoord.x -= 1;
                    newPos.x = (voxelMap.dataWidth - 1) + newPos.x;
                }

                if(newPos.y > voxelMap.dataHeight - 1)
                {
                    currChunkCoord.y += 1;
                    newPos.y = newPos.y - (voxelMap.dataHeight - 1);
                }
                else if(newPos.y < 0)
                {
                    currChunkCoord.y -= 1;
                    newPos.y = (voxelMap.dataHeight - 1) + newPos.y;
                }

                if(newPos.z > voxelMap.dataDepth - 1)
                {
                    currChunkCoord.z += 1;
                    newPos.z = newPos.z - (voxelMap.dataDepth - 1);
                }
                else if(newPos.z < 0)
                {
                    currChunkCoord.z -= 1;
                    newPos.z = (voxelMap.dataDepth - 1) + newPos.z;
                }
            }

            //Step 5: Re[B]ursion: Repeat clearing step if the position is in an existing chunk
            //and hasn't reached its desination yet, otherwise, return
            if(currChunkCoord == destChunkCoord && Vector3.Distance(pos, destinationCenter) < clearRange)
            {
                return;
            }
            if(world.ContainsKey(currChunkCoord))// Vector3.Distance(pos, destinationCenter) > clearRange &&
            {
                WalkableWorms(world, currChunkCoord, destChunkCoord, newPos, (currChunkCoord * world[currChunkCoord].data.dataDepth), time + 1);
            }
        }
    }

    void RadialAdd(VoxelData map, int range, Vector3 origin)
    {
        var rangef = (float)range;
        /*The basics is make sure that this loop doesn't
        go out of bounds of range, map.data(AXIS) - 1 for any axis*/
        int minX = origin.x - 2*rangef >= 0 ? (int)(origin.x - 2*rangef) : 0;
        int maxX = origin.x + 2*rangef < map.dataWidth ? (int)(origin.x + 2*rangef) : map.dataWidth - 1;

        int minY = origin.y - 2*rangef >= 0 ? (int)(origin.y - 2*rangef) : 0;
        int maxY = origin.y + 2*rangef < map.dataHeight ? (int)(origin.y + 2*rangef) : map.dataHeight - 1;

        int minZ = origin.z - 2*rangef >= 0 ? (int)(origin.z - 2*rangef) : 0;
        int maxZ = origin.z + 2*rangef < map.dataDepth ? (int)(origin.z + 2*rangef) : map.dataDepth - 1;

        for(int x = minX; x <= maxX; ++x)
        {
            for(int y = minY; y <= maxY; ++y)
            {
                for(int z = minZ; z <= maxZ; ++z)
                {
                    //if this cell is "empty" and we're in range, set to 1
                    if(map.GetCell(x, y, z) <= -1 && Vector3.Distance(origin, new Vector3(x, y, z)) <= rangef)
                    {
                        map.SetCell(1, x, y, z);
                    }
                }
            }
        }
    }

    //Everything in the 3D noise functions taken from https://github.com/keijiro/PerlinNoise/blob/master/Assets/Perlin.cs
    //this perlin implementation based on the og : https://mrl.cs.nyu.edu/~perlin/noise/
    public static float PerlinNoise3D(float x, float y, float z)
    {
        var X = Mathf.FloorToInt(x) & 0xff;
        var Y = Mathf.FloorToInt(y) & 0xff;
        var Z = Mathf.FloorToInt(z) & 0xff;
        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        z -= Mathf.Floor(z);
        var u = Fade(x);
        var v = Fade(y);
        var w = Fade(z);
        var A  = (perm[X  ] + Y) & 0xff;
        var B  = (perm[X+1] + Y) & 0xff;
        var AA = (perm[A  ] + Z) & 0xff;
        var BA = (perm[B  ] + Z) & 0xff;
        var AB = (perm[A+1] + Z) & 0xff;
        var BB = (perm[B+1] + Z) & 0xff;
        return Lerp(w, Lerp(v, Lerp(u, Grad(perm[AA  ], x, y  , z  ), Grad(perm[BA  ], x-1, y  , z  )),
                               Lerp(u, Grad(perm[AB  ], x, y-1, z  ), Grad(perm[BB  ], x-1, y-1, z  ))),
                       Lerp(v, Lerp(u, Grad(perm[AA+1], x, y  , z-1), Grad(perm[BA+1], x-1, y  , z-1)),
                               Lerp(u, Grad(perm[AB+1], x, y-1, z-1), Grad(perm[BB+1], x-1, y-1, z-1))));
    }

    public static float PerlinNoise2D(float x, float y)
    {
        var X = Mathf.FloorToInt(x) & 0xff;
        var Y = Mathf.FloorToInt(y) & 0xff;
        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        var u = Fade(x);
        var v = Fade(y);
        var A = (perm[X  ] + Y) & 0xff;
        var B = (perm[X+1] + Y) & 0xff;
        return Lerp(v, Lerp(u, Grad(perm[A  ], x, y  ), Grad(perm[B  ], x-1, y  )),
                       Lerp(u, Grad(perm[A+1], x, y-1), Grad(perm[B+1], x-1, y-1)));
    }
    static float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    static float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    static int[] perm = {
        151,160,137,91,90,15,
        131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
        190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
        88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
        77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
        102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
        135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
        5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
        223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
        129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
        251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
        49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
        138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180,
        151
    };
    static float Grad(int hash, float x, float y, float z) //3D
    {
        var h = hash & 15;
        var u = h < 8 ? x : y;
        var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    static float Grad(int hash, float x, float y) //2D
    {
        return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
    }
}
