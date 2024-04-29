using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private int boardLength;
    private int currentCellIndex;
    SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        boardLength = GameBoard.cellAxis.Length;
    }

    public void SetCellIndex(int index)
    {
        currentCellIndex = index;
        transform.position = GameBoard.cellAxis[currentCellIndex];
    }

    public void MoveCell(int distance, bool clockwise)
    {
        if (!clockwise) distance *= -1;
        currentCellIndex = (currentCellIndex + boardLength + distance) % boardLength;

        transform.position = GameBoard.cellAxis[currentCellIndex];
    }
}
