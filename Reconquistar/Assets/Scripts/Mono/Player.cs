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
    private GameBoard.TileInfo currentTileInfo;
    private SpriteRenderer sr;
    private GameObject arrow;
    public List<Card> cardList = new List<Card>();

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        boardLength = GameBoard.tileInfos.Length;
        arrow = transform.GetChild(0).gameObject;
    }

    public void SetCellIndex(int index)
    {
        currentTileInfo = GameBoard.tileInfos[index];
        transform.position = currentTileInfo.GetCellAxis();
    }

    public int GetCellIndex()
    {
        return currentTileInfo.GetIndex();
    }

    public IEnumerator PlayerMove(int distance, bool clockwise)
    {
        for (int i = 0; i < distance; i++)
        {
            if (clockwise) currentTileInfo = currentTileInfo.leftTileInfo;
            else currentTileInfo = currentTileInfo.rightTileInfo;

            yield return StartCoroutine(MoveToNextTile(currentTileInfo.GetCellAxis()));
        }

        transform.position = currentTileInfo.GetCellAxis();
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

    public void AddCard()
    {
        Card card;
        card.KingdomType = currentTileInfo.GetMapTile().Owner;
        card.CardType = Random.Range(1, 14);
        Debug.Log(card.KingdomType + "의 " + card.CardType + " 카드를 뽑았습니다.");
        cardList.Add(card);
    }
}
