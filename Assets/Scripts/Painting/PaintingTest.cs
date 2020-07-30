using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/*
 * 还需修改的地方：
 * 1、根据各种坐标绘图，让画笔粗细合适(解决方案：固定纸张大小）
 * 2、结合VR操作进行画图（如必须得用手捡起来笔才可以写字）
 * 3、一些设定的添加（如纸的背面不能写字）（解决方案：通过坐标来计算）
 * 4、当射线射到背后cube上时，point的计算有问题，需要重新计算
 * 5、笔刷粗细（太粗）
 */

public class PaintingTest : MonoBehaviour, IStateObserver
{
    private GameManager GM;

    //public RawImage test;

    private RenderTexture texRender;   //画布
    public Material mat;     //给定的shader新建材质
    public Texture brushTypeTexture;   //画笔纹理，半透明
    private bool isPainting;    //是否可以写字
    private Camera mainCamera;
    private float brushScale = 0.5f;
    private float disFromPapaer = 0.001f;
    public Color brushColor = Color.black;
    public RawImage raw;                   //使用UGUI的RawImage显示，方便进行添加UI,将pivot设为(0.5,0.5)
    private float lastDistance;
    private Vector3[] PositionArray = new Vector3[4];
    private int posIndex = 0;
    private float[] speedArray = new float[4];
    private int speedIndex = 0;
    public int num = 50;
    private int texWeight = 1024;       //绘制的Texture宽度
    private int texHeight = 1024;       //绘制的Texture高度

    public float maxDistance = 0.045f;       //画笔距离画板超过该值则不能画画

    Vector2 rawMousePosition;            //raw图片的左下角对应鼠标位置
    float rawWidth;                               //raw图片宽度
    float rawHeight;                              //raw图片长度

    void Awake()
    {
        GetRawImageInfo(raw.GetComponent<RectTransform>());

        //添加GameManager
        GM = GameManager.Instance;
        GM.AddObserver(this);
    }

