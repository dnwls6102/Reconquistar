using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public static Vector3[] vertex;
    public static Vector3[] cellAxis;

    private static GameBoard instance;
    public static GameBoard Instance => instance;

    [SerializeField] private GameObject MapTile;

    private int tilePerLine;

    GameBoard()
    {
        instance = this;
    }

    private void Awake()
    {
        // Get cell axis
        vertex = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            vertex[i] = transform.GetChild(i).position;
        }

        DrawTile();
    }

    private void DrawTile()
    {
        tilePerLine = 11 - 1;
        cellAxis = new Vector3[tilePerLine * 4];

        for (int i = 0; i < 4; i++)
        {
            Vector3 distance = (vertex[i] - vertex[(i+1)%vertex.Length])/tilePerLine;
            for (int j = 0; j < tilePerLine; j++)
            {
                Vector3 loc = vertex[i] - distance * j;
                cellAxis[tilePerLine * i + j] = loc;
                Instantiate(MapTile, loc, Quaternion.identity);
            }
        }
    }
}
