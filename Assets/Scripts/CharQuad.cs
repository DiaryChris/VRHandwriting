using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharQuad : MonoBehaviour
{
    public float speed = 1;
    public float endZ = 0;
    public float exitTime;

    private bool isMove;


    private void Start()
    {
        isMove = true;
    }

    private void Update()
    {
        //Debug.Log("count: " + count);
        if(isMove)
        {
            transform.Translate(0, 0, speed * Time.deltaTime);
        }
        if(transform.position.z < endZ)
        {
            if (isMove)
            {
                GameManager.Instance.PlayEffect(0);   //播放提示音效
                GameManager.Instance.ChangeState(WorldState.DiscriminateBefore);
                Debug.Log("CharQuad: " + exitTime);
                StartCoroutine(DelayToInvoke.DelayToInvokeDo(() => { Destroy(gameObject); }, exitTime));
            }
                
            isMove = false;
            //Destroy(gameObject);         
        }
    }
}
