using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DiceThrower : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI TotalDiceNumText;
    private Dice3D[] Dices;
    public static int totalDiceNum;
    public static bool isRolled; // 주사위 굴렸는지

    private void Start()
    {
        isRolled = false;
        Dice3D.OnDiceResult += CheckDiceFinish;
        Dices = transform.GetComponentsInChildren<Dice3D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !Dices[0].IsRolling && !Dices[1].IsRolling)
        {
            TotalDiceNumText.text = "Dice Total: Rolling";
            totalDiceNum = 0;
            for (int i = 0; i < Dices.Length; i++)
            {
                Dices[i].RollDice();
            }
        }
    }

    private void CheckDiceFinish(int diceIndex, int diceResult)
    {
        Debug.Log($"Dice {diceIndex}: {diceResult}");
        totalDiceNum += diceResult;
        for (int i = 0; i < Dices.Length; i++)
        {
            if (Dices[i].IsRolling) return;
        }
        TotalDiceNumText.text = $"Dice Total: {totalDiceNum}";
        isRolled = true;
    }
}
