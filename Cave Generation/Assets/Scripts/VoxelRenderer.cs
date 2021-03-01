using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//This script is based off of one made by Board to Bits

[RequireComponent (typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelRenderer : MonoBehaviour {

	Mesh mesh;
	List<Vector3> vertices;
	List<int> triangles;

	public float scale = 1f;

	public int size = 10;
	public Vector3 origin = new Vector3(0, 0, 0);
	//public int posX, posY, posZ;

	float adjScale;



	// Use this for initialization
	void Awake () {
		mesh = GetComponent<MeshFilter> ().mesh;
		adjScale = scale * 0.5f;
	}

	void Start() {
		//MakeCube (adjScale, new Vector3((float)posX * scale, (float)posY * scale, (float)posZ * scale));

        GenerateVoxelMesh(new VoxelData(size, origin));
		UpdateMesh ();
	}

    void GenerateVoxelMesh(VoxelData data)
    {
        vertices = new List<Vector3> ();
		triangles = new List<int> ();

        for(int z = 0; z < data.dataDepth; ++z)
        {
            for(int y = 0; y < data.dataHeight; ++y)
			{
				for(int x = 0; x < data.dataWidth; ++x)
            	{
                	if(data.GetCell(x, y, z) == 0)
                	{
                	    continue;
                	}
                	else
                	{	//
                	    MakeCube(adjScale, new Vector3((float)x * scale, (float)y * scale, (float)z * scale));
                	}
            	}
			}
        }
    }

	void MakeCube (float cubeScale, Vector3 cubePos){
		for (int i = 0; i < 6; i++) {
			MakeFace (i, cubeScale, cubePos);
		}
	}

	void MakeFace (int dir, float faceScale, Vector3 facePos){
		vertices.AddRange (CubeMeshData.faceVertices (dir, faceScale, facePos));
		int vCount = vertices.Count;

		triangles.Add (vCount - 4);
		triangles.Add (vCount - 4 + 1);
		triangles.Add (vCount - 4 + 2);
		triangles.Add (vCount - 4);
		triangles.Add (vCount - 4 + 2);
		triangles.Add (vCount - 4 + 3);

	}

	void UpdateMesh(){
		mesh.Clear ();

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals ();
	}
}