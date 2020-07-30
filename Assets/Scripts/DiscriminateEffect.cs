using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using VRTK;

public class DiscriminateEffect : MonoBehaviour, IStateObserver
{
    public CanvasGroup excellent;
    public CanvasGroup good;
    public CanvasGroup moderate;
    public CanvasGroup poor;

    private int excellentScore = 90;
    private int goodScore = 80;
    private int moderateScore = 60;

    private GameManager GM;

    public float exitTime = 10.0f;

    private void Awake()
    {
        GM = GameManager.Instance;
        GM.AddObserver(this);
        //Debug.Log(score);
    }

    private void Start()
    {
        DisableAllEffect();
    }

    public void OnStateChange(WorldState state)
    {
        if (state == WorldState.DiscriminateEnd)
        {
            if(!GM.isSpace)
            {
                Debug.Log("DiscriminateEffect: " + gameObject.name);
                Debug.Log("The " + GM.charIndex + " times, score is " + GM.score[GM.charIndex - 1]);
                //txt_score.text = "" + GM.score[GM.charIndex - 1];


                float score = GM.score[GM.charIndex - 1];
                //根据分数显示判定效果图片
                if (score < moderateScore)
                {
                    effectEnable(poor);
                }
                else if (score < goodScore)
                {
                    effectEnable(moderate);
                }
                else if (score < excellentScore)
                {
                    effectEnable(good);
                }
                else
                {
                    effectEnable(excellent);
                }

                //判定时手柄抖动
                //VRTK_ControllerHaptics.TriggerHapticPulse(VRTK_ControllerReference.GetControllerReference(gameObject), 0.3f);
            }
            

            //exitTime时间后，判定效果消失，进入生成文字阶段
            StartCoroutine(DelayToInvoke.DelayToInvokeDo(() =>
            {
                DisableAllEffect();
                GM.ChangeState(WorldState.Generate);
            }, exitTime));

            //DisableAllEffect();
            //GM.ChangeState(WorldState.Generate);
        }
    }

    private void effectDisable(CanvasGroup cg)
    {
        cg.alpha = 0;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }

    private void effectEnable(CanvasGroup cg)
    {
        cg.alpha = 1;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private void DisableAllEffect()
    {
        effectDisable(excellent);
        effectDisable(good);
        effectDisable(moderate);
        effectDisable(poor);
    }
}
