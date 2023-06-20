
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
using SonicBloom.Koreo;
using UnityEngine;

public class NoteObject : MonoBehaviour
{

    public SpriteRenderer visuals;

    public Sprite[] noteSprites;

    private KoreographyEvent trackedEvent;

    public bool isLongNote;

    public bool isLoneNoteEnd;

    private LineController _lineController;

    private RhyGameController _rhyGameController;

    // 判断击打的分数
    public int hitOffset;
    
    
    /// <summary>
    /// 初始化音符对象
    /// </summary>
    /// <param name="evt"></param>
    /// <param name="noteValue"></param>
    /// <param name="lineController"></param>
    /// <param name="rhyGameController"></param>
    /// <param name="isLongStart"></param>
    /// <param name="isLongEnd"></param>
    
    public void Initialized(KoreographyEvent evt, int noteValue, LineController lineController, 
        RhyGameController rhyGameController, bool isLongStart, bool isLongEnd)
    {
        trackedEvent = evt;
        _lineController = lineController;
        _rhyGameController = rhyGameController;
        isLongNote= isLongStart;
        isLoneNoteEnd = isLongEnd;
        int spriteNum = noteValue;
        if (isLongNote)
        {
            spriteNum += 6;
        } else if (isLoneNoteEnd)
        {
            spriteNum += 12;
        }

        visuals.sprite = noteSprites[spriteNum - 1];
        
    }

    /// <summary>
    /// 重置对象
    /// </summary>
    private void ResetNote()
    {
        trackedEvent = null;
        _lineController = null;
        _rhyGameController = null;
    }

    /// <summary>
    /// 将对象放回对象池
    /// </summary>
    private void ReturnToPool()
    {
        _rhyGameController.ReturnNoteObjectToPool(this);
        ResetNote();
    }

    /// <summary>
    /// 击中音符对象
    /// </summary>
    public void OnHit()
    {
        ReturnToPool();
    }

    /// <summary>
    /// 更新音符的位置
    /// </summary>
    private void UpdateNotePosition()
    {
        Vector3 pos = _lineController.keyPosition;
        pos.z -= _rhyGameController.noteSpeed * ((_rhyGameController.DelayedSampleTime - trackedEvent.StartSample) / (float)_rhyGameController.SampleRate);
        transform.position = pos;
    }

    private void Update()
    {
        if (_rhyGameController.isPauseState) return;
        UpdateNotePosition();
        GetHitOffset();

        if (transform.position.z <= _lineController.keyBottomTrans.position.z)
        {
            _rhyGameController.ReturnNoteObjectToPool(this);
            ResetNote();
        }
    }

    /// <summary>
    /// 获取击中的窗口的距离，用来判断击中等级
    /// </summary>
    private void GetHitOffset()
    {
        // 当前播放音乐的时间
        int currentTime = _rhyGameController.DelayedSampleTime;
        // 当前事件时间，也就是产生音符的时间.样本点时间
        int noteTime = trackedEvent.StartSample;
        // 命中窗口,玩家可以自己设置
        int hitWindow = _rhyGameController._hitWindowRangeInSamples;
        hitOffset = hitWindow - Mathf.Abs(noteTime - currentTime);
    }
    

    /// <summary>
    /// 判断音符是否处于missed转状态
    /// </summary>
    /// <returns></returns>
    public bool IsNoteMissed()
    {
        bool isMissed = true;
        // 处于激活状态
        if (enabled)
        {
            int currentTime = _rhyGameController.DelayedSampleTime;
            int noteTime = trackedEvent.StartSample;
            int hitWindow = _rhyGameController._hitWindowRangeInSamples;
            isMissed = (currentTime - noteTime) > hitWindow;
        }

        return isMissed;
    }

    /// <summary>
    /// 判定音符击中的等级，great,miss,perfect
    /// </summary>
    /// <returns></returns>
    public int IsNoteHittable()
    {
        int hitLevel = 0;
        if (hitOffset >= 0) 
        {
            if (hitOffset >= 4500 && hitOffset <= 7500)
            {
                // perfect等级
                hitLevel = 2;
            }
            else
            {
                // great等级
                hitLevel = 1;
            }
        }
        else
        {
            // miss或者未点击
            this.enabled = false;
        }
        return hitLevel;
    }

}
