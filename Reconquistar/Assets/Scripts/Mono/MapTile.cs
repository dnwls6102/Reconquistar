using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTile : MonoBehaviour
{
    private Color[] regionColor = { Color.red, Color.blue, Color.green, Color.yellow };

    private SpriteRenderer sr;
    public int Owner => owner;
    private int owner;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        owner = -1;
    }

    public void ChangeOwner(int attacker)
    {
        if (owner != -1)
        {

        }
        owner = attacker;
        sr.color = regionColor[owner];
    }

    public bool CompareOwner(int player)
    {
        return player == owner;
    }
}
