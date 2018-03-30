using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManage : MonoBehaviour {
    public const int BrickNum = 16;         // 每行砖块数量

    // 空白地板
    public GameObject brickPrefab;

    private SpriteRenderer[,] brickRenderers = new SpriteRenderer[16, 16];
    private int[,] brickDatas = new int[16, 16];            // 存储砖块数量
    // 0空白  1 墙  2地板  3空坑  4满坑  5豆  6上 7下 8左 9右
    public Sprite[] brickSprites;

    private int curLevel = 0;       // 当前关卡
    private int curStep = 0;        // 当前步数
    private Vector2 currentPoint;   // 当前位置
    private bool isOnEmptyBrick;    // 当前是否在空坑上
    private bool isFinished;        // 当前是否胜利
    private int numOfEmptyBrick = 0;    // 空坑数目


    public Text curLevelText;       // 当前关数组件
    public Text curStepText;        // 当前步数组件
    public GameObject finishedGo;   // 胜利窗口

	// Use this for initialization
	void Start () {
        InitMap();          // 初始化地图数据
	}
	
	// Update is called once per frame
	void Update () {
        UpdateMap();         // 更新地图
        PlayerMove();       // 监听键盘控制人物移动
        if (isFinished)     // 若胜利
        {
            finishedGo.SetActive(true);
            isFinished = false;
        }
	}

    // 创建实体，并将实体放在当前组件下
    private GameObject CreateItem(GameObject createCameObject, Vector3 createPosition, Quaternion createRotation)
    {
        GameObject itemGo = Instantiate(createCameObject, createPosition, createRotation);  // 创建实体
        itemGo.transform.SetParent(gameObject.transform);                                   // 放在该目录下
        return itemGo;
    }

    // 初始化地图模型
    private void InitMap()
    {
        curStep = 0;            // 初始化当前步数为 0
        curLevel = PlayerPrefs.GetInt("currentLevel");                  // 获取选择的关卡
        string[] level = Config.Instance.levels[curLevel].Split(",".ToCharArray());         // 加载当前关卡数据
        for (int i = 0; i < level.Length; ++i)
        {
            string tmp = level[i];                                                          // 第i行数据
            for (int j = 0; j < tmp.Length; ++j)                                            // 创建每一个方块
            {
                Vector3 pos = transform.position + new Vector3(1.23f * j, -1.23f * i, 0);
                brickDatas[j, i] = int.Parse(tmp[j].ToString());
                if (brickDatas[j, i] >= 6)
                {
                    currentPoint = new Vector2();           // 保存当前玩家位置
                    currentPoint.y = i;
                    currentPoint.x = j;
                }
                else if (brickDatas[j, i] == 3)             // 记录空坑数量
                {
                    numOfEmptyBrick++;
                }
                // 实例化
                GameObject itemGo = CreateItem(brickPrefab, pos, Quaternion.identity);
                brickRenderers[j, i] = itemGo.GetComponent<SpriteRenderer>();           // 获取渲染组件
            }
        }
    }

    private void LoadMapData()
    {
        curStep = 0;            // 初始化
        isFinished = false;
        numOfEmptyBrick = 0;
        string[] level = Config.Instance.levels[curLevel].Split(",".ToCharArray());         // 加载当前关卡数据
        for (int i = 0; i < level.Length; ++i)
        {
            string tmp = level[i];                                                          // 第i行数据
            for (int j = 0; j < tmp.Length; ++j)                                            // 创建每一个方块
            {
                brickDatas[j, i] = int.Parse(tmp[j].ToString());
                if (brickDatas[j, i] >= 6)
                {
                    currentPoint = new Vector2();           // 保存当前玩家位置
                    currentPoint.y = i;                     // y为竖直方向，即为行数
                    currentPoint.x = j;                     // x为水平方向，即为列数
                }
                else if (brickDatas[j, i] == 3)             // 记录空坑数量
                {
                    numOfEmptyBrick++;
                }
            }
        }
    }

    // 更新地图数据
    private void UpdateMap()
    {
        for (int i = 0; i < BrickNum; ++i)
        {
            for (int j = 0; j < BrickNum; ++j)
            {
                // 更新显示
                brickRenderers[i, j].sprite = brickSprites[brickDatas[i, j]];
            }
        }
        curLevelText.text = string.Format("{0:000}", curLevel + 1);         // 格式化显示
        curStepText.text = string.Format("{0:000}", curStep);
    }

    // 模拟移动
    private void MakeMove(Vector2 point1, Vector2 point2)
    {
        int x = (int)currentPoint.x, x1 = (int)point1.x, x2 = (int)point2.x, y = (int)currentPoint.y, y1 = (int)point1.y, y2 = (int)point2.y;    // 取整
        int data = brickDatas[x, y], data1 = brickDatas[x1, y1], data2 = brickDatas[x2, y2];             // 取数据
        
        if (data1 == 2)                             // 下一个方块为空
        {
            currentPoint = point1;                  // 交换
            brickDatas[x1, y1] = data;
            if (!isOnEmptyBrick)
            {
                brickDatas[x, y] = 2;
            }
            else                                   // 若当前位之前为空坑，还原
            {
                brickDatas[x, y] = 3;
            }
            isOnEmptyBrick = false;
            curStep++;
        }
        else if (data1 == 3)
        {
            brickDatas[x1, y1] = data;
            if (!isOnEmptyBrick)
            {
                brickDatas[x, y] = 2;
            }
            else                                   // 若当前位之前为空坑，还原
            {
                brickDatas[x, y] = 3;
            }
            isOnEmptyBrick = true;
            currentPoint = point1;
            curStep++;
        }
        else if (data1 == 4)                        // 若下一个方块为满坑
        {
            if (data2 == 2)                         // 空
            {
                brickDatas[x2, y2] = 5;
                brickDatas[x1, y1] = data;
                if (!isOnEmptyBrick)
                {
                    brickDatas[x, y] = 2;
                }
                else                                   // 若当前位之前为空坑，还原
                {
                    brickDatas[x, y] = 3;
                }
                isOnEmptyBrick = true;
                currentPoint = point1;
                curStep++;
            }
            else if (data2 == 3)
            {
                brickDatas[x2, y2] = 4;
                brickDatas[x1, y1] = data;
                if (!isOnEmptyBrick)
                {
                    brickDatas[x, y] = 2;
                }
                else                                   // 若当前位之前为空坑，还原
                {
                    brickDatas[x, y] = 3;
                }
                isOnEmptyBrick = true;
                currentPoint = point1;
                curStep++;
            }
        }
        else if (data1 == 5)                        // 若下一个方块为豆子
        {
            if (data2 == 2)                         // 空
            {
                brickDatas[x2, y2] = 5;
                brickDatas[x1, y1] = data;
                if (!isOnEmptyBrick)
                {
                    brickDatas[x, y] = 2;
                }
                else                                   // 若当前位之前为空坑，还原
                {
                    brickDatas[x, y] = 3;
                }
                currentPoint = point1;
                curStep++;
            }
            else if (data2 == 3)                    // 空坑
            {
                brickDatas[x2, y2] = 4;
                brickDatas[x1, y1] = data;
                if (!isOnEmptyBrick)
                {
                    brickDatas[x, y] = 2;
                }
                else                                   // 若当前位之前为空坑，还原
                {
                    brickDatas[x, y] = 3;
                }
                isOnEmptyBrick = false;
                currentPoint = point1;
                curStep++;
                numOfEmptyBrick--;
                cheackFinished();
            }
        }
    }

    private void PlayerMove()
    {
        Vector2 point1 = currentPoint;
        Vector2 point2 = currentPoint;
        if (Input.GetKeyDown(KeyCode.W))            // 向上
        {
            point1.y--;
            point2.y -= 2;
            brickDatas[(int)currentPoint.x, (int)currentPoint.y] = 6;
            MakeMove(point1, point2);
        }
        else if (Input.GetKeyDown(KeyCode.S))       // 向下
        {
            point1.y++;
            point2.y += 2;
            brickDatas[(int)currentPoint.x, (int)currentPoint.y] = 7;
            MakeMove(point1, point2);
        }
        else if (Input.GetKeyDown(KeyCode.A))       // 向左
        {
            point1.x--;
            point2.x -= 2;
            brickDatas[(int)currentPoint.x, (int)currentPoint.y] = 8;
            MakeMove(point1, point2);
        }
        else if (Input.GetKeyDown(KeyCode.D))       // 向右
        {
            point1.x++;
            point2.x += 2;
            brickDatas[(int)currentPoint.x, (int)currentPoint.y] = 9;
            MakeMove(point1, point2);
        }
    }

    // 检测是否胜利
    private void cheackFinished()
    {
        if (numOfEmptyBrick == 0)
        {
            isFinished = true;
        }
    }

    // 重玩游戏
    public void OnRestartBtnClick()
    {
        LoadMapData();
    }

    // 上一关
    public void OnPreLevelBtnClick()
    {
        if (curLevel > 0)
        {
            curLevel--;
            LoadMapData();
        }
    }

    // 下一关
    public void OnLastLevelBtnClick()
    {
        if (curLevel < 99)
        {
            curLevel++;
            LoadMapData();
        }
    }

    // 胜利界面点击确定按钮
    public void OnFinishedOkBtnClick()
    {
        finishedGo.SetActive(false);
        OnLastLevelBtnClick();
    }

    // 回到菜单
    public void OnReturnToMenuBtnClick()
    {
        SceneManager.LoadScene(0);
    }

    // 退出游戏
    public void OnQuitGameBtnClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();      
#endif
    }
}
