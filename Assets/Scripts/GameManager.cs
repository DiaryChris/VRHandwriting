using System.Collections.Generic;
using UnityEngine;

public enum WorldState
{
    BeginGame,
    Generate,
    GenerateEnd,
    DiscriminateBefore,
    Discriminate,
    DiscriminateEnd,
    EndGame
}

public interface IStateObserver
{
    void OnStateChange(WorldState state);
}


public class GameManager : MonoBehaviour
{
    //public static GameManager instance;

    //私有的静态实例
    private static GameManager _instance = null;
    //共有的唯一的，全局访问点
    public static GameManager Instance {
        get {
            if (_instance == null)
            {    //查找场景中是否已经存在单例
                _instance = GameObject.FindObjectOfType<GameManager>();
                if (_instance == null)
                {    //创建游戏对象然后绑定单例脚本
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }


    

    //状态-观察者模式
    public WorldState defaultState;
    public WorldState worldState;
    public List<IStateObserver> stateObservers = new List<IStateObserver>();

    //音乐播放
    public AudioClip BGM;
    public AudioClip[] soundEffects;
    public AudioSource BGMPlayer;
    public AudioSource effectPlayer;

    //字符List
    public List<char> characters = new List<char>();
    public List<Texture2D> charTex = new List<Texture2D>();
    public List<Texture2D> charTexRed = new List<Texture2D>();
    public List<Texture2D> charTexUser = new List<Texture2D>();
    public List<Transform> charQuad = new List<Transform>();
    public List<float> score = new List<float>();
    public int charIndex = 0;
    public bool isSpace = false;
    public byte redAlpha = 255;

    //字符计时器
    public float spaceInterval = 3;
    public float spaceTimer = 0;


    private void Awake()
    {
        //防止存在多个单例
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);

    }


    private void Start()
    {
        worldState = defaultState;

        PlayBGM();

        charIndex = 0;
        spaceTimer = 0;
    }

    private void Update()
    {
        Debug.Log("背景音乐是否播放： " + BGMPlayer.isPlaying);
    }


    // Change State
    public void ChangeState(WorldState state)
    {
        Debug.Log("index: " + charIndex + " ChangeState: " + state);
        worldState = state;
        NotifyObservers(state);   
    }

    //Observer Pattern
    public void AddObserver(IStateObserver obs)
    {
        stateObservers.Add(obs);
        Debug.Log("Add Observer "+ obs + " successed.");
    }
    public bool RemoveObserver(IStateObserver obs)
    {
        return stateObservers.Remove(obs);
    }

    public void NotifyObservers(WorldState state)
    {
        foreach (IStateObserver obs in stateObservers)
        {
            obs.OnStateChange(state);
        }
    }

    private void OnStateChange()
    {
        
    }

    //分数转为汉字字符串
    public string ScoreToString(float score)
    {
        string[] numDict = { "零", "一", "二", "三", "四", "五", "六", "七", "八", "九" };
        int scoreInt = Mathf.RoundToInt(score);
        if (scoreInt >= 100)
        {
            return "满分";
        }
        if (scoreInt <= 0)
        {
            return "零分";
        }

        int dec = scoreInt / 10;
        string decStr;
        if (dec == 0)
        {
            decStr = "";
        }
        else if (dec == 1)
        {
            decStr = "十";
        }
        else
        {
            decStr = numDict[dec] + "十";
        }
        int one = scoreInt % 10;
        string oneStr;
        if (one == 0)
        {
            oneStr = "";
        }
        else
        {
            oneStr = numDict[one];
        }

        return decStr + oneStr + "分";
    }


    //Audio Play
    public void PlayBGM()
    {
        //Debug.Log("播放音乐");
        BGMPlayer.clip = BGM;
        BGMPlayer.Play();
    }

    public void PauseBGM()
    {
        BGMPlayer.Pause();
    }
    public void UnPauseBGM()
    {
        BGMPlayer.UnPause();
    }
    public void StopBGM()
    {
        BGMPlayer.Stop();
    }

    public void PlayEffect(int effectId)
    {
        effectPlayer.clip = soundEffects[effectId];
        effectPlayer.Play();
    }
}

