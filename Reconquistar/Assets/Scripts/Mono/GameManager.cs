using System.Collections;
using System.Collections.Generic;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;

    private int playerNum = 4;
    private int currentPlayer;
    public static bool isRolled;
    public static bool isMoving;
    private Player[] players;

    private void Start()
    {
        InitializePlayers();
        currentPlayer = 0;
        isRolled = false;
        isMoving = false;
        players[currentPlayer].SetArrow(true);
    }

    private void InitializePlayers()
    {
        players = new Player[playerNum];
        for (int i = 0; i < playerNum; i++)
        {
            GameObject playerObject = Instantiate(PlayerPrefab);
            Player player = playerObject.GetComponent<Player>();
            player.SetCellIndex(i * GameBoard.tilePerLine);
            players[i] = player;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRolled)
        {
            StartCoroutine(Dice.Instance.RollDice());
        }

        else if (isRolled && !isMoving)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                isMoving = true;
                StartCoroutine(PlayerTurn(false));
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                isMoving = true;
                StartCoroutine(PlayerTurn(true));
            }
        }
    }

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
