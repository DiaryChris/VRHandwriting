using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
//using UnityEditor.SceneManagement;
//using UnityEditor;

public class UIManager : MonoBehaviour, IStateObserver
{
    public CanvasGroup paintCanvas;      //写字canvas

    //public CanvasGroup UICanvas;
    public CanvasGroup startPanel;   //开始界面
    public CanvasGroup endPanel;     //结束界面

    //public RawImage startRawImage;

    public Text scoreTxt;

    private GameManager GM;

    private void Awake()
    {
        GM = GameManager.Instance;
        GM.AddObserver(this);
        //Debug.Log(score);
    }

    private void Start()
    {
        DisableCanvas(paintCanvas);

        //EnableCanvas(UICanvas);
        EnableCanvas(startPanel);
        DisableCanvas(endPanel);
    }


    public void OnStateChange(WorldState state)
    {
        //开始游戏出现开始界面
        if(state == WorldState.BeginGame)
        {
            DisableCanvas(paintCanvas);

            //EnableCanvas(UICanvas);
            EnableCanvas(startPanel);
            DisableCanvas(endPanel);
        }
        //点击开始游戏后，开始界面隐藏，出现画纸，开始生成文字
        else if(state == WorldState.Generate)
        {
            //DisableCanvas(UICanvas);
            DisableCanvas(startPanel);
            DisableCanvas(endPanel);
            EnableCanvas(paintCanvas);
        }
        //游戏结束后，结束界面显示
        else if(state == WorldState.EndGame)
        {
            //显示分数
            EndGame();

            DisableCanvas(paintCanvas);

            //EnableCanvas(UICanvas);
            EnableCanvas(endPanel);
            DisableCanvas(startPanel);
        }
    }

    /// <summary>
    /// 隐藏UI
    /// </summary>
    /// <param name="cg"></param>
    private void DisableCanvas(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    /// <summary>
    /// 显示UI
    /// </summary>
    /// <param name="cg"></param>
    private void EnableCanvas(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    /// <summary>
    /// 游戏结束，显示最终分数
    /// </summary>
    void EndGame()
    {
        float score = 0;
        int num = GM.score.Count;
        for(int i =0;i<num;i++)
        {
            score += GM.score[i];
        }
        score = score / num;
        string res = GM.ScoreToString(score);
        Debug.Log("The last score: " + res);
        scoreTxt.text = res;
    }

    /// <summary>
    /// 重新开始
    /// </summary>
    public void Restart()
    {
        Debug.Log("button Restart click.");
        SceneManager.LoadScene("PaintingTest");
    }

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        Debug.Log("button StartGame click.");
        GM.ChangeState(WorldState.Generate);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void ExitGame()
    {
        Debug.Log("button ExitGame click.");
        Application.Quit();
    }
}
