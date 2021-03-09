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
        return PerlinNoise2D(v.x, v.z);
    }

    float YawNoise(Vector3 v)
    {
        return PerlinNoise2D(v.x, v.y);
    }

    float RollNoise(Vector3 v)
    {
        return PerlinNoise2D(v.y, v.z);
    }

    float TrueRandom(Vector3 v)
    {
        var randomScale = 135f;
        return UnityEngine.Random.Range(-randomScale, randomScale);
    }

    

    private static Vector3[] gradients3D = {
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 1f,-1f, 0f),
		new Vector3(-1f,-1f, 0f),
		new Vector3( 1f, 0f, 1f),
		new Vector3(-1f, 0f, 1f),
		new Vector3( 1f, 0f,-1f),
		new Vector3(-1f, 0f,-1f),
		new Vector3( 0f, 1f, 1f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f, 1f,-1f),
		new Vector3( 0f,-1f,-1f),
		
		new Vector3( 1f, 1f, 0f),
		new Vector3(-1f, 1f, 0f),
		new Vector3( 0f,-1f, 1f),
		new Vector3( 0f,-1f,-1f)
	};

    private const int hashMask = 255;
    private const int gradientsMask3D = 15;

    private static int[] hash = {
		151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
		140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
		247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
		 57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
		 74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
		 60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
		 65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
		200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
		 52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
		207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
		119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
		129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
		218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
		 81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
		184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
		222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
	};
    //Everything in the 3D noise functions taken from https://github.com/keijiro/PerlinNoise/blob/master/Assets/Perlin.cs
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
        //point *= frequency;
        /*Vector3 point = new Vector3(x, y, z);
        float mag = point.magnitude;
        point = point/mag;
		int ix0 = Mathf.FloorToInt(point.x);
		int iy0 = Mathf.FloorToInt(point.y);
		int iz0 = Mathf.FloorToInt(point.z);
		float tx0 = point.x - ix0;
		float ty0 = point.y - iy0;
		float tz0 = point.z - iz0;
		float tx1 = tx0 - 1f;
		float ty1 = ty0 - 1f;
		float tz1 = tz0 - 1f;
		ix0 &= hashMask;
		iy0 &= hashMask;
		iz0 &= hashMask;
		int ix1 = ix0 + 1;
		int iy1 = iy0 + 1;
		int iz1 = iz0 + 1;
		
		int h0 = hash[ix0];
		int h1 = hash[ix1];
		int h00 = hash[h0 + iy0];
		int h10 = hash[h1 + iy0];
		int h01 = hash[h0 + iy1];
		int h11 = hash[h1 + iy1];
		Vector3 g000 = gradients3D[hash[h00 + iz0] & gradientsMask3D];
		Vector3 g100 = gradients3D[hash[h10 + iz0] & gradientsMask3D];
		Vector3 g010 = gradients3D[hash[h01 + iz0] & gradientsMask3D];
		Vector3 g110 = gradients3D[hash[h11 + iz0] & gradientsMask3D];
		Vector3 g001 = gradients3D[hash[h00 + iz1] & gradientsMask3D];
		Vector3 g101 = gradients3D[hash[h10 + iz1] & gradientsMask3D];
		Vector3 g011 = gradients3D[hash[h01 + iz1] & gradientsMask3D];
		Vector3 g111 = gradients3D[hash[h11 + iz1] & gradientsMask3D];

		float v000 = Dot(g000, tx0, ty0, tz0);
		float v100 = Dot(g100, tx1, ty0, tz0);
		float v010 = Dot(g010, tx0, ty1, tz0);
		float v110 = Dot(g110, tx1, ty1, tz0);
		float v001 = Dot(g001, tx0, ty0, tz1);
		float v101 = Dot(g101, tx1, ty0, tz1);
		float v011 = Dot(g011, tx0, ty1, tz1);
		float v111 = Dot(g111, tx1, ty1, tz1);

		float tx = Smooth(tx0);
		float ty = Smooth(ty0);
		float tz = Smooth(tz0);
		return mag*Mathf.Lerp(
			Mathf.Lerp(Mathf.Lerp(v000, v100, tx), Mathf.Lerp(v010, v110, tx), ty),
			Mathf.Lerp(Mathf.Lerp(v001, v101, tx), Mathf.Lerp(v011, v111, tx), ty),
			tz);*/
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

    private static float Dot (Vector3 g, float x, float y, float z) {
		return g.x * x + g.y * y + g.z * z;
	}

    private static float Smooth (float t) {
		return t * t * t * (t * (t * 6f - 15f) + 10f);
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
    static float Grad(int hash, float x, float y, float z)
    {
        var h = hash & 15;
        var u = h < 8 ? x : y;
        var v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    static float Grad(int hash, float x, float y)
    {
        return ((hash & 1) == 0 ? x : -x) + ((hash & 2) == 0 ? y : -y);
    }
}
