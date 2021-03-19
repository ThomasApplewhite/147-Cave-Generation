using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SmoothMeshRenderer : MonoBehaviour
{
    public GameObject crystal;
    Mesh generatedMesh;
    [Header("Smooth shading increases load times significantly")]
    public bool smoothShade = true;
    System.Random rand;

    void Awake() {
        rand = new System.Random();    
    }
    
    #region Lookup tables by Cory Gene Bloyd
    int[] edgeTable={
        0x0  , 0x109, 0x203, 0x30a, 0x406, 0x50f, 0x605, 0x70c,
        0x80c, 0x905, 0xa0f, 0xb06, 0xc0a, 0xd03, 0xe09, 0xf00,
        0x190, 0x99 , 0x393, 0x29a, 0x596, 0x49f, 0x795, 0x69c,
        0x99c, 0x895, 0xb9f, 0xa96, 0xd9a, 0xc93, 0xf99, 0xe90,
        0x230, 0x339, 0x33 , 0x13a, 0x636, 0x73f, 0x435, 0x53c,
        0xa3c, 0xb35, 0x83f, 0x936, 0xe3a, 0xf33, 0xc39, 0xd30,
        0x3a0, 0x2a9, 0x1a3, 0xaa , 0x7a6, 0x6af, 0x5a5, 0x4ac,
        0xbac, 0xaa5, 0x9af, 0x8a6, 0xfaa, 0xea3, 0xda9, 0xca0,
        0x460, 0x569, 0x663, 0x76a, 0x66 , 0x16f, 0x265, 0x36c,
        0xc6c, 0xd65, 0xe6f, 0xf66, 0x86a, 0x963, 0xa69, 0xb60,
        0x5f0, 0x4f9, 0x7f3, 0x6fa, 0x1f6, 0xff , 0x3f5, 0x2fc,
        0xdfc, 0xcf5, 0xfff, 0xef6, 0x9fa, 0x8f3, 0xbf9, 0xaf0,
        0x650, 0x759, 0x453, 0x55a, 0x256, 0x35f, 0x55 , 0x15c,
        0xe5c, 0xf55, 0xc5f, 0xd56, 0xa5a, 0xb53, 0x859, 0x950,
        0x7c0, 0x6c9, 0x5c3, 0x4ca, 0x3c6, 0x2cf, 0x1c5, 0xcc ,
        0xfcc, 0xec5, 0xdcf, 0xcc6, 0xbca, 0xac3, 0x9c9, 0x8c0,
        0x8c0, 0x9c9, 0xac3, 0xbca, 0xcc6, 0xdcf, 0xec5, 0xfcc,
        0xcc , 0x1c5, 0x2cf, 0x3c6, 0x4ca, 0x5c3, 0x6c9, 0x7c0,
        0x950, 0x859, 0xb53, 0xa5a, 0xd56, 0xc5f, 0xf55, 0xe5c,
        0x15c, 0x55 , 0x35f, 0x256, 0x55a, 0x453, 0x759, 0x650,
        0xaf0, 0xbf9, 0x8f3, 0x9fa, 0xef6, 0xfff, 0xcf5, 0xdfc,
        0x2fc, 0x3f5, 0xff , 0x1f6, 0x6fa, 0x7f3, 0x4f9, 0x5f0,
        0xb60, 0xa69, 0x963, 0x86a, 0xf66, 0xe6f, 0xd65, 0xc6c,
        0x36c, 0x265, 0x16f, 0x66 , 0x76a, 0x663, 0x569, 0x460,
        0xca0, 0xda9, 0xea3, 0xfaa, 0x8a6, 0x9af, 0xaa5, 0xbac,
        0x4ac, 0x5a5, 0x6af, 0x7a6, 0xaa , 0x1a3, 0x2a9, 0x3a0,
        0xd30, 0xc39, 0xf33, 0xe3a, 0x936, 0x83f, 0xb35, 0xa3c,
        0x53c, 0x435, 0x73f, 0x636, 0x13a, 0x33 , 0x339, 0x230,
        0xe90, 0xf99, 0xc93, 0xd9a, 0xa96, 0xb9f, 0x895, 0x99c,
        0x69c, 0x795, 0x49f, 0x596, 0x29a, 0x393, 0x99 , 0x190,
        0xf00, 0xe09, 0xd03, 0xc0a, 0xb06, 0xa0f, 0x905, 0x80c,
        0x70c, 0x605, 0x50f, 0x406, 0x30a, 0x203, 0x109, 0x0   };
    int[,] tritable = 
        {{-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        {3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        {3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        {3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1},
        {9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        {2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        {8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        {4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1},
        {3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1},
        {1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1},
        {4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        {4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        {5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        {2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1},
        {9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1},
        {0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1},
        {2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1},
        {10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1},
        {5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1},
        {5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1},
        {9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        {1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1},
        {10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1},
        {8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1},
        {2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1},
        {7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        {2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1},
        {11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1},
        {5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1},
        {11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1},
        {11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1},
        {9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1},
        {2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1},
        {6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        {3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1},
        {6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1},
        {10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1},
        {6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1},
        {8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1},
        {7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1},
        {3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1},
        {5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1},
        {0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1},
        {9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1},
        {8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1},
        {5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1},
        {0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1},
        {6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1},
        {10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1},
        {10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        {8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        {1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1},
        {0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1},
        {10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1},
        {3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1},
        {6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1},
        {9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1},
        {8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1},
        {3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1},
        {6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        {0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1},
        {10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1},
        {10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        {2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1},
        {7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1},
        {7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1},
        {2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1},
        {1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1},
        {11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1},
        {8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1},
        {0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1},
        {7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        {10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        {2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1},
        {6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1},
        {7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        {2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1},
        {1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1},
        {10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        {10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1},
        {0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1},
        {7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1},
        {6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1},
        {8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1},
        {9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1},
        {6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1},
        {4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1},
        {10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1},
        {8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        {0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1},
        {1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1},
        {8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1},
        {10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1},
        {4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1},
        {10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1},
        {5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        {11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1},
        {9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1},
        {6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1},
        {7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1},
        {3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1},
        {7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1},
        {3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1},
        {6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1},
        {9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1},
        {1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1},
        {4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1},
        {7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1},
        {6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        {3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1},
        {0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1},
        {6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1},
        {0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1},
        {11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1},
        {6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1},
        {5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1},
        {9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1},
        {1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1},
        {1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1},
        {10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1},
        {0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1},
        {5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1},
        {10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1},
        {11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1},
        {9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1},
        {7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1},
        {2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        {8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1},
        {9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1},
        {9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1},
        {1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1},
        {9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1},
        {9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        {5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1},
        {0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1},
        {10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1},
        {2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1},
        {0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1},
        {0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1},
        {9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1},
        {5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1},
        {3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1},
        {5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1},
        {8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1},
        {0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1},
        {9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        {0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1},
        {1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1},
        {3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1},
        {4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1},
        {9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1},
        {11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1},
        {11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1},
        {2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1},
        {9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1},
        {3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1},
        {1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1},
        {4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1},
        {4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1},
        {0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1},
        {3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1},
        {3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1},
        {0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1},
        {9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1},
        {1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}};

#endregion


    public List<Mesh> MakeChunk(Dictionary<Vector3Int, Chunk> world, Vector3Int currChunkCoord, int chunkSize) {
        _Start = Vector3.zero;
        _End = chunkSize * Vector3.one;
        _Diff = _End - _Start;
        
        _XCount = chunkSize + 1;
        _YCount = chunkSize + 1;
        _ZCount = chunkSize;

        vertices = new List<Vector3>();
        tris = new List<Vector3>();
        return GenerateMeshInput(world, currChunkCoord, currChunkCoord * chunkSize);
    }

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> tris = new List<Vector3>();
    Vector3 _Start = Vector3.zero;
    Vector3 _End;
    Vector3 _Diff;
    float _CellSize = 1f;
    int _XCount;
    int _YCount;
    int _ZCount;
    public float _IsoLevel = 0f;
    const int MAX_VERTS = 65534;

    List<Mesh> GenerateMeshInput(Dictionary<Vector3Int, Chunk> world, Vector3Int currChunkCoord, Vector3 offset) //generates mesh from input data
    {
        List<Mesh> chunkMesh = new List<Mesh>();

        VoxelData data = world[currChunkCoord].data;

        float[] TopGrid = new float[_XCount * _YCount];
        float[] BottomGrid = new float[_XCount * _YCount];
        
        int z = 0;
        while(z < _ZCount)
        {
            vertices.Clear();
            tris.Clear();

            FillGrid(TopGrid, z, world, currChunkCoord);
            z++;
            int vertexCount = 0;
            while(z < _ZCount && vertexCount < MAX_VERTS - 12000)//max verts minus the most verts that could be returned (12 verts per cube, 1000 cubes in a grid pair)
            {
                FillGrid(BottomGrid, z, world, currChunkCoord);
                int newVerts = PolygonizeGrids(TopGrid, BottomGrid, z, Vector3.zero);
                Debug.Log("New verts from polygonization: " + newVerts);
                vertexCount += newVerts;
                var temp = TopGrid;
                TopGrid = BottomGrid;
                BottomGrid = temp;
                z++;
            }
            if(z< _ZCount)
            {
                Debug.LogWarning("Exceeded Max Vert Count, splitting mesh");
            }
            
            Vector3Int topZ = currChunkCoord + new Vector3Int(0, 0, 1);
            //if we go out the top of a chunk, add a bridge between data sets
            if(z >= _ZCount && world.ContainsKey(topZ))
            {
                FillGrid(BottomGrid, 0, world, topZ);
                vertexCount += PolygonizeGrids(TopGrid, BottomGrid, 0, new Vector3Int(0, 0, data.dataWidth));
                var temp = TopGrid;
                TopGrid = BottomGrid;
                BottomGrid = temp;
                FillGrid(BottomGrid, 1, world, topZ);
                vertexCount += PolygonizeGrids(TopGrid, BottomGrid, 1, new Vector3Int(0, 0, data.dataWidth));
            }
           

            Debug.Log("Verts: " + vertices.Count);
            Debug.Log("Tris: " + tris.Count);

            //mesh generation from calculated vertices
            generatedMesh = new Mesh();
            Vector3[] genVerts = new Vector3[vertices.Count];
            Vector3[] meshNormals = new Vector3[vertices.Count];
            Color[] meshColors = new Color[vertices.Count]; //colors for debug only
            //mesh vertices
            for(int i = 0; i < genVerts.Length; i++)
            {
                genVerts[i] = vertices[i] + offset;
                Vector3 vertValue = (vertices[i]); 
                //meshColors[i] = new Color(vertValue.y/100f, vertValue.y/100f, vertValue.y/100f);
            }
            generatedMesh.vertices = genVerts;

            //triangle vertices
            int[] triIndices = new int[tris.Count * 3];
            for(int i = 0; i<(triIndices.Length/3) - 2; i++) //forwards triangles
            {
                triIndices[(i * 3)] =(int) tris[i].z;
                triIndices[(i * 3) + 1] =(int) tris[i].y;
                triIndices[(i * 3) + 2] =(int) tris[i].x;
                Vector3 cross = Vector3.Cross(genVerts[triIndices[(i * 3) + 1]] - genVerts[triIndices[(i * 3)]], genVerts[triIndices[(i * 3) + 2]] - genVerts[triIndices[(i * 3)]]);
                meshNormals[triIndices[(i * 3)]] += cross;
                meshNormals[triIndices[(i * 3) + 1]] += cross;
                meshNormals[triIndices[(i * 3) + 2]] += cross;

                meshColors[triIndices[(i * 3)]] = new Color((float)i/((triIndices.Length/3) - 2), (float)i/((triIndices.Length/3) - 2), (float)i/((triIndices.Length/3) - 2));
                meshColors[triIndices[(i * 3) + 1]] = new Color((float)i/((triIndices.Length/3) - 2), (float)i/((triIndices.Length/3) - 2), (float)i/((triIndices.Length/3) - 2));
                meshColors[triIndices[(i * 3) + 1]] = new Color((float)i/((triIndices.Length/3) - 2), (float)i/((triIndices.Length/3) - 2), (float)i/((triIndices.Length/3) - 2));
            }
            generatedMesh.triangles = triIndices;

            //smooth mesh normals or flat mesh normals
            //toggle-able because it increases load time a lot
            Vector3[] smoothMeshNormals = null;
            if(smoothShade)
            {
                smoothMeshNormals = new Vector3[meshNormals.Length];
                /*for(int i = 0; i<smoothMeshNormals.Length; i++)
                {
                    smoothMeshNormals[i] = ComputeVertexNormal(genVerts[i] - offset, meshNormals);
                    if(rand.NextDouble() < .0001)
                    {
                        Quaternion cRot = new Quaternion(0, 0, 0, 0);
                        Vector3 arbitraryForwards = new Vector3(1, 2, ((-1 * smoothMeshNormals[i].x) + (-2 * smoothMeshNormals[i].y))/smoothMeshNormals[i].z);
                        //cRot.SetLookRotation(arbitraryForwards, smoothMeshNormals[i]);
                        Instantiate(crystal, genVerts[i], cRot);
                    }
                }*/
                for(int i = 0; i<tris.Count; i++)
                {
                    smoothMeshNormals[(int)tris[i].z] = ComputeVertexNormal(genVerts[(int)tris[i].z] - offset, i,  (int)tris[i].z, meshNormals);
                    smoothMeshNormals[(int)tris[i].y] = ComputeVertexNormal(genVerts[(int)tris[i].y] - offset, i,  (int)tris[i].y, meshNormals);
                    smoothMeshNormals[(int)tris[i].x] = ComputeVertexNormal(genVerts[(int)tris[i].x] - offset, i,  (int)tris[i].x, meshNormals);
                }
            }
            else
            {
                for(int i = 0; i<meshNormals.Length; i++)
                {
                    meshNormals[i] = Vector3.Normalize(meshNormals[i]);
                    if(rand.NextDouble() < .0001)
                    {
                        Quaternion cRot = new Quaternion(0, 0, 0, 0);
                        Vector3 arbitraryForwards = new Vector3(1, 2, ((-1 * meshNormals[i].x) + (-2 * meshNormals[i].y))/meshNormals[i].z);
                        //cRot.SetLookRotation(arbitraryForwards, meshNormals[i]);
                        Instantiate(crystal, genVerts[i], Quaternion.identity);
                    }
                }
            }
            if(smoothShade)
            {
                generatedMesh.normals = smoothMeshNormals;
            }
            else
            {
                generatedMesh.normals = meshNormals;
            }

            generatedMesh.uv = UvCalculator.CalculateUVs(genVerts, triIndices, 1.0f);
            generatedMesh.colors = meshColors;
            
            //meshes stored in a list because one chunk tends to go over the maximum vertex count
            chunkMesh.Add(generatedMesh);

            //if mesh splits mid gen, go over and do a bridging section between meshes
            if(z<_ZCount)
            {
                z--;
            }
        }
        //clear lists
        vertices.Clear();
        tris.Clear();
        return chunkMesh;
    }

    Vector3 ComputeVertexNormal(Vector3 vertex, int triIndex, int vertIndex, Vector3[] meshNormals)
    {
        //find faces around current vertex
        List<int> adjFaces = new List<int>();
                    //newPos = newPos.magnitude > voxelMap.dataWidth ? newPos 
            //    : newPos.normalized * (newPos.magnitude % voxelMap.dataWidth);
        int startIndex = triIndex - 400 > 0 ? triIndex - 400 : 0;
        int endIndex = triIndex + 400 <= tris.Count ? triIndex + 400 : tris.Count;
        int pFace = startIndex;
        for(int i = startIndex; i<endIndex; i++, pFace ++)
        {
            if(vertices[(int)tris[pFace].x] == vertex)
            {
                adjFaces.Add(pFace);
            }
            else if(vertices[(int)tris[pFace].y] == vertex)
            {
                adjFaces.Add(pFace);
            }
            if(vertices[(int)tris[pFace].z] == vertex)
            {
                adjFaces.Add(pFace);
            }
        }

        //calculate avg of face normals at vertex
        Vector3 p = Vector3.zero;
        for(int j = 0; j<adjFaces.Count; j++)
        {
            int iFace = adjFaces[j];
            p.x += meshNormals[(int)tris[iFace].x].x;
            p.y += meshNormals[(int)tris[iFace].y].y;
            p.z += meshNormals[(int)tris[iFace].z].z;
        }
        p /= adjFaces.Count;
        return p.normalized;
    }

    struct GridData{
        public float[] value;
        public Vector3[] loc;
    }

    //algorithm and some code interpreted into c# from here: http://paulbourke.net/geometry/polygonise/ 
    //and here https://graphics.stanford.edu/~mdfisher/MarchingCubes.html
    int Polygonize(GridData gridCell, float isolevel, List<Vector3> triangles, List<Vector3> localVertices, out int newVertexCount)
    {
        //bitmask to figure out which vertices of the cube are above/below the isosurface
        int cubeindex = 0;
        if (gridCell.value[0] <= isolevel) cubeindex |= 1;
        if (gridCell.value[1] <= isolevel) cubeindex |= 2;
        if (gridCell.value[2] <= isolevel) cubeindex |= 4;
        if (gridCell.value[3] <= isolevel) cubeindex |= 8;
        if (gridCell.value[4] <= isolevel) cubeindex |= 16;
        if (gridCell.value[5] <= isolevel) cubeindex |= 32;
        if (gridCell.value[6] <= isolevel) cubeindex |= 64;
        if (gridCell.value[7] <= isolevel) cubeindex |= 128;

        Vector3[] vertlist = new Vector3[12];
        Vector3[] newVertList = new Vector3[12];
        int[] LocalRemap = new int[12];
        newVertexCount = 0;

        //look up these in the edge table to find the place along the edges to place vertices
        if (edgeTable[cubeindex] == 0)
		    return 0;

        if ((edgeTable[cubeindex] & 1) != 0)
            vertlist[0] =
                VertexInterp(isolevel, gridCell.loc[0],gridCell.loc[1],gridCell.value[0],gridCell.value[1]);
        if ((edgeTable[cubeindex] & 2) != 0)
            vertlist[1] =
                VertexInterp(isolevel,gridCell.loc[1],gridCell.loc[2],gridCell.value[1],gridCell.value[2]);
        if ((edgeTable[cubeindex] & 4) != 0)
            vertlist[2] =
                VertexInterp(isolevel,gridCell.loc[2],gridCell.loc[3],gridCell.value[2],gridCell.value[3]);
        if ((edgeTable[cubeindex] & 8) != 0)
            vertlist[3] =
                VertexInterp(isolevel,gridCell.loc[3],gridCell.loc[0],gridCell.value[3],gridCell.value[0]);
        if ((edgeTable[cubeindex] & 16) != 0)
            vertlist[4] =
                VertexInterp(isolevel,gridCell.loc[4],gridCell.loc[5],gridCell.value[4],gridCell.value[5]);
        if ((edgeTable[cubeindex] & 32) != 0)
            vertlist[5] =
                VertexInterp(isolevel,gridCell.loc[5],gridCell.loc[6],gridCell.value[5],gridCell.value[6]);
        if ((edgeTable[cubeindex] & 64) != 0)
            vertlist[6] =
                VertexInterp(isolevel,gridCell.loc[6],gridCell.loc[7],gridCell.value[6],gridCell.value[7]);
        if ((edgeTable[cubeindex] & 128) !=0)
            vertlist[7] =
                VertexInterp(isolevel,gridCell.loc[7],gridCell.loc[4],gridCell.value[7],gridCell.value[4]);
        if ((edgeTable[cubeindex] & 256) !=0)
            vertlist[8] =
                VertexInterp(isolevel,gridCell.loc[0],gridCell.loc[4],gridCell.value[0],gridCell.value[4]);
        if ((edgeTable[cubeindex] & 512) !=0)
            vertlist[9] =
                VertexInterp(isolevel,gridCell.loc[1],gridCell.loc[5],gridCell.value[1],gridCell.value[5]);
        if ((edgeTable[cubeindex] & 1024) !=0)
            vertlist[10] =
                VertexInterp(isolevel,gridCell.loc[2],gridCell.loc[6],gridCell.value[2],gridCell.value[6]);
        if ((edgeTable[cubeindex] & 2048) !=0)
            vertlist[11] =
                VertexInterp(isolevel,gridCell.loc[3],gridCell.loc[7],gridCell.value[3],gridCell.value[7]);


        //put everything in the correct order to add new vertices and triangles to our global vert/tri list
        for(int i = 0; i<12; i++)
        {
            LocalRemap[i] = -1;
        }

        for(int i = 0; tritable[cubeindex, i] != -1; i++)
        {
            if(LocalRemap[tritable[cubeindex, i]] == -1)
            {
                newVertList[newVertexCount] = vertlist[tritable[cubeindex, i]];
                LocalRemap[tritable[cubeindex, i]] = newVertexCount;
                newVertexCount ++;
            }
        }

        for(int i=0; i<newVertexCount; i++)
        {
            localVertices.Add(newVertList[i]);
        }

        int triangleCount = 0;
        for(int i=0; tritable[cubeindex, i] != -1; i+=3)
        {
            triangles.Add(new Vector3(LocalRemap[tritable[cubeindex, i]], LocalRemap[tritable[cubeindex, i + 1]], LocalRemap[tritable[cubeindex, i + 2]]));
            triangleCount++;
        }

        return triangleCount;
    }

    //interpreted into c# from https://graphics.stanford.edu/~mdfisher/MarchingCubes.html
    int PushPolygons(GridData g, Vector3 offset){
	    int NewVertexCount = vertices.Count;
        int IndexShift = vertices.Count;
        List<Vector3> vertStorage = new List<Vector3>();
        List<Vector3> triStorage = new List<Vector3>();

        //turn the grid data of this cube into some polygons! 
	    int NewFaceCount = Polygonize(g, _IsoLevel, triStorage, vertStorage, out NewVertexCount);
	    if(NewFaceCount != 0)
	    {
            //add our new triangles to the global triangle list
		    for(int FaceIndex = 0; FaceIndex < NewFaceCount; FaceIndex++)
		    {
                Vector3 thisTri = triStorage[FaceIndex];
			    thisTri.x += IndexShift;
			    thisTri.y += IndexShift;
			    thisTri.z += IndexShift;
                triStorage[FaceIndex] = thisTri;
			    tris.Add(triStorage[FaceIndex]);
		    }
            //add new vertices to the global vertex list
		    for(int VertexIndex = 0; VertexIndex < NewVertexCount; VertexIndex++)
		    {
			    vertices.Add(vertStorage[VertexIndex] + offset);
		    }
		    return NewVertexCount;
	    }
	    return 0;
    }

    //interpreted to c# from http://paulbourke.net/geometry/polygonise/
    Vector3 VertexInterp(float isoLevel, Vector3 p1, Vector3 p2, float valp1, float valp2)
    {
        //find the midpoint between two weighted points
        if(Mathf.Abs(isoLevel - valp1) < .00001f)
        {
            return p1;
        }
        if(Mathf.Abs(isoLevel - valp2) < .00001f)
        {
            return p2;
        }
        if(Mathf.Abs(valp1 - valp2) < .00001f)
        {
            return p1;
        }

        float mu = (isoLevel - valp1)/(valp2-valp1);
        Vector3 p = new Vector3();
        p[0] = p1[0] + mu * (p2[0] - p1[0]);
        p[1] = p1[1] + mu * (p2[1] - p1[1]);
        p[2] = p1[2] + mu * (p2[2] - p1[2]);
        return p;
    }
  
    void FillGrid(float[] Grid, int z, Dictionary<Vector3Int, Chunk> world, Vector3Int currChunkCoord)
    {
        for (int x = 0; x< _XCount; x++)
        {
            for(int y = 0; y< _YCount; y++)
            {
                //if it is not an edge location, get the value at the location normally
                if(x < _XCount - 1 && y < _YCount - 1) //regular cell
                {
                    Vector3 Pos = new Vector3(_Start.x + (_CellSize * x), _Start.y + (_CellSize * y), _Start.z + (_CellSize * (z)));
                    Grid[(int)(x-_Start.x) * _YCount + (int)(y - _Start.y)] = world[currChunkCoord].data.GetCell((int)(Pos.x), (int)(Pos.y), (int)(Pos.z));
                }
                else if(x >= _XCount - 1 && y < _YCount - 1) //if we are on the +x edge, step into next chunk and get that data
                {
                    currChunkCoord.x += 1;
                    if(world.ContainsKey(currChunkCoord))
                    {
                        Vector3 Pos = new Vector3(0, _Start.y + (_CellSize * y), _Start.z + (_CellSize * (z)));
                        Grid[(int)(x-_Start.x) * _YCount + (int)(y - _Start.y)] = world[currChunkCoord].data.GetCell((int)(Pos.x), (int)(Pos.y), (int)(Pos.z));
                    }
                    else
                    {
                        Grid[(int)(x-_Start.x) * _YCount + (int)(y - _Start.y)] = -1f;
                    }
                    currChunkCoord.x -= 1;
                }
                else if(x < _XCount - 1 && y >= _YCount - 1) //if we are on the +y edge, step into next chunk and get that data
                {
                    currChunkCoord.y += 1;
                    if(world.ContainsKey(currChunkCoord))
                    {
                        Vector3 Pos = new Vector3(_Start.x + (_CellSize * x), 0, _Start.z + (_CellSize * (z)));
                        //Debug.Log("Get Cell: " + new Vector3((int)(Pos.x + data._size/2), (int)(Pos.y + data._size/2), (int)(Pos.z + data._size/2)));
                        Grid[(int)(x-_Start.x) * _YCount + (int)(y - _Start.y)] =   world[currChunkCoord].data.GetCell((int)(Pos.x), (int)(Pos.y), (int)(Pos.z));
                    }
                    else
                    {
                        Grid[(int)(x-_Start.x) * _YCount + (int)(y - _Start.y)] = -1f;
                    }
                    currChunkCoord.y -= 1;
                }
                else if(x >= _XCount - 1 && y >= _YCount - 1) //if we are in the +x +y corner piece, step into the +(1, 1, 0) chunk and get that data
                {
                    currChunkCoord.x += 1;
                    currChunkCoord.y += 1;
                    if(world.ContainsKey(currChunkCoord))
                    {
                        Vector3 Pos = new Vector3(0, 0, _Start.z + (_CellSize * (z)));
                        Grid[(int)(x-_Start.x) * _YCount + (int)(y - _Start.y)] =   world[currChunkCoord].data.GetCell((int)(Pos.x), (int)(Pos.y), (int)(Pos.z));
                    }
                    else
                    {
                        Grid[(int)(x-_Start.x) * _YCount + (int)(y - _Start.y)] = -1f;
                    }
                    currChunkCoord.x -= 1;
                    currChunkCoord.y -= 1;
                }
                else //this will never execute but I am terrified of removing it
                {
                    Grid[(int)(x-_Start.x) * _YCount + (int)(y - _Start.y)] = -1f;
                }
            }
        }
    }
    
    //interpreted to c# from https://graphics.stanford.edu/~mdfisher/MarchingCubes.html
    int PolygonizeGrids(float[] TopVals, float[] BottomVals, int z, Vector3 offset)
    {
        int newVertices = 0;
        //go through our two grids and find the polygons spanning them
        for(int x = 0; x < _XCount - 1; x++)
        {
		    for(int y = 0; y < _YCount - 1; y++)
		    {
                //create cube of samples of our data
                GridData g = new GridData();
                g.loc = new Vector3[8];
                g.value = new float[8];
			    g.loc[0] = new Vector3(_Start.x + _CellSize * (x+0),_Start.y + _CellSize * (y+0),_Start.z + _CellSize * (z+0));
			    g.loc[1] = new Vector3(_Start.x + _CellSize * (x+1),_Start.y + _CellSize * (y+0),_Start.z + _CellSize * (z+0));
			    g.loc[2] = new Vector3(_Start.x + _CellSize * (x+1),_Start.y + _CellSize * (y+1),_Start.z + _CellSize * (z+0));
			    g.loc[3] = new Vector3(_Start.x + _CellSize * (x+0),_Start.y + _CellSize * (y+1),_Start.z + _CellSize * (z+0));

			    g.value[0] = TopVals[x*_YCount+y];
			    g.value[1] = TopVals[(x+1)*_YCount+y];
			    g.value[2] = TopVals[(x+1)*_YCount+y+1];
			    g.value[3] = TopVals[x*_YCount+y+1];

			    g.loc[4] = new Vector3(_Start.x + _CellSize * (x+0),_Start.y + _CellSize * (y+0),_Start.z + _CellSize * (z+1));
			    g.loc[5] = new Vector3(_Start.x + _CellSize * (x+1),_Start.y + _CellSize * (y+0),_Start.z + _CellSize * (z+1));
			    g.loc[6] = new Vector3(_Start.x + _CellSize * (x+1),_Start.y + _CellSize * (y+1),_Start.z + _CellSize * (z+1));
			    g.loc[7] = new Vector3(_Start.x + _CellSize * (x+0),_Start.y + _CellSize * (y+1),_Start.z + _CellSize * (z+1));

			    g.value[4] = BottomVals[x*_YCount+y];
			    g.value[5] = BottomVals[(x+1)*_YCount+y];
			    g.value[6] = BottomVals[(x+1)*_YCount+y+1];
			    g.value[7] = BottomVals[x*_YCount+y+1];

			    bool Valid = true;
                //make sure the numbers check out (no overflows)
			    for(int VertexIndex = 0; VertexIndex < 8 && Valid; VertexIndex++)
			    {
				    if(g.value[VertexIndex] == float.MaxValue)
				    {
					    Valid = false;
				    }
			    }
                //calculate the actual polygons at the GridData cube
			    if(Valid)
			    {
				    newVertices += PushPolygons(g, offset);
			    }
		    }
        }
        return newVertices;
    }
    //UV calculator modified from https://answers.unity.com/questions/64410/generating-uvs-for-a-scripted-mesh.html by user intrepidis
    public class UvCalculator
    {
        private enum Facing { Up, Forward, Right };
        
        public static Vector2[] CalculateUVs(Vector3[] v/*vertices*/, int[] triangles, float scale)
        {
            var uvs = new Vector2[v.Length];
            
            for (int i = 0 ; i < (triangles.Length) - 2; i +=3)
            {
                int i0 = i;
                int i1 = i+1;
                int i2 = i+2;
                
                Vector3 v0 = v[triangles[i0]];
                Vector3 v1 = v[triangles[i1]];
                Vector3 v2 = v[triangles[i2]];
                
                Vector3 side1 = v1 - v0;
                Vector3 side2 = v2 - v0;
                var direction = Vector3.Cross(side1, side2);
                var facing = FacingDirection(direction);
                switch (facing)
                {
                case Facing.Forward:
                    uvs[triangles[i0]] = ScaledUV(v0.x, v0.y, scale);
                    uvs[triangles[i1]] = ScaledUV(v1.x, v1.y, scale);
                    uvs[triangles[i2]] = ScaledUV(v2.x, v2.y, scale);
                    break;
                case Facing.Up:
                    uvs[triangles[i0]] = ScaledUV(v0.x, v0.z, scale);
                    uvs[triangles[i1]] = ScaledUV(v1.x, v1.z, scale);
                    uvs[triangles[i2]] = ScaledUV(v2.x, v2.z, scale);
                    break;
                case Facing.Right:
                    uvs[triangles[i0]] = ScaledUV(v0.y, v0.z, scale);
                    uvs[triangles[i1]] = ScaledUV(v1.y, v1.z, scale);
                    uvs[triangles[i2]] = ScaledUV(v2.y, v2.z, scale);
                    break;
                }
            }
            return uvs;
        }
        
        private static bool FacesThisWay(Vector3 v, Vector3 dir, Facing p, ref float maxDot, ref Facing ret)
        {
            float t = Vector3.Dot(v, dir);
            if (t > maxDot)
            {
                ret = p;
                maxDot = t;
                return true;
            }
            return false;
        }
        
        private static Facing FacingDirection(Vector3 v)
        {
            var ret = Facing.Up;
            float maxDot = 0f;
            
            if (!FacesThisWay(v, Vector3.right, Facing.Right, ref maxDot, ref ret))
                FacesThisWay(v, Vector3.left, Facing.Right, ref maxDot, ref ret);
            
            if (!FacesThisWay(v, Vector3.forward, Facing.Forward, ref maxDot, ref ret))
                FacesThisWay(v, Vector3.back, Facing.Forward, ref maxDot, ref ret);
            
            if (!FacesThisWay(v, Vector3.up, Facing.Up, ref maxDot, ref ret))
                FacesThisWay(v, Vector3.down, Facing.Up, ref maxDot, ref ret);
            
            return ret;
        }
        
        private static Vector2 ScaledUV(float uv1, float uv2, float scale)
        {
            return new Vector2(uv1 / scale, uv2 / scale);
        }
    }
}
