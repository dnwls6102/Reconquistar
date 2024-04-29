using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;

    private int playerNum = 1;
    private int currentPlayer;
    private int diceResult;
    public static bool isRolled;
    public static bool isMoving;
    private Player[] players;

    private void Start()
    {
        InitializePlayers();
        currentPlayer = 0;
        isRolled = false;
        isMoving = false;
    }

    private void InitializePlayers()
    {
        players = new Player[playerNum];
        for (int i = 0; i < playerNum; i++)
        {
            GameObject playerObject = Instantiate(PlayerPrefab);
            Player player = playerObject.GetComponent<Player>();
            player.SetCellIndex(i);
            players[i] = player;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isRolled)
        {
            diceResult = RollDice();
            Debug.Log(diceResult);
        }

        else if (isRolled && !isMoving)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                isMoving = true;
                StartCoroutine(players[currentPlayer].PlayerMove(diceResult, false));
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                isMoving = true;
                StartCoroutine(players[currentPlayer].PlayerMove(diceResult, true));
            }
        }
    }

    private int RollDice()
    {
        int n = Random.Range(1, 7);
        isRolled = true;
        return n;
    }

}
