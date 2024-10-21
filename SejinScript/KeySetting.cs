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
        // �� ��ư�� ������ �߰�
        changeSpaceKeyButton.onClick.AddListener(() => StartRebinding("SpaceKey"));
        changeCtrlKeyButton.onClick.AddListener(() => StartRebinding("CtrlKey"));
        changeToggleKeyButton.onClick.AddListener(() => StartRebinding("ToggleKey"));
        changeDashKeyButton.onClick.AddListener(() => StartRebinding("DashKey"));
        changeZoomKeyButton.onClick.AddListener(() => StartRebinding("ZoomKey"));
        commitButton.onClick.AddListener(() => SaveKeyBindings());
        resetButton.onClick.AddListener(() => resetKey());

        // �� Ű�� ���� �ؽ�Ʈ ������Ʈ
        UpdateAllKeyTexts();
    }

    void Update()
    {
        // Ű ���� ����� ���� Ű �Է��� ����
        if (isRebinding)
        {
            // � Ű�� ���ȴ��� Ȯ��
            if (Input.anyKeyDown)
            {
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                {
                    // ���� Ű�� ����
                    if (Input.GetKeyDown(key))
                    {
                        RebindKey(currentBinding, key);  // ���� ���ε��� Ű�� ����
                        isRebinding = false;  // Ű ���� ��� ����
                        UpdateAllKeyTexts();
                        break;
                    }
                }
            }
        }
    }

    // Ű ���� ��� ����
    void StartRebinding(string binding)
    {
        isRebinding = true;
        currentBinding = binding;

        // ���� �ٲٷ��� Ű�� �ؽ�Ʈ ������Ʈ
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

    // �� Ű ���ε��� ������Ʈ�ϴ� �Լ�
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

    // ��� Ű �ؽ�Ʈ ������Ʈ
    void UpdateAllKeyTexts()
    {
        SpaceKeyText.text = SpaceKey.ToString();
        CtrlKeyText.text = CtrlKey.ToString();
        ToggleKeyText.text = ToggleKey.ToString();
        DashKeyText.text = DashKey.ToString();
        ZoomKeyText.text = ZoomKey.ToString();
    }

    // Ű���� PlayerPrefs�� ����
    void SaveKeyBindings()
    {
        PlayerPrefs.SetInt("SpaceKey", (int)SpaceKey);
        PlayerPrefs.SetInt("CtrlKey", (int)CtrlKey);
        PlayerPrefs.SetInt("ToggleKey", (int)ToggleKey);
        PlayerPrefs.SetInt("DashKey", (int)DashKey);
        PlayerPrefs.SetInt("ZoomKey", (int)ZoomKey);
        PlayerPrefs.Save();
    }

    // ����� Ű���� �ҷ���
    void LoadKeyBindings()
    {
        // ����� ���� ������ �⺻������ ����
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
