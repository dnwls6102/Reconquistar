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

    private int playerNum = 4;
    private int currentPlayer;
    private int currentTurn;

    public static bool isRolled; // 주사위 굴렸는지
    public static bool isMoving; // 이동 중인지

    private Player[] players;
    private Dictionary<int, int> tilePerPlayer; // player 소유한 타일 수
    private List<int> playerTurnOrder = new List<int>();

    [SerializeField] private Button ClockwiseBtn;
    [SerializeField] private Button CounterClockwiseBtn;

    private void Start()
    {
        InitializePlayers();
        isRolled = false;
        isMoving = false;

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

        currentPlayer = playerTurnOrder[0];
        players[currentPlayer].SetArrow(true);
        currentTurn = 0;
    }

    // 이동 버튼 클릭
    public void ArrowBtnClick(bool clockwise)
    {
        if (!isRolled || isMoving)
        {
            Debug.Log("이동 중이거나 주사위를 굴리지 않았습니다.");
            return;
        }

        isMoving = true;
        StartCoroutine(PlayerTurn(clockwise));
    }

    // 이동 -> 다음 플레이어 턴
    private IEnumerator PlayerTurn(bool clockwise)
    {
        yield return StartCoroutine(players[currentPlayer].PlayerMove(Dice.finalDiceValue, clockwise));

        isMoving = false;
        isRolled = false;
        OccupyRegion(); // 나중에 지워

        NextTurn();
    }

    private void NextTurn()
    {
        players[currentPlayer].SetArrow(false);
        currentTurn++;
        
        // 모두 턴 가졌으면 다음 라운드
        if (currentTurn == playerNum) SortPlayerTurnOrder();
        else
        {
            currentPlayer = playerTurnOrder[currentTurn];
            players[currentPlayer].SetArrow(true);
        }
    }

    // 점령 버튼 클릭 시 작동 - 지금은 도착만 해도 작동
    public void OccupyRegion()
    {
        // 싸우는 스크립트 넣는 곳

        bool win = true;
        int currentCellIndex = players[currentPlayer].GetCellIndex();
        MapTile currentMapTile = GameBoard.mapTiles[currentCellIndex];
        if (win)
        {
            int currentOwner = currentMapTile.Owner;
            currentMapTile.ChangeOwner(currentPlayer);

            tilePerPlayer[currentOwner]--;
            tilePerPlayer[currentPlayer]++;
        }
    }
}
