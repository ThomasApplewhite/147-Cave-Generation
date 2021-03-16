using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour{
    public Vector3Int coord;

    [HideInInspector]
    public Mesh[] mesh;
    [HideInInspector]
    public MeshFilter[] meshFilter;
    MeshRenderer[] meshRenderer;
    MeshCollider[] meshCollider;
    bool generateCollider;
    public VoxelData data;

    public bool visited;

    public Chunk[] neighbors;

    public void DestroyOrDisable () {
        if (Application.isPlaying) {
            foreach(Mesh m in mesh)
            {
                m.Clear ();
            }
            gameObject.SetActive (false);
        } else {
            DestroyImmediate (gameObject, false);
        }
    }

    // Add components/get references in case lost (references can be lost when working in the editor)
    public void SetUp (Material mat, bool generateCollider) {
        this.generateCollider = generateCollider;
        mesh = new Mesh[transform.childCount];
        meshRenderer = new MeshRenderer[transform.childCount];
        meshFilter = new MeshFilter[transform.childCount];
        meshCollider = new MeshCollider[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
        {
            var thisObj = transform.GetChild(i);
            meshFilter[i] = thisObj.gameObject.GetComponent<MeshFilter> ();
            meshRenderer[i] = thisObj.gameObject.GetComponent<MeshRenderer> ();
            meshCollider[i] = thisObj.gameObject.GetComponent<MeshCollider> ();
            if (meshFilter == null) {
                meshFilter[i] = thisObj.gameObject.AddComponent<MeshFilter> ();
            }

            if (meshRenderer == null) {
                meshRenderer[i] = thisObj.gameObject.AddComponent<MeshRenderer> ();
            }

            if (meshCollider == null && generateCollider) {
                meshCollider[i] = thisObj.gameObject.AddComponent<MeshCollider> ();
            }
            if (meshCollider != null && !generateCollider) {
                DestroyImmediate (meshCollider[i]);
            }

            mesh[i] = meshFilter[i].sharedMesh;
            if (mesh == null) {
                mesh[i] = new Mesh ();
                mesh[i].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                meshFilter[i].sharedMesh = mesh[i];
            }

            if (generateCollider) {
                if (meshCollider[i].sharedMesh == null) {
                    meshCollider[i].sharedMesh = mesh[i];
                }
                // force update
                meshCollider[i].enabled = false;
                meshCollider[i].enabled = true;
            }

            meshRenderer[i].material = mat;
        }
    }
}