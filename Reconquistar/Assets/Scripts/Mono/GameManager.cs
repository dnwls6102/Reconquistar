using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject PlayerPrefab;

    private int playerNum = 1;
    private int currentPlayer;
    private int diceResult;
    private bool isRolled;
    private Player[] players;

    private void Start()
    {
        InitializePlayers();
        currentPlayer = 0;
        isRolled = false;
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

        else if (isRolled)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                players[currentPlayer].MoveCell(diceResult, false);
                isRolled = false;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                players[currentPlayer].MoveCell(diceResult, true);
                isRolled = false;
            }
        }
    }

    private int RollDice()
    {
        int n = Random.Range(1, 4);
        isRolled = true;
        return n;
    }

}
