using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PenController : MonoBehaviour
{
    public RawImage raw;
    public GameObject pen;
    public GameObject penParent;
    public GameObject penController;
    public float threshold = 0.05f;  //距离超过阈值开始吸附
    public float distance = 0.02f;  //吸附到与平面距离为distance的位置处
    float[] pos = new float[4];
    float posZ;

    void Start()
    {
        Vector3[] v = new Vector3[4];
        raw.GetComponent<RectTransform>().GetWorldCorners(v);   //从左下角开始逆时针依次存储四个角的坐标
        pos[0] = v[0].x;    //x中的较小值
        pos[1] = v[3].y;    //y中的较小值
        pos[2] = v[2].x;    //x中的较大值
        pos[3] = v[1].y;    //y中的较大值
        posZ = v[0].z;

        //Debug.Log(posZ);

        //Vector3 localPos = pen.transform.localPosition;
        //Vector3 worldPos = penParent.transform.TransformPoint(localPos);

        //Debug.Log("localPos: " + localPos + "worldPos: " + worldPos);
    }


    void Update()
    {
        //Vector3 localPos = pen.transform.localPosition;
        //Vector3 worldPos = penParent.transform.TransformPoint(localPos);
        //Debug.Log("localPos: " + localPos + "\t worldPos: " + worldPos + "\t pos: " + transform.position + "\t ParentPos: " + penParent.transform.position);
        if (transform.position.x >= pos[0] && transform.position.x <= pos[2] && transform.position.y >= pos[1] && transform.position.y <= pos[3])
        {
            //Debug.Log(transform.position);
            //Debug.Log("在绘制面板的正前方或者正后方");
            if (System.Math.Abs(transform.position.z - posZ) <= threshold)
            {
                //Debug.Log("开始吸附");
                //if(transform.position.z < posZ)
                //    pen.transform.position = new Vector3(pen.transform.position.x, pen.transform.position.y, posZ - distance);
                //else
                //    pen.transform.position = new Vector3(pen.transform.position.x, pen.transform.position.y, posZ - distance);
                Vector3 before = pen.transform.position;
                
                pen.transform.position = new Vector3(pen.transform.position.x, pen.transform.position.y, posZ - distance);  //默认笔的z轴为负数
                Vector3 after = pen.transform.position;
                if(before!=after)
                    Debug.Log("吸附前： " + pen.transform.position.ToString("F6") + "  吸附后： " + pen.transform.position.ToString("F6"));
            }
            else
                pen.transform.position = penController.transform.position;
        }
        else
            pen.transform.position = penController.transform.position;
    }
}
