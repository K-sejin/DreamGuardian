using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CraftTable : Block
{
    public Recipes recipes;
    public float workRate;

    void Awake()
    {
        workRate = 1.575f;
    }
    void Start()
    {
        recipes = new Recipes();
    }

    void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (progressBar.currentValue >= progressBar.maxValue)
        {
            if (liftedItems.Count > 0)
            {
                string item = recipes.CheckCrafting(liftedItems);
                if (item != "")
                {
                    replaceTop(item);
                }
            }
            progressBar.ResetProgress();
        }
    }

    [PunRPC]
    public void work()
    {
        if (liftedItems.Count > 0 && recipes.CheckCrafting(liftedItems) != "")
        {
            progressBar.plus(workRate);
        }
    }
}
