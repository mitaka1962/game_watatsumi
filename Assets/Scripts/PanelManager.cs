using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject initiallyOpen;
    private GameObject m_Open;

    void Start()
    {
        if (initiallyOpen == null)
            return;

        OpenPanel(initiallyOpen);
    }

    public void OpenPanel(GameObject panel)
    {
        if (m_Open == panel)
            return;

        if (m_Open != null)
            m_Open.SetActive(false);
        
        panel.SetActive(true);
        m_Open = panel;
    }
}
