
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
using TMPro;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using SonicBloom.Koreo.Players;
using UnityEngine.SceneManagement;

public class RhyGameController : MonoBehaviour
{
    // 给出友好提示
    [Tooltip("用于目标生成的轨道的事件对应ID")]
    [EventID] // 检测unity中所有的事件
    private string eventID;
    
    // 音符速度,单位为m/s
    public float noteSpeed = 1;

    // 击打音符的时间区间,在8毫秒~300毫秒
    [Tooltip("音符命中区间窗口，单位是毫秒")]
    [Range(8f, 300f)]
    public float hitWindowRangeInMS;
    
    // 以unity为单位访问当前命中窗口的大小
    public float WindowSizeInUnits
    {
        get
        {
            return noteSpeed * (hitWindowRangeInMS * 0.001f);
        }
    }
    
    // 音乐样本中的命中窗口，通过样本点的个数得到窗口长度
    [HideInInspector]
    public  int _hitWindowRangeInSamples;

    // 采样率,由Koreographer决定
    public int SampleRate
    {
        get
        {
            return playingKoreo.SampleRate;
        }
    }

    public Stack<NoteObject> _noteObjectPool = new Stack<NoteObject>();

    public Stack<GameObject> _downEffectObjectPool = new Stack<GameObject>();

    public Stack<GameObject> _hitEffectObjectPool = new Stack<GameObject>();

    // public Stack<GameObject> _hitLongEffectObjectPool = new Stack<GameObject>();

    // 获取 koreagrapy对象
    private Koreography playingKoreo;
    
    // 切歌
    // public List<Koreography> kgyList = new List<Koreography>();
    private Dictionary<string, Koreography> hashMap = new Dictionary<string, Koreography>();
    // 选择的koreo的轨道名称
    public List<string> musicName;
    public List<Koreography> musicKoreo;

    // 音乐播放对象的获取
    private SimpleMusicPlayer _simpleMusicPlayer;

    public GameObject SimpleMusicPlayGO;
    
    // 预设体资源
    // 音符资源
    // public GameObject noteObject
    public NoteObject noteObject;

    // 按下特效
    public GameObject downEffectGo;
    
    // 击中音符的特效
    public GameObject hitEffectGo;
    
    // 击中长音符特效
    public GameObject hitLongNoteEffectGo;
    
    // 播放音乐
    public AudioSource audioCom;
    

    // 其他
    [Tooltip("开始播放音频之前提供的时间量(以秒为单位)，也就是提前调用时间")]
    public float leadInTime;

    // 音频播放之前的剩余时间量，其实就等于从看到音符，到音符触及到按键的时间差，这个是不变的
    private float leadInTimeLeft;
    
    // 音乐开始之前的倒计时器,音乐接触到第一个音符之后才调用播放音乐
    private float timeLeftToPlay;

    public List<LineController> noteLines = new List<LineController>();

    // 当前的采样时间，包含任何必要的延迟，对于Koreograpy是延迟,也就是当前播放到的事件
    public int DelayedSampleTime
    {
        get
        {
            // 音符出现的时间，也就是最新的采样时间 - 需要延迟的时间。
            return playingKoreo.GetLatestSampleTime() - (int)(SampleRate  * leadInTimeLeft);
        }
    }
    
    // 计时器，隐藏当前命中等级
    private float hideHitLevelImageTimeVal;

    // 显示等级图片的动画播放
    public Animator hitLevelAnim;

    public Animator comboTextAnim;

    public int comboNum;
    public int score;
    public int hp = 10;
    public int desHp = 1;

    // 是否为暂停状态
    public bool isPauseState;
    
    // UI
    public Slider slider;

    // 分数的分数面板
    public TMP_Text scoreText;
    
    // 命中等级
    public Image hitLevelImage;
    
    // 连续击中数
    public TMP_Text comboText;
    
    // 资源
    public Sprite[] hitLevelSprites;
    
