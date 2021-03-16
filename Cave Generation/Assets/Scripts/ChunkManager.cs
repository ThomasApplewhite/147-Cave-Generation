using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public Vector3Int numChunks = Vector3Int.one;
    public Material mat;
    public int chunkSize = 100;
    public bool generateColliders = true;

    [Header("Worm Settings: X is length, Y is worm radius")]
	public List<Vector2> wormSettings;
    GameObject chunkHolder;
    const string chunkHolderName = "Chunks Holder";
    List<Chunk> chunks;
    Dictionary<Vector3Int, Chunk> existingChunks;
    Queue<Chunk> recycleableChunks;

    void Start() {
        meshGenerator = FindObjectOfType<SmoothMeshRenderer>();
        existingChunks = new Dictionary<Vector3Int, Chunk>();
        InitChunks ();
        UpdateAllChunks ();
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
        List<Chunk> oldChunks = new List<Chunk> (FindObjectsOfType<Chunk> ());

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
                            oldChunks.RemoveAt (i);
                            chunkAlreadyExists = true;
                            break;
                        }
                    }

                    // Create new chunk
                    VoxelData data = null;
                    if (!chunkAlreadyExists) {
                        var newChunk = CreateChunk (coord);
                        chunks.Add (newChunk);
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
        foreach(Chunk c in chunks)
        {
            foreach(Vector2 wormSetting in wormSettings)
		    {
			    //Debug.Log("Worming...");
                //Debug.Log("Coord: " + c.coord);
			    var worm = new PerlinWorm((int)wormSetting.x, (int)wormSetting.y, new Vector3((c.data.dataWidth / 2) + c.coord.x, (c.data.dataHeight / 2) + c.coord.y, (c.data.dataDepth / 2) + c.coord.z),
                             Mathf.Sin(wormSetting.x), Mathf.Cos(wormSetting.y), Mathf.Tan(wormSetting.x + wormSetting.y));
                //Debug.Log("Worm center: " + new Vector3(chunkSize / 2, chunkSize / 2, chunkSize / 2));
			    worm.Wormify(existingChunks, c.coord, new Vector3(chunkSize / 2, chunkSize / 2, chunkSize / 2), c.coord * chunkSize);
		    }
        }
        var walkableWorms = new PerlinWorm(1000, 5, new Vector3((chunks[0].data.dataWidth / 2) + chunks[0].coord.x, (chunks[0].data.dataHeight / 2) + chunks[0].coord.y, (chunks[0].data.dataDepth / 2) + chunks[0].coord.z),
             Mathf.Sin(1000), Mathf.Cos(10), Mathf.Tan(1010));
        walkableWorms.WalkableWorms(existingChunks, chunks[0].coord, chunks[1].coord, new Vector3(chunkSize / 2, chunkSize / 2, chunkSize / 2), chunks[0].coord * chunkSize);
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
