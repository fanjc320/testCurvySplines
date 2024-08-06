using System.Collections;
using System.Collections.Generic;
using ToolBuddy.Pooling.Collections;
using UnityEngine;

public class TestMatrixTRS : MonoBehaviour
{
    // Translate, rotate and scale a mesh. Try altering
    // the parameters in the inspector while running
    // to see the effect they have.

    public Vector3 translation;
    public Vector3 eulerAngles;
    public Vector3 scale = new Vector3(1, 2, 1);


    MeshFilter mf;
    Vector3[] origVerts;
    Vector3[] newVerts;


    void Start()
    {
        // Get the Mesh Filter component, save its original vertices
        // and make a new vertex array for processing.
        mf = GetComponent<MeshFilter>();
        Debug.Log(mf);
        origVerts = mf.mesh.vertices;
        newVerts = new Vector3[origVerts.Length];
        Debug.Log(newVerts.Length);
    }


    void Update()
    {
        // Set a Quaternion from the specified Euler angles.
        Quaternion rotation = Quaternion.Euler(eulerAngles.x, eulerAngles.y, eulerAngles.z);
        //Debug.Log(rotation);

        // Set the translation, rotation and scale parameters.
        Matrix4x4 m = Matrix4x4.TRS(translation, rotation, scale);

        // For each vertex...
        for (int i = 0; i < origVerts.Length; i++)
        {
            // Apply the matrix to the vertex.
            newVerts[i] = m.MultiplyPoint3x4(origVerts[i]);
        }

        // Copy the transformed vertices back to the mesh.
        mf.mesh.vertices = newVerts;
    }

    void TestTRS()
    {
        Matrix4x4 m4 = Matrix4x4.identity;
        //trs这是针对点的，要想让整个笔刷，即矩形整体旋转缩放，需要分别对矩形四个点都运算。
        m4 = Matrix4x4.TRS(new Vector3(10, 10, 0), Quaternion.identity, Vector2.one);
        Matrix4x4 m_matrixBurshUV = Matrix4x4.TRS(new Vector3(10, 10, 0), Quaternion.identity, Vector2.one / 2);//发现是首先scale，然后translate的
        //m_matrixBurshUV = Matrix4x4.TRS(translate, rot, brushScale);
        Vector2 testUV = m4.MultiplyPoint3x4(new Vector3(10, 10, 0));
        Vector3 newUV = m_matrixBurshUV.MultiplyPoint3x4(new Vector3(10, 10, 0));
        Debug.Log("PaintView LerpPaint testUV:" + testUV + " newUV:" + newUV);

    }

    /// <summary>
    /// 重映射  默认  value 为1-100
    /// </summary>
    /// <param name="value"></param>
    /// <param name="maxValue"></param>
    /// <param name="minValue"></param>
    /// <returns></returns>
    private float Remap(float value, float startValue, float enValue)
    {
        float returnValue = (value - 1.0f) / (100.0f - 1.0f);
        returnValue = (enValue - startValue) * returnValue + startValue;
        return returnValue;
    }

    private void TransformToWorldSpace(SubArray<Vector3> localSpaceVectors)
    {
        Matrix4x4 matrix = transform.localToWorldMatrix;
        Vector3[] resultArray = localSpaceVectors.Array;
        for (int i = 0; i < localSpaceVectors.Count; i++)
            resultArray[i] = matrix.MultiplyPoint3x4(resultArray[i]);
    }

    private void TestMatrix()
    {
        //Vector2 point = new Vector2(200, 200);
        //Vector2 testPoint = new Vector2(270.7f, 200);
        //Vector2 lastPoint = new Vector2(50, 50);
        //_brushSize = 100;
        //_screenWidth = 400;
        //_screenHeight = 600;
        //float brushWid = _brushSize / _screenWidth;
        //float brushHei = _brushSize / _screenHeight;

        //float angleOfLine = Mathf.Atan2((point.y - lastPoint.y), (point.x - lastPoint.x)) * 180 / Mathf.PI;
        //Quaternion rot = Quaternion.Euler(0, 0, angleOfLine);
        //Quaternion rotReverse = Quaternion.Euler(0, 0, -angleOfLine);
        ////Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
        //// 将四元数转换为矩阵
        //Matrix4x4 matrix1 = Matrix4x4.Rotate(rotReverse);
        //// 应用矩阵变换到世界坐标
        //Vector2 leftPoint = new Vector2(_brushSize / 2, _brushSize / 2) - point;

        //Vector3 newPosition = matrix1.MultiplyPoint(testPoint);
        //Vector3 newPosition1 = newPosition.Addition(leftPoint);
        ////Vector3 newPosition = Matrix4x4.Translate();
        //Debug.Log("Paint TestMatrix:" + brushWid + " brushHei:" + brushHei + " angleOfLine:" + angleOfLine + " leftPoint:" + leftPoint + " newPos:" + newPosition + " newPos1:" + newPosition1);


        //Matrix4x4 m4 = Matrix4x4.identity;
        //Vector3 scale = new Vector3(_brushSize / _screenWidth, _brushSize / _screenHeight, 1);
        //m4.SetTRS(new Vector3(-point.x, -point.y), rotReverse, scale);
        //_paintBrushMat.SetMatrix("_BrushRotation", m4);

    }
}