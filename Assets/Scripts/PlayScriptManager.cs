using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/*
Script functions
---------------------------------------------------------------------------------------
(function name : parameters)
chapterStart : chapterNumber
showTextbox
hideTextbox
fadeout
fadeoutExtra : fadeTime r g b
fadeoutBackground
fadeoutBackgroundExtra : fadeTime r g b
fadein : imageName
fadeinExtra : imageName fadeTime
fadeinBackground : imageName
fadeinBackgroundExtra : imageName fadeTime
bgmFadeout
bgmFadeoutExtra : fadeTime
bgmFadein : bgmName
bgmFadeinExtra : bgmName fadeTime
playSE : seName
playSEExtra : seName volume
*/

public class PlayScriptManager : MonoBehaviour
{
    public PlayTextManager playTextManager;
    public SpriteRenderer background;
    public Image fadeColorImageComp;
    public AudioSource bgmSource;
    public AudioSource seSource;

    private bool skipFlag = false;
    private IEnumerator fadeBgCoroutine;
    private IEnumerator bgmFadeinCoroutine;
    private bool bgmFadeoutFlag = false;
    
    // フェードアウト中などにゲームを中断する際、その前の状態から再開するために背景やBGMはいったんnextに保存する
    // 次のテキスト行に到達したとき、currentを更新
    private string currentBackgroundName;
    private string currentBgmName;
    private string nextBackgroundName;
    private string nextBgmName;

    private string bgmNameTmp;
    private float bgmFadeTimeTmp;


    public void loadSaveData()
    {
        // 続きから再開するためのセーブデータの読み込み
        float r = PlayerPrefs.GetFloat(PlaySceneManager.FADECOLOR_R_KEY, PlaySceneManager.FADECOLOR_R_DEFAULT);
        float g = PlayerPrefs.GetFloat(PlaySceneManager.FADECOLOR_G_KEY, PlaySceneManager.FADECOLOR_G_DEFAULT);
        float b = PlayerPrefs.GetFloat(PlaySceneManager.FADECOLOR_B_KEY, PlaySceneManager.FADECOLOR_B_DEFAULT);
        fadeColorImageComp.color = new Color(r, g, b, 0.0f);

        currentBackgroundName = PlayerPrefs.GetString(PlaySceneManager.BACKGROUND_KEY, PlaySceneManager.BACKGROUND_DEFAULT);
        currentBgmName = PlayerPrefs.GetString(PlaySceneManager.BGM_KEY, PlaySceneManager.BGM_DEFAULT);
        if (!String.IsNullOrEmpty(currentBackgroundName))
            StartCoroutine(initBackground(currentBackgroundName));
        if (!String.IsNullOrEmpty(currentBgmName))
            StartCoroutine(initBgm(currentBgmName));
    }

