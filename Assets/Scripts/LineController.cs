
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
using SonicBloom.Koreo;
using Unity.VisualScripting;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LineController : MonoBehaviour
{
    protected RhyGameController _gameController;

    // 音轨ID
    [Tooltip("音轨对应事件的编号")] public int laneID;
    
    // 包含在此音轨中的所有事件列表
    private List<KoreographyEvent> laneEvents = new List<KoreographyEvent>();
    
    // 定义音轨的键盘按键
    [Tooltip("这是我们此音轨使用的键盘按键")]
    public KeyCode keyboardButton;

    private Button button;
    
    // 取得按键特效的位置
    [Tooltip("对目标位置的键盘按下的视觉效果")]
    public Transform keyVisuals;
    
    // 上下边界
    public Transform keyTopTrans;
    public Transform keyBottomTrans;
    
    // 音符对象的队列,包含此音轨当前活动的所有音符对象的队列
    protected Queue<NoteObject> trackedNotes = new Queue<NoteObject>();
    
    // 检测此音轨中生成的下一个事件的索引
    private int pendingEventNextIndex = 0;
    
    // 音符移动的目标位置
    public Vector3 keyPosition;

    // 按下按键的特效
    public GameObject downVisual;
    
    // 是否由长音符
    public bool hasLongNote;

    // 按住长音符的时长
    public float timeValue = 0;
    
    // 长音符特效
    public GameObject longNoteEffectGo;

    private string btnName="";

    private void Awake()
    {
        keyPosition = transform.position;
        button = GetComponent<Button>();
    }
    

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="rhyGameController"></param>
    public void Initialized(RhyGameController rhyGameController)
    {
        _gameController = rhyGameController;
    }

    /// <summary>
    /// 检测事件是否匹配当前编号的音轨
    /// </summary>
    /// <param name="noteID"></param>
    /// <returns></returns>
    public bool DoesMatch(int noteID)
    {
        return noteID == laneID;
    }

    /// <summary>
    /// 如果匹配，则吧当前事件添加进音轨所持有的事件列表
    /// </summary>
    /// <param name="evt"></param>
    public void AddEventToLane(KoreographyEvent evt)
    {
        laneEvents.Add(evt);
    }
    
    /// <summary>
    /// 音符在印谱上产生的位置,获取样本产生位置的偏移量
    /// </summary>
    /// <returns></returns>
    private int GetSpawnSampleOffset()
    {
        // 出生位置与目标点的位置
        float spawnDisToTarget = keyTopTrans.position.z - transform.position.z;
        
        // 到达目标点的事件
        float spawnPosToTargetTime = spawnDisToTarget / _gameController.noteSpeed;

        return (int)spawnPosToTargetTime * _gameController.SampleRate;
    }

    /// <summary>
    /// 检测音符事件,并生成音符对象
    /// </summary>
    protected void CheckSpawnNext()
    {
        int sampleToKey = GetSpawnSampleOffset();
        // 当前音乐播放到的时间
        int currentTime = _gameController.DelayedSampleTime;

        while (pendingEventNextIndex < laneEvents.Count 
               && laneEvents[pendingEventNextIndex].StartSample < currentTime + sampleToKey)
        {
            KoreographyEvent evt = laneEvents[pendingEventNextIndex];
            int noteValue = evt.GetIntValue();
            NoteObject nobj = _gameController.GetFreshNoteObject();
            bool isLongNoteStart = false;
            bool isLoneNoteEnd = false;
            if (noteValue > 6)
            {
                isLongNoteStart = true;
                noteValue -= 6;
                if (noteValue > 6)
                {
                    isLoneNoteEnd = true;
                    isLongNoteStart = false;
                    noteValue -= 6;
                }
            }
            nobj.Initialized(evt, noteValue, this, _gameController, isLongNoteStart, isLoneNoteEnd);
            trackedNotes.Enqueue(nobj);
            pendingEventNextIndex++;
        }
    }

    private void Update()
    {
        if (_gameController.isPauseState) return;
        // 清除无效音符
        while (trackedNotes.Count > 0 && trackedNotes.Peek().IsNoteMissed())
        {
            if (trackedNotes.Peek().isLoneNoteEnd)
            {
                // 如果是长音符的结尾
                hasLongNote = false;
                timeValue = 0;
                downVisual.SetActive(false);
                longNoteEffectGo.SetActive(false);
            }
            _gameController.ChangeHitLevelSprite(0);
            _gameController.comboNum = 0;
            _gameController.HideComboText();
            _gameController.UpdateHP(_gameController.desHp);
            // 如果是miss状态，需要出队
            trackedNotes.Dequeue();
        }
        
        // 检测新音符的产生
        CheckSpawnNext();
        
      
        // Debug.Log("gameObject" + gameObject.name);
    
        // // 检测玩家的输入}
        // if (Input.GetKeyDown(keyboardButton))
        // {
        //     CheckNoteHit();
        //     downVisual.SetActive(true);
        // } else if (Input.GetKey(keyboardButton))
        // {
        //     // 一直按下，检测长音符
        //     if (hasLongNote)
        //     {
        //         // 计时器累加
        //         if (timeValue >= 0.15f)
        //         {
        //             // 激活特效
        //             if (!longNoteEffectGo.activeSelf)
        //             {
        //                 // 显示命中等级(Great，Perfect)
        //                 _gameController.ChangeHitLevelSprite(2);
        //                 CreateHitLongEffect();
        //             }
        //             timeValue = 0;
        //         }
        //         else
        //         {
        //             timeValue += Time.deltaTime;
        //         }
        //     }
        // } else if (Input.GetKeyUp(keyboardButton))
        // {
        //     // 需要将特效关闭
        //     downVisual.SetActive(false);
        //     // 检测长音符
        //     if (hasLongNote)
        //     {
        //         longNoteEffectGo.SetActive(false);
        //         CheckNoteHit();
        //     }
        // }
        // GetTouch();
        // 检测玩家的输入
        // UpdateClick();
        // 检测玩家的输入
        // if (Input.touches[0].phase == TouchPhase.Began)
        // UpdateClick();
    }


    protected void UpdateClick()
    {
        // 检测玩家的输入
        // if (Input.touches[0].phase == TouchPhase.Began)
        if (Input.GetMouseButtonDown(0))
        {
            CheckNoteHit();
            downVisual.SetActive(true); 
            // } else if (Input.touches[0].phase == TouchPhase.Moved)
        } else if (Input.GetMouseButton(0))
        {
            // 一直按下，检测长音符
            if (hasLongNote)
            {
                // 计时器累加
                if (timeValue >= 0.15f)
                {
                    // 激活特效
                    if (!longNoteEffectGo.activeSelf)
                    {
                        // 显示命中等级(Great，Perfect)
                        _gameController.ChangeHitLevelSprite(2);
                        CreateHitLongEffect();
                    }
                    timeValue = 0;
                }
                else
                {
                    timeValue += Time.deltaTime;
                }
            }
        } else if (Input.GetMouseButtonUp(0))
            // } else if (Input.touches[0].phase == TouchPhase.Ended)
        {
            // 需要将特效关闭
            downVisual.SetActive(false);
            // 检测长音符
            if (hasLongNote)
            {
                longNoteEffectGo.SetActive(false);
                CheckNoteHit();
            }
        }
    }


    private void OnMouseDown()
    {
        CheckNoteHit();
        downVisual.SetActive(true); 
    }

    private void OnMouseDrag()
    {
        // 一直按下，检测长音符
        if (hasLongNote)
        {
            // 计时器累加
            if (timeValue >= 0.15f)
            {
                // 激活特效
                if (!longNoteEffectGo.activeSelf)
                {
                    // 显示命中等级(Great，Perfect)
                    _gameController.ChangeHitLevelSprite(2);
                    CreateHitLongEffect();
                }
                timeValue = 0;
            }
            else
            {
                timeValue += Time.deltaTime;
            }
        }
    }

    private void OnMouseUp()
    {
        // 需要将特效关闭
        downVisual.SetActive(false);
        // 检测长音符
        if (hasLongNote)
        {
            longNoteEffectGo.SetActive(false);
            CheckNoteHit();
        }
    }


    /// <summary>
    /// 检测是否击中音符对象
    /// </summary>
    public void CheckNoteHit()
    {
        if (!_gameController.gameObject) return;
        if (trackedNotes.Count > 0)
        {
            NoteObject noteObj = trackedNotes.Peek();
            if (noteObj.hitOffset > -6000)
            {
                // 将音符出队，
                trackedNotes.Dequeue();
                int hitLevel = noteObj.IsNoteHittable();
                // 显示命中等级
                _gameController.ChangeHitLevelSprite(hitLevel);
                if (hitLevel > 0)
                {
                    // 更新分数
                    _gameController.UpdateScoreText(100 * hitLevel);
                    // 产生击中特效
                    if (noteObj.isLongNote)
                    {
                        // 长音符的开始
                        hasLongNote = true;
                        CreateHitLongEffect();
                    }
                    else if (noteObj.isLoneNoteEnd)
                    {
                        hasLongNote = false;
                    }
                    else
                    {
                        CreateEffect(_gameController._hitEffectObjectPool, _gameController.hitEffectGo);   
                    }
                    // 增加最大连击数
                    _gameController.comboNum++;
                }
                else
                {
                    // 未击中
                    // 减少玩家HP
                    _gameController.UpdateHP(_gameController.desHp);
                    // 断掉玩家最大命中连接数
                    _gameController.comboNum = 0;
                    _gameController.HideComboText();
                }
                noteObj.OnHit();
            }
            else
            {
                CreateEffect(_gameController._downEffectObjectPool, _gameController.downEffectGo);
            }
        }
        else
        {
            CreateDownEffect();
        }
    }

    /// <summary>
    /// 生成特效的有关方法
    /// </summary>
    private void CreateDownEffect()
    {
        GameObject downEffect = _gameController.GetFreshEffectObject(_gameController._downEffectObjectPool, _gameController.downEffectGo);
        downEffect.transform.position = keyVisuals.position;
    
    }
    
    /// <summary>
    /// 生成短音符击中的特效
    /// </summary>
    private void CreateHitEffect()
    {
        GameObject hitEffect = _gameController.GetFreshEffectObject(_gameController._hitEffectObjectPool, _gameController.hitEffectGo);
        hitEffect.transform.position = keyVisuals.position;
    }
    
    /// <summary>
    /// 生成长音符击中的特效
    /// </summary>
    protected void CreateHitLongEffect()
    {
        longNoteEffectGo.SetActive(true);
        longNoteEffectGo.transform.position = keyVisuals.position;
    }

    /// <summary>
    /// 生成特效
    /// </summary>
    /// <param name="stack"></param>
    /// <param name="effect"></param>
    private void CreateEffect(Stack<GameObject> stack, GameObject effect)
    {
        GameObject effectPre = _gameController.GetFreshEffectObject(stack, effect);
        effectPre.transform.position = keyVisuals.position;
    }
    
}
