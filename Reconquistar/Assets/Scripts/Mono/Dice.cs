using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    public Sprite[] diceSprites;

    private SpriteRenderer sr;
    public static int finalDiceValue;
    private bool isRolling = false;

    public static Dice Instance => instance;
    private static Dice instance;
    Dice() { instance = this; }

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public IEnumerator RollDice()
    {
        if (isRolling) yield break;
        isRolling = true;

        float rollDuration = 2f; // 주사위 회전 총 소요 시간
        float currentTime = 0f;

        while (currentTime < rollDuration)
        {
            int randomValue = Random.Range(1, 7);
            sr.sprite = diceSprites[randomValue - 1];

            yield return new WaitForSeconds(0.1f);
            currentTime += 0.1f;
        }

        finalDiceValue = Random.Range(1, 7);
        sr.sprite = diceSprites[finalDiceValue - 1];

        GameManager.isRolled = true;
        isRolling = false;
    }
}
