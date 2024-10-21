using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeySetting : MonoBehaviour
{
    public Text SpaceKeyText;
    public Text CtrlKeyText;
    public Text ToggleKeyText;
    public Text DashKeyText;
    public Text ZoomKeyText;

    public Button changeSpaceKeyButton;
    public Button changeCtrlKeyButton;
    public Button changeToggleKeyButton;
    public Button changeDashKeyButton;
    public Button changeZoomKeyButton;

    private KeyCode SpaceKey;
    private KeyCode CtrlKey;
    private KeyCode ToggleKey;
    private KeyCode DashKey;
    private KeyCode ZoomKey;

    private bool isRebinding = false;
    private string currentBinding = "";

    public Button commitButton;
    public Button resetButton;

    void OnEnable()
    {
        LoadKeyBindings();
        // 각 버튼에 리스너 추가
        changeSpaceKeyButton.onClick.AddListener(() => StartRebinding("SpaceKey"));
        changeCtrlKeyButton.onClick.AddListener(() => StartRebinding("CtrlKey"));
        changeToggleKeyButton.onClick.AddListener(() => StartRebinding("ToggleKey"));
        changeDashKeyButton.onClick.AddListener(() => StartRebinding("DashKey"));
        changeZoomKeyButton.onClick.AddListener(() => StartRebinding("ZoomKey"));
        commitButton.onClick.AddListener(() => SaveKeyBindings());
        resetButton.onClick.AddListener(() => resetKey());

        // 각 키에 대한 텍스트 업데이트
        UpdateAllKeyTexts();
    }

    void Update()
    {
        // 키 변경 모드일 때만 키 입력을 받음
        if (isRebinding)
        {
            // 어떤 키라도 눌렸는지 확인
            if (Input.anyKeyDown)
            {
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                {
                    // 눌린 키를 감지
                    if (Input.GetKeyDown(key))
                    {
                        RebindKey(currentBinding, key);  // 현재 바인딩된 키를 변경
                        isRebinding = false;  // 키 변경 모드 종료
                        UpdateAllKeyTexts();
                        break;
                    }
                }
            }
        }
    }

    // 키 변경 모드 시작
    void StartRebinding(string binding)
    {
        isRebinding = true;
        currentBinding = binding;

        // 현재 바꾸려는 키의 텍스트 업데이트
        switch (binding)
        {
            case "SpaceKey":
                SpaceKeyText.text = "Press any key...";
                break;
            case "CtrlKey":
                CtrlKeyText.text = "Press any key...";
                break;
            case "ToggleKey":
                ToggleKeyText.text = "Press any key...";
                break;
            case "DashKey":
                DashKeyText.text = "Press any key...";
                break;
            case "ZoomKey":
                ZoomKeyText.text = "Press any key...";
                break;
        }
    }

    // 각 키 바인딩을 업데이트하는 함수
    void RebindKey(string binding, KeyCode newKey)
    {
        switch (binding)
        {
            case "SpaceKey":
                SpaceKey = newKey;
                break;
            case "CtrlKey":
                CtrlKey = newKey;
                break;
            case "ToggleKey":
                ToggleKey = newKey;
                break;
            case "DashKey":
                DashKey = newKey;
                break;
            case "ZoomKey":
                ZoomKey = newKey;
                break;
        }
    }

    // 모든 키 텍스트 업데이트
    void UpdateAllKeyTexts()
    {
        SpaceKeyText.text = SpaceKey.ToString();
        CtrlKeyText.text = CtrlKey.ToString();
        ToggleKeyText.text = ToggleKey.ToString();
        DashKeyText.text = DashKey.ToString();
        ZoomKeyText.text = ZoomKey.ToString();
    }

    // 키값을 PlayerPrefs에 저장
    void SaveKeyBindings()
    {
        PlayerPrefs.SetInt("SpaceKey", (int)SpaceKey);
        PlayerPrefs.SetInt("CtrlKey", (int)CtrlKey);
        PlayerPrefs.SetInt("ToggleKey", (int)ToggleKey);
        PlayerPrefs.SetInt("DashKey", (int)DashKey);
        PlayerPrefs.SetInt("ZoomKey", (int)ZoomKey);
        PlayerPrefs.Save();
    }

    // 저장된 키값을 불러옴
    void LoadKeyBindings()
    {
        // 저장된 값이 없으면 기본값으로 설정
        SpaceKey = (KeyCode)PlayerPrefs.GetInt("SpaceKey", (int)KeyCode.Space);
        CtrlKey = (KeyCode)PlayerPrefs.GetInt("CtrlKey", (int)KeyCode.LeftControl);
        ToggleKey = (KeyCode)PlayerPrefs.GetInt("ToggleKey", (int)KeyCode.LeftShift);
        DashKey = (KeyCode)PlayerPrefs.GetInt("DashKey", (int)KeyCode.LeftAlt);
        ZoomKey = (KeyCode)PlayerPrefs.GetInt("ZoomKey", (int)KeyCode.Z);
    }

    void resetKey()
    {
        PlayerPrefs.SetInt("SpaceKey", (int)KeyCode.Space);
        PlayerPrefs.SetInt("CtrlKey", (int)KeyCode.LeftControl);
        PlayerPrefs.SetInt("ToggleKey", (int)KeyCode.LeftShift);
        PlayerPrefs.SetInt("DashKey", (int)KeyCode.LeftAlt);
        PlayerPrefs.SetInt("ZoomKey", (int)KeyCode.Z);
        PlayerPrefs.Save();
        LoadKeyBindings();
        UpdateAllKeyTexts();
    }
}
