
/**
 *Copyright(C) 2015 by #COMPANY#
 *All rights reserved.
 *ProductName:  #PRODUCTNAME#
 *FileName:     #SCRIPTFULLNAME#
 *Author:       #AUTHOR#
 *Version:      #VERSION#
 *UnityVersion：#UNITYVERSION#
 *CreateTime:   #CreateTime#
 *Description:   
 *History:
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PauseButton : MonoBehaviour
{
    public Sprite[] sprites;

    public RhyGameController rhyGameController;
    
    private Button button;

    private Image image;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PauseOrPlayMusic);
        image = GetComponent<Image>();
    }

    /// <summary>
    /// 暂停或播放音乐
    /// </summary>
    private void PauseOrPlayMusic()
    {
        rhyGameController.isPauseState = !rhyGameController.isPauseState;
        if (rhyGameController.isPauseState)
        {
            image.sprite = sprites[1];
            rhyGameController.PauseMusic();
            return;
        }
        image.sprite = sprites[0];
        rhyGameController.PlayMusic();
    }
}
