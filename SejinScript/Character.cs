using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;

public class Character : MonoBehaviourPunCallbacks
{
    public Recipes recipes;
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public Vector3 inputVec;
    public Vector3 spawnPoint;
    public float speed;
    public float dashSpeed; // 대시 속도
    public float dashDuration; // 대시 지속 시간
    public float dashCooldown; // 대시 쿨타임
    private bool isDashing; // 대시 여부
    private bool canDash; // 대시 가능 여부
    Rigidbody rb; // Rigidbody 컴포넌트
    public GameObject target;
    public GameObject _highlightTarget;
    public GameObject highlightTarget
    {
        get { return _highlightTarget; }
        set
        {
            if (_highlightTarget != value)
            {
                _highlightTarget = value;
                OnHLTargetChanged();  // 타겟이 바뀔 때 호출되는 콜백 함수
            }
        }
    }
    public GameObject CanvasObject;
    public GameObject TimerCanvas;
    public TextMeshProUGUI timerText;

    public GameObject holdingObject;
    public string statement = "STORY";
    private bool isWorkCoroutineRunning = false;
    public bool focusing = true;

    public float raycastDistance = 1f; // 레이캐스트 거리
    public LayerMask collisionLayer; // 충돌 판정을 받을 레이어

    public PhotonView PV;

    private Animator animator; // 애니메이터 컴포넌트
    public float rotationSpeed = 10f; // 회전 속도

    private Transform highlightTransform;
    private GameObject prevTarget;

    private string[] liftableTags = { "Pot", "Pedestal", "Body", "Gun", "Capsule", "Sand", "Clay", "Wood", "WoodenPlank", "MindStone", "BluePrint", "Steel", "ForgedSteel", "Doll" };
    private string[] trashableTags = { "Pedestal", "Body", "Gun", "Capsule", "Sand", "Clay", "Wood", "WoodenPlank", "MindStone", "BluePrint", "Steel", "MeltedSteel", "ForgedSteel", "Doll" };
    private string[] placeableOnWorkTableTags = { "Pedestal", "Body", "Gun", "Capsule", "Sand", "Clay", "Wood", "WoodenPlank", "MindStone", "BluePrint", "MeltedSteel" };
    private string[] placeableOnCraftTableTags = { "Pedestal", "Body", "Gun", "Sand", "Clay", "Wood", "WoodenPlank", "BluePrint", "Steel", "ForgedSteel", "MindStone" };

    private GameManager gameManager;

    public KeyCode SpaceKey;
    public KeyCode CtrlKey;
    public KeyCode ToggleKey;
    public KeyCode DashKey;

    public int playerCnt;
    public bool CanToggle = true;

    void Awake()
    {
        statement = "STORY";
        transform.eulerAngles = new Vector3(0, -180, 0);
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        PV = GetComponent<PhotonView>();
        holdingObject = null;
        isDashing = false; //대쉬상태 초기화
        recipes = new Recipes();
        dashSpeed = 15f;
        dashDuration = 0.1f;
        dashCooldown = 0.2f;
        canDash = true;
        if (PV.IsMine)
        {
            CanvasObject.SetActive(true);
        }
        gameManager = GameManager.instance;

        Player[] players = PhotonNetwork.PlayerList;
        int playerNumber = System.Array.IndexOf(players, PhotonNetwork.LocalPlayer);
        spawnPoint = gameManager.spawnPoints[playerNumber];
        playerCnt = PhotonNetwork.PlayerList.Length;
        CanToggle = (playerCnt == 1) && (PlayerPrefs.GetString("isTutorial") == "0");

        SpaceKey = (KeyCode)PlayerPrefs.GetInt("SpaceKey", (int)KeyCode.Space);
        CtrlKey = (KeyCode)PlayerPrefs.GetInt("CtrlKey", (int)KeyCode.LeftControl);
        ToggleKey = (KeyCode)PlayerPrefs.GetInt("ToggleKey", (int)KeyCode.LeftShift);
        DashKey = (KeyCode)PlayerPrefs.GetInt("DashKey", (int)KeyCode.LeftAlt);
    }

