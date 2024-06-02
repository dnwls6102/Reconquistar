using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;
    [SerializeField] private GameObject SelectionPanel;

    private int playerNum = 4;
    public static int currentPlayerNum;
    private int currentTurn;
    public static bool isRolled; // 주사위 굴렸는지
    public static int isMoving; // 0: 이동 전 / 1: 이동 중 / 2: 이동 후
    private bool isComplete; // 턴 종료 가능한지

    public static Player[] players;
    public static Player currentPlayer => players[currentPlayerNum];
    private Dictionary<int, int> tilePerPlayer; // player 소유한 타일 수
    private List<int> playerTurnOrder = new List<int>();

    public static GameManager Instance;
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

    private void Start()
    {
        InitializePlayers();
        isRolled = false;
        isMoving = 0;
        isComplete = false;

        SortPlayerTurnOrder();
    }

    // player 생성, 소유 타일 수 지정
    private void InitializePlayers()
    {
        players = new Player[playerNum];
        tilePerPlayer = new Dictionary<int, int>();

        for (int i = 0; i < playerNum; i++)
        {
            GameObject playerObject = Instantiate(PlayerPrefab);
            Player player = playerObject.GetComponent<Player>();
            player.SetCellIndex(i * GameBoard.tilePerLine);
            players[i] = player;
            tilePerPlayer.Add(i, GameBoard.tilePerLine);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRolled)
        {
            StartCoroutine(Dice.Instance.RollDice());
        }
    }

    // 라운드 시작 전 플레이어 순서 정렬
    private void SortPlayerTurnOrder()
    {
        tilePerPlayer = tilePerPlayer.OrderBy(item => item.Value).ThenBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);

        playerTurnOrder.Clear();
        foreach (KeyValuePair<int, int> player in tilePerPlayer)
        {
            playerTurnOrder.Add(player.Key);
        }
        Debug.Log("라운드 순서: " + String.Join(", ", playerTurnOrder));

        currentPlayerNum = playerTurnOrder[0];
        players[currentPlayerNum].SetArrow(true);
        currentTurn = 0;
    }

    // 이동 버튼 클릭
    public void ArrowBtnClick(bool clockwise)
    {
        if (!isRolled || isMoving >= 1)
        {
            Debug.Log("이미 이동했거나 주사위를 굴리지 않았습니다.");
            return;
        }

        isMoving = 1;
        StartCoroutine(PlayerTurn(clockwise));
    }

    // 이동 -> 다음 플레이어 턴
    private IEnumerator PlayerTurn(bool clockwise)
    {
        yield return StartCoroutine(players[currentPlayerNum].PlayerMove(Dice.finalDiceValue, clockwise));

        isMoving = 2;

        SelectionPanel.SetActive(true);
    }

    public void NextTurn()
    {
        if (!isComplete)
        {
            Debug.Log("아직 턴이 종료되지 않았습니다.");
            return;
        }
        players[currentPlayerNum].SetArrow(false);
        currentTurn++;

        // 모두 턴 가졌으면 다음 라운드
        if (currentTurn == playerNum) SortPlayerTurnOrder();
        else
        {
            currentPlayerNum = playerTurnOrder[currentTurn];
            players[currentPlayerNum].SetArrow(true);
        }

        layoutgroupcontroller.Instance.RefreshLayoutGroup(players[currentPlayerNum].cardList);
        isComplete = false;
        isRolled = false;
        isMoving = 0;
    }

    // 점령 버튼 클릭 시 작동
    public void OccupyRegion()
    {
        // 싸우는 스크립트 넣는 곳

        bool win = true;
        int currentCellIndex = players[currentPlayerNum].GetCellIndex();
        MapTile currentMapTile = GameBoard.tileInfos[currentCellIndex].GetMapTile();
        if (win)
        {
            int currentOwner = currentMapTile.Owner;
            currentMapTile.ChangeOwner(currentPlayerNum);

            tilePerPlayer[currentOwner]--;
            tilePerPlayer[currentPlayerNum]++;
        }

        SelectionPanel.SetActive(false);
        isComplete = true;
    }

    // 징집/모집 버튼 클릭 시 작동
    public void DraftArmy(int type)
    {
        players[currentPlayerNum].AddCard(type);
        SelectionPanel.SetActive(false);
        isComplete = true;
    }
}
