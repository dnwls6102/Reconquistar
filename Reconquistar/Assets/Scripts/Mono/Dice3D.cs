using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Dice3D : MonoBehaviour
{
    private Rigidbody rb;
    private Vector3 DiceBoardPos;

    private bool waitingResult;
    private bool isRolling;
    private float DiceBoardScope = 4f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        waitingResult = false;
        isRolling = false;
        DiceBoardPos = transform.parent.position;
    }

    private void Update()
    {
        if (waitingResult) return;

        if (!isRolling && rb.velocity.sqrMagnitude == 0f)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isRolling = true;
                RollDice();
            }
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

    public void RollDice()
    {
        waitingResult = true;
        float randomForce = Random.Range(9f, 11f);
        float rollForce = 1f;

        Debug.Log("Dice Throw Force: " + randomForce);
        Debug.Log("Dice Roll Force: " + rollForce);
        rb.AddForce(-Physics.gravity.normalized * randomForce, ForceMode.Impulse);

        float randX = Random.Range(0f, 1f);
        float randY = Random.Range(0f, 1f);
        float randZ = Random.Range(0f, 1f);

        rb.AddTorque(new Vector3(randX, randY, randZ) * rollForce, ForceMode.Impulse);
        waitingResult = true;
        isRolling = false;
        WaitResult();
    }

    private async void WaitResult()
    {
        await Task.Delay(1000);
        waitingResult = false;
    }
}
