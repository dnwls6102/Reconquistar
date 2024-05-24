using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    public static Vector3[] vertex;
    public static TileInfo[] tileInfos;

    private static GameBoard instance;
    public static GameBoard Instance => instance;

    [SerializeField] private GameObject tileObject;

    public static int tilePerLine;

    GameBoard()
    {
        instance = this;
    }

    public class TileInfo
    {
        private int index;
        private Vector3 cellAxis;
        private MapTile mapTile;
        public TileInfo leftTileInfo { get; set; }
        public TileInfo rightTileInfo { get; set; }

        public TileInfo(int index, Vector3 cellAxis, MapTile mapTile, TileInfo rightTileInfo)
        {
            this.index = index;
            this.cellAxis = cellAxis;
            this.mapTile = mapTile;
            this.rightTileInfo = rightTileInfo;
        }

        public TileInfo(int index, Vector3 cellAxis, MapTile mapTile)
        {
            this.index = index;
            this.cellAxis = cellAxis;
            this.mapTile = mapTile;
        }
        public int GetIndex()
        {
            return index;
        }

        public Vector3 GetCellAxis()
        {
            return cellAxis;
        }

        public MapTile GetMapTile()
        {
            return mapTile;
        }
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
        GameObject tile;
        MapTile mapTile;
        tilePerLine = 11 - 1;
        tileInfos = new TileInfo[tilePerLine * 4 + 2];

        for (int i = 0; i < 4; i++)
        {
            Vector3 distance = (vertex[i] - vertex[(i + 1) % vertex.Length]) / tilePerLine;
            for (int j = 0; j < tilePerLine; j++)
            {
                int idx = tilePerLine * i + j;
                Vector3 loc = vertex[i] - distance * j;

                tile = Instantiate(tileObject, loc, Quaternion.identity);
                mapTile = tile.GetComponent<MapTile>();
                mapTile.ChangeOwner(i);

                if (idx == 0)
                    tileInfos[idx] = new TileInfo(idx, loc, mapTile);
                else
                {
                    tileInfos[idx] = new TileInfo(idx, loc, mapTile, tileInfos[idx - 1]);
                    tileInfos[idx - 1].leftTileInfo = tileInfos[idx];
                }
            }
        }
        tileInfos[0].rightTileInfo = tileInfos[tilePerLine * 4 - 1];
        tileInfos[tilePerLine * 4 - 1].leftTileInfo = tileInfos[0];

        // 그라나다 오른쪽 모서리
        int right_idx = tilePerLine * 4;
        Vector3 right = tileInfos[19].GetCellAxis() + tileInfos[21].GetCellAxis() - tileInfos[20].GetCellAxis();
        tile = Instantiate(tileObject, right, Quaternion.identity);
        mapTile = tile.GetComponent<MapTile>();
        tileInfos[right_idx] = new TileInfo(right_idx, right, mapTile);

        tileInfos[tilePerLine * 2 - 1].leftTileInfo = tileInfos[right_idx];
        tileInfos[right_idx].leftTileInfo = tileInfos[tilePerLine * 2 + 1];

        // 그라나다 왼쪽 모서리 - 왼쪽으로 갈 수 없음
        int left_idx = tilePerLine * 4 + 1;
        Vector3 left = tileInfos[29].GetCellAxis() + tileInfos[19].GetCellAxis() - tileInfos[20].GetCellAxis();
        Instantiate(tileObject, left, Quaternion.identity);
        mapTile = tile.GetComponent<MapTile>();
        tileInfos[left_idx] = new TileInfo(left_idx, left, mapTile);

        tileInfos[tilePerLine * 3 + 1].rightTileInfo = tileInfos[left_idx];
        tileInfos[left_idx].rightTileInfo = tileInfos[tilePerLine * 3 - 1];

    }
}
