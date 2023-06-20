
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

public class DestoryEffect : MonoBehaviour
{
    // 销毁预制体对象
    public RhyGameController rhyGameController;

    // 是否被击中
    public bool isHitted;
    
    // 动画播放时长
    public float animationTime;
    
    private void OnEnable()
    {
        // 动画播放完之后调用
        Invoke("ReturnToPool", animationTime);
    }

    /// <summary>
    /// 将特效放回对象池
    /// </summary>
    private void ReturnToPool()
    {
        if (isHitted)
        {
            rhyGameController.ReturnEffectGoToPool(rhyGameController._hitEffectObjectPool, gameObject);
            gameObject.SetActive(false);
        }
        else
        {
            rhyGameController.ReturnEffectGoToPool(rhyGameController._downEffectObjectPool, gameObject);
            gameObject.SetActive(false);
        }
    }
}
