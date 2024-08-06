using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMatrix : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MultiplyPoint();
        //MultiplyVector();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public bool isPoint;
    public bool inverse;

    public Transform orgCoor;	// 原始坐标系
    public Transform newCoor;	// 新坐标系

    public Transform tranOrg;	// 原始位置
    public Transform tranNew;	// 转换后的位置

    /// <summary>
    /// 转点
    /// </summary>
    private void MultiplyPoint()
    {
        //Debug.Log("MultiplyPoint");
        //Matrix4x4 matrix4X4 = Matrix4x4.TRS(newCoor.position, newCoor.rotation, newCoor.localScale);
        Matrix4x4 matrix4X4 = Matrix4x4.TRS(newCoor.position, newCoor.rotation, new Vector3(4,4,4));

        if (!inverse)
        {
            //老坐标系下的位置、旋转、缩放转换到新坐标系下。
            tranNew.position = matrix4X4.MultiplyPoint(tranOrg.position);
            //tranNew.rotation = matrix4X4.rotation * tranOrg.rotation;
            //tranNew.localScale = matrix4X4.lossyScale();//老坐标系下的位置和旋转转换到新坐标系下。
        }
        else
            tranNew.position = matrix4X4.inverse.MultiplyPoint(tranOrg.position); // 这里的 position 依然是相对于世界空间坐标系(orgCoor)来说的

        //Vector4 a = matrix4X4.inverse * (tranOrg.position);
    }

    /// <summary>
    /// 转向量
    /// </summary>
    private void MultiplyVector()
    {
        Matrix4x4 matrix4X4 = Matrix4x4.TRS(newCoor.position, newCoor.rotation, newCoor.localScale);

        if (!inverse)
        {
            tranNew.position = matrix4X4.MultiplyVector(tranOrg.position - orgCoor.position) + newCoor.position;
            tranNew.rotation = matrix4X4.rotation * tranOrg.rotation;
        }
        else
        {
            tranNew.position = matrix4X4.inverse.MultiplyVector(tranOrg.position - newCoor.position) + orgCoor.position;
            tranNew.rotation = matrix4X4.inverse.rotation * tranOrg.rotation;   // 四元数相乘 改朝向
        }
    }

    /// <summary>
    /// 球面上的地面侦察范围Mesh创建(带碰撞)
    /// </summary>
    /// <param name="trans">球面上某点的Transform</param>
    /// <param name="radius">侦察半径</param>
    /// <param name="layer">层级</param>
    /// <returns></returns>
    public static Mesh CreateAntDetectionRange(Transform trans, float radius, int layer, int upHeight = 20)
    {
        // 此处涉及2个空间坐标系，分别为 unity世界空间坐标系、物体的模型空间坐标系
        Matrix4x4 matrix4X4 = Matrix4x4.TRS(trans.position, trans.rotation, trans.localScale);  // 构造从 unity世界空间坐标系 到 物体的模型空间坐标系 矩阵
        Vector3 orgPoint = trans.position;
        Vector3 upPoint = matrix4X4.MultiplyPoint(Vector3.up * upHeight);   // 中间点的 unity世界坐标点

        //List<Vector3> vertices = new List<Vector3> { Vector3.up * upHeight };
        List<Vector3> vertices = new List<Vector3>();

        for (int i = 0; i <= 360; i += 3)
        {
            Vector3 endPosSelf = Quaternion.AngleAxis(i, Vector3.up) * Vector3.forward * radius;    // 模型空间中的位置信息
            Vector3 endPos = matrix4X4.MultiplyPoint(endPosSelf);   // 模型空间相对位置信息 转化为 世界空间位置信息
            if (Physics.Raycast(upPoint, (endPos - upPoint).normalized, out RaycastHit hit, radius, layer))
            {
                endPos = hit.point;
                Debug.DrawLine(upPoint, endPos, Color.yellow);
            }
            else
            {
                Debug.DrawLine(upPoint, endPos, Color.red);
            }

            // //Debug.DrawLine(Vector3.up * upHeight + orgPoint, Quaternion.AngleAxis(i, Vector3.up) * Vector3.forward * radius + orgPoint, Color.green);


            //Vector3 direction = matrix4X4.MultiplyVector(Quaternion.AngleAxis(i, Vector3.up) * Vector3.forward * radius - Vector3.up * upHeight);  // 自身射线方向 转到 世界空间射线方向
            //Vector3 endPos = upPoint + direction;
            //if (Physics.Raycast(upPoint, direction.normalized, out RaycastHit hit, radius, layer))
            //{
            //    endPos = hit.point;
            //    Debug.DrawLine(upPoint, endPos, Color.yellow);
            //}
            //else
            //{
            //    Debug.DrawLine(upPoint, endPos, Color.red);
            //}
            vertices.Add(matrix4X4.inverse.MultiplyPoint(endPos));
        }
        return CreatePolygonMesh(matrix4X4.inverse.MultiplyPoint(upPoint), vertices.ToArray());
    }
    /// <summary>
    /// 简单多边形网格(单中心点连边)
    /// </summary>
    /// <param name="center">中点</param>
    /// <param name="points">边</param>
    /// <returns></returns>
    public static Mesh CreatePolygonMesh(Vector3 center, Vector3[] points, bool coverEnd = true)
    {
        List<Vector3> vertices = new List<Vector3>();
        vertices.Add(center);
        vertices.AddRange(points);
        List<int> triangles = new List<int>();
        for (int i = 1; i < vertices.Count; i++)
        {
            if (i == vertices.Count - 1)
            {
                if (coverEnd)
                {
                    triangles.Add(0);
                    triangles.Add(i);
                    triangles.Add(1);
                }
            }
            else
            {
                triangles.Add(0);
                triangles.Add(i);
                triangles.Add(i + 1);
            }
        }

        Mesh mesh = new Mesh()
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray()
        };
        //mesh.RecalculateNormals();

        return mesh;
    }

}
//https://blog.csdn.net/yizhe0731/article/details/108410116