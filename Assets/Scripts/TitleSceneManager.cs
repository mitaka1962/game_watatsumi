using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    public Text titleText;
    public Transform chapterWrapper;

    private Button chapterButton;
    private int chapNum;

    void Start()
    {
        if (PlayerPrefs.HasKey(PlaySceneManager.CUR_TEXT_LINE_KEY))
        {
            titleText.text = "続きから始める";
        }
        else
        {
            titleText.text = "ゲームを始める";
        }

        // チャプター解除情報（第n章解除済みの場合、右からn桁目が1の二進数）の読み込み
        int unlockedChaps = PlayerPrefs.GetInt(PlaySceneManager.UNLOCKED_CHAP_KEY, PlaySceneManager.UNLOCKED_CHAP_DEFAULT);
        for (int i = 0; i < chapterWrapper.childCount; i++)
        {
            Button chapterButton = chapterWrapper.GetChild(i).GetComponent<Button>();
            chapterButton.interactable = (unlockedChaps >> i & 1) == 1;
        }       
    }

    public void loadChapter(int num)
    {
        chapNum = num;
        SceneManager.sceneLoaded += chapterLoaded;
        SceneManager.LoadScene("Play");
    }

    private void chapterLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayTextManager manager = GameObject.FindWithTag("PlayTextManager").GetComponent<PlayTextManager>();
        manager.setLoadedChap(chapNum);
        SceneManager.sceneLoaded -= chapterLoaded;
    }

    public void resetSaveData() 
    {
        PlayerPrefs.DeleteAll();
    }
}
