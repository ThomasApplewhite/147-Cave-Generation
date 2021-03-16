﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public Vector3Int numChunks = Vector3Int.one;
    public Material mat;
    public int chunkSize = 100;
    public bool generateColliders = true;

    public string seedInput = "default seed";
    private int seed;

    public int maxWorms = 7;
    public int maxWormLength = 150;
    public int minWormLength = 20;

    public int maxWormRadius;
    public int minWormRadius;
    [Header("Worm Settings: X is length, Y is worm radius")]
	public List<Vector2> wormSettings;
    GameObject chunkHolder;
    const string chunkHolderName = "Chunks Holder";
    List<Chunk> chunks;
    List<bool> visited;
    Dictionary<Vector3Int, Chunk> existingChunks;
    Queue<Chunk> recycleableChunks;

    System.Random rand;

    void Start() {
        meshGenerator = FindObjectOfType<SmoothMeshRenderer>();
        existingChunks = new Dictionary<Vector3Int, Chunk>();
        visited = new List<bool>();
        rand = new System.Random(HashSeed());
        InitChunks ();
        WormifyChunks();
        UpdateAllChunks ();
    }

    int HashSeed()
    {
        //take string input seed, add together char values
        //to get numerical seed
        char[] seedChars = seedInput.ToCharArray();
        for(int i = 0; i<seedChars.Length; i++)
        {
            seed += seedChars[i];
        }
        return seed;
    }
    
    void CreateChunkHolder () {
        // Create/find mesh holder object for organizing chunks under in the hierarchy
        if (chunkHolder == null) {
            if (GameObject.Find (chunkHolderName)) {
                chunkHolder = GameObject.Find (chunkHolderName);
            } else {
                chunkHolder = new GameObject (chunkHolderName);
            }
        }
    }

    void InitChunks () {
        CreateChunkHolder ();
        chunks = new List<Chunk> ();
        List<Chunk> oldChunks = new List<Chunk>(chunks);

        // Go through all coords and create a chunk there if one doesn't already exist
        for (int x = 0; x < numChunks.x; x++) {
            for (int y = 0; y < numChunks.y; y++) {
                for (int z = 0; z < numChunks.z; z++) {
                    Vector3Int coord = new Vector3Int (x, y, z);
                    bool chunkAlreadyExists = false;

                    // If chunk already exists, add it to the chunks list, and remove from the old list.
                    for (int i = 0; i < oldChunks.Count; i++) {
                        if (oldChunks[i].coord == coord) {
                            chunks.Add (oldChunks[i]);
                            visited.Add(false);
                            oldChunks.RemoveAt (i);
                            chunkAlreadyExists = true;
                            break;
                        }
                    }

                    // Create new chunk
                    VoxelData data = null;
                    if (!chunkAlreadyExists) {
                        Chunk newChunk = CreateChunk(coord);
                        chunks.Add (newChunk);
                        visited.Add(false);
                        data = new VoxelData(chunkSize, chunkSize/2 * Vector3.one, coord);
                        newChunk.data = data;
                        existingChunks.Add(coord, newChunk);
                    }
                    chunks[chunks.Count - 1].SetUp(mat, generateColliders);
                }
            }
        }

        // Delete all unused chunks
        for (int i = 0; i < oldChunks.Count; i++) {
            oldChunks[i].DestroyOrDisable ();
        }
    }

    void WormifyChunks()
    {
        foreach(Chunk c in chunks)
        {
            //number of worms (1, maxWorms)
            int numWorms = (int)((float)rand.NextDouble() * (maxWorms - 1)) + 1;
            Debug.Log("wormSettings:" + numWorms);
            for(int i = 0; i<numWorms; i++)
		    {
			    int wormLength = (int) minWormLength + (int)((float)rand.NextDouble() * (maxWormLength - minWormLength));
                int wormRadius = (int) minWormRadius + (int)((float)rand.NextDouble() * (maxWormRadius - minWormRadius));
                var worm = new PerlinWorm(wormLength, wormRadius, new Vector3((c.data.dataWidth / 2) + c.coord.x, (c.data.dataHeight / 2) + c.coord.y, (c.data.dataDepth / 2) + c.coord.z),
                             Mathf.Sin(wormLength), Mathf.Cos(wormRadius), Mathf.Tan(wormLength + wormRadius));
			    worm.Wormify(existingChunks, c.coord, new Vector3(chunkSize / 2, chunkSize / 2, chunkSize / 2), c.coord * chunkSize);
		    }
        }

        for (int i = 0; i< chunks.Count; i++)
        {
            if(!chunks[i].visited)
            {
                BuildPaths(chunks[i]);
            }
        }
        /*var walkableWorms = new PerlinWorm(1000, 5, new Vector3((chunks[0].data.dataWidth / 2) + chunks[0].coord.x, (chunks[0].data.dataHeight / 2) + chunks[0].coord.y, (chunks[0].data.dataDepth / 2) + chunks[0].coord.z),
             Mathf.Sin(1000), Mathf.Cos(10), Mathf.Tan(1010));
        walkableWorms.WalkableWorms(existingChunks, chunks[0].coord, chunks[1].coord, new Vector3(chunkSize / 2, chunkSize / 2, chunkSize / 2), chunks[0].coord * chunkSize);*/
    }

    void BuildPaths(Chunk node)//recursive DFS
    {
        if(node.visited)
        {
            return;
        }
        Chunk[] neighbors = new Chunk[8];
        int ctr = 0;
        for(int x = -1; x <= 1; x++)
        {
            for(int y = -1; y <= 1; y++)
            {
                for(int z = -1; z <= 1; z++)
                {
                    float chanceOfVisit = (float) rand.NextDouble();
                    if(chanceOfVisit >= .75 && existingChunks.ContainsKey(node.coord + new Vector3Int(x, y, z)))
                    {
                        neighbors[ctr] = existingChunks[node.coord + new Vector3Int(x, y, z)];
                    }
                }
            }
        }
        node.visited = true;
        for(int j = 0; neighbors[j] != null; j++)
        {
            int wormLength = (int) minWormLength + (int)((float)rand.NextDouble() * (maxWormLength - minWormLength));
            int wormRadius = (int) minWormRadius + (int)((float)rand.NextDouble() * (maxWormRadius - minWormRadius));
            var walkableWorms = new PerlinWorm(node.data.dataWidth * 3, wormRadius, new Vector3((node.data.dataWidth / 2) + node.coord.x, (node.data.dataHeight / 2) + node.coord.y, (node.data.dataDepth / 2) + node.coord.z),
                    Mathf.Sin(wormLength), Mathf.Cos(wormRadius), Mathf.Tan(wormLength + wormRadius));
            walkableWorms.WalkableWorms(existingChunks, node.coord, neighbors[j].coord, new Vector3(chunkSize / 2, chunkSize / 2, chunkSize / 2), neighbors[j].coord * chunkSize);
            BuildPaths(neighbors[j]);
        }
    }

    SmoothMeshRenderer meshGenerator;
    Chunk CreateChunk (Vector3Int coord) {
        GameObject chunk = new GameObject ($"Chunk ({coord.x}, {coord.y}, {coord.z})");
        chunk.transform.parent = chunkHolder.transform;
        Chunk newChunk = chunk.AddComponent<Chunk> ();
        newChunk.coord = coord;
        return newChunk;
    }

    public void UpdateAllChunks () {

        // Create mesh for each chunk
        foreach (Chunk chunk in chunks) {
            UpdateChunkMesh (chunk);
        }
    }

    void UpdateChunkMesh(Chunk chunk)
    {
        List<Mesh> d = meshGenerator.MakeChunk(existingChunks, chunk.coord, chunkSize, chunk.coord);
        Mesh[] submeshes = new Mesh[d.Count];
        int index = 0;
        foreach(Mesh m in d)
        {
            submeshes[index] = m;
            GameObject chunkSub = new GameObject("Submesh " + index);
            MeshFilter mFilt = chunkSub.AddComponent<MeshFilter>();
            MeshRenderer mRend = chunkSub.AddComponent<MeshRenderer>();
            MeshCollider mCol = chunkSub.AddComponent<MeshCollider>();
            mFilt.sharedMesh = m;
            mRend.material = mat;
            mCol.sharedMesh = m;
            chunkSub.transform.parent = chunk.transform;
            index++;
        }
        chunk.mesh = submeshes;
        //chunk.meshFilter.mesh = chunk.mesh;
    }
}