using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.Demo.Procedural;

public class Block : MonoBehaviourPunCallbacks
{
    public Stack<GameObject> liftedItems = new Stack<GameObject>();
    public ProgressBar progressBar;
    public int ownerPlayerId = -1;

    public string[] nonTriggerItem = { "Capsule", "Wood", "Sand", "Clay", "Steel", "MindStone", "WoodenPlank", "ForgedSteel" };

    public void PutDownObject(GameObject obj)
    {
        GetComponent<PhotonView>().RPC("PutDownObjectPT", RpcTarget.All, obj.GetComponent<PhotonView>().ViewID);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    [PunRPC]
    public void PutDownObjectPT(int objPhotonViewID)
    {
        PhotonView objPhotonView = PhotonNetwork.GetPhotonView(objPhotonViewID);
        GameObject obj = objPhotonView.gameObject;
        if (obj != null)
        {
            // ��ü�� ���� �ڽ����� ����
            obj.transform.parent = transform;

            // ��ü�� ������ 0, 0, 0���� ����
            obj.transform.rotation = Quaternion.Euler(0, 0, 0);

            if (!IsContain(obj.tag, nonTriggerItem))
            {
                Collider col = obj.GetComponent<Collider>();
                col.isTrigger = false;
            }

            // ��ü�� �ݶ��̴� ũ�⸦ �����Ͽ� �� ���� ����
            Vector3 blockTopPosition = GetBlockTopPosition(); // ���� ��� ��ġ�� �������� �޼���
            float objectHeight = GetObjectHeight(obj); // ��ü�� ���̸� ���

            // ��ü�� ��ġ�� �� ���� ���� (��ü�� �ٴ��� ���� ������ �´굵��)
            if (obj.tag == "Capsule" && liftedItems.Count == 3 && liftedItems.Peek().tag == "Gun")
            {
                obj.transform.position = new Vector3(blockTopPosition.x, blockTopPosition.y + objectHeight + 0.035f, blockTopPosition.z);
            }
            else if (obj.tag == "Sand")
            {
                obj.transform.position = new Vector3(blockTopPosition.x, blockTopPosition.y + objectHeight + GetTotalStackHeight() - 0.3f, blockTopPosition.z);
            }
            else if (obj.tag == "Doll")
            {
                obj.transform.position = new Vector3(blockTopPosition.x, blockTopPosition.y + objectHeight + GetTotalStackHeight() - 0.7f, blockTopPosition.z);
                obj.transform.rotation = Quaternion.Euler(0, -180, 0);
            }
            else
            {
                obj.transform.position = new Vector3(blockTopPosition.x, blockTopPosition.y + objectHeight + GetTotalStackHeight(), blockTopPosition.z);
            }

            // ��ü�� Rigidbody�� �������� ���Ը� ������� ����
            Rigidbody handRb = obj.GetComponent<Rigidbody>();
            if (handRb != null)
            {
                handRb.isKinematic = false; // ��ü�� ���� ������ ������ �޵��� ����
            }

            // Stack�� ��ü �߰�
            liftedItems.Push(obj);

            Item itemScript = obj.GetComponent<Item>();

            if (PhotonNetwork.IsMasterClient)
            {
                itemScript.ownerPlayerId = -1;
                if (ownerPlayerId == -3) photonView.RPC("resetOwnerBlock", RpcTarget.MasterClient);
                else if (ownerPlayerId != -1 && ownerPlayerId >= 10000) ownerPlayerId = -3;
                else if (ownerPlayerId == -5) ownerPlayerId = -1;
            }
            if (progressBar != null) progressBar.ResetProgress();
        }
    }

    // ��ü�� �ݶ��̴����� ���̸� �������� �޼���
    float GetObjectHeight(GameObject obj)
    {
        Collider objectCollider = obj.GetComponent<Collider>();
        if (objectCollider != null)
        {
            return objectCollider.bounds.size.y; // �ݶ��̴��� y ũ�⸦ ��ȯ
        }
        return obj.transform.localScale.y; // �ݶ��̴��� ������ ��ü�� y ũ�⸦ ��ȯ
    }

    // ���� ��� ��ġ�� ����ϴ� �޼���
    Vector3 GetBlockTopPosition()
    {
        // ���� �߽� ��ġ�� ��� �������� ���ؼ� ��ġ�� ���
        return transform.position + new Vector3(0, transform.localScale.y / 2, 0);
    }
    float GetTotalStackHeight()
    {
        float totalHeight = 0f;
        foreach (GameObject item in liftedItems)
        {
            totalHeight += GetObjectHeight(item); // ���ÿ� �ִ� ��� �������� ���̸� ����
        }
        return totalHeight;
    }
    public void removeTop(int targetPhotonViewID, int playerPhotonViewID, int BlockPhotonViewID)
    {
        photonView.RPC("removeTopPT", RpcTarget.All, targetPhotonViewID, playerPhotonViewID, BlockPhotonViewID);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    [PunRPC]
    public void removeTopPT(int targetPhotonViewID, int playerPhotonViewID, int BlockPhotonViewID)
    {
        PhotonView targetObjectPhotonView = PhotonNetwork.GetPhotonView(targetPhotonViewID);
        GameObject targetObject = targetObjectPhotonView.gameObject;
        GameObject newTarget = liftedItems.Pop();

        resetProgressBlockPT();
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.GetPhotonView(playerPhotonViewID).RPC("liftUpResult", RpcTarget.All, true, newTarget.GetComponent<PhotonView>().ViewID, playerPhotonViewID, BlockPhotonViewID);
    }
    [PunRPC]
    public void resetOwnerBlock()
    {
        ownerPlayerId = -1;
    }

    [PunRPC]
    public void replaceTop(string ItemName)
    {
        photonView.RPC("replaceTopRequest", RpcTarget.MasterClient, ItemName);
    }

    [PunRPC]
    public void replaceTopRequest(string ItemName)
    {
        if (ownerPlayerId != -1) return;
        ownerPlayerId = -5;

        while (liftedItems.Count > 0)
        {
            PhotonNetwork.Destroy(liftedItems.Pop());
        }
        if (ownerPlayerId != -5) Debug.Log("replace bug");
        photonView.RPC("clearStackPT", RpcTarget.All);
        photonView.RPC("SpawnObjectOnBlockPT", RpcTarget.MasterClient, ItemName);
        photonView.RPC("resetProgressBlockPT", RpcTarget.AllViaServer);
    }

    [PunRPC]
    public void clearStackPT()
    {
        liftedItems.Clear();
    }

    [PunRPC]
    public void resetProgressBlockPT()
    {
        if (this.tag == "WorkTable" || this.tag == "CraftTable" || this.tag == "Mixer")
        {
            progressBar.ResetProgress();
        }
    }

    [PunRPC]
    public void SpawnObjectOnBlockPT(string prefabName)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        GameObject spawnedObject = PhotonNetwork.InstantiateRoomObject(prefab.name, Vector3.zero, Quaternion.identity);
        StartCoroutine(WaitObjectAndPut(spawnedObject));
    }

    IEnumerator WaitObjectAndPut(GameObject obj)
    {
        while (obj == null) yield return null; // ���� �����ӱ��� ���
        PutDownObject(obj);
    }

    bool IsContain(string tag, string[] array)
    {
        foreach (string t in array)
        {
            if (t == tag)
            {
                return true;
            }
        }
        return false;
    }
}
