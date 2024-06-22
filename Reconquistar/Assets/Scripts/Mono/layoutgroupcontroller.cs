using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class layoutgroupcontroller : MonoBehaviour
{
    public HorizontalLayoutGroup LayoutGroup;

    public GameObject cardPrefab;
    // Start is called before the first frame update
    public static layoutgroupcontroller Instance;
    private static CardInfo removedCard;

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void RefreshLayoutGroup(List<CardInfo> cardInfos)
    {
        SortLayoutGroup(cardInfos);
    }

    private void SortLayoutGroup(List<CardInfo> cardInfos)
    {
        for (var i = 0; i < transform.childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < cardInfos.Count; i++)
        {
            GameObject card = Instantiate(cardPrefab);
            card.transform.SetParent(transform);
            card.GetComponent<CardDrag>().Initialize(i);
            card.GetComponent<Image>().color = cardInfos[i].CardColor;

            // 모집 중 삭제할 카드 선택 가능하도록
            card.GetComponent<Button>().interactable = cardInfos[i].CheckDeletion();

            int cardType = cardInfos[i].CardType;
            if (cardType == 0) cardType = 1;
            else if (cardType == 1) cardType = 2;
            card.GetComponent<RectTransform>().sizeDelta = new Vector2(cardType * 10 * 2.4f, 100);
            
            card.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = cardInfos[i].CardType.ToString();
        }
    }

    public static void RemoveCard(int cardIndex)
    {
        removedCard = GameManager.currentPlayer.cardList[cardIndex];
        GameManager.currentPlayer.cardList.RemoveAt(cardIndex);
    }

    public void InsertCard(Vector3 position)
    {
        List<CardInfo> cardInfos = GameManager.currentPlayer.cardList;
        float width = 0;
        int idx = 0;

        while (width < position.x)
        {
            width += 100; // 수치 바꾸기
            idx++;
        }

        if (width - 100 / 2 >= position.x) idx--;

        if (idx < cardInfos.Count)
        {
            Debug.Log("Insert at " + idx);
            GameManager.currentPlayer.cardList.Insert(idx, removedCard);
        }
        else
        {
            Debug.Log("Add at last");
            GameManager.currentPlayer.cardList.Add(removedCard);
        }
        RefreshLayoutGroup(GameManager.currentPlayer.cardList);
    }
}
