
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
 
public class LevelSelect : MonoBehaviour
{
 
    public TMP_Dropdown dropDown;
    public List<string> trackName;

    // 是否是代码设置下拉 Item 值
    private bool isCodeSetItemValue = false;
 
    void Start()
    {
        InitDropDown();
        // 设置监听
        SetDropDownAddListener(OnValueChange);
        SetDropDownItemValue(0);
    }
 
    /// <summary>
    /// 当点击后值改变时触发 (切换下拉选项)
    /// </summary>
    /// <param name="v">是点击的选项在OptionData下的索引值</param>
    void OnValueChange(int v)
    {
        // //切换选项 时处理其他的逻辑...
        // Debug.Log("点击下拉控件的索引是..." + v);
        PlayerPrefs.SetString("musicName", dropDown.options[v].text);
    }

    private void InitDropDown()
    {
        dropDown.options.Clear();
        for (int i = 0; i < trackName.Count; i++)
        {
            TMP_Dropdown.OptionData op = new TMP_Dropdown.OptionData();
            op.text = trackName[i];
            dropDown.options.Add(op);   
        }
    }
 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) {
 
            List<TMP_Dropdown.OptionData> listOptions = new List<TMP_Dropdown.OptionData>();
            listOptions.Add(new TMP_Dropdown.OptionData("Option 0"));
            listOptions.Add(new TMP_Dropdown.OptionData("Option 1"));
 
            AddDropDownOptionsData(listOptions);
        }
 
        if (Input.GetKeyDown(KeyCode.A))
        {
 
            AddDropDownOptionsData("Option " + dropDown.options.Count);
        }
 
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveAtDropDownOptionsData(dropDown.options.Count - 1);
        }
 
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearDropDownOptionsData();
        }
    }
 
    /// <summary>
    /// 设置选择的下拉Item
    /// </summary>
    /// <param name="ItemIndex"></param>
    void SetDropDownItemValue(int ItemIndex)
    {
        // 代码设置的值
        isCodeSetItemValue = true;
 
        if (dropDown.options == null) {
 
            Debug.Log(GetType()+ "/SetDropDownItemValue()/下拉列表为空，请检查");
            return;
        }
        if (ItemIndex >= dropDown.options.Count) {
            ItemIndex = dropDown.options.Count - 1;
        }
 
        if (ItemIndex < 0 )
        {
            ItemIndex = 0;
        }
 
        dropDown.value = ItemIndex;
    }
 
 
    /// <summary>
    /// 是否可以点击
    /// </summary>
    void SetDropDownInteractable() {
        //是否可以点击
        dropDown.interactable = true;
    }
 
    /// <summary>
    /// 设置显示字体大小
    /// </summary>
    /// <param name="fontSize"></param>
    void SetDropDownCaptionTextFontSize(int fontSize) {
        //设置显示字体大小
        dropDown.captionText.fontSize = fontSize;
    }
 
    /// <summary>
    /// 设置下拉Item显示字体大小
    /// </summary>
    /// <param name="fontSize"></param>
    void SetDropDownItemTextFontSize(int fontSize)
    {
        //设置下拉Item显示字体大小
        dropDown.itemText.fontSize = fontSize;
    }
 
    /// <summary>
    /// 添加一个列表下拉数据
    /// </summary>
    /// <param name="listOptions"></param>
    void AddDropDownOptionsData(List<TMP_Dropdown.OptionData> listOptions) {
        dropDown.AddOptions(listOptions);
    }
 
    /// <summary>
    /// 添加一个下拉数据
    /// </summary>
    /// <param name="itemText"></param>
    void AddDropDownOptionsData(string itemText) {
        //添加一个下拉选项
        TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
        data.text = itemText;
        //data.image = "指定一个图片做背景不指定则使用默认"；
        dropDown.options.Add(data);
    }
 
 
    /// <summary>
    /// 移除指定位置   参数:索引
    /// </summary>
    /// <param name="index"></param>
    void RemoveAtDropDownOptionsData(int index) {
 
        // 安全校验
        if (index >= dropDown.options.Count || index < 0) {
            return;
        }
 
        //移除指定位置   参数:索引
        dropDown.options.RemoveAt(index);
    }
 
 
    /// <summary>
    /// 直接清理掉所有的下拉选项
    /// </summary>
    void ClearDropDownOptionsData() {
        //直接清理掉所有的下拉选项，
        dropDown.ClearOptions();
    }
 
    /// <summary>
    /// 当点击后值改变是触发 (切换下拉选项)
    /// </summary>
    void SetDropDownAddListener(UnityAction<int> OnValueChangeListener)
    {
        
 
        //当点击后值改变是触发 (切换下拉选项)
        dropDown.onValueChanged.AddListener((value)=> {
            // 手动代码设置的值不触发事件（根据需要可以保留或者去掉）
            if (isCodeSetItemValue == true)
            {
 
                isCodeSetItemValue = false;
 
                return;
            }
 
            OnValueChangeListener(value);
        });
    }
 
    
 
}
 