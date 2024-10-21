using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furnace : Block
{
    float boilInterval = 0.05f;
    float boilTimer = 0.0f;
    void Start()
    {
        // 게임 시작 시 Furnace 위에 Pot 스폰하기
        if(PhotonNetwork.IsMasterClient) spawnPot();
    }
    void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (liftedItems.Count > 0) {
                Pot pot = liftedItems.Peek().GetComponent<Pot>();
                if (pot.content != null)
                {
                    boilTimer += Time.deltaTime;
                    if (boilTimer > boilInterval)
                    {
                        boilTimer = 0.0f;
                        pot.GetComponent<PhotonView>().RPC("boil", RpcTarget.AllViaServer);
                    }
                    if (pot.content.tag == "MeltedMindStone" && !pot.Warn.activeSelf && pot.progressBar.currentValue >= pot.progressBar.maxValue / 3)
                    {
                        pot.GetComponent<PhotonView>().RPC("setWarn", RpcTarget.All, true);
                    }
                }
            }
        }
    }
    public void spawnPot()
    {
        photonView.RPC("SpawnObjectPT", RpcTarget.MasterClient, "Pot");
    }

    [PunRPC]
    void SpawnObjectPT(string prefabName)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        GameObject spawnedObject = PhotonNetwork.InstantiateRoomObject(prefab.name, Vector3.zero, Quaternion.identity);
        StartCoroutine(WaitObjectAndPut(spawnedObject));

    }

    IEnumerator WaitObjectAndPut(GameObject obj)
    {
        while (obj == null) yield return null; // 다음 프레임까지 대기
        PutDownObject(obj);
    }
}
