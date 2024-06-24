using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    private int boardLength;
    private int kingdomType;
    private GameBoard.TileInfo currentTileInfo;
    private SpriteRenderer sr;
    private GameObject arrow;
    public List<CardInfo> cardList = new List<CardInfo>();
    public layoutgroupcontroller deckcontroller;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        boardLength = GameBoard.tileInfos.Length;
        arrow = transform.GetChild(0).gameObject;
    }

    private void Start()
    {
        deckcontroller = layoutgroupcontroller.Instance;
    }

    public void Initialize(int kingdomType, int cellIndex)
    {
        this.kingdomType = kingdomType;
        currentTileInfo = GameBoard.tileInfos[cellIndex];
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

    public int GetCurrentTileOwner()
    {
        return currentTileInfo.GetMapTile().Owner;
    }

    public void AddCard(int type)
    {
        CardInfo card;
        int kingdomType = currentTileInfo.GetMapTile().Owner;

        if (type == 1) // 징집
        {
            Debug.Log("징집병 뽑기");
            List<CardInfo> c1 = GameManager.cardPool1[kingdomType];
            int idx = Random.Range(0, c1.Count);
            card = c1[idx];
            c1.RemoveAt(idx);
        }
        else // 모집
        {
            List<CardInfo> c2 = GameManager.cardPool2[kingdomType];

            if (kingdomType == this.kingdomType) // 본인 땅인 경우
            {
                if (Random.Range(0, 4) == 3) // 1/4 확률로 귀족풀 선택 (임시)
                {
                    Debug.Log("귀족 뽑기");
                    List<CardInfo> c3 = GameManager.cardPool3[kingdomType];
                    int idx = Random.Range(0, c3.Count);
                    card = c3[idx];
                    c3.RemoveAt(idx);
                }
                else // 무장병
                {
                    Debug.Log("본인 무장병 뽑기");
                    int idx = Random.Range(0, c2.Count);
                    card = c2[idx];
                    c2.RemoveAt(idx);
                }
            }
            else // 상대 땅인 경우 - 무장병만
            {
                Debug.Log("상대방 무장병 뽑기");

                int idx = Random.Range(0, c2.Count);
                card = c2[idx];
                c2.RemoveAt(idx);
            }
        }

        card.CardColor = currentTileInfo.tileColor;
        Debug.Log(card.KingdomType + "의 " + card.CardType + " 카드를 뽑았습니다.");
        cardList.Add(card);
        layoutgroupcontroller.Instance.RefreshLayoutGroup(cardList);
    }

    // 모집할 때 징병 카드 삭제
    public void RemoveCard(int index)
    {
        CardInfo removedCard = cardList[index];
        cardList.RemoveAt(index);
        layoutgroupcontroller.Instance.RefreshLayoutGroup(cardList);
        GameManager.cardPool1[currentTileInfo.GetMapTile().Owner].Add(removedCard);
    }

    // 해당 나라 징집병 카드 가지고 있는지 확인
    public bool CheckMojip()
    {
        int kingdomType = currentTileInfo.GetMapTile().Owner;
        if (this.kingdomType == kingdomType) return true;

        foreach (CardInfo c in cardList)
        {
            if (c.KingdomType == kingdomType && c.CardType >= 6) return true;
        }
        return false;
    }

    public bool CheckJeomryeong()
    {
        if (kingdomType == currentTileInfo.GetMapTile().Owner) return true;
        return false;
    }
    public IEnumerator SelectDeleteCard()
    {
        int kingdomType = currentTileInfo.GetMapTile().Owner;
        if (this.kingdomType != kingdomType)
        {
            for (int i = 0; i < cardList.Count; i++)
            {
                if (cardList[i].KingdomType == kingdomType && cardList[i].CardType >= 6)
                    cardList[i].DeleteCandidate = true;
            }
            layoutgroupcontroller.Instance.RefreshLayoutGroup(cardList);

            yield return new WaitUntil(() => GameManager.isSelected);
            GameManager.isSelected = false;

            for (int i = 0; i < cardList.Count; i++) { cardList[i].DeleteCandidate = false; }
            layoutgroupcontroller.Instance.RefreshLayoutGroup(cardList);
        }

        AddCard(2);
    }
}