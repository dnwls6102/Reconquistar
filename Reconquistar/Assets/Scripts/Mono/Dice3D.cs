using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

public class Dice3D : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 DiceBoardPos;

    private bool waitingResult;
    private bool isRolling;
    public bool IsRolling
    {
        get { return isRolling; }
        set { isRolling = value; }
    }
    
    public static UnityAction<int, int> OnDiceResult;
    private static float DiceBoardScope = 4f;

    private int diceResult;
    public int DiceResult
    {
        get { return diceResult; }
        set { diceResult = value; }
    }
    private int diceIndex;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        waitingResult = false;
        isRolling = false;
        diceResult = 0;
        DiceBoardPos = transform.parent.position;
    }

    private void Update()
    {
        if (waitingResult) return;
        
        if (isRolling && rb.velocity.sqrMagnitude == 0f)
        {
            isRolling = false;
            GetDiceResult();
        }
    }

    private void FixedUpdate()
    {
        float px = transform.position.x - DiceBoardPos.x;
        float py = transform.position.y - DiceBoardPos.y;
        float pz = transform.position.z - DiceBoardPos.z;

        if (Mathf.Abs(px) > DiceBoardScope)
        {
            transform.position = new Vector3(DiceBoardPos.x + DiceBoardScope * Mathf.Sign(px), py, pz);
        }

        if (Mathf.Abs(pz) > DiceBoardScope)
        {
            transform.position = new Vector3(px, py, DiceBoardPos.z + DiceBoardScope * Mathf.Sign(pz));
        }
    }

    public void RollDice(int idx)
    {
        diceIndex = idx;
        isRolling = true;

        float randomForce = Random.Range(9f, 11f);
        float rollForce = 1f;

        rb.AddForce(-Physics.gravity.normalized * randomForce, ForceMode.Impulse);

        float randX = Random.Range(0f, 1f);
        float randY = Random.Range(0f, 1f);
        float randZ = Random.Range(0f, 1f);
        rb.AddTorque(new Vector3(randX, randY, randZ) * rollForce, ForceMode.Impulse);

        waitingResult = true;
        WaitResult();
    }

    private async void WaitResult()
    {
        await Task.Delay(1000);
        waitingResult = false;
    }

    private void GetDiceResult()
    {
        int diceResult = 1;
        float maxY = transform.GetChild(0).transform.position.y- DiceBoardPos.y;

        for (int i = 1; i < 6; i++)
        {
            float py = transform.GetChild(i).transform.position.y - DiceBoardPos.y;
            if (py > maxY)
            {
                diceResult = i+1;
                maxY = py;
            }
        }

        OnDiceResult?.Invoke(diceIndex, diceResult);
        isRolling = false;
    }
}
