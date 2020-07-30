using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GetRedChar : MonoBehaviour, IStateObserver
{

    private GameManager GM;
    public RawImage char_red;
    Texture2D tex_redChar;

    // Use this for initialization
    void Awake() {
        //Debug.Log("GetRedChar");
        GM = GameManager.Instance;
        //GM = GameManager.GetInstance();
        GM.AddObserver(this);
        //Debug.Log(GM);
    }

    void Update()
    {
        //Debug.Log("GetRedChar" + GM.stateObservers.Count);
    }


    public void OnStateChange(WorldState state)
    {
        //Debug.Log("GetRedChar1");
        if (state == WorldState.GenerateEnd)
        {
            //Debug.Log("GetRedChar2");
            tex_redChar = GM.charTexRed[GM.charIndex - 1];
            char_red.texture = tex_redChar;
            if (tex_redChar != null)
                char_red.color = new Color(char_red.color.r, char_red.color.g, char_red.color.b, GM.redAlpha);
            else
                char_red.color = new Color(char_red.color.r, char_red.color.g, char_red.color.b, 0.0f);
        }
        //判定结束后红字消除
        else if (state == WorldState.DiscriminateEnd)
        {
            char_red.color = new Color(char_red.color.r, char_red.color.g, char_red.color.b, 0.0f);
        }
    }
}
