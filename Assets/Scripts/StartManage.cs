using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

public class StartManage : MonoBehaviour {
    public GameObject chooseLevelGo;
    private Dropdown chooseLevelDropdown;

    public void Awake()
    {
    chooseLevelDropdown = chooseLevelGo.GetComponent<Dropdown>();       // 得到下拉框组件
    List<string> levelsList = new List<string>();                       // 选项存在 List 里面
    for (int i = 1; i <= 100; ++i)
    {
        levelsList.Add(i + "");
    }
    chooseLevelDropdown.ClearOptions();                                 // 清空选项
    chooseLevelDropdown.AddOptions(levelsList);                         // 添加选项
    }

    // 新游戏
    public void OnNewGameBtnClick()
    {
        PlayerPrefs.SetInt("currentLevel", 0);
        SceneManager.LoadScene(1);
    }

    // 选择关卡
    public void OnChooseLevelBtnClick()
    {
        chooseLevelGo.SetActive(true);
    }

    // 选择关卡确定按钮
    public void OnChooseLevelOkBtnClick()
    {
        chooseLevelGo.SetActive(false);             // 隐藏选择关卡界面
        // 保存输入的选择关卡数据
        PlayerPrefs.SetInt("currentLevel", chooseLevelDropdown.value);
        SceneManager.LoadScene(1);                  // 加载游戏场景
    }

    // 退出游戏按钮
    public void OnCancelBtnClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();      
#endif
    }
}
