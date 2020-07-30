using UnityEngine;


public class Brush : MonoBehaviour
{

    //笔头的骨骼节点，childBone[0]为不移动的根节点，后一个节点均为前一个节点的子节点
    public Transform[] childBone;
    //毛笔运动速度阈值
    public float threshold = 1;
    //骨骼节点本地竖直轴
    public Vector3 boneVerticalAxis;
    //骨骼偏移乘法因子
    public float factor = 0.15f;
    //趋近速度
    public float approachSpeed = 10;
    //键盘毛笔移动速度
    public float speed = 1;
    //停止运动是否保留毛笔形态
    public bool keep = true;


    //骨骼初始位置
    private Vector3[] initLocalPos;
    private Vector3 lastPos;
    private Vector3 curPos;
    private Vector3 moveDir;


    private void Start()
    {

        lastPos = transform.position;
        curPos = transform.position;
        initLocalPos = new Vector3[childBone.Length];
        for (int i = 0; i < initLocalPos.Length; i++)
        {
            initLocalPos[i] = childBone[i].localPosition;
        }
    }

    private void Update()
    {
        //float h = Input.GetAxis("Horizontal");
        //float v = Input.GetAxis("Vertical");
        //transform.Translate(new Vector3(h, v, 0) * speed * Time.deltaTime);
    }

    private void LateUpdate()
    {
        handleBrushBone();
    }

    private void handleBrushBone()
    {
        lastPos = curPos;
        curPos = transform.position;
        moveDir = curPos - lastPos;

        Vector3 targetPos;
        Vector3 moveDirLocal;
        Vector3 moveDirLocalHorizontal;
        //若要保留形态，则仅在运动速度超过阈值时处理
        if (!(keep && moveDir.magnitude / Time.deltaTime < threshold))
        {
            //毛笔形态仅与移动方向有关，与移动距离无关
            moveDir = moveDir.normalized;
            for (int i = 0; i < childBone.Length; i++)
            {
                //从世界坐标系转换到本地坐标系
                moveDirLocal = childBone[i].InverseTransformDirection(moveDir);
                moveDirLocalHorizontal = Vector3.ProjectOnPlane(moveDirLocal, boneVerticalAxis);
                //每个节点最终移动距离为等差数列前n项和Sn = 0.5 * (n * n + n），为二次函数
                targetPos = initLocalPos[i] - moveDirLocalHorizontal * i * factor;
                //每帧趋近目标位置
                childBone[i].localPosition = Vector3.Lerp(childBone[i].localPosition, targetPos, Time.deltaTime * approachSpeed);
            }
        }
    }
}