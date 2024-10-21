using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkTable : Block
{
    public float workRate;
    void Awake()
    {
        workRate = 3.15f;
    }

    void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (progressBar.currentValue >= progressBar.maxValue)
        {
            if (liftedItems.Count > 0)
            {
                if (liftedItems.Peek().tag == "Wood")
                    replaceTop("WoodenPlank");
                else if (liftedItems.Peek().tag == "MeltedSteel")
                    replaceTop("ForgedSteel");
            }
            progressBar.ResetProgress();
        }
    }

    [PunRPC]
    public void work()
    {
        if (liftedItems.Count == 1)
        {
            if (liftedItems.Peek().tag == "Wood")
                progressBar.plus(workRate);

            else if (liftedItems.Peek().tag == "MeltedSteel")
                progressBar.plus(workRate);
        }
    }
}
