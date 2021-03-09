using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinWorm
{
    public int duration{ get; set; }

    //public int stepDistance{ get; set; }

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

    //private Vector3 realPosition = new Vector3(0, 0, 0);

    public PerlinWorm(int duration, int clearRange, float x=0, float y=0, float z=0)
    {
        this.duration = duration;
        this.clearRange = clearRange;

        this.wormRoll = x;
        this.wormPitch = y;
        this.wormYaw = z;
    }

    public void Wormify(VoxelData voxelMap, Vector3 pos, int time=0)
    {
        //Step 1: Clear where we currently are.
        //RadialClear(voxelMap, clearRange, pos);
        RadialAdd(voxelMap, clearRange, pos);

        //Step 2: Check if we've done enough clears. If not, contiune
        if(time < duration)
        {
            //Step 3: Adjust Pitch and Yaw with their noise values
            /*var newRoll = wormRoll + RollNoise(pos);
            var newPitch = wormPitch + PitchNoise(pos);
            var newYaw = wormYaw + YawNoise(pos);*/

            var newRoll = wormRoll + TrueRandom(pos);
            var newPitch = wormPitch + TrueRandom(pos);
            var newYaw = wormYaw + TrueRandom(pos);

            wormRoll = newRoll;
            wormPitch = newPitch;
            wormYaw = newYaw;
            
            //making sure we don't go over the maximum rotations...
            /*wormRoll = Mathf.Abs(newRoll) <= wormRollMax ? newRoll : wormRoll;
            wormPitch = Mathf.Abs(newPitch) <= wormPitchMax ? newPitch : wormPitch;
            wormYaw = Mathf.Abs(newYaw) <= wormYawMax ? newYaw : wormYaw;*/
            Debug.Log($"Rotation is Roll: {wormRoll} Pitch: {wormPitch} Yaw: {wormYaw}");

            //Step 4: Calculate a new position based on the new Yaw and Pitch that is "1" away i.e. normal
            internalForward = new Vector3(wormRoll, wormPitch, wormYaw).normalized;// * this.clearRange;


            /*from my research and poor recollection of AMS 10, it is sufficient to add the original position
            (treating it as a vector from origin to pos) to the forward vector to create a vector from origin
            to the new pos, which is a vector3 representation of position.
            Luckily for us, Vector Addition is a defined operator in Unity.*/
            var newPos = pos + internalForward;

            //visualize with debugging ray
            Debug.DrawRay(pos, internalForward, Color.white, 100.0f, false);

            //Step 4.5: If the worm would go out-of-bounds, normalize it back in
            newPos = newPos.magnitude > voxelMap.dataWidth ? newPos 
                : newPos.normalized * (newPos.magnitude % voxelMap.dataWidth);

            //Step 5: Re[B]ursion: Repeat clearing step
            Wormify(voxelMap, newPos, time + 1);
        }
        
    }

    void RadialClear(VoxelData map, int range, Vector3 origin)
    {
        Debug.Log("Clearing around " + origin);
        var rangef = (float)range;
        //double check to make sure the 
        /*Not sure what I was thinking here, the basics is make sure that this loop doesn't
        go out of bounds of range, map.data(AXIS) - 1 for any axis
        /int orX = origin.x >= (float)range ? Mathf.FloorToInt(origin.x % map.dataWidth) : range;
        int orY = origin.y >= (float)range ? Mathf.FloorToInt(origin.y % map.dataWidth) : range;
        int orZ = origin.z >= (float)range ? Mathf.FloorToInt(origin.z % map.dataWidth) : range;*/
        int minX = origin.x - rangef >= 0 ? (int)(origin.x - rangef) : 0;
        int maxX = origin.x + rangef < map.dataWidth ? (int)(origin.x + rangef) : map.dataWidth - 1;

        int minY = origin.y - rangef >= 0 ? (int)(origin.y - rangef) : 0;
        int maxY = origin.y + rangef < map.dataHeight ? (int)(origin.y + rangef) : map.dataHeight - 1;

        int minZ = origin.z - rangef >= 0 ? (int)(origin.z - rangef) : 0;
        int maxZ = origin.z + rangef < map.dataDepth ? (int)(origin.z + rangef) : map.dataDepth - 1;

        for(int x = minX; x <= maxX; ++x)
        {
            for(int y = minY; y <= maxY; ++y)
            {
                for(int z = minZ; z <= maxZ; ++z)
                {
                    map.SetCell(0, x, y, z);
                }
            }
        }
    }

    void RadialAdd(VoxelData map, int range, Vector3 origin)
    {
        Debug.Log("Adding around " + origin);
        var rangef = (float)range;
        //double check to make sure the 
        /*Not sure what I was thinking here, the basics is make sure that this loop doesn't
        go out of bounds of range, map.data(AXIS) - 1 for any axis
        /int orX = origin.x >= (float)range ? Mathf.FloorToInt(origin.x % map.dataWidth) : range;
        int orY = origin.y >= (float)range ? Mathf.FloorToInt(origin.y % map.dataWidth) : range;
        int orZ = origin.z >= (float)range ? Mathf.FloorToInt(origin.z % map.dataWidth) : range;*/
        int minX = origin.x - rangef >= 0 ? (int)(origin.x - rangef) : 0;
        int maxX = origin.x + rangef < map.dataWidth ? (int)(origin.x + rangef) : map.dataWidth - 1;

        int minY = origin.y - rangef >= 0 ? (int)(origin.y - rangef) : 0;
        int maxY = origin.y + rangef < map.dataHeight ? (int)(origin.y + rangef) : map.dataHeight - 1;

        int minZ = origin.z - rangef >= 0 ? (int)(origin.z - rangef) : 0;
        int maxZ = origin.z + rangef < map.dataDepth ? (int)(origin.z + rangef) : map.dataDepth - 1;

        for(int x = minX; x <= maxX; ++x)
        {
            for(int y = minY; y <= maxY; ++y)
            {
                for(int z = minZ; z <= maxZ; ++z)
                {
                    map.SetCell(1, x, y, z);
                }
            }
        }
    }

    /*There are MUCH better Perlin noise functions than Unity's built in, but whatever
    we'll improve it later*/
    float PitchNoise(Vector3 v)
    {
        return Mathf.PerlinNoise(v.x, v.z);
    }

    float YawNoise(Vector3 v)
    {
        return Mathf.PerlinNoise(v.x, v.y);
    }

    float RollNoise(Vector3 v)
    {
        return Mathf.PerlinNoise(v.y, v.z);
    }

    float TrueRandom(Vector3 v)
    {
        var randomScale = 135f;
        return UnityEngine.Random.Range(-randomScale, randomScale);
    }

    

    //private void PerlinNoise2D(Vector)

    //private void PerlinNoise3D(Vector3 x){}
}
