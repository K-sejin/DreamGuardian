using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Pot : MonoBehaviourPunCallbacks
{
    public GameObject content = null;
    public ProgressBar progressBar;
    public GameObject Warn;
    public int ownerPlayerId = -1;
    public float workRate;
    void Awake()
    {
        workRate = 1.0f;
    }
    void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (content != null)
        {
            if (progressBar.currentValue >= progressBar.maxValue)
            {
                if (content.tag == "MindStone")
                {
                    replacePotContent("MeltedMindStone");
                    photonView.RPC("toggleProgress", RpcTarget.AllViaServer, false);
                }
                else if (content.tag == "Steel")
                    replacePotContent("MeltedSteel");
                else if (content.tag == "MeltedMindStone")
                {
                    photonView.RPC("toggleProgress", RpcTarget.AllViaServer, true);
                    replacePotContent("BurnedMindStone");
                }
                photonView.RPC("resetProgressPotPT", RpcTarget.AllViaServer);
            }
        }
    }
    [PunRPC]
    public void setWarn(bool value)
    {
        Warn.SetActive(value);
    }

    [PunRPC]
    public void toggleProgress(bool value)
    {
        if (value)
        {
            Warn.SetActive(false);
            progressBar.toggleImages(true);
            progressBar.ResetProgress();
        }
        else
        {
            progressBar.toggleImages(false);
        }
    }

    [PunRPC]
    public void boil()
    {
        if (content == null) return;
        if (content.tag == "MindStone" || content.tag == "Steel") progressBar.plus(workRate);
        else if(content.tag == "MeltedMindStone") progressBar.plus(workRate / 2);
    }

    public void putInItem(GameObject obj)
    {
        GetComponent<PhotonView>().RPC("putInItemPT", RpcTarget.All, obj.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    public void putInItemPT(int objPhotonViewID)
    {
        PhotonView objPhotonView = PhotonNetwork.GetPhotonView(objPhotonViewID);
        GameObject obj = objPhotonView.gameObject;

        // 물체를 벽의 자식으로 설정
        obj.transform.parent = transform;

        // 물체의 콜라이더 크기를 고려하여 벽 위로 조정
        Vector3 blockTopPosition = GetBlockTopPosition(); // 벽의 상단 위치를 가져오는 메서드
        float objectHeight = GetObjectHeight(obj); // 물체의 높이를 계산

        // 물체의 위치를 벽 위로 조정 (물체의 바닥이 벽의 꼭대기와 맞닿도록)
        float offset = 0.25f;
        if (obj.tag == "MeltedMindStone" || obj.tag == "MindStone" || obj.tag == "BurnedMindStone") offset = 0.4f;
        else if (obj.tag == "MeltedSteel") offset = 0.5f;
        obj.transform.localPosition = new Vector3(0,offset, 0);
        toggleProgress(true);

        content = obj;
    }

    // 물체의 콜라이더에서 높이를 가져오는 메서드
    float GetObjectHeight(GameObject obj)
    {
        Collider objectCollider = obj.GetComponent<Collider>();
        if (objectCollider != null)
        {
            return objectCollider.bounds.size.y; // 콜라이더의 y 크기를 반환
        }
        return obj.transform.localScale.y; // 콜라이더가 없으면 물체의 y 크기를 반환
    }

    // 벽의 상단 위치를 계산하는 메서드
    Vector3 GetBlockTopPosition()
    {
        // 벽의 중심 위치와 상단 오프셋을 더해서 위치를 계산
        return transform.position + new Vector3(0, transform.localScale.y / 2, 0);
    }

    public void trashItem(bool trash = true)
    {
        GetComponent<PhotonView>().RPC("trashItemPT", RpcTarget.All, trash);
    }

    [PunRPC]
    public void trashItemPT(bool trash = true)
    {
        if (trash && PhotonNetwork.IsMasterClient) PhotonNetwork.Destroy(content);
        content = null;
        setWarn(false);
        progressBar.ResetProgress();
    }

    [PunRPC]
    public void replacePotContent(string ItemName)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(content); // 여기도 동기처리 필요
            photonView.RPC("SpawnObjectPotContentPT", RpcTarget.MasterClient, ItemName);
        }
        photonView.RPC("resetProgressPotPT", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void resetProgressPotPT()
    {
        progressBar.ResetProgress();
    }

    [PunRPC]
    public void resetOwnerPot()
    {
        ownerPlayerId = -1;
    }

    [PunRPC]
    public void SpawnObjectPotContentPT(string prefabName)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        GameObject spawnedObject = PhotonNetwork.InstantiateRoomObject(prefab.name, Vector3.zero, Quaternion.identity);
        StartCoroutine(WaitObjectAndPut(spawnedObject));
    }

    IEnumerator WaitObjectAndPut(GameObject obj)
    {
        while (obj == null) yield return null; // 다음 프레임까지 대기
        putInItem(obj);
    }
}
