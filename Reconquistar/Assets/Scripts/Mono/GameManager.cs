using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;

    private int playerNum = 4;
    private int currentPlayer;
    public static bool isRolled; // 주사위 굴렸는지
    public static bool isMoving; // 이동 중인지
    private Player[] players;
    private int[] tilePerPlayer; // player 소유한 타일 수

    [SerializeField] private Button ClockwiseBtn;
    [SerializeField] private Button CounterClockwiseBtn;

    private void Start()
    {
        InitializePlayers();
        currentPlayer = 0;
        isRolled = false;
        isMoving = false;
        players[currentPlayer].SetArrow(true);
    }

    // player 생성, 소유 타일 수 지정
    private void InitializePlayers()
    {
        players = new Player[playerNum];
        tilePerPlayer = new int[playerNum];

        for (int i = 0; i < playerNum; i++)
        {
            GameObject playerObject = Instantiate(PlayerPrefab);
            Player player = playerObject.GetComponent<Player>();
            player.SetCellIndex(i * GameBoard.tilePerLine);
            players[i] = player;
            tilePerPlayer[i] = GameBoard.tilePerLine;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRolled)
        {
            StartCoroutine(Dice.Instance.RollDice());
        }
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

        NextTurn();
    }

    private void NextTurn()
    {
        players[currentPlayer].SetArrow(false);
        currentPlayer = (currentPlayer + 1) % playerNum;
        players[currentPlayer].SetArrow(true);
    }
}