    void Update()
    {
        if (!PV.IsMine) return;
        if (statement == "STORY") return;
        if (statement == "WORKING" && target != null && !isWorkCoroutineRunning)
        {
            StartCoroutine(WorkCoroutine());
            isWorkCoroutineRunning = true;
        }
        else if (statement == "IDLE" && isWorkCoroutineRunning)
        {
            StopCoroutine(WorkCoroutine());
            SetMakingState(false);
            SetMixingState(false);
            isWorkCoroutineRunning = false;
        }
        if (CanToggle && Input.GetKeyDown(ToggleKey)) toggleCharacter();
        ShootRaycast();
        if (!IsMoveAble()) return;

        if (Input.GetKeyDown(SpaceKey)) OnSpaceDown();
        if (Input.GetKeyDown(CtrlKey)) OnCtrlDown();
        if (Input.GetKeyDown(DashKey) && statement == "IDLE" && canDash) // 기본 상태에서 Alt 버튼을 눌렀을 때 대시 시작
        {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        if (!PV.IsMine || isDashing) return; // 대시 중일 때는 이동 처리 안 함
        if (!IsMoveAble())
        {
            if (!GetComponent<Rigidbody>().isKinematic) rb.velocity = Vector3.zero;
            return;
        }
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.z = Input.GetAxisRaw("Vertical");

        // 이동 벡터 계산
        inputVec.Normalize();
        Vector3 movement = new Vector3(inputVec.x, 0.0f, inputVec.z);

        // Rigidbody를 이용한 이동
        rb.velocity = movement * speed;

        // 이동 벡터의 크기가 0보다 클 때
        if (inputVec.magnitude > 0.1f)
        {
            // 이동 방향으로 플레이어 회전
            Quaternion toRotation = Quaternion.LookRotation(inputVec, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // 움직임이 없을 때 속도를 0으로 설정
            rb.velocity = Vector3.zero;
        }
    }

    private void LateUpdate()
    {
        if (!PV.IsMine) return;
        if (!IsMoveAble())
        {
            SetWalkingSpeed(0f);
            return;
        }
        SetWalkingSpeed(inputVec.magnitude);
    }

    void OnHLTargetChanged()
    {
        if (statement == "DEAD") return;
        if (highlightTarget != prevTarget)
        {
            if (prevTarget != null)
            {
                RemoveHighlight();
            }
            if (highlightTarget != null)
            {
                HighlightObject(highlightTarget);
            }
        }
        else if (highlightTarget == null && prevTarget != null)
        {
            RemoveHighlight();
        }
        statement = "IDLE";
    }


    void ShootRaycast()
    {
        // 콜라이더의 중심에서 레이캐스트 발사
        Vector3 rayOrigin = transform.position + new Vector3(0f, 0.01f, 0f);

        Debug.DrawRay(rayOrigin, transform.forward, Color.red, 1f);

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, transform.forward, out hit, raycastDistance, collisionLayer))
        {
            target = hit.collider.gameObject;
            GameObject newHighlightTarget = getHLTarget(hit.collider.gameObject);

            // highlightTarget이 null이거나, 새로 감지된 highlightTarget이 기존 highlightTarget과 다른 경우
            if (highlightTarget != newHighlightTarget)
            {
                highlightTarget = newHighlightTarget;
            }
        }
        else
        {
            target = null;
            if (highlightTarget != null)
            {
                highlightTarget = null;
            }
        }
    }
    GameObject getHLTarget(GameObject obj)
    {
        Block blockScript = obj.GetComponent<Block>();
        if (blockScript == null) return obj;
        if (blockScript.liftedItems.Count > 0)
        {
            return blockScript.liftedItems.Peek();
        }
        return obj;
    }

