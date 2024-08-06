using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixDemo : MonoBehaviour
{
    public Matrix4x4 matrix1;
    // public Matrix4x4 matrixRotate;
    public Vector4 MovePoint;


    public Vector3 RotateData;
    public Vector3 ScaleData = Vector3.one;
    // Start is called before the first frame update
    void Start()
    {
        //这里是赋值整个变换矩阵  M=T*R*S
        //matrix1.SetTRS(transform.position, transform.rotation, transform.localScale);
        matrix1.SetTRS(transform.position, Quaternion.identity, new Vector3(10,10,10));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnMatrix4x4Move();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnMatrix4x4Rotate();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnMatrix4x4Scale();
        }
    }
    /// <summary>
    /// 矩阵平移,第四列是标记坐标信息的
    /// </summary>
    public void OnMatrix4x4Move()
    {
        Vector4 v4 = new Vector4(0, 0, 0, 1);

        matrix1[0, 3] = MovePoint.x;
        matrix1[1, 3] = MovePoint.y;
        matrix1[2, 3] = MovePoint.z;

        v4 = matrix1 * v4;
        transform.position = new Vector3(v4.x, v4.y, v4.z);
    }
    /// <summary>
    /// 矩阵旋转，这里是按本地旋转来的,例如先转x轴30再转Y，这里Y是基于X30度去转的
    /// </summary>
    public void OnMatrix4x4Rotate()
    {
        Matrix4x4 matrixRotateX = Matrix4x4.identity;
        Matrix4x4 matrixRotateY = Matrix4x4.identity;
        Matrix4x4 matrixRotateZ = Matrix4x4.identity;
        Matrix4x4 matrixRotate = Matrix4x4.identity;
        //  if (RotateData.x != 0)
        {
            matrixRotateX[1, 1] = Mathf.Cos(RotateData.x * Mathf.Deg2Rad);
            matrixRotateX[2, 1] = Mathf.Sin(RotateData.x * Mathf.Deg2Rad);
            matrixRotateX[1, 2] = -Mathf.Sin(RotateData.x * Mathf.Deg2Rad);
            matrixRotateX[2, 2] = Mathf.Cos(RotateData.x * Mathf.Deg2Rad);
        }
        // if (RotateData.y != 0)
        {
            matrixRotateY[0, 0] = Mathf.Cos(RotateData.y * Mathf.Deg2Rad);
            matrixRotateY[2, 0] = -Mathf.Sin(RotateData.y * Mathf.Deg2Rad);
            matrixRotateY[0, 2] = Mathf.Sin(RotateData.y * Mathf.Deg2Rad);
            matrixRotateY[2, 2] = Mathf.Cos(RotateData.y * Mathf.Deg2Rad);
        }
        //  if (RotateData.z != 0)
        {
            matrixRotateZ[0, 0] = Mathf.Cos(RotateData.z * Mathf.Deg2Rad);
            matrixRotateZ[1, 0] = Mathf.Sin(RotateData.z * Mathf.Deg2Rad);
            matrixRotateZ[0, 1] = -Mathf.Sin(RotateData.z * Mathf.Deg2Rad);
            matrixRotateZ[1, 1] = Mathf.Cos(RotateData.z * Mathf.Deg2Rad);
        }
        matrixRotate = matrixRotateX * matrixRotateY * matrixRotateZ;

        float vW = Mathf.Sqrt(matrixRotate.m00 + matrixRotate.m11 + matrixRotate.m22 + 1) / 2;
        float w = vW * 4;
        float vX = (matrixRotate.m21 - matrixRotate.m12) / w;
        float vY = (matrixRotate.m02 - matrixRotate.m20) / w;
        float vZ = (matrixRotate.m10 - matrixRotate.m01) / w;

        transform.rotation = new Quaternion(vX, vY, vZ, vW);


    }
    /// <summary>
    /// 矩阵缩放
    /// </summary>
    public void OnMatrix4x4Scale()
    {
        Matrix4x4 matrixScale = Matrix4x4.identity;
        Vector4 v4 = new Vector4(1, 1, 1, 1);

        matrixScale.m00 = ScaleData.x;
        matrixScale.m11 = ScaleData.y;
        matrixScale.m22 = ScaleData.z;

        v4 = matrixScale * v4;
        transform.localScale = new Vector3(v4.x, v4.y, v4.z);
    }
}