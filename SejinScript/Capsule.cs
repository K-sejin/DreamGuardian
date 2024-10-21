using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Capsule : MonoBehaviourPunCallbacks
{
    public float capsuleHP = 0;
    public ProgressBar progressBar;
    // Start is called before the first frame update
    void Start()
    {
        progressBar.progressBarObject.SetActive(true);
        progressBar.maxValue = 1000f;
    }

    // Update is called once per frame
    void Update()
    {
        if (progressBar.currentValue > 0f || progressBar.currentValue < progressBar.maxValue)
        {
            progressBar.progressBarObject.SetActive(true);
        }
        progressBar.currentValue = capsuleHP;
    }

    [PunRPC]
    public void fillCapsule(float value, int potPhotonViewID = -1)
    {
        progressBar.progressBarObject.SetActive(true);
        capsuleHP = value;
        progressBar.maxValue = value;
        if(potPhotonViewID != -1 && PhotonNetwork.IsMasterClient)
        {
            Pot pot = PhotonNetwork.GetPhotonView(potPhotonViewID).gameObject.GetComponent<Pot>();
            pot.ownerPlayerId = -1;
        }
    }
    public void DecreaseHp(float liquidLevel)    // liquidLevel �Ű������� �־ ó��
    {
        capsuleHP -= liquidLevel;
        Light capsuleLight = gameObject.GetComponentInChildren<Light>();
        capsuleLight.intensity -= 0.3f;
        //Debug.Log("ĸ�� ������");
        //if (capsuleHP <= 0)
        //{
        //    Debug.Log("ĸ�� �뷮 ��� ����");
        //}
    }
}
