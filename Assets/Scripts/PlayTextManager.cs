using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayTextManager : MonoBehaviour
{
    public Text mainText;
    public Text historyText;
    public PlayScriptManager playScriptManager;
    public GameObject textPanel;
    public GameObject playTouchBox;
    public GameObject buttonPanel;
    public GameObject settingsButton;
    public GameObject historyBackButton;
    public AudioSource SE_UI;
    public AudioClip uiAudioClip;
    
    private Animator textPanelAnimator;
    private ScrollRect scrollView;
    private Scrollbar scrollBar;
    
    private List<string> textList = new List<string>();
    private IEnumerator displayCoroutine = null;
    private Image skipButtonImage;

    private int loadedChap = 0;
    private int currentTextChap;
    private int currentTextLine;
    private int backLine = 0;

    private float textSpeed;
    private float autoSpeed;
    private bool skipUnread;
    private int unreadLineNum;

    private bool isDisplayDone = false;
    private bool isTextCoroutineDone = false; 
    private bool flushFlag = false;
    private bool autoFlag = false;
    private bool skipFlag = false;
    private bool hideFlag = false;

    void Start()
    {
        textPanelAnimator = textPanel.GetComponent<Animator>();
        scrollView = textPanel.transform.GetChild(0).gameObject.GetComponent<ScrollRect>();
        scrollBar = scrollView.transform.GetChild(1).gameObject.GetComponent<Scrollbar>();

        float value1 = PlayerPrefs.GetFloat(SettingsManager.TEXT_SPEED_KEY, SettingsManager.TEXT_SPEED_DEFAULT);
        textSpeed = SettingsManager.getTextSpeed(value1);
        float value2 = PlayerPrefs.GetFloat(SettingsManager.AUTO_SPEED_KEY, SettingsManager.AUTO_SPEED_DEFAULT);
        autoSpeed = SettingsManager.getAutoSpeed(value2);
        int value3 = PlayerPrefs.GetInt(SettingsManager.SKIP_UNREAD_KEY, SettingsManager.SKIP_UNREAD_DEFAULT);
        skipUnread = SettingsManager.getSkipUnread(value3);

        // チャプターから読み込まれた場合
        if (loadedChap > 0)
        {
            currentTextChap = loadedChap;
            currentTextLine = 0;
        }
        // 「続きから始める」から読み込まれた場合
        else
        {
            currentTextChap = PlayerPrefs.GetInt(PlaySceneManager.CUR_TEXT_CHAP_KEY, PlaySceneManager.CUR_TEXT_CHAP_DEFAULT);
            currentTextLine = PlayerPrefs.GetInt(PlaySceneManager.CUR_TEXT_LINE_KEY, PlaySceneManager.CUR_TEXT_LINE_DEFAULT);
            playScriptManager.loadSaveData();
        }
        
        unreadLineNum = PlayerPrefs.GetInt(PlaySceneManager.UNREAD_LINE_NUM_KEY_ + currentTextChap, PlaySceneManager.UNREAD_LINE_NUM_DEFAULT);

        StartCoroutine(loadTextFile($"textChap{currentTextChap}.txt"));
    }

    private IEnumerator loadTextFile(string address)
    {
        AsyncOperationHandle<TextAsset> handle = Addressables.LoadAssetAsync<TextAsset>(address);

        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            TextAsset result = handle.Result;
            createTextList(result.text);
        }
            
    }

    private async void createTextList(string textdata)
    {
        string readText;
        using (StringReader reader = new StringReader(textdata))
        {
            while ((readText = await reader.ReadLineAsync()) != null)
            {
                textList.Add(readText);
            }
        }

        loadText();
    }

    public void touchText()
    {
        if (hideFlag)
        {
            SE_UI.PlayOneShot(uiAudioClip);
            textPanel.SetActive(true);
            buttonPanel.SetActive(true);
            settingsButton.SetActive(true);
            hideFlag = false;
            startTextCoroutine();
            return;
        }

        if (skipFlag)
        {
            skipButtonDown(skipButtonImage);
            return;
        }

        if (isDisplayDone)
        {
            nextText();
        }
        else
        {
            flushFlag = true;
        }
        
    }

    // 最初の一文を読み込む際に使用し、それ以外はnextText()を使用
    public void loadText() {
        stopTextCoroutine();
        if (currentTextLine < textList.Count)
        {            
            string text = textList[currentTextLine];

            if (text.StartsWith("//"))
            {
                backLine++;
                nextText();
                return;
            }

            if (text.StartsWith("##"))
            {
                // フェード処理の途中などで中断された場合に、処理前のテキスト行まで戻る行数を保存
                backLine++;

                text = text.Replace("##", "");
                string[] arr = text.Split(new char[]{' '}, 2, StringSplitOptions.RemoveEmptyEntries);
                MethodInfo mi = typeof(PlayScriptManager).GetMethod(arr[0]);
                if (arr.Length > 1)
                {
                    mi.Invoke(playScriptManager, arr[1].Split(new char[]{' '}, StringSplitOptions.RemoveEmptyEntries));
                }
                else
                {
                    mi.Invoke(playScriptManager, null);
                }
            }
            else
            {
                if (backLine > 0)
                {
                    backLine = 0;
                    // テキスト行に到達したら現在の背景やBGMを更新
                    playScriptManager.setCurrentItems();
                }
                displayCoroutine = displayText(text);
                if (textPanel.activeInHierarchy)
                {
                    StartCoroutine(displayCoroutine);
                }                    
            }
        }
    }

    public void nextText()
    {   
        // skipUnread == falseの時、スキップ中に未読行まで来たらストップ
        if (!skipUnread && skipFlag && currentTextLine >= unreadLineNum) {
            skipButtonDown(skipButtonImage);
            return;
        }
        currentTextLine++;
        loadText();
    }

    private IEnumerator displayText(string text) 
    {
        isDisplayDone = false;
        isTextCoroutineDone = false;

        for (int i = 0; i < text.Length; i++)
        {
            mainText.text = text.Substring(0, i + 1);
            for (float counter = 0.0f; counter < textSpeed; counter += Time.deltaTime)
            {
                if (skipFlag || flushFlag)
                    break;
                yield return null;
            }
        }
        isDisplayDone = true;
        flushFlag = false;

        // Don't use WaitForSeconds!
        for (float counter = 0.0f; counter < 0.05f + autoSpeed * (15.0f + text.Length); counter += Time.deltaTime)
        {
            if (skipFlag && counter > 0.05f)
                break;
            yield return null;
        }
        isTextCoroutineDone = true;

        if (autoFlag || skipFlag)
            nextText();
    }

    public void autoButtonDown(Image button_image)
    {
        if (autoFlag)
        {
            autoFlag = false;
            button_image.color = new Color(0.0f, 0.0f, 0.0f, 0.3f);
        }
        else
        {
            autoFlag = true;
            button_image.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
        }

        if (isTextCoroutineDone)
            nextText();
    }

    public void skipButtonDown(Image button_image)
    {
        skipButtonImage = button_image;

        if (skipFlag)
        {
            skipFlag = false;
            button_image.color = new Color(0.0f, 0.0f, 0.0f, 0.3f);
            playScriptManager.toggleSkipFlag();
        }
        else
        {
            // skipUnread == falseの時、未読行以上ではスキップ無効
            if (!skipUnread && currentTextLine >= unreadLineNum)
                return;
            
            skipFlag = true;
            button_image.color = new Color(0.0f, 0.0f, 0.0f, 0.5f);
            playScriptManager.toggleSkipFlag();
            if (isTextCoroutineDone)
                nextText();
        }
    }

    public void hideButtonDown()
    {
        textPanel.SetActive(false);
        buttonPanel.SetActive(false);
        settingsButton.SetActive(false);
        hideFlag = true;
        stopTextCoroutine();
    }

    public void historyButtonDown()
    {
        textPanelAnimator.SetBool("expandTextPanel", true);
    }

    public void historyBackButtonDown()
    {
        textPanelAnimator.SetBool("expandTextPanel", false);
    }

    public void displayHistory()
    {
        int index, count;

        stopTextCoroutine();

        count = 0;
        for (int i = 0; count < 20; i++)
        {
            if ((index = currentTextLine - 1 - i) < 0)
                break;
            
            string text = textList[index];
            if (text.StartsWith("##") || text.StartsWith("//"))
                continue;
            
            Text historyTextClone = Instantiate(historyText, mainText.transform.parent) as Text;
            historyTextClone.text = text;
            count++;
        }

        buttonPanel.SetActive(false);
        settingsButton.SetActive(false);
        playTouchBox.SetActive(false);
        historyBackButton.SetActive(true);

        scrollView.vertical = true;
    }

    public void hideHistory()
    {       
        Transform parent = mainText.transform.parent.transform;
        for (int i = 1; i < parent.childCount; i++)
        {
            Destroy(parent.GetChild(i).gameObject);
        }

        scrollBar.value = 0;
        scrollView.vertical = false;

        buttonPanel.SetActive(true);
        settingsButton.SetActive(true);
        playTouchBox.SetActive(true);
        historyBackButton.SetActive(false);

        startTextCoroutine();
    }

    public void startTextCoroutine()
    {
        if (displayCoroutine != null)
            StartCoroutine(displayCoroutine);
    }

    public void stopTextCoroutine()
    {
        if (displayCoroutine != null)
            StopCoroutine(displayCoroutine);
    }

    public void textSpeedSliderChanged(Slider slider)
    {
        textSpeed = SettingsManager.getTextSpeed(slider.value);
    }

    public void autoSpeedSliderChanged(Slider slider)
    {
        autoSpeed = SettingsManager.getAutoSpeed(slider.value);
    }

    public void skipUnreadToggleChanged(Toggle toggle)
    {
        skipUnread = toggle.isOn;
    }

    public void showTextbox(bool active)
    {
        textPanel.SetActive(active);
        buttonPanel.SetActive(active);
        playTouchBox.SetActive(active);
    }

    public int getCurTextChap()
    {
        return currentTextChap;
    }

    public int getCurTextLine()
    {
        // スクリプト行やコメント行の分は戻す
        return currentTextLine - backLine;
    }

    // チャプターパネルからシーンがロードされたときに呼び出される
    public void setLoadedChap(int num)
    {
        loadedChap = num;
    }
}
