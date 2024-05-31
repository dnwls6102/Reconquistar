using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class layoutgroupcontroller : MonoBehaviour
{
    public HorizontalLayoutGroup LayoutGroup;
    public Player[] players;
    public List<Card> cardList;
    public int currentPlayer;
    private GameManager gameManager;

    public GameObject card;
    // Start is called before the first frame update
    public static layoutgroupcontroller Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        gameManager = GameManager.Instance;
        players = gameManager.players;
        currentPlayer = gameManager.currentPlayer;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RefreshLayoutGroup(List<Card> cardList)
    {
        SortLayoutGroup(cardList);
    }

    private void SortLayoutGroup(List<Card> cardList)
    {
        for (var i = 0; i<LayoutGroup.gameObject.transform.childCount; i ++)
        {
            Destroy(LayoutGroup.gameObject.transform.GetChild(i).gameObject);
        }
        GameObject Card = Instantiate(card) as GameObject;
        Card.transform.parent = LayoutGroup.gameObject.transform;
        
    }
}