    void Update()
    {
        /*需要结合VR更改逻辑*/
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -transform.forward, out hit))
        {
            //print("Found an object - distance: " + hit.distance);
            //Debug.Log("Colider: " + hit.collider.name + ", point: " + hit.point + ", transform: " + hit.transform);
            Debug.DrawRay(transform.position, -transform.forward, Color.green);
            if (hit.distance < maxDistance)
            {
                if(isPainting)
                {
                    if (hit.collider.name == "PaintingCanvas")
                        OnDrawPenMove(new Vector3(hit.point.x, hit.point.y, 0), hit.distance);
                    else
                    {
                        Vector3 pos = CalcPoint(transform.position, hit.point);
                        OnDrawPenMove(pos, disFromPapaer);
                    }
                }  
            }
            else
                OnDrawPenUp();
        }
        else
        {
            Debug.DrawRay(transform.position, -transform.forward, Color.green);
            //Debug.Log("Not hit.");
            OnDrawPenUp();
        }
        
        DrawImage();
    }


    #region Painting

    Vector3 startPosition = Vector3.zero;
    Vector3 endPosition = Vector3.zero;

    /// <summary>
    /// 清除字迹
    /// </summary>
    public void Clear()
    {
        Graphics.SetRenderTarget(texRender);
        GL.PushMatrix();
        //GL.Clear(true, true, Color.white);
        GL.Clear(true, true, new Color(1.0f, 1.0f, 1.0f, 0.0f));
        GL.PopMatrix();
        
    }

    /// <summary>
    /// 绘画
    /// </summary>
    /// <param name="pos">画笔位置</param>
    void OnDrawPenMove(Vector3 pos, float dis)
    {
        if (startPosition == Vector3.zero)
        {
            startPosition = pos;
        }

        endPosition = pos;
        float distance = Vector3.Distance(startPosition, endPosition);
        brushScale = SetScale(distance, dis);
        ThreeOrderBézierCurse(pos, distance, 0.005f, dis);

        startPosition = endPosition;
        lastDistance = distance;
    }

    /// <summary>
    /// 绘画结束
    /// </summary>
    void OnDrawPenUp()
    {
        startPosition = Vector3.zero;
        posIndex = 0;
        speedIndex = 0;
    }

    /// <summary>
    /// 将绘制好的Texture复制到Image上
    /// </summary>
    void DrawImage()
    {
        raw.texture = texRender;
        //test.texture = GetRTPixels(texRender);
    }

    /// <summary>
    /// 将RenderTexture转换为Texture2D
    /// </summary>
    /// <param name="rt"></param>
    /// <returns></returns>
    Texture2D toTexture2D(RenderTexture rt)
    {
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = rt;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(rt.width, rt.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        // Restorie previously active render texture
        RenderTexture.active = currentActiveRT;
        tex.Apply();
        return tex;
    }

    /// <summary>
    /// 缩放Texture2D的大小
    /// </summary>
    /// <param name="source"></param>
    /// <param name="targetWidth"></param>
    /// <param name="targetHeight"></param>
    /// <returns></returns>
    Texture2D ScaleTexture(Texture2D source, float targetWidth, float targetHeight)
    {
        Texture2D result = new Texture2D((int)targetWidth, (int)targetHeight, source.format, false);

        for (int i = 0; i < result.height; ++i)
        {
            for (int j = 0; j < result.width; ++j)
            {
                Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                result.SetPixel(j, i, newColor);
            }
        }

        result.Apply();
        return result;
    }

    #endregion


    #region help functions

    /// <summary>
    /// 获取RawImage的信息
    /// </summary>
    /// <param name="rt"></param>
    void GetRawImageInfo(RectTransform rt)
    {
        Vector3[] v = new Vector3[4];
        rt.GetWorldCorners(v);
        rawMousePosition = new Vector2(v[0].x, v[0].y);
        rawWidth = v[3].x - v[0].x;
        rawHeight = v[1].y - v[0].y;
        //texRender = new RenderTexture((int)rawWidth, (int)rawHeight, 24, RenderTextureFormat.ARGB32);
        texRender = new RenderTexture(texWeight, texHeight, 24, RenderTextureFormat.ARGB32);
        Clear();
    }

    /// <summary>
    /// 根据长度设置画笔宽度
    /// </summary>
    /// <param name="distance">两点之间的距离</param>
    /// <param name="dis">笔尖与纸面的距离</param>
    /// <returns></returns>
    float SetScale(float distance, float dis)
    {
        float Scale = 0;
        if (distance < 100)
        {
            Scale = 0.8f - 0.005f * distance;
        }
        else
        {
            Scale = 0.425f - 0.00125f * distance;
        }
        if (Scale <= 0.05f)
        {
            Scale = 0.05f;
        }
        return (Scale - dis * 5) * 1.5f;
    }

    /// <summary>
    /// 三阶贝塞尔曲线，获取连续4个点坐标，通过调整中间2点坐标，画出部分（我使用了num/1.5实现画出部分曲线）来使曲线平滑;通过速度控制曲线宽度。
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="distance"></param>
    /// <param name="targetPosOffset"></param>
    private void ThreeOrderBézierCurse(Vector3 pos, float distance, float targetPosOffset, float dis)
    {
        //记录坐标
        PositionArray[posIndex] = pos;
        posIndex++;
        //记录速度
        speedArray[speedIndex] = distance;
        speedIndex++;
        if (posIndex == 4)
        {
            Vector3 temp1 = PositionArray[1];
            Vector3 temp2 = PositionArray[2];

            //修改中间两点坐标（？为啥）
            Vector3 middle = (PositionArray[0] + PositionArray[2]) / 2;
            PositionArray[1] = (PositionArray[1] - middle) * 1.5f + middle;
            middle = (temp1 + PositionArray[3]) / 2;
            PositionArray[2] = (PositionArray[2] - middle) * 2.1f + middle;

            //（为什么需要除以1.5f）
            for (int index1 = 0; index1 < num; index1++)
            {
                float t1 = (1.0f / num) * index1;
                //三阶贝塞尔曲线公式
                Vector3 target = Mathf.Pow(1 - t1, 3) * PositionArray[0] +
                                 3 * PositionArray[1] * t1 * Mathf.Pow(1 - t1, 2) +
                                 3 * PositionArray[2] * t1 * t1 * (1 - t1) + PositionArray[3] * Mathf.Pow(t1, 3);
                //float deltaspeed = (float)(distance - lastDistance) / num;
                //获取速度差值（存在问题，参考）
                float deltaspeed = (float)(speedArray[3] - speedArray[0]) / num;
                //float randomOffset = Random.Range(-1/(speedArray[0] + (deltaspeed * index1)), 1 / (speedArray[0] + (deltaspeed * index1)));
                //模拟毛刺效果
                float randomOffset = Random.Range(-targetPosOffset, targetPosOffset);
                DrawBrush(texRender, target.x + randomOffset, target.y + randomOffset, brushTypeTexture, brushColor, SetScale(speedArray[0] + (deltaspeed * index1), dis));
            }

            PositionArray[0] = temp1;
            PositionArray[1] = temp2;
            PositionArray[2] = PositionArray[3];

            speedArray[0] = speedArray[1];
            speedArray[1] = speedArray[2];
            speedArray[2] = speedArray[3];
            posIndex = 3;
            speedIndex = 3;
        }
        else
        {
            DrawBrush(texRender, (int)endPosition.x, (int)endPosition.y, brushTypeTexture,
                brushColor, brushScale);
        }

    }

    /// <summary>
    /// 画笔工具
    /// </summary>
    /// <param name="destTexture"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="sourceTexture"></param>
    /// <param name="color"></param>
    /// <param name="scale"></param>
    void DrawBrush(RenderTexture destTexture, float x, float y, Texture sourceTexture, Color color, float scale)
    {
        DrawBrush(destTexture, new Rect(x, y, sourceTexture.width, sourceTexture.height), sourceTexture, color, scale);
    }

    /// <summary>
    /// 画笔工具
    /// </summary>
    /// <param name="destTexture"></param>
    /// <param name="destRect"></param>
    /// <param name="sourceTexture"></param>
    /// <param name="color"></param>
    /// <param name="scale"></param>
    void DrawBrush(RenderTexture destTexture, Rect destRect, Texture sourceTexture, Color color, float scale)
    {

        //增加鼠标位置根据raw图片位置换算。
        //float left = (destRect.xMin - rawMousePosition.x) - destRect.width * scale / 2.0f;
        //float right = (destRect.xMin - rawMousePosition.x) + destRect.width * scale / 2.0f;
        //float top = (destRect.yMin - rawMousePosition.y) - destRect.height * scale / 2.0f;
        //float bottom = (destRect.yMin - rawMousePosition.y) + destRect.height * scale / 2.0f;

        float left = (destRect.xMin - rawMousePosition.x) * texWeight / rawWidth - destRect.width * scale / 2.0f;
        float right = (destRect.xMin - rawMousePosition.x) * texWeight / rawWidth + destRect.width * scale / 2.0f;
        float top = (destRect.yMin - rawMousePosition.y) * texHeight / rawHeight - destRect.height * scale / 2.0f;
        float bottom = (destRect.yMin - rawMousePosition.y) * texHeight / rawHeight + destRect.height * scale / 2.0f;

        //float left = (destRect.xMin - rawMousePosition.x) - destRect.width * scale / 2.0f;
        //float right = (destRect.xMin - rawMousePosition.x) + destRect.width * scale / 2.0f;
        //float top = (destRect.yMin - rawMousePosition.y) - destRect.height * scale / 2.0f;
        //float bottom = (destRect.yMin - rawMousePosition.y) + destRect.height * scale / 2.0f;

        //Debug.Log("x: " + destRect.x + ", y: " + destRect.y + ", left: " + left + ", right: " + right + ", top: " + top + ", bottom: " + bottom);

        Graphics.SetRenderTarget(destTexture);

        GL.PushMatrix();
        GL.LoadOrtho();

        mat.SetTexture("_MainTex", brushTypeTexture);
        mat.SetColor("_Color", color);
        mat.SetPass(0);

        GL.Begin(GL.QUADS);

        //GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(left, top, 0);
        //GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(right, top, 0);
        //GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(right, bottom, 0);
        //GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(left, bottom, 0);

        GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(left / texWeight, top / texHeight, 0);
        GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(right / texWeight, top / texHeight, 0);
        GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(right / texWeight, bottom / texHeight, 0);
        GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(left / texWeight, bottom / texHeight, 0);

        //GL.TexCoord2(0.0f, 0.0f); GL.Vertex3(left / rawWidth, top / rawHeight, 0);
        //GL.TexCoord2(1.0f, 0.0f); GL.Vertex3(right / rawWidth, top / rawHeight, 0);
        //GL.TexCoord2(1.0f, 1.0f); GL.Vertex3(right / rawWidth, bottom / rawHeight, 0);
        //GL.TexCoord2(0.0f, 1.0f); GL.Vertex3(left / rawWidth, bottom / rawHeight, 0);

        GL.End();
        GL.PopMatrix();
    }

    /// <summary>
    /// 将RenderTexture转换成Texture2D格式
    /// </summary>
    /// <param name="rt"></param>
    /// <returns></returns>
    public Texture2D GetRTPixels(RenderTexture rt)
    {
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = rt;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(256, 256);
        Debug.Log(rt.width + "/" + rt.height);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

        // Restorie previously active render texture
        RenderTexture.active = currentActiveRT;
        tex.Apply();
        //test.texture = tex;
        return tex;
    }

    /// <summary>
    /// 将Texture2D保存在本地
    /// </summary>
    /// <param name="path"></param>
    /// <param name="texture2D"></param>
    private void SavePNG(string path, Texture2D texture2D)
    {
        byte[] bytes = texture2D.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    /// 通过线段的两端，求中间某个点的坐标
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private Vector3 CalcPoint(Vector3 a, Vector3 b)
    {
        float rate = (-a.z) / (b.z - a.z);
        float x = rate * (b.x - a.x) + a.x;
        float y = rate * (b.y - a.y) + a.y;
        Vector3 res = new Vector3(x, y, 0);
        Debug.Log("a: " + a + "\tb: " + b + "\tc: " + res);
        return res;
    }
    #endregion


    public void OnStateChange(WorldState state)
    {
        if (state == WorldState.DiscriminateBefore)
        {
            //将写的字保存到List：charTexUser中
            Texture2D testTex = toTexture2D(texRender);
            testTex = ScaleTexture(testTex, 256, 256);
            GM.charTexUser.Add(testTex);
            SavePNG(Application.dataPath + "/CharImages/User/" + (GM.charIndex - 1) + ".png", testTex);
            GM.ChangeState(WorldState.Discriminate);
        }
        else if (state == WorldState.GenerateEnd)
        {
            //开始生成新字的时候清空写字板
            GM.PlayEffect(1);   //播放擦除音效
            Clear();
        }
        else if(state == WorldState.Generate)
        {
            isPainting = true;
        }
        else if(state == WorldState.Discriminate)
        {
            isPainting = false;
        }
    }


}
