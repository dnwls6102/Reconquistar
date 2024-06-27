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
    private static float DiceBoardScope = 3f;

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
        
        if (transform.localPosition.x < 0) diceIndex = 0;
        else diceIndex = 1;
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
        if (rb.velocity.sqrMagnitude < 0.5f) return;

        float px = transform.localPosition.x;
        float pz = transform.localPosition.z;

        float vx = rb.velocity.x;
        float vy = rb.velocity.y;
        float vz = rb.velocity.z;

        if ((px < (diceIndex-1) * DiceBoardScope || px > diceIndex * DiceBoardScope) && Mathf.Sign(px) == Mathf.Sign(vx))
        {
            rb.velocity = new Vector3(-vx/2, vy, vz);
        }

        if (Mathf.Abs(pz) > DiceBoardScope && Mathf.Sign(pz) == Mathf.Sign(vz))
        {
            rb.velocity = new Vector3(vx, vy, -vz/2);
        }
    }

    public void RollDice()
    {
        isRolling = true;

        float randomForce = Random.Range(30f, 35f);
        float rollForce = 3f;

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
        float maxY = transform.GetChild(0).transform.position.y - DiceBoardPos.y;

        for (int i = 1; i < 6; i++)
        {
            float py = transform.GetChild(i).transform.position.y - DiceBoardPos.y;
            if (py > maxY)
            {
                diceResult = i + 1;
                maxY = py;
            }
        }

        OnDiceResult?.Invoke(diceIndex, diceResult);
        isRolling = false;
    }
}
