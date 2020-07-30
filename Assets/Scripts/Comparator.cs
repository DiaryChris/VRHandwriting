using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Comparator : MonoBehaviour, IStateObserver
{
    private Texture2D tex1;
    private Texture2D tex2;

    //public Text txt_score;
    public float lerpRatio = 0.5f;

    public float colorThreshold = 127;

    private Color32[] tex1Colors;
    private Color32[] tex2Colors;
    private GameManager GM;

    private void Awake()
    {
        GM = GameManager.Instance;
        GM.AddObserver(this);
        //Debug.Log(score);
    }


    public void OnStateChange(WorldState state)
    {
        if (state == WorldState.Discriminate)
        {
            tex1 = GM.charTex[GM.charIndex - 1];
            tex2 = GM.charTexUser[GM.charIndex - 1];
            GM.score.Add(CompareTexture());
            //Debug.Log("The " + GM.charIndex + " times, score is " + GM.score[GM.charIndex - 1]);
            //txt_score.text =  "" + GM.score[GM.charIndex - 1];
            GM.ChangeState(WorldState.DiscriminateEnd);
        }
    }

    public float RatioToScore(float x)
    {
        return -100 * x * x + 200 * x;
    }

    private float CompareTexture()
    {
        if(tex1 == null || tex2 == null)
        {
            return 0;
        }

        tex1Colors = tex1.GetPixels32();
        tex2Colors = tex2.GetPixels32();
        float grayScale1;
        float grayScale2;

        int overlapPixelNum = 0;
        int tex1PixelNum = 0;
        int tex2PixelNum = 0;

        for (int i = 0; i < tex1Colors.Length; i++)
        {
            grayScale1 = (tex1Colors[i].r + tex1Colors[i].g + tex1Colors[i].b) / 3;
            grayScale2 = (tex2Colors[i].r + tex2Colors[i].g + tex2Colors[i].b) / 3;
            

            if (grayScale1 < colorThreshold && grayScale2 < colorThreshold)
            {
                overlapPixelNum++;
            }
            if (grayScale1 < colorThreshold)
            {
                tex1PixelNum++;
            }
            if (grayScale2 < colorThreshold)
            {
                tex2PixelNum++;
            }
        }

        if(tex1PixelNum == 0 || tex2PixelNum == 0)
        {
            return 0;
        }

        float coverage1 = (float)overlapPixelNum / tex1PixelNum;
        float coverage2 = (float)overlapPixelNum / tex2PixelNum;
        //Debug.Log(coverage1);
        //Debug.Log(coverage2);

        float score = RatioToScore(Mathf.Lerp(coverage1, coverage2, lerpRatio));

        return score;
    }
}