    // 游戏结束的UI
    public GameObject gameOver;
    
    // 游戏正式开启
    private bool gameStart;
    
    
    /// <summary>
    /// 初始化引导时间
    /// </summary>
    void InitializedLeadIn()
    {
        // 实现音乐的提前调用
        if (leadInTime > 0)
        {
            leadInTimeLeft = leadInTime;
            timeLeftToPlay = leadInTime;
        }
        else
        {
            // 没有引导时间
            audioCom.Play();
        }
    }

    private void Start()
    {
        // PlayerPrefs.DeleteAll();
        InitializedLeadIn();
        InitKoreography();
        _simpleMusicPlayer = SimpleMusicPlayGO.transform.GetComponent<SimpleMusicPlayer>();
        // Koreography k = _simpleMusicPlayer.GetComponent<Koreography>();
        // k = GetKoreography();

        Koreography koreo = GetKoreography();
        // List<KoreographyTrackBase> koreographyTrackBases = koreo.Tracks;
        eventID = koreo.Tracks[0].EventID;

// 切歌的示范
        _simpleMusicPlayer.LoadSong(koreo,0, false);
        
        for (int i = 0; i < noteLines.Count; i++)
        {
            noteLines[i].Initialized(this);
        }

        // 获取到Koreographer对象
        playingKoreo = Koreographer.Instance.GetKoreographyAtIndex(0);
        // 获取事件轨迹
        KoreographyTrackBase rhythmTrack = playingKoreo.GetTrackByID(eventID);
        
        // 获取事件
        List<KoreographyEvent> koreographyEvents = rhythmTrack.GetAllEvents();

        for (int i = 0; i < koreographyEvents.Count; i++)
        {
            KoreographyEvent evt = koreographyEvents[i];
            int noteId = (int)evt.GetIntValue();

            // 遍历每一个音轨，做一个匹配
            for (int j = 0; j < noteLines.Count; j++)
            {
                LineController lane = noteLines[j];
                if (noteId > 6)
                {
                    noteId -= 6;
                    if (noteId > 6)
                    {
                        noteId -= 6;
                    }
                }
                if (lane.DoesMatch(noteId))
                {
                    lane.AddEventToLane(evt);
                    break;
                }
            }
        } 
        _hitWindowRangeInSamples = (int)(SampleRate * hitWindowRangeInMS * 0.001);
    }

    private void Update()
    {
        if (isPauseState) return;
        if (timeLeftToPlay > 0)
        {
            // 不受时间缩放的影响
            timeLeftToPlay -= Time.unscaledDeltaTime;

            if (timeLeftToPlay <= 0)
            {
                audioCom.Play();
                gameStart = true;
                timeLeftToPlay = 0;
            }
        }
        
        // 倒数我们的引导时间
        if (leadInTimeLeft > 0)
        {
            leadInTimeLeft = Mathf.Max(leadInTimeLeft - Time.unscaledDeltaTime, 0);
        }

        if (hitLevelImage.gameObject.activeSelf)
        {
            if (hideHitLevelImageTimeVal > 0)
            {
                hideHitLevelImageTimeVal -= Time.deltaTime;
            }
            else
            {
                // 隐藏当前ComBo
                HideComboText();
                // 隐藏等级图片
                HideLevelImage();
            }
        }

        if (gameStart)
        {
            if (!_simpleMusicPlayer.IsPlaying)
            {
                gameOver.SetActive(true);   
            }
        }
    }
    
    // 对象池技术
    /// <summary>
    /// 从池中取对象
    /// </summary>
    /// <returns></returns>
    public NoteObject GetFreshNoteObject()
    {
        NoteObject nobj;
        if (_noteObjectPool.Count > 0)
        {
            nobj = _noteObjectPool.Pop();
        }
        else
        {
            nobj = Instantiate(noteObject);
        }
        nobj.transform.position=Vector3.one * 2;
        nobj.gameObject.SetActive(true);
        nobj.enabled = true;
        return nobj;
    }
    
