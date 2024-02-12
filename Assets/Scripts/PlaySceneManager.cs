using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySceneManager : MonoBehaviour
{
    // クイックセーブ用キー
    public static readonly string BACKGROUND_KEY = "savedBackground";
    public static readonly string FADECOLOR_R_KEY = "savedFadecolorR";
    public static readonly string FADECOLOR_G_KEY = "savedFadecolorG";
    public static readonly string FADECOLOR_B_KEY = "savedFadecolorB";
    public static readonly string BGM_KEY = "savedBgm";
    public static readonly string CUR_TEXT_CHAP_KEY = "savedCurTextChap";
    public static readonly string CUR_TEXT_LINE_KEY = "savedCurTextLine";

    public static readonly string BACKGROUND_DEFAULT = "";
    public static readonly float FADECOLOR_R_DEFAULT = 0.0f;
    public static readonly float FADECOLOR_G_DEFAULT = 0.0f;
    public static readonly float FADECOLOR_B_DEFAULT = 0.0f;
    public static readonly string BGM_DEFAULT = "";
    public static readonly int CUR_TEXT_CHAP_DEFAULT = 1;
    public static readonly int CUR_TEXT_LINE_DEFAULT = 0;

    // ゲーム進度の保存用キー
    public static readonly string UNLOCKED_CHAP_KEY = "unlockedChapters";
    public static readonly string UNREAD_LINE_NUM_KEY_ = "unreadLineNumber";

    public static readonly int UNLOCKED_CHAP_DEFAULT = 0;
    public static readonly int UNREAD_LINE_NUM_DEFAULT = 0;
    
    public PlayTextManager playTextManager;
    public PlayScriptManager playScriptManager;

    void OnDisable() {
        // 現在の章とテキスト行、背景等をクイックセーブ（「続きから」で再開）
        int chap = playTextManager.getCurTextChap();
        int line = playTextManager.getCurTextLine();
        PlayerPrefs.SetInt(CUR_TEXT_CHAP_KEY, chap);
        PlayerPrefs.SetInt(CUR_TEXT_LINE_KEY, line);
        PlayerPrefs.SetFloat(FADECOLOR_R_KEY, playScriptManager.getFadecolor('r'));
        PlayerPrefs.SetFloat(FADECOLOR_G_KEY, playScriptManager.getFadecolor('g'));
        PlayerPrefs.SetFloat(FADECOLOR_B_KEY, playScriptManager.getFadecolor('b'));
        PlayerPrefs.SetString(BACKGROUND_KEY, playScriptManager.getBackground());
        PlayerPrefs.SetString(BGM_KEY, playScriptManager.getBgm());

        // 未読行の更新
        if (line > PlayerPrefs.GetInt(UNREAD_LINE_NUM_KEY_ + chap, UNREAD_LINE_NUM_DEFAULT))
            PlayerPrefs.SetInt(UNREAD_LINE_NUM_KEY_ + chap, line);
    }
}
