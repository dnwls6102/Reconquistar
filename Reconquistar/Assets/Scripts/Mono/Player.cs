using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public struct Card
{
    public int KingdomType;
    public int CardType;
    // public List<Buff> BuffList;
}

public class Player : MonoBehaviour
{
    private int boardLength;
    private int currentCellIndex;
    private SpriteRenderer sr;
    private GameObject arrow;
    public List<Card> cardList = new List<Card>();

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        boardLength = GameBoard.cellAxis.Length;
        arrow = transform.GetChild(0).gameObject;
    }

    public void SetCellIndex(int index)
    {
        currentCellIndex = index;
        transform.position = GameBoard.cellAxis[currentCellIndex];
    }

    public int GetCellIndex()
    {
        return currentCellIndex;
    }

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

    public void SetArrow(bool display)
    {
        arrow.SetActive(display);
    }
}
