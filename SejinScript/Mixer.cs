using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Mixer : Block
{
    public float workRate;
    void Awake()
    {
        workRate = 2.28f;
    }

    void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (progressBar.currentValue >= progressBar.maxValue)
        {
            if (liftedItems.Count > 0 && liftedItems.Peek().tag == "Sand")
            {
                replaceTop("Clay");
            }
            progressBar.ResetProgress();
        }
    }

    [PunRPC]
    public void work()
    {
        if (liftedItems.Count == 1 && liftedItems.Peek().tag == "Sand")
        {
            progressBar.plus(workRate);
        }
    }
}
