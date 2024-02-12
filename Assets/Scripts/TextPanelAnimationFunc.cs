using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextPanelAnimationFunc : MonoBehaviour
{
    public PlayTextManager playTextManager;

    public void displayHistory()
    {
        playTextManager.displayHistory();
    }

    public void hideHistory()
    {
        playTextManager.hideHistory();
    }
}