    void HighlightObject(GameObject obj)
    {
        // 현재 obj의 자식 중 HighlightCube 가져오기
        MeshRenderer[] meshRenderers = obj.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshRenderer in meshRenderers)
        {
            if (meshRenderer.gameObject.name.Contains("HighlightCube"))
            {
                meshRenderer.enabled = true;
            }
        }
        // prevTarget에 obj를 넣기
        prevTarget = obj;
    }

    void RemoveHighlight()
    {
        if (prevTarget != null)
        {
            // prevTarget의 모든 자식 오브젝트 중 이름에 "HighlightCube"가 포함된 오브젝트의 MeshRenderer를 끄기
            MeshRenderer[] meshRenderers = prevTarget.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                if (meshRenderer.gameObject.name.Contains("HighlightCube"))
                {
                    meshRenderer.enabled = false;
                }
            }
            // prevTarget을 null로 만들기
            prevTarget = null;
        }
    }

    IEnumerator WorkCoroutine()
    {
        while (statement == "WORKING" && target != null)
        {
            // Block 컴포넌트에서 work 메서드 RPC 호출
            Block block = target.GetComponent<Block>();
            block.GetComponent<PhotonView>().RPC("work", RpcTarget.AllViaServer);

            // 1초 대기 후 다시 실행
            yield return new WaitForSeconds(0.05f);
        }
    }
    void OnSpaceDown()
    {
        if (holdingObject != null) // 빈손이 아닐 때
        {
            if (target == null) return;
            interaction(holdingObject, target);
        }
        else // 빈손일 때
        {
            if (target == null) return;
            if (IsContain(target.tag, liftableTags)) // 들 수 있는 물체면 들기 (확장)
            {
                //liftUp(target);
            }
            else
            {
                if (target.tag == "BlueprintBox")
                {
                    //statement = "BLUEPRINT";
                    BlueprintBox blueprintBox = target.GetComponent<BlueprintBox>();
                    blueprintBox.Toggle();
                }
                else bareHand(target);
            }
        }
    }

    void bareHand(GameObject target)
    {
        PhotonView targetPhotonView = target.GetComponent<PhotonView>();
        if (targetPhotonView == null) return;
        photonView.RPC("bareHandPT", RpcTarget.MasterClient, targetPhotonView.ViewID);
    }

    [PunRPC]
    void bareHandPT(int targetPhotonViewID)
    {
        GameObject target = PhotonNetwork.GetPhotonView(targetPhotonViewID).gameObject;
        if (target == null) return;

        switch (target.tag)
        {
            case "SandBox":
                SpawnAndLiftPrefab("Sand");
                break;
            case "WoodBox":
                SpawnAndLiftPrefab("Wood");
                break;
            case "MindStoneMine":
                SpawnAndLiftPrefab("MindStone");
                break;
            case "SteelBox":
                SpawnAndLiftPrefab("Steel");
                break;
            case "CapsuleBox": // 최대 개수 지정
                CapsuleBox capsuleBox = target.GetComponent<CapsuleBox>();
                if (capsuleBox.canGetCapsule())
                {
                    SpawnAndLiftPrefab("Capsule");
                    capsuleBox.minusCapsule();
                }
                break;
            case "RandomBox":
                SpawnAndLiftPrefab(RandomBox.GetRandomItem());
                break;
            case "Wall":
            case "WorkTable":
            case "CraftTable":
            case "Mixer":
            case "Furnace":
                Block blockScript = target.GetComponent<Block>();
                if (blockScript.liftedItems.Count > 0)
                {
                    GameObject topItemOnBlock = blockScript.liftedItems.Peek();
                    if (IsContain(topItemOnBlock.tag, liftableTags))
                    {
                        if (topItemOnBlock.tag == "Capsule" && blockScript.liftedItems.Count == 4) // 터렛 끄기
                        {
                            blockScript.liftedItems.ToArray()[1].GetComponent<PhotonView>().RPC("offTurret", RpcTarget.All);
                        }
                        if (target.tag == "Furnace")
                        {
                            Pot pot = blockScript.liftedItems.Peek().GetComponent<Pot>();
                            pot.GetComponent<PhotonView>().RPC("setWarn", RpcTarget.All, false);
                        }
                        liftUp(topItemOnBlock, blockScript.GetComponent<PhotonView>().ViewID);
                    }
                }
                break;
            case "TrashCan":
                trashObject();
                break;
        }
    }

    [PunRPC]
    void dropObject(int holdingObjectPhotonViewID)
    {
        PhotonView holdingObjectPhotonView = PhotonNetwork.GetPhotonView(holdingObjectPhotonViewID);

        if (holdingObjectPhotonView != null)
        {
            // 물체를 플레이어의 자식에서 분리
            holdingObject.transform.parent = null;

            // 물체를 비활성화하거나 이동
            holdingObject.SetActive(true); // 물체를 다시 활성화하려면 true로 설정

            holdingObject.layer = 20;

            holdingObject = null;
        }
    }

    public void liftUp(GameObject target, int BlockPhotonViewID = -1)
    {
        int targetPhotonViewID = target.GetComponent<PhotonView>().ViewID;
        int playerPhotonViewID = GetComponent<PhotonView>().ViewID;
        PV.RPC("liftUpRequest", RpcTarget.MasterClient, targetPhotonViewID, playerPhotonViewID, BlockPhotonViewID);
        PhotonNetwork.SendAllOutgoingCommands();
    }

    [PunRPC]
    public void liftUpRequest(int targetPhotonViewID, int playerPhotonViewID, int BlockPhotonViewID = -1)
    {
        PhotonView targetPhotonView = PhotonNetwork.GetPhotonView(targetPhotonViewID);

        if (!PhotonNetwork.IsMasterClient) return;

        Item item = targetPhotonView.GetComponent<Item>();

        // 블록에서 아이템을 드는 경우
        if (BlockPhotonViewID != -1)
        {
            PhotonView blockPhotonView = PhotonNetwork.GetPhotonView(BlockPhotonViewID);
            Block block = blockPhotonView.GetComponent<Block>();

            // 블록이 잠겨있지 않은지 확인하고, 잠금
            if (block.ownerPlayerId != -1) return;

            // 아이템이 잠겨있지 않은지 확인하고, 잠금
            if (item.ownerPlayerId != -1) return;

            // 블록을 플레이어에게 잠금 설정
            block.ownerPlayerId = playerPhotonViewID;

            // 아이템을 플레이어에게 잠금 설정
            item.ownerPlayerId = playerPhotonViewID;

            liftUpPT(targetPhotonViewID, playerPhotonViewID, BlockPhotonViewID);
        }
        else
        {
            // 아이템이 잠겨있지 않은지 확인하고, 잠금
            if (item.ownerPlayerId != -1) return;

            // 아이템을 플레이어에게 잠금 설정
            item.ownerPlayerId = playerPhotonViewID;

            liftUpPT(targetPhotonViewID, playerPhotonViewID, -1);
        }
    }

    [PunRPC]
    void liftUpPT(int targetPhotonViewID, int playerPhotonViewID, int BlockPhotonViewID)
    {
        PhotonView targetPhotonView = PhotonNetwork.GetPhotonView(targetPhotonViewID);
        GameObject targetObject = targetPhotonView.gameObject;
        Item itemScript = targetObject.GetComponent<Item>();

        if (holdingObject != null)
        {
            PV.RPC("liftUpResult", RpcTarget.All, false, targetPhotonViewID, playerPhotonViewID, BlockPhotonViewID);
            return;
        }

        if (BlockPhotonViewID != -1)
        {
            Block block = PhotonNetwork.GetPhotonView(BlockPhotonViewID).gameObject.GetComponent<Block>();

            if (block.ownerPlayerId != playerPhotonViewID || itemScript.ownerPlayerId != playerPhotonViewID)
            {
                PV.RPC("liftUpResult", RpcTarget.All, false, targetPhotonViewID, playerPhotonViewID, BlockPhotonViewID);
                return;
            }
            if (PhotonNetwork.IsMasterClient) block.removeTop(targetPhotonViewID, playerPhotonViewID, BlockPhotonViewID);
        }
        else
        {
            if (PhotonNetwork.IsMasterClient) PV.RPC("liftUpResult", RpcTarget.All, true, targetPhotonViewID, playerPhotonViewID, BlockPhotonViewID);
        }
    }
    [PunRPC]
    void liftUpResult(bool success, int targetObjectPhotonViewID, int playerPhotonViewID, int BlockPhotonViewID)
    {
        if (playerPhotonViewID != GetComponent<PhotonView>().ViewID) return;
        PhotonView targetObjectPhotonView = PhotonNetwork.GetPhotonView(targetObjectPhotonViewID);
        GameObject targetObject = targetObjectPhotonView.gameObject;

        if (success)
        {
            RemoveHighlight();
            holdingObject = targetObject;
            MusicManager.Liftup();

            holdingObject.layer = 2;  // 해당 물체를 특정 레이어로 설정

            Collider col = holdingObject.GetComponent<Collider>();
            col.isTrigger = true;

            // 물체의 Rigidbody를 가져오고 무게를 0으로 설정
            Rigidbody targetRb = holdingObject.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                targetRb.isKinematic = true; // 물체가 물리 엔진의 영향을 받지 않도록 설정
            }

            // 물체를 플레이어의 자식으로 설정
            holdingObject.transform.parent = transform;

            // 물체의 위치를 플레이어의 손 위치로 조정
            holdingObject.transform.localPosition = new Vector3(0, 3.7f, 0); // 손 위치에 맞게 조정
            OnHLTargetChanged();
            if (BlockPhotonViewID != -1)
            {
                PhotonView BlockPhotonView = PhotonNetwork.GetPhotonView(BlockPhotonViewID);
                Block block = BlockPhotonView.gameObject.GetComponent<Block>();
                if (block.ownerPlayerId == playerPhotonViewID) BlockPhotonView.RPC("resetOwnerBlock", RpcTarget.MasterClient);
            }
        }
        else
        {
            Debug.Log("fail lift");
            if (BlockPhotonViewID != -1)
            {
                PhotonView BlockPhotonView = PhotonNetwork.GetPhotonView(BlockPhotonViewID);
                Block block = BlockPhotonView.gameObject.GetComponent<Block>();
                BlockPhotonView.RPC("resetOwnerBlock", RpcTarget.MasterClient);
            }
            Item item = targetObject.GetComponent<Item>();
            item.ownerPlayerId = -1; // 여기 andGate?
        }
    }

    void interaction(GameObject hand, GameObject target)
    {
        int handPhotonViewID = hand.GetComponent<PhotonView>().ViewID;
        int targetPhotonViewID = target.GetComponent<PhotonView>().ViewID;

        photonView.RPC("interactionPT", RpcTarget.MasterClient, handPhotonViewID, targetPhotonViewID);
    }

    [PunRPC]
    void interactionPT(int handPhotonViewID, int targetPhotonViewID)
    {
        GameObject hand = PhotonNetwork.GetPhotonView(handPhotonViewID).gameObject;
        GameObject target = PhotonNetwork.GetPhotonView(targetPhotonViewID).gameObject;
        if (hand == null || target == null) return;

        switch (target.tag) // target이 wall인데 그 위에 물건이 올라가 있을 때도 고려해야함
        {
            case "WorkTable":
                Block block = target.GetComponent<Block>();
                if (IsContain(hand.tag, placeableOnWorkTableTags) && block.liftedItems.Count == 0)
                {
                    putDown(hand, target);
                }
                else if (hand.tag == "Pot")
                {
                    Pot pot = hand.GetComponent<Pot>();
                    if (pot.content != null && pot.content.tag == "MeltedSteel" && block.liftedItems.Count == 0)
                    {
                        // 수정 필요
                        putDown(pot.content, target);
                        //pot.content = null;
                        //pot.progressBar.currentValue = 0;
                        //pot.GetComponent<PhotonView>().RPC("trashItem", PhotonNetwork.LocalPlayer, false);
                        pot.trashItem(false);
                    }
                }
                break;
            case "Mixer":
                if (hand.CompareTag("Sand"))
                {
                    Mixer mixer = target.GetComponent<Mixer>();
                    if(mixer.liftedItems.Count == 0) putDown(hand, target);
                }
                break;
            case "CraftTable":
                if (IsContain(hand.tag, placeableOnCraftTableTags)) putDown(hand, target);
                break;
            case "Wall":
                Wall wall = target.GetComponent<Wall>();
                if (wall.liftedItems.Count == 0 ||
                (wall.liftedItems.Count == 1 && wall.liftedItems.Peek().name == "Pedestal_enforce(Clone)" && hand.tag == "Body") ||
                (wall.liftedItems.Count == 2 && wall.liftedItems.Peek().name == "Body_enforce(Clone)" && hand.tag == "Gun") ||
                (wall.liftedItems.Count == 1 && wall.liftedItems.Peek().name == "Pedestal_basic(Clone)" && hand.name == "Body_basic(Clone)") ||
                (wall.liftedItems.Count == 2 && wall.liftedItems.Peek().name == "Body_basic(Clone)" && (hand.name == "Gun_basic(Clone)" || hand.name == "Gun_double(Clone)")))
                {
                    putDown(hand, target);
                }
                else if (wall.liftedItems.Count == 3 && wall.liftedItems.Peek().tag == "Gun" && hand.tag == "Capsule")
                {
                    GameObject gun = wall.liftedItems.Peek();
                    putDown(hand, target);
                    gun.GetComponent<PhotonView>().RPC("runTurret", RpcTarget.All);
                }
                else if (wall.liftedItems.Peek().tag == "Pot")
                {
                    tryPutInPot(hand, wall.liftedItems.Peek());
                }
                else if (wall.liftedItems.Count > 0 && wall.liftedItems.Peek().tag == "Capsule")
                {
                    if (hand.tag == "Pot" && hand.GetComponent<Pot>().content != null && hand.GetComponent<Pot>().content.tag == "MeltedMindStone")
                        fillCapsuleWithPot(wall.liftedItems.Peek(), hand.GetComponent<Pot>());
                    else if (hand.tag == "Capsule")
                    {
                        Capsule handCapsule = hand.GetComponent<Capsule>();
                        Capsule targetCapsule = wall.liftedItems.Peek().GetComponent<Capsule>();
                        CapsuleToCapsule(handCapsule, targetCapsule);
                    }
                }
                break;
            case "Pedestal":
                break;
            case "Body":
                break;
            case "Gun":
                break;
            case "Character":
                break;
            case "TrashCan":
                if (hand.tag == "Pot")
                {
                    Pot pot = hand.GetComponent<Pot>();
                    if (pot.content != null)
                    {
                        pot.trashItem();
                    }
                }
                else if (hand.tag == "Capsule")
                {
                    // 캡슐박스에 남은 캡슐 개수 + 1
                    CapsuleBox capsuleBox = GameObject.FindWithTag("CapsuleBox").GetComponent<CapsuleBox>();
                    capsuleBox.plusCapsule();
                    trashObject();
                }
                else trashObject();
                break;
            case "Furnace":
                Furnace furnace = target.GetComponent<Furnace>();
                if (furnace.liftedItems.Count == 1)
                {
                    tryPutInPot(hand, furnace.liftedItems.Peek());
                }
                else
                {
                    if (hand.tag == "Pot") putDown(hand, target);
                }
                break;
            case "Capsule":
                // 비어있는것만
                break;
        }
    }

    void CapsuleToCapsule(Capsule handCapsule, Capsule targetCapsule)
    {
        int handCapsulePhotonViewID = handCapsule.GetComponent<PhotonView>().ViewID;
        int targetCapsulePhotonViewID = targetCapsule.GetComponent<PhotonView>().ViewID;
        int playerPhotonViewID = GetComponent<PhotonView>().ViewID;
        PV.RPC("CTC", RpcTarget.MasterClient, handCapsulePhotonViewID, targetCapsulePhotonViewID, playerPhotonViewID);
    }

    [PunRPC]
    void CTC(int handCapsulePhotonViewID, int targetCapsulePhotonViewID, int playerPhotonViewID)
    {
        Capsule handCapsule = PhotonNetwork.GetPhotonView(handCapsulePhotonViewID).gameObject.GetComponent<Capsule>();
        Capsule targetCapsule = PhotonNetwork.GetPhotonView(targetCapsulePhotonViewID).gameObject.GetComponent<Capsule>();
        Item capsuleItem = targetCapsule.GetComponent<Item>();

        if (capsuleItem.ownerPlayerId != -1) return;
        capsuleItem.ownerPlayerId = playerPhotonViewID;

        PV.RPC("CTCResult", RpcTarget.All, handCapsulePhotonViewID, targetCapsulePhotonViewID, playerPhotonViewID);
    }

    [PunRPC]
    void CTCResult(int handCapsulePhotonViewID, int targetCapsulePhotonViewID, int playerPhotonViewID)
    {
        Capsule handCapsule = PhotonNetwork.GetPhotonView(handCapsulePhotonViewID).gameObject.GetComponent<Capsule>();
        Capsule targetCapsule = PhotonNetwork.GetPhotonView(targetCapsulePhotonViewID).gameObject.GetComponent<Capsule>();
        Item capsuleItem = targetCapsule.GetComponent<Item>();

        float moveAmount = Mathf.Min(handCapsule.capsuleHP, targetCapsule.progressBar.maxValue - targetCapsule.capsuleHP);
        handCapsule.capsuleHP -= moveAmount;
        targetCapsule.capsuleHP += moveAmount;

        if (PhotonNetwork.IsMasterClient) capsuleItem.ownerPlayerId = -1;
    }

    void tryPutInPot(GameObject hand, GameObject potObject)
    {
        Pot pot = potObject.GetComponent<Pot>();
        if (pot.content == null)
        {
            if (hand.tag == "MindStone" || hand.tag == "Steel")
            {
                putInPot(hand, pot);
            }
        }
        else if (hand.tag == "Capsule" && pot.content.tag == "MeltedMindStone")
        {
            fillCapsuleWithPot(hand, pot);
        }
    }

    void putInPot(GameObject hand, Pot pot)
    {
        int handPhotonViewID = hand.GetComponent<PhotonView>().ViewID;
        int potPhotonViewID = pot.gameObject.GetComponent<PhotonView>().ViewID;
        int playerPhotonViewID = GetComponent<PhotonView>().ViewID;
        photonView.RPC("putInPotPT", RpcTarget.MasterClient, handPhotonViewID, potPhotonViewID, playerPhotonViewID);
    }

    [PunRPC]
    void putInPotPT(int handPhotonViewID, int potPhotonViewID, int playerPhotonViewID)
    {
        GameObject hand = PhotonNetwork.GetPhotonView(handPhotonViewID).gameObject;
        Pot pot = PhotonNetwork.GetPhotonView(potPhotonViewID).gameObject.GetComponent<Pot>();

        if (pot.ownerPlayerId != -1) return;
        pot.ownerPlayerId = playerPhotonViewID;

        pot.putInItem(hand);
        photonView.RPC("putInPotResult", RpcTarget.All, playerPhotonViewID, potPhotonViewID);
    }

    [PunRPC]
    void putInPotResult(int playerPhotonViewID, int potPhotonViewID)
    {
        if (playerPhotonViewID != GetComponent<PhotonView>().ViewID) return;
        PhotonView potPhotonView = PhotonNetwork.GetPhotonView(potPhotonViewID);
        potPhotonView.RPC("resetOwnerPot", RpcTarget.MasterClient);
        holdingObject = null;
        MusicManager.Boil();
    }

    void fillCapsuleWithPot(GameObject cap, Pot pot)
    {
        Capsule capsule = cap.GetComponent<Capsule>();

        int potPhotonViewID = pot.GetComponent<PhotonView>().ViewID;
        int capsulePhotonViewID = cap.GetComponent<PhotonView>().ViewID;
        int playerPhotonViewID = GetComponent<PhotonView>().ViewID;
        photonView.RPC("FCWP", RpcTarget.MasterClient, potPhotonViewID, capsulePhotonViewID, playerPhotonViewID);
    }

    [PunRPC]
    void FCWP(int potPhotonViewID, int capsulePhotonViewID, int playerPhotonViewID)
    {
        Pot pot = PhotonNetwork.GetPhotonView(potPhotonViewID).GetComponent<Pot>();
        Capsule capsule = PhotonNetwork.GetPhotonView(capsulePhotonViewID).GetComponent<Capsule>();
        if (pot.ownerPlayerId != -1) return;
        pot.ownerPlayerId = playerPhotonViewID;

        pot.trashItem();
        capsule.photonView.RPC("fillCapsule", RpcTarget.All, 1000f, potPhotonViewID);
        //pot.ownerPlayerId = -1; // 되는지 확인 필요
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

    bool IsMoveAble()
    {
        if (gameManager.isChatFocused) return false;
        //if (statement == "BLUEPRINT" || statement == "TUTORIAL" || statement == "STORY") return false;
        if (statement == "TUTORIAL" || statement == "STORY" || statement == "DEAD" || !focusing) return false;
        return true;
    }

    void putDown(GameObject hand, GameObject target)
    {
        int targetPhotonViewID = target.GetComponent<PhotonView>().ViewID;
        int handObjectPhotonViewID = hand.GetComponent<PhotonView>().ViewID;
        int playerPhotonViewID = GetComponent<PhotonView>().ViewID;

        PV.RPC("putDownRequest", RpcTarget.MasterClient, handObjectPhotonViewID, targetPhotonViewID, playerPhotonViewID);
        PhotonNetwork.SendAllOutgoingCommands();
    }
    [PunRPC]
    public void putDownRequest(int handPhotonViewID, int targetPhotonViewID, int playerPhotonViewID)
    {
        PhotonView targetPhotonView = PhotonNetwork.GetPhotonView(targetPhotonViewID);
        GameObject targetObject = targetPhotonView.gameObject;
        Block block = targetObject.GetComponent<Block>();

        if (!PhotonNetwork.IsMasterClient) return;

        // 블록이 현재 사용 가능한 상태인지 확인
        if (block.ownerPlayerId != -1) return;

        block.ownerPlayerId = playerPhotonViewID + 10000;

        // 상태 변경 후 putDownObject 수행
        putDownPT(handPhotonViewID, targetPhotonViewID, playerPhotonViewID);
    }

    [PunRPC]
    void putDownPT(int handObjectPhotonViewID, int targetPhotonViewID, int playerPhotonViewID)
    {
        PhotonView targetPhotonView = PhotonNetwork.GetPhotonView(targetPhotonViewID);
        PhotonView handObjectPhotonView = PhotonNetwork.GetPhotonView(handObjectPhotonViewID);
        GameObject targetObject = targetPhotonView.gameObject;
        GameObject handObject = handObjectPhotonView.gameObject;
        Block blockScript = targetObject.GetComponent<Block>();

        if (holdingObject == null || blockScript.ownerPlayerId != playerPhotonViewID + 10000)
        {
            PV.RPC("putDownResult", RpcTarget.All, false, handObjectPhotonViewID, playerPhotonViewID, targetPhotonViewID);
            return;
        }
        if (PhotonNetwork.IsMasterClient)
        {
            blockScript.PutDownObject(handObject);
            PV.RPC("putDownResult", RpcTarget.All, true, handObjectPhotonViewID, playerPhotonViewID, targetPhotonViewID);
            PhotonNetwork.SendAllOutgoingCommands();
        }
    }
    [PunRPC]
    void putDownResult(bool success, int handObjectPhotonViewID, int playerPhotonViewID, int targetPhotonViewID)
    {
        if (playerPhotonViewID != GetComponent<PhotonView>().ViewID) return;
        if (success)
        {
            PhotonView handObjectPhotonView = PhotonNetwork.GetPhotonView(handObjectPhotonViewID);
            GameObject handObject = handObjectPhotonView.gameObject;
            PhotonView targetPhotonView = PhotonNetwork.GetPhotonView(targetPhotonViewID);
            Block block = targetPhotonView.gameObject.GetComponent<Block>();

            if (handObject == holdingObject)
            {
                holdingObject = null;
                MusicManager.Liftdown();
            }

            if (block.ownerPlayerId == -3) targetPhotonView.RPC("resetOwnerBlock", RpcTarget.MasterClient);
            else if (block.ownerPlayerId == playerPhotonViewID + 10000) block.ownerPlayerId = -3;
        }
        else
        {
            PhotonView targetPhotonView = PhotonNetwork.GetPhotonView(targetPhotonViewID);
            Block block = targetPhotonView.gameObject.GetComponent<Block>();
            targetPhotonView.RPC("resetOwnerBlock", RpcTarget.MasterClient);
            Debug.Log("fail Put");
        }
    }

    void SpawnAndLiftPrefab(string prefabName)
    {
        photonView.RPC("SpawnObjectPT", RpcTarget.MasterClient, prefabName);
    }

    [PunRPC]
    void SpawnObjectPT(string prefabName)
    {
        GameObject prefab = Resources.Load<GameObject>(prefabName);
        GameObject spawnedObject = PhotonNetwork.InstantiateRoomObject(prefab.name, Vector3.zero, Quaternion.identity);

        // 객체가 생성된 후 로컬 플레이어에게 liftUp 실행 요청
        photonView.RPC("RequestLiftUpSpawned", RpcTarget.AllBuffered, spawnedObject.GetComponent<PhotonView>().ViewID);
    }

    [PunRPC]
    void RequestLiftUpSpawned(int viewID)
    {
        GameObject obj = PhotonNetwork.GetPhotonView(viewID).gameObject;

        // 해당 객체를 찾은 뒤 로컬 플레이어가 liftUp 실행
        if (photonView.IsMine)
        {
            StartCoroutine(WaitObjectAndLift(obj));
        }
    }

    IEnumerator WaitObjectAndLift(GameObject obj)
    {
        while (obj == null) yield return null;
        liftUp(obj);
    }

    void trashObject()
    {
        Pot pot = null;
        foreach (Transform child in transform)
        {
            if (IsContain(child.tag, trashableTags))
            {
                PV.RPC("trashObjectPT", RpcTarget.All, child.gameObject.GetComponent<PhotonView>().ViewID);
            }
            else if (child.tag == "Pot") pot = child.GetComponent<Pot>();
        }
        if (pot == null) holdingObject = null;
        else if (pot.content != null)
        {
            pot.trashItem();
        }
    }

    [PunRPC]
    void trashObjectPT(int objectPhotonViewID)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonView objectPhotonView = PhotonNetwork.GetPhotonView(objectPhotonViewID);
        GameObject objectToDelete = objectPhotonView.gameObject;
        PhotonNetwork.Destroy(objectToDelete); // 네트워크 상에서 오브젝트 삭제
    }

    void SetMixingState(bool isMixing)
    {
        animator.SetBool("isMixing", isMixing);
        // 네트워크를 통해 다른 플레이어에게 상태를 알림
        photonView.RPC("RPC_SetMixingState", RpcTarget.Others, isMixing);
    }

    [PunRPC]
    void RPC_SetMixingState(bool isMixing)
    {
        animator.SetBool("isMixing", isMixing);
    }

    void SetMakingState(bool isMaking)
    {
        animator.SetBool("isMaking", isMaking);
        // 네트워크를 통해 다른 플레이어에게 상태를 알림
        photonView.RPC("RPC_SetMakingState", RpcTarget.Others, isMaking);
    }

    [PunRPC]
    void RPC_SetMakingState(bool isMaking)
    {
        animator.SetBool("isMaking", isMaking);
    }

    void SetWalkingSpeed(float speed)
    {
        if (speed >= 0.1f)
        {
            animator.SetFloat("speed", speed);
            photonView.RPC("RPC_SetSpeed", RpcTarget.Others, speed);
        }
        else
        {
            animator.SetFloat("speed", 0);
            photonView.RPC("RPC_SetSpeed", RpcTarget.Others, 0f);
        }
    }

    [PunRPC]
    void RPC_SetSpeed(float speed)
    {
        animator.SetFloat("speed", speed);
    }
    private IEnumerator Dash()
    {
        if (isDashing) yield break; // 이미 대시 중일 경우 실행하지 않음

        isDashing = true;
        canDash = false; // 대시 불가능 상태로 설정
        animator.SetBool("isDashing", true); // 대시 애니메이션 시작
        PV.RPC("RPC_SetDashingState", RpcTarget.Others, true); // 다른 클라이언트에 대시 시작 알림

        Vector3 dashDirection = inputVec.normalized; // 대시 방향 설정
        rb.velocity = dashDirection * dashSpeed; // 대시 속도로 이동

        yield return new WaitForSeconds(dashDuration); // 대시 지속 시간만큼 대기

        rb.velocity = Vector3.zero; // 대시 후 속도 초기화
        animator.SetBool("isDashing", false); // 대시 애니메이션 종료
        PV.RPC("RPC_SetDashingState", RpcTarget.Others, false); // 다른 클라이언트에 대시 끝 알림
        isDashing = false; // 대시 상태 종료

        yield return new WaitForSeconds(dashCooldown); // 쿨타임 동안 대기

        canDash = true; // 대시 가능 상태로 설정
    }

    [PunRPC]
    private void RPC_SetDashingState(bool isDashing)
    {
        animator.SetBool("isDashing", isDashing); // 대시 애니메이션 상태 설정
    }

    void OnCtrlDown()
    {
        if (holdingObject != null) return; // 나중에 던지기 추가
        if (target == null) return;
        if (target.GetComponent<Block>() != null)
        {
            Block blockScript = target.GetComponent<Block>();
            if (blockScript.liftedItems.Count > 0)
            {
                GameObject topItemOnBlock = blockScript.liftedItems.Peek();

                switch (target.tag)
                {
                    case "WorkTable":
                        if (blockScript.liftedItems.Count == 1)
                        {
                            if (topItemOnBlock.tag == "Wood" || topItemOnBlock.tag == "MeltedSteel")
                            {
                                statement = "WORKING";
                                SetMakingState(true);
                            }
                        }
                        break;
                    case "CraftTable":
                        if (recipes.CheckCrafting(blockScript.liftedItems) != "")
                        {
                            statement = "WORKING";
                            SetMakingState(true);
                        }
                        break;
                    case "Mixer":
                        if (topItemOnBlock.tag == "Sand" && blockScript.liftedItems.Count == 1)
                        {
                            statement = "WORKING";
                            SetMixingState(true);
                        }
                        break;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("DeathPanel"))
        {
            die();
        }
    }
    
    public void die()
    {
        RemoveHighlight();
        photonView.RPC("diePT", RpcTarget.All);
    }

    [PunRPC]
    public void diePT()
    {
        statement = "DEAD";

        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Collider>().enabled = false;

        SetRenderersActive(false);
        transform.position = spawnPoint;
        transform.SetParent(null);
        transform.eulerAngles = new Vector3(0, -180, 0);

        if (PhotonNetwork.IsMasterClient) trashObject();
        StartCoroutine(AutoRespawn());
    }

    public void respawn()
    {
        photonView.RPC("respawnPT", RpcTarget.All);
    }

    [PunRPC]
    public void respawnPT()
    {
        if (statement != "DEAD") return;
        statement = "IDLE";

        GetComponent<Rigidbody>().isKinematic = false;
        GetComponent<Collider>().enabled = true;
        SetRenderersActive(true);
    }

    private void SetRenderersActive(bool value)
    {
        // 캐릭터에 있는 모든 Renderer 컴포넌트 비활성화/활성화
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.enabled = value;
        }
        if (PV.IsMine && focusing) CanvasObject.SetActive(value);
    }

    private IEnumerator AutoRespawn()
    {
        TimerCanvas.SetActive(true);
        int timer = 5;
        timerText.text = timer.ToString();

        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer--;
            timerText.text = timer.ToString();
        }

        TimerCanvas.SetActive(false);
        respawn();
    }

    public void toggleCharacter()
    {
        focusing = !focusing;
        if (statement != "DEAD") CanvasObject.SetActive(focusing);
    }
}

