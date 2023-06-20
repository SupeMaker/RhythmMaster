
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStart : MonoBehaviour
{
      /// <summary>
      /// 开始游戏
      /// </summary>
      public void StartGame()
      {
        SceneManager.LoadScene(2);
      }

      public void SelectLevel()
      {
          SceneManager.LoadScene(1);
      }
}