    private IEnumerator initBackground(string imageName)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(imageName + "[" + imageName + "]");
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            background.sprite = handle.Result;
        }
    }

    private IEnumerator initBgm(string bgmName)
    {
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(bgmName);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            bgmSource.clip = handle.Result;
            bgmSource.volume = 1.0f;
            bgmSource.Play();
        }
    }

    // PlayTextManagerのスキップフラグ変更時に呼び出し
    public void toggleSkipFlag()
    {
        skipFlag = !skipFlag;
    }

    // PlaySceneManagerにおいて背景をセーブする際に呼び出し
    public string getBackground()
    {
        return currentBackgroundName;
    }

    // PlaySceneManagerにおいてBGMをセーブする際に呼び出し
    public string getBgm()
    {
        return currentBgmName;
    }

    // PlaySceneManagerにおいてフェード色をセーブする際に呼び出し
    public float getFadecolor(char c)
    {
        Color color = fadeColorImageComp.color;
        switch (c)
        {
            case 'r':
                return color.r;
            case 'g':
                return color.g;
            case 'b':
                return color.b;
            default:
                return 0.0f;
        }
    }

    // テキスト行に到達した場合、現在の背景やBGMを更新
    public void setCurrentItems()
    {
        Debug.Log("next");   
        Debug.Log(nextBackgroundName);
        Debug.Log(nextBgmName);   
        Debug.Log("current");  
        Debug.Log(currentBackgroundName);
        Debug.Log(currentBgmName);
        if (nextBackgroundName != null)   
            currentBackgroundName = nextBackgroundName;
        if (nextBgmName != null)
            currentBgmName = nextBgmName;
        nextBackgroundName = null;
        nextBgmName = null;
    }


    // script functions

    // チャプター解除情報を更新
    public void chapterStart(string chapterNumber)
    {
        int chapNum = int.Parse(chapterNumber);
        int unlockedChaps = PlayerPrefs.GetInt(PlaySceneManager.UNLOCKED_CHAP_KEY, PlaySceneManager.UNLOCKED_CHAP_DEFAULT);
        unlockedChaps |= (1 << (chapNum - 1));
        PlayerPrefs.SetInt(PlaySceneManager.UNLOCKED_CHAP_KEY, unlockedChaps);
        playTextManager.nextText();
    }

    public void showTextbox()
    {
        playTextManager.showTextbox(true);
        playTextManager.nextText();
    }

    public void hideTextbox()
    {
        playTextManager.showTextbox(false);
        playTextManager.nextText();
    }


    public void fadeoutBg()
    {
        fadeoutBgExtra("1", "0", "0", "0");
    }

    public void fadeoutBgExtra(string fadeTime, string r, string g, string b)
    {
        playTextManager.showTextbox(false);
        fadeColorImageComp.color = new Color(float.Parse(r), float.Parse(g), float.Parse(b), 0.0f);
        nextBackgroundName = "";
        stopBackgroundCoroutine();
        fadeBgCoroutine = fadeoutBgAsync(float.Parse(fadeTime), true);
        StartCoroutine(fadeBgCoroutine);
    }

    // 背景のみ単独でフェードアウト
    public void fadeoutBgOnly()
    {
        fadeoutBgOnlyExtra("1", "0", "0", "0");
    }

    public void fadeoutBgOnlyExtra(string fadeTime, string r, string g, string b)
    {
        fadeColorImageComp.color = new Color(float.Parse(r), float.Parse(g), float.Parse(b), 0.0f);
        nextBackgroundName = "";
        stopBackgroundCoroutine();
        fadeBgCoroutine = fadeoutBgAsync(float.Parse(fadeTime), false);
        StartCoroutine(fadeBgCoroutine);

        playTextManager.nextText();
    }

    private IEnumerator fadeoutBgAsync(float fadeTime, bool nextText)
    {
        while (fadeColorImageComp.color.a < 1.0f)
        {
            float aFadeTime = skipFlag ? 0.1f : fadeTime;
            float alpha = fadeColorImageComp.color.a + (Time.deltaTime / aFadeTime);
            
            Color color = fadeColorImageComp.color;
            color.a = alpha > 1.0f ? 1.0f : alpha;
            fadeColorImageComp.color = color;

            yield return null;
        }

        background.sprite = null;
        
        if (nextText)
            playTextManager.nextText();
    }


    public void fadeinBg(string imageName)
    {
        fadeinBgExtra(imageName, "1");
    }

    public void fadeinBgExtra(string imageName, string fadeTime)
    {
        playTextManager.showTextbox(false);
        Color color = fadeColorImageComp.color;
        fadeColorImageComp.color = new Color(color.r, color.g, color.b, 1.0f);
        nextBackgroundName = imageName;
        stopBackgroundCoroutine();
        fadeBgCoroutine = fadeinBgAsync(imageName, float.Parse(fadeTime), true);
        StartCoroutine(fadeBgCoroutine);
    }

    // 背景のみ単独でフェードイン
    public void fadeinBgOnly(string imageName)
    {
        fadeinBgOnlyExtra(imageName, "1");
    }

    public void fadeinBgOnlyExtra(string imageName, string fadeTime)
    {
        Color color = fadeColorImageComp.color;
        fadeColorImageComp.color = new Color(color.r, color.g, color.b, 1.0f);
        nextBackgroundName = imageName;
        stopBackgroundCoroutine();
        fadeBgCoroutine = fadeinBgAsync(imageName, float.Parse(fadeTime), false);
        StartCoroutine(fadeBgCoroutine);

        playTextManager.nextText();
    }

    private IEnumerator fadeinBgAsync(string imageName, float fadeTime, bool nextText)
    {
        AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(imageName + "[" + imageName + "]");
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            background.sprite = handle.Result;
        }

        while (fadeColorImageComp.color.a > 0.0f)
        {
            float aFadeTime = skipFlag ? 0.1f : fadeTime;
            float alpha = fadeColorImageComp.color.a - (Time.deltaTime / aFadeTime);

            Color color = fadeColorImageComp.color;
            color.a = alpha < 0.0f ? 0.0f : alpha;
            fadeColorImageComp.color = color;

            yield return null;
        }

        playTextManager.showTextbox(true);

        if (nextText)
            playTextManager.nextText();
    }

    // 設定パネルの開閉によるフェード処理の中断・再開
    public void stopBackgroundCoroutine()
    {
        if (fadeBgCoroutine != null)
            StopCoroutine(fadeBgCoroutine);
    }

    public void startBackgroundCoroutine()
    {
        if (fadeBgCoroutine != null)
            StartCoroutine(fadeBgCoroutine);
    }


    public void bgmFadeout()
    {
        bgmFadeoutExtra("1");
    }

    public void bgmFadeoutExtra(string fadeTime)
    {
        nextBgmName = "";
        bgmNameTmp = null;

        // フェードアウト中に呼び出された場合、処理なし
        if (!bgmFadeoutFlag) 
        {
            // フェードイン中に呼び出された場合、フェードインを中止
            if (bgmFadeinCoroutine != null)
                StopCoroutine(bgmFadeinCoroutine);
            StartCoroutine(bgmFadeoutBgAsync(float.Parse(fadeTime)));
        }

        playTextManager.nextText();
    }

    private IEnumerator bgmFadeoutBgAsync(float fadeTime)
    {
        bgmFadeoutFlag = true;
        while (bgmSource.volume > 0.0f)
        {
            float vol = bgmSource.volume - (Time.deltaTime / fadeTime);
            bgmSource.volume = vol < 0.0f ? 0.0f : vol;
            yield return null;
        }
        bgmFadeoutFlag = false;
        bgmSource.Pause();

        // フェードアウト中にフェードインが呼び出されていた場合に実行
        if (bgmNameTmp != null)
        {
            bgmFadeinCoroutine = bgmFadeinBgAsync(bgmNameTmp, bgmFadeTimeTmp);
            StartCoroutine(bgmFadeinCoroutine);
        }
    }


    public void bgmFadein(string bgmName)
    {
        bgmFadeinExtra(bgmName, "1");
    }

    public void bgmFadeinExtra(string bgmName, string fadeTime)
    {
        // フェードアウト中に呼び出された場合、bgmNameTmpの設定以外に処理なし
        nextBgmName = bgmName;
        bgmNameTmp = bgmName;
        bgmFadeTimeTmp = float.Parse(fadeTime);
        if (!bgmFadeoutFlag)
        {
            // フェードイン中に呼び出された場合、フェードインを中止
            if (bgmFadeinCoroutine != null)
                StopCoroutine(bgmFadeinCoroutine);
            bgmFadeinCoroutine = bgmFadeinBgAsync(bgmName, float.Parse(fadeTime));
            StartCoroutine(bgmFadeinCoroutine);
        }
        
        playTextManager.nextText();
    }

    private IEnumerator bgmFadeinBgAsync(string bgmName, float fadeTime)
    {
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(bgmName);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            bgmSource.clip = handle.Result;
            bgmSource.Play();

            while (bgmSource.volume < 1.0f)
            {
                float vol = bgmSource.volume + (Time.deltaTime / fadeTime);
                bgmSource.volume = vol > 1.0f ? 1.0f : vol;
                yield return null;
            }
        }
    }


    public void playSE(string seName)
    {
        playSEExtra(seName, "1");
    }

    public void playSEExtra(string seName, string volume)
    {
        if (!seSource.isPlaying)
            StartCoroutine(loadSE(seName, float.Parse(volume)));
        playTextManager.nextText();
    }

    private IEnumerator loadSE(string seName, float volume)
    {
        AsyncOperationHandle<AudioClip> handle = Addressables.LoadAssetAsync<AudioClip>(seName);
        yield return handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            seSource.PlayOneShot(handle.Result, volume);
        }
    }
}
