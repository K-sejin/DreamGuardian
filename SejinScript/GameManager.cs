using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;
    public float gameTime;
    public int killCnt;
    public Vector3[] spawnPoints;
    private PhotonView pv;
    public Text gameTimeText;
    public Text gameTimeTextShadow;
    public Text StageText;
    public Text StageTextShadow;
    private string nowStage;
    private string nowLevel;
    private int playerNum;
    public GameObject ChatPanel;
    public Text[] ChatText;
    public InputField ChatInput;
    public bool isChatFocused;
    public bool isStoryEnd;
    [Header("EscPanel")]
    public GameObject EscPanel;
    public Button LobbyBtn;
    public Button RoomBtn;
    public GameObject SoundSetting;
    public GameObject KeySetting;
    [Header("KeySettingText")]
    public Text SpaceKeyText;
    public Text CtrlKeyText;
    public Text ToggleKeyText;
    public Text DashKeyText;
    public Text ZoomKeyText;

    private string TempStage;
    private string InputStage;
    public int bossCount = 1;
    public int staticBossCount = 1;

    void Awake()
    {
        isStoryEnd = false;
        instance = this;
        pv = GetComponent<PhotonView>();
        gameTime = 0f;
        killCnt = 0;

        nowStage = PlayerPrefs.GetString("checkedStage");
        nowLevel = PlayerPrefs.GetString("checkedLevel");
        playerNum = PhotonNetwork.CurrentRoom.PlayerCount;
        isChatFocused = false;

        // 저장되어 있는 KeyCode 가져오기
        KeyCode TempSpaceKey = (KeyCode)PlayerPrefs.GetInt("SpaceKey", (int)KeyCode.Space);
        SpaceKeyText.text = "물건 집기 : " + TempSpaceKey.ToString() + " Key";
        KeyCode TempCtrlKey = (KeyCode)PlayerPrefs.GetInt("CtrlKey", (int)KeyCode.LeftControl);
        CtrlKeyText.text = "작업/상호작용 : " + TempCtrlKey.ToString() + " Key";
        KeyCode TempToggleKey = (KeyCode)PlayerPrefs.GetInt("ToggleKey", (int)KeyCode.LeftShift);
        ToggleKeyText.text = "캐릭터 전환(1인) : " + TempToggleKey.ToString() + " Key";
        KeyCode TempDashKey = (KeyCode)PlayerPrefs.GetInt("DashKey", (int)KeyCode.LeftAlt);
        DashKeyText.text = "빠른 이동 : " + TempDashKey.ToString() + " Key";
        KeyCode TempZooomKey = (KeyCode)PlayerPrefs.GetInt("ZoomKey", (int)KeyCode.Z);
        ZoomKeyText.text = "카메라 전환 : " + TempZooomKey.ToString() + " Key";

        TempStage = PlayerPrefs.GetString("checkedStage");

        if (TempStage.Contains("Final"))
        {
            if (TempStage == "Final Stage 1")
            {
                InputStage = "Final 1";
            }
            else if (TempStage == "Final Stage 2")
            {
                InputStage = "Final 2";
            }
        }
        else if (PlayerPrefs.GetString("isTutorial") == "0")
        {
            InputStage = TempStage.Substring(5);
        }

        if (PlayerPrefs.GetString("isTutorial") == "1")
        {
            StageText.text = "Tutorial";
            StageTextShadow.text = "Tutorial";
        }
        else
        {
            // 현재 스테이지, 난이도 가져오기
            StageText.text = InputStage + " < " + PlayerPrefs.GetString("checkedLevel") + " >";
            StageTextShadow.text = InputStage + " < " + PlayerPrefs.GetString("checkedLevel") + " >";
        }
        staticBossCount = bossCount;
    }

    void Start()
    {
        spawn();

        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (PlayerPrefs.GetString("isTutorial") == "1")
                {
                    LobbyBtn.gameObject.SetActive(true);
                    RoomBtn.gameObject.SetActive(false);
                }
                else
                {
                    LobbyBtn.gameObject.SetActive(true);
                    RoomBtn.gameObject.SetActive(false);
                    // LobbyBtn.gameObject.SetActive(false);
                    // RoomBtn.gameObject.SetActive(true);
                }
            }
            else
            {
                LobbyBtn.gameObject.SetActive(true);
                RoomBtn.gameObject.SetActive(false);
            }
        }
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++) ChatText[i].text = "";
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                LobbyBtn.gameObject.SetActive(true);
                RoomBtn.gameObject.SetActive(false);
                // LobbyBtn.gameObject.SetActive(false);
                // RoomBtn.gameObject.SetActive(true);
            }
            else
            {
                LobbyBtn.gameObject.SetActive(true);
                RoomBtn.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        // ESC 키 입력 감지
        if (isStoryEnd && Input.GetKeyDown(KeyCode.Escape))
        {
            if (isChatFocused) // 채팅 입력하다가 esc 입력 시 채팅 내용 초기화 및 포커스 해제
            {
                ChatInput.DeactivateInputField();
                ChatInput.text = "";
                isChatFocused = false;
                return;
            }
            if (EscPanel.activeSelf)
            {
                if (SoundSetting.activeSelf)
                {
                    SoundSetting.SetActive(false);
                }
                else if (KeySetting.activeSelf)
                {
                    KeySetting.SetActive(false);
                }
                else
                {
                    EscPanel.SetActive(false);
                }
            }
            else
            {
                EscPanel.SetActive(true);
            }
        }
        // 엔터 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!isChatFocused)
            {
                // 채팅 입력란에 포커스
                ChatInput.ActivateInputField();
                isChatFocused = true;
            }
            else
            {
                // 채팅 메시지 전송
                Send();
                isChatFocused = false;
            }
        }

    }

    void FixedUpdate()
    {
        if (!isChatFocused)
        {
            ChatInput.DeactivateInputField();
        }
        gameTime += Time.fixedDeltaTime;
        gameTimeText.text = GetFormattedGameTime();
        gameTimeTextShadow.text = GetFormattedGameTime();
    }

    public string GetFormattedGameTime()
    {
        int minutes = Mathf.FloorToInt(gameTime / 60f);
        int seconds = Mathf.FloorToInt(gameTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    string[] Characters = { "Character", "Character2", "Character3", "Character4" };
    public void spawn()
    {
        Player[] players = PhotonNetwork.PlayerList;
        int playerNumber = System.Array.IndexOf(players, PhotonNetwork.LocalPlayer);

        Vector3 spawnPoint = spawnPoints[playerNumber];
        GameObject character = PhotonNetwork.Instantiate(Characters[playerNumber], spawnPoint, Quaternion.identity);
        character.GetComponentInChildren<StoryManager>().StoryStart();

        if (players.Length == 1 && PlayerPrefs.GetString("isTutorial") == "0")
        {
            Vector3 newSpawnPoint = spawnPoints[playerNumber + 1];
            GameObject dummy = PhotonNetwork.Instantiate(Characters[playerNumber + 1], newSpawnPoint, Quaternion.identity);
            Character dummyCharacter = dummy.GetComponent<Character>();
            dummyCharacter.toggleCharacter();
            dummyCharacter.spawnPoint = spawnPoints[playerNumber + 1];
            dummyCharacter.statement = "IDLE";
            //dummyCharacter
        }
    }

    public void gameClear()
    {
        PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerPrefs.SetString("isCleared", "<"+ nowLevel + "> " + nowStage + " 클리어! " + " / " + playerNum + "인");
            PlayerPrefs.SetString("killCnt", killCnt.ToString() + " 마리");
            PlayerPrefs.SetString("gameTime", GetFormattedGameTime());
            PlayerPrefs.SetString("baseHealth", (Math.Round(100d * Base.instance.currentHealth / Base.instance.maxHealth, 1).ToString() + " %"));
            PhotonNetwork.LoadLevel("ResultScene");
        }
    }

    public void gameOver()
    {
        PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
        if (PhotonNetwork.IsMasterClient)
        {
            PlayerPrefs.SetString("isCleared", "<" + nowLevel + "> " + nowStage + " 실패 " + " / " + playerNum + "인");
            PlayerPrefs.SetString("killCnt", killCnt.ToString() + " 마리");
            PlayerPrefs.SetString("gameTime", GetFormattedGameTime());
            PlayerPrefs.SetString("baseHealth", "0 %");
            PhotonNetwork.LoadLevel("ResultScene");
        }
    }

    public void ToRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
            PhotonNetwork.DestroyAll();
            PhotonNetwork.LoadLevel("Lobby2");
        }
    }

    public void ToLobbyScene()
    {
        PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        PhotonNetwork.LoadLevel("Lobby2");
    }

    public void quitGame()
    {
        PhotonNetwork.OpCleanActorRpcBuffer(PhotonNetwork.LocalPlayer.ActorNumber);
        Application.Quit();
    }

    public void ShowSoundSetting()
    {
        SoundSetting.SetActive(true);
    }

    public void ShowKeySetting()
    {
        KeySetting.SetActive(true);
    }

    public void CloseKeySetting()
    {
        KeySetting.SetActive(false);
    }

    public void Send()
    {
        ChatInput.DeactivateInputField();
        if (string.IsNullOrWhiteSpace(ChatInput.text))
        {
            return; // 빈 메시지이거나 공백만 있는 경우 전송하지 않음
        }
        pv.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";

        // 채팅 입력 후 포커스 해제
        isChatFocused = false;
    }

    [PunRPC] // RPC는 플레이어가 속해있는 방 모든 인원에게 전달한다
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput) // 꽉차면 한칸씩 위로 올림
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
}