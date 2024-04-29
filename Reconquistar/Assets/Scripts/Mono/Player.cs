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

    // public void MoveCell(int distance, bool clockwise)
    // {
    //     if (!clockwise) distance *= -1;
    //     currentCellIndex = (currentCellIndex + boardLength + distance) % boardLength;

    //     transform.position = GameBoard.cellAxis[currentCellIndex];
    // }

    public IEnumerator PlayerMove(int distance, bool clockwise)
    {
        int direction = 1;
        if (!clockwise) direction = -1;

        for (int i = 0; i < distance; i++)
        {
            currentCellIndex = (currentCellIndex + boardLength + direction) % boardLength;
            yield return StartCoroutine(MoveToNextTile(GameBoard.cellAxis[currentCellIndex]));
        }

        transform.position = GameBoard.cellAxis[currentCellIndex];
        GameManager.isMoving = false;
        GameManager.isRolled = false;
    }

    private IEnumerator MoveToNextTile(Vector3 end)
    {
        Vector3 start = transform.position;
        float duration = 0.3f;
        float currentTime = 0f;

        while (currentTime < duration)
        {
            float t = currentTime / duration;
            float axisY = Mathf.Sin(t * Mathf.PI) * 0.25f;

            transform.position = Vector3.Lerp(start, end, t) + new Vector3(0, axisY, 0);
            currentTime += Time.deltaTime;
            yield return null;
        }

        transform.position = end;
    }
}