    /// <summary>
    /// 将音符对象放回对象池
    /// </summary>
    /// <param name="obj"></param>
    public void ReturnNoteObjectToPool(NoteObject obj)
    {
        if (null != obj)
        {
            obj.enabled = false;
            obj.gameObject.SetActive(false);
            _noteObjectPool.Push(obj);
        }
    }

    /// <summary>
    /// 获取音符特效的对象
    /// </summary>
    /// <param name="stack"></param>
    /// <param name="effectPrefab"></param>
    /// <returns></returns>
    public GameObject GetFreshEffectObject(Stack<GameObject> stack, GameObject effectPrefab)
    {
        GameObject effectGo;
        if (stack.Count > 0)
        {
            effectGo = stack.Pop();
        }
        else
        {
            effectGo = Instantiate(effectPrefab);
        }
        effectGo.SetActive(true);
        return effectGo;
    }

    /// <summary>
    /// 将特效对象放回对象池中
    /// </summary>
    /// <param name="stack"></param>
    /// <param name="effctObj"></param>
    public void ReturnEffectGoToPool(Stack<GameObject> stack, GameObject effectObj)
    {
        if (effectObj != null)
        {
            effectObj.SetActive(false);
            stack.Push(effectObj);
        }
    }
    
    /// <summary>
    /// 显示当前命中等级的图片
    /// </summary>
    /// <param name="level"></param>
    public void ChangeHitLevelSprite(int level)
    {
        hideHitLevelImageTimeVal = 1;
        hitLevelImage.sprite = hitLevelSprites[level];
        // 不让图片压缩
        hitLevelImage.SetNativeSize();
        hitLevelImage.gameObject.SetActive(true);
        // 播放动画
        hitLevelAnim.SetBool("isNoteHittable", true);
        if (comboNum >= 5)
        {
            comboText.gameObject.SetActive(true);
            // 如果连接数量大于5的时候才显示出来
            comboText.text = comboNum.ToString();
            comboTextAnim.SetBool("isNoteHittable", true);
        }
        // hitLevelAnim.Play("UIAnimation");
    }

    /// <summary>
    /// 隐藏命中等级的图片
    /// </summary>
    public void HideLevelImage()
    {
        hitLevelImage.gameObject.SetActive(false);
    }

    /// <summary>
    /// 隐藏当前Combo
    /// </summary>
    public void HideComboText()
    {
        comboText.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 更新分数
    /// </summary>
    /// <param name="addNum"></param>
    public void UpdateScoreText(int addNum)
    {
        score += addNum;
        scoreText.text = score.ToString();
    }

    /// <summary>
    /// 更新玩家的血条
    /// </summary>
    public void UpdateHP(int des)
    {
        hp -= des;
        slider.value = (float)hp / 10;
        if (hp == 0)
        {
            // 游戏结束
            isPauseState = true;
            gameOver.SetActive(true);
            PauseMusic();
        }
    }

    /// <summary>
    /// 暂停音乐
    /// </summary>
    public void PauseMusic()
    {
        if (!gameStart) return;
        _simpleMusicPlayer.Pause();
    }

    /// <summary>
    /// 播放音乐
    /// </summary>
    public void PlayMusic()
    {
        if (!gameStart) return;
        _simpleMusicPlayer.Play();
    }

    /// <summary>
    /// 游戏结束重玩
    /// </summary>
    public void Retry()
    {
        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// 游戏结束返回
    /// </summary>
    public void ReturnToMain()
    {
        SceneManager.LoadScene(1);
    }

    Koreography GetKoreography()
    {
        string trackName = PlayerPrefs.GetString("musicName", "RhythmGameKoreo");
        return hashMap[trackName];
    }

    private void InitKoreography()
    {
        for (int i = 0; i < Math.Min(musicName.Count, musicKoreo.Count); i++)
        {
            hashMap.Add(musicName[i],musicKoreo[i]);
        }
    }
    

}
