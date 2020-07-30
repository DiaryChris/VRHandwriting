using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CharGenerator : MonoBehaviour, IStateObserver
{
    public Font font;
    public string poemFolder;
    public int levelIndex;
    public Camera characterCamera;
    public Text textUI;

    public Transform spawner;
    public Transform charPrefab;
    public Transform charParent;

    public float basicTime;
    public float additiveTime;
    public float spaceTime;
    public float endZ = 0;
    public float colorThreshold = 127;

    private GameManager GM;

    private void Awake()
    {
        GM = GameManager.Instance;
        GM.AddObserver(this);
        LoadTXT();
    }

    private void Start()
    {
        //GM.ChangeState(WorldState.Generate);
    }

    public void OnStateChange(WorldState state)
    {
        //Debug.Log("CharGenerator1");
        if (state == WorldState.Generate)
        {
            if (GM.charIndex < GM.characters.Count)
            {
                Generate();
                //GM.charTimer = 0;
                //Debug.Log("ChangeState");
                GM.ChangeState(WorldState.GenerateEnd);
            }
            else
                GM.ChangeState(WorldState.EndGame);
        }
        
    }

    //读取TXT文件
    private void LoadTXT()
    {
        //GM.characters = new List<char>();

        StreamReader sr = new StreamReader(Application.dataPath + "/" + poemFolder + "/" + levelIndex + ".txt");
        string line;
        while ((line = sr.ReadLine()) != null)
        {
            line = line.Trim();
            foreach (char ch in line)
            {
                GM.characters.Add(ch);
            }
            GM.characters.Add(' ');
        }
    }

    //执行一次生成
    public void Generate()
    {
        GetCharTexture();
        GenerateCharQuad();
        GM.charIndex++;
    }
    //取得文字纹理
    private void GetCharTexture()
    {
        Texture2D curCharTex;
        Texture2D curCharTexRed;
        string text = GM.characters[GM.charIndex].ToString();
        textUI.text = text;
        textUI.color = Color.black;
        if(text == " ")
        {
            GM.isSpace = true;
            GM.charTex.Add(null);
            GM.charTexRed.Add(null);
            return;
        }
        GM.isSpace = false;

        //获取相机渲染纹理并设为目前活动
        RenderTexture renderTexture = characterCamera.targetTexture;
        RenderTexture.active = renderTexture;

        //调用一次字符摄像机渲染
        characterCamera.Render();

        curCharTex = new Texture2D(renderTexture.width, renderTexture.height);
        curCharTex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        curCharTex.Apply();

        GM.charTex.Add(curCharTex);
        SavePNG(Application.dataPath + "/CharImages/Black/" + levelIndex + "_" + GM.charIndex + ".png", curCharTex);

        //渲染红色字帖到纹理
        textUI.color = new Color32(255, 0, 0, GM.redAlpha);
        //textUI.color = new Color32(255, 0, 0, 1);
        characterCamera.Render();

        curCharTexRed = new Texture2D(renderTexture.width, renderTexture.height);
        curCharTexRed.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        curCharTexRed.Apply();

        GM.charTexRed.Add(curCharTexRed);
        SavePNG(Application.dataPath + "/CharImages/Red/" + levelIndex + "_" + GM.charIndex + ".png", curCharTexRed);

        //Debug.Log(characterTex.width + "/" + characterTex.height);
    }

    //计算字帖像素比例
    private float calcPixelRatio(Texture2D tex)
    {
        if (tex == null)
        {
            return 0;
        }

        Color32[] pixels = tex.GetPixels32();
        float grayScale = 0;
        int count = 0;


        for (int i = 0; i < pixels.Length; i++)
        {
            grayScale = (pixels[i].r + pixels[i].g + pixels[i].b) / 3;
            if(grayScale < colorThreshold)
            {
                count++;
            }
        }
        return (float)count / pixels.Length;
    }

    //缓动函数
    private float ease(float x)
    {
    	return 6 * Mathf.Pow(x, 5) - 15 * Mathf.Pow(x, 4) + 10 * Mathf.Pow(x, 3);
    }

    //生成字帖正方形
    private void GenerateCharQuad()
    {
        //if (GM.isSpace)
        //{
        //    GM.charQuad.Add(null);
        //    return;
        //}
        Transform curCharQuad = Instantiate(charPrefab, spawner.position, Quaternion.identity, charParent);
        float speed;
        Texture2D tex = GM.charTex[GM.charIndex];
        if (tex == null)
        {
            curCharQuad.GetComponent<MeshRenderer>().enabled = false;
            speed = (endZ - spawner.position.z) / spaceTime;
        }
        else
        {
            curCharQuad.GetComponent<MeshRenderer>().material.mainTexture = tex;
            speed = (endZ - spawner.position.z) / (basicTime + additiveTime * ease(calcPixelRatio(tex)));
        }
        curCharQuad.GetComponent<CharQuad>().speed = speed;
        curCharQuad.GetComponent<CharQuad>().endZ = endZ;
        //curCharQuad.GetComponent<CharQuad>().exitTime = 0.0f;
        GM.charQuad.Add(curCharQuad);
    }

    private void SavePNG(string path, Texture2D texture2D)
    {
        //Debug.Log("Save Path:" + path);
        byte[] bytes = texture2D.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }
}
