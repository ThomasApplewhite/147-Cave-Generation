using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    public Vector3Int numChunks = Vector3Int.one;
    public Material mat;
    public bool generateColliders = true;
    GameObject chunkHolder;
    const string chunkHolderName = "Chunks Holder";
    List<Chunk> chunks;
    Dictionary<Vector3Int, Chunk> existingChunks;
    Queue<Chunk> recycleableChunks;
    public class Chunk : MonoBehaviour{
        public Vector3Int coord;

        [HideInInspector]
        public Mesh mesh;
        [HideInInspector]
        public MeshFilter meshFilter;
        MeshRenderer meshRenderer;
        MeshCollider meshCollider;
        bool generateCollider;

        public void DestroyOrDisable () {
            if (Application.isPlaying) {
                mesh.Clear ();
                gameObject.SetActive (false);
            } else {
                DestroyImmediate (gameObject, false);
            }
        }

        // Add components/get references in case lost (references can be lost when working in the editor)
        public void SetUp (Material mat, bool generateCollider) {
            this.generateCollider = generateCollider;

            meshFilter = GetComponent<MeshFilter> ();
            meshRenderer = GetComponent<MeshRenderer> ();
            meshCollider = GetComponent<MeshCollider> ();

            if (meshFilter == null) {
                meshFilter = gameObject.AddComponent<MeshFilter> ();
            }

            if (meshRenderer == null) {
                meshRenderer = gameObject.AddComponent<MeshRenderer> ();
            }

            if (meshCollider == null && generateCollider) {
                meshCollider = gameObject.AddComponent<MeshCollider> ();
            }
            if (meshCollider != null && !generateCollider) {
                DestroyImmediate (meshCollider);
            }

            mesh = meshFilter.sharedMesh;
            if (mesh == null) {
                mesh = new Mesh ();
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                meshFilter.sharedMesh = mesh;
            }

            if (generateCollider) {
                if (meshCollider.sharedMesh == null) {
                    meshCollider.sharedMesh = mesh;
                }
                // force update
                meshCollider.enabled = false;
                meshCollider.enabled = true;
            }

            meshRenderer.material = mat;
        }
    }
    void Start() {
        meshGenerator = FindObjectOfType<SmoothMeshRenderer>();
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
                    if (!chunkAlreadyExists) {
                        var newChunk = CreateChunk (coord);
                        chunks.Add (newChunk);
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
        List<Mesh> d = meshGenerator.MakeChunk(100, chunk.coord);
        chunk.mesh = d[0];
        chunk.meshFilter.mesh = chunk.mesh;
    }
}
