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

    public Transform orgCoor;	// ԭʼ����ϵ
    public Transform newCoor;	// ������ϵ

    public Transform tranOrg;	// ԭʼλ��
    public Transform tranNew;	// ת�����λ��

    /// <summary>
    /// ת��
    /// </summary>
    private void MultiplyPoint()
    {
        //Debug.Log("MultiplyPoint");
        //Matrix4x4 matrix4X4 = Matrix4x4.TRS(newCoor.position, newCoor.rotation, newCoor.localScale);
        Matrix4x4 matrix4X4 = Matrix4x4.TRS(newCoor.position, newCoor.rotation, new Vector3(4,4,4));

        if (!inverse)
        {
            //������ϵ�µ�λ�á���ת������ת����������ϵ�¡�
            tranNew.position = matrix4X4.MultiplyPoint(tranOrg.position);
            //tranNew.rotation = matrix4X4.rotation * tranOrg.rotation;
            //tranNew.localScale = matrix4X4.lossyScale();//������ϵ�µ�λ�ú���תת����������ϵ�¡�
        }
        else
            tranNew.position = matrix4X4.inverse.MultiplyPoint(tranOrg.position); // ����� position ��Ȼ�����������ռ�����ϵ(orgCoor)��˵��

        //Vector4 a = matrix4X4.inverse * (tranOrg.position);
    }

    /// <summary>
    /// ת����
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
            tranNew.rotation = matrix4X4.inverse.rotation * tranOrg.rotation;   // ��Ԫ����� �ĳ���
        }
    }

    /// <summary>
    /// �����ϵĵ�����췶ΧMesh����(����ײ)
    /// </summary>
    /// <param name="trans">������ĳ���Transform</param>
    /// <param name="radius">���뾶</param>
    /// <param name="layer">�㼶</param>
    /// <returns></returns>
    public static Mesh CreateAntDetectionRange(Transform trans, float radius, int layer, int upHeight = 20)
    {
        // �˴��漰2���ռ�����ϵ���ֱ�Ϊ unity����ռ�����ϵ�������ģ�Ϳռ�����ϵ
        Matrix4x4 matrix4X4 = Matrix4x4.TRS(trans.position, trans.rotation, trans.localScale);  // ����� unity����ռ�����ϵ �� �����ģ�Ϳռ�����ϵ ����
        Vector3 orgPoint = trans.position;
        Vector3 upPoint = matrix4X4.MultiplyPoint(Vector3.up * upHeight);   // �м��� unity���������

        //List<Vector3> vertices = new List<Vector3> { Vector3.up * upHeight };
        List<Vector3> vertices = new List<Vector3>();

        for (int i = 0; i <= 360; i += 3)
        {
            Vector3 endPosSelf = Quaternion.AngleAxis(i, Vector3.up) * Vector3.forward * radius;    // ģ�Ϳռ��е�λ����Ϣ
            Vector3 endPos = matrix4X4.MultiplyPoint(endPosSelf);   // ģ�Ϳռ����λ����Ϣ ת��Ϊ ����ռ�λ����Ϣ
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


            //Vector3 direction = matrix4X4.MultiplyVector(Quaternion.AngleAxis(i, Vector3.up) * Vector3.forward * radius - Vector3.up * upHeight);  // �������߷��� ת�� ����ռ����߷���
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
    /// �򵥶��������(�����ĵ�����)
    /// </summary>
    /// <param name="center">�е�</param>
    /// <param name="points">��</param>
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