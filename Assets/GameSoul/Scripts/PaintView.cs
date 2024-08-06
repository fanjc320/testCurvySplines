//-----------------------------------------------------------------------
// <copyright file="PaintView.cs" company="Codingworks Game Development">
//     Copyright (c) codingworks. All rights reserved.
// </copyright>
// <author> codingworks </author>
// <email> coding2233@163.com </email>
// <time> 2017-12-10 </time>
//-----------------------------------------------------------------------

using FluffyUnderware.Curvy.Utils;
using System.Drawing;
using ToolBuddy.Pooling.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Color = UnityEngine.Color;
using Image = UnityEngine.UI.Image;
using Slider = UnityEngine.UI.Slider;

public class PaintView : MonoBehaviour
{
    #region 属性

    //绘图shader&material
    [SerializeField]
    private Shader _paintBrushShader;
    private Material _paintBrushMat;
    //清理renderTexture的shader&material
    [SerializeField]
    private Shader _clearBrushShader;
    private Material _clearBrushMat;
    //默认笔刷RawImage
    [SerializeField]
    private RawImage _defaultBrushRawImage;
    //默认笔刷&笔刷合集
    [SerializeField]
    private Texture _defaultBrushTex;
    //renderTexture
    private RenderTexture _renderTex;
    private RenderTexture _canvasTex;
    //默认笔刷RawImage
    [SerializeField]
    private Image _defaultColorImage;
    //绘画的画布
    [SerializeField]
    private RawImage _paintCanvas;
    //笔刷的默认颜色&颜色合集
    [SerializeField]
    private Color _defaultColor;
    //笔刷大小的slider
    private Text _brushSizeText;
    //笔刷的大小
    private float _brushSize = 100;
    public Vector2 _brushRectangle = new Vector2(100,100);
    //屏幕的宽高
    public int _screenWidth;
    public int _screenHeight;
    //笔刷的间隔大小
    private float _brushLerpSize;
    //默认上一次点的位置
    private Vector2 _lastPoint;

    public Matrix4x4 matrix = Matrix4x4.identity;
    Matrix4x4 m_matrixBurshUV = Matrix4x4.identity;
    public enum Axle { X, Y, Z }
    public GameObject oldGo;
    public GameObject go;

    #endregion

    void Start()
	{
		InitData();
	}

	private void Update()
	{
		Color clearColor = new Color(0, 0, 0, 0);
		if (Input.GetKeyDown(KeyCode.Space))
			_paintBrushMat.SetColor("_Color", clearColor);
	}


	#region 外部接口

	public void SetBrushSize(float size)
    {
       _brushSize = size;
       _paintBrushMat.SetFloat("_Size", _brushSize);
        Debug.Log("setBrushSize:" + size);
    }

    public void SetBrushTexture(Texture texture)
    {
        _defaultBrushTex = texture;
        _paintBrushMat.SetTexture("_BrushTex", _defaultBrushTex);
        _defaultBrushRawImage.texture = _defaultBrushTex;
    }

    public void SetBrushColor(UnityEngine.Color color)
    {
        _defaultColor = color;
        _paintBrushMat.SetColor("_Color", _defaultColor);
        _defaultColorImage.color = _defaultColor;
    }
    /// <summary>
    /// 选择颜色
    /// </summary>
    /// <param name="image"></param>
    public void SelectColor(Image image)
    {
        SetBrushColor(image.color);
    }
    /// <summary>
    /// 选择笔刷
    /// </summary>
    /// <param name="rawImage"></param>
    public void SelectBrush(RawImage rawImage)
    {
        SetBrushTexture(rawImage.texture);
    }
    /// <summary>
    /// 设置笔刷大小
    /// </summary>
    /// <param name="value"></param>
    public void BrushSizeChanged(Slider slider)
    {
      //  float value = slider.maxValue + slider.minValue - slider.value;
        //SetBrushSize(Remap(slider.value,300.0f,30.0f));
        SetBrushSize(slider.value);
        if (_brushSizeText == null)
        {
            _brushSizeText=slider.transform.Find("Background/Text").GetComponent<Text>();
        }
        _brushSizeText.text = slider.value.ToString("f2");

        TestMatrix();
    }

    /// <summary>
    /// 拖拽
    /// </summary>
    public void DragUpdate()
    {
        if (_renderTex && _paintBrushMat)
        {

            if (Input.GetMouseButton(0))
                LerpPaint(Input.mousePosition);

           
        }
    }
    /// <summary>
    /// 拖拽结束
    /// </summary>
    public void DragEnd()
    {
        if (Input.GetMouseButtonUp(0))
            _lastPoint = Vector2.zero;
    }

    #endregion

    #region 内部函数
	
    //初始化数据
    void InitData()
    {
        //_brushSize = 100.0f;
        //_brushLerpSize = (_defaultBrushTex.width + _defaultBrushTex.height) / 2.0f / _brushSize;
        _brushLerpSize = (_defaultBrushTex.width + _defaultBrushTex.height) / 2.0f / 100 * _brushSize;
        _lastPoint = Vector2.zero;

        Debug.Log("PaintView InitData brushWid:" + _defaultBrushTex.width + " brushHei:" + _defaultBrushTex.height + " brushSize:" + _brushSize + " brushLerpSize:" + _brushLerpSize);

        if (_paintBrushMat == null)
        {
            UpdateBrushMaterial();
        }
        if(_clearBrushMat==null)
        _clearBrushMat = new Material(_clearBrushShader);
        if (_renderTex == null)
        {
            _screenWidth = Screen.width;
            _screenHeight = Screen.height;

            _renderTex = RenderTexture.GetTemporary(_screenWidth, _screenHeight, 24);
            //_paintCanvas.texture = _renderTex;
            _canvasTex = RenderTexture.GetTemporary(_screenWidth, _screenHeight, 24);
            _paintCanvas.texture = _canvasTex;
        }
        Graphics.Blit(null, _renderTex, _clearBrushMat);

        Debug.Log("PaintView InitData brushLerpSize:" + _brushLerpSize + " wid:" + _screenWidth + " hei:" + _screenHeight);
    }

    //更新笔刷材质
    private void UpdateBrushMaterial()
    {
        _paintBrushMat = new Material(_paintBrushShader);
        _paintBrushMat.SetTexture("_BrushTex", _defaultBrushTex);
        _paintBrushMat.SetColor("_Color", _defaultColor);
        _paintBrushMat.SetFloat("_Size", _brushSize);
    }

    //插点
    private void LerpPaint(Vector2 point)
    {
        Paint(point);

        if (_lastPoint == Vector2.zero)
        {
            _lastPoint = point;
            return;
        }

        float brushWid = _brushSize / _screenWidth;
        float brushHei = _brushSize / _screenHeight;
        _paintBrushMat.SetFloat("_BrushUVWidth", brushWid);
        _paintBrushMat.SetFloat("_BrushUVHeight", brushHei);

        float angleOfLine = Mathf.Atan2((point.y - _lastPoint.y), (point.x - _lastPoint.x)) * 180 / Mathf.PI;
        Quaternion rot = Quaternion.Euler(0, 0, angleOfLine);
        Quaternion rotReverse = Quaternion.Euler(0, 0, -angleOfLine);

        Vector2 pointUV = new Vector2(point.x/_screenWidth, point.y/_screenHeight);
        Matrix4x4 matScaleRecover = Matrix4x4.Scale(new Vector3(_screenWidth, _screenHeight));
        Vector2 pointRecover = matScaleRecover.MultiplyPoint3x4(pointUV);
        Debug.Log("Paint BrushWid:" + brushWid + " brushHei:" + brushHei + " angleOfLine:" + angleOfLine + " pointUV:" + pointUV +" pointRecover:" + pointRecover);

        Vector2 brushDir = (point - _lastPoint).normalized;
        //bursh坐标的起点
        Vector2 originPt = new Vector2(point.x - brushDir.x * _brushRectangle.x / 2, point.y - brushDir.y * _brushRectangle.y / 2);
        //Vector2 traslatedPt = point - originPt;
        Matrix4x4 matTranslate = Matrix4x4.Translate(-originPt);
        Vector2 traslatedPt = matTranslate.MultiplyPoint3x4(point);
        //Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
        // 将四元数转换为矩阵
        Matrix4x4 matRot = Matrix4x4.Rotate(rotReverse);
        Vector3 rotatedPt = matRot.MultiplyPoint3x4(traslatedPt);
        //Vector3 rotatedPt = matTranslate.Rotate(rotReverse);

        //Vector3 brushScale = new Vector3(_brushRectangle.x / _screenWidth, _brushRectangle.y / _screenHeight, 1);
        Vector3 brushScale = new Vector3(1.0f / _brushRectangle.x, 1.0f /_brushRectangle.y, 1);
        //Vector3 brushScale = new Vector3(1.0f / _screenWidth, 1.0f / _screenHeight, 1);
        Matrix4x4 matScale = Matrix4x4.Scale(brushScale);
        Vector3 scaledPt = matScale.MultiplyPoint3x4(rotatedPt);

        Matrix4x4 mat4 = matScale * matRot * matTranslate;
        Vector3 uvPt = mat4.MultiplyPoint3x4(point);
        //_paintBrushMat.SetMatrix("_Matrix", mat4);

        Matrix4x4 matFromUV = matScale * matRot * matTranslate * matScaleRecover;
        Vector3 uvPoint = matFromUV.MultiplyPoint3x4(pointUV);
        _paintBrushMat.SetMatrix("_Matrix", matFromUV);

        //三个结果都是(0.5,0),说明是对的
        Debug.Log("Paint point:" + point + " originPt:" + originPt + " traslatedPt:" + traslatedPt + " rotatedPt:" + rotatedPt + " scaledPt:" + scaledPt + " uvPt" + uvPt + " uvPoint:" + uvPoint);



        //Vector2 translate = Vector2.zero - originPt;
        //Matrix4x4 m4 = Matrix4x4.identity;
        //m4 = Matrix4x4.TRS(new Vector3(10, 10, 0), Quaternion.identity, Vector2.one);
        //m_matrixBurshUV = Matrix4x4.TRS(new Vector3(10,10,0), Quaternion.identity, Vector2.one/2);//发现是首先scale，然后translate的
        ////m_matrixBurshUV = Matrix4x4.TRS(translate, rot, brushScale);
        //Vector2 testUV = m4.MultiplyPoint3x4(new Vector3(10,10,0));
        //Vector3 newUV = m_matrixBurshUV.MultiplyPoint3x4(new Vector3(10, 10, 0));
        //_paintBrushMat.SetVector("_BrushUV", new Vector2(newUV.x, newUV.y));
        //Debug.Log("PaintView LerpPaint testUV:" + testUV + " newUV:" + newUV);
        //Debug.Log("PaintView LerpPaint point:"+ point+ " originPt:"+ originPt + " translate:" + translate + " brushScale:" + brushScale +" newUV:" + newUV);

        //Vector3 newPt = m4.MultiplyPoint3x4(point);
        //oldGo.transform.position = new Vector3(point.x, point.y, 0);
        //go.transform.position = newPt;
        //go.transform.rotation = rot;
        //go.transform.localScale = scale;


        float dis = Vector2.Distance(point, _lastPoint);
        if (dis > _brushLerpSize)
        {
            Vector2 dir = (point - _lastPoint).normalized;
            int num = (int)(dis / _brushLerpSize);
            for (int i = 0; i < num; i++)
            {
                Vector2 newPoint = _lastPoint + dir * (i + 1) * _brushLerpSize;
                Paint(newPoint);
            }
        }
        //Debug.Log("PaintView LerpPaint brushLerpSize:" + _brushLerpSize + " dis:" + dis + " point:" + point + " _lastPoint:" + _lastPoint);
        _lastPoint = point;
    }

    //画点
    private void Paint(Vector2 point)
    {
        if (point.x < 0 || point.x > _screenWidth || point.y < 0 || point.y > _screenHeight)
            return;

        Vector2 uv = new Vector2(point.x / (float)_screenWidth,
            point.y / (float)_screenHeight);
        //_paintBrushMat.SetVector("_UV", uv);

        //float brushWid = _brushSize / _screenWidth;
        //float brushHei = _brushSize / _screenHeight;
        //_paintBrushMat.SetFloat("_BrushUVWidth", brushWid);
        //_paintBrushMat.SetFloat("_BrushUVHeight", brushHei);
        //Graphics.Blit(_renderTex, _renderTex, _paintBrushMat);
        Graphics.Blit(_renderTex, _canvasTex, _paintBrushMat);
    }

    void TestTRS()
    {
        Matrix4x4 m4 = Matrix4x4.identity;
        //trs这是针对点的，要想让整个笔刷，即矩形整体旋转缩放，需要分别对矩形四个点都运算。
        m4 = Matrix4x4.TRS(new Vector3(10, 10, 0), Quaternion.identity, Vector2.one);
        m_matrixBurshUV = Matrix4x4.TRS(new Vector3(10, 10, 0), Quaternion.identity, Vector2.one / 2);//发现是首先scale，然后translate的
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

    #endregion
    private void TransformToWorldSpace(SubArray<Vector3> localSpaceVectors)
    {
        Matrix4x4 matrix = transform.localToWorldMatrix;
        Vector3[] resultArray = localSpaceVectors.Array;
        for (int i = 0; i < localSpaceVectors.Count; i++)
            resultArray[i] = matrix.MultiplyPoint3x4(resultArray[i]);
    }

    private void TestMatrix()
    {
        Vector2 point = new Vector2(200,200);
        Vector2 testPoint = new Vector2(270.7f,200);
        Vector2 lastPoint = new Vector2(50,50);
        _brushSize = 100;
        _screenWidth = 400;
        _screenHeight = 600;
        float brushWid = _brushSize / _screenWidth;
        float brushHei = _brushSize / _screenHeight;

        float angleOfLine = Mathf.Atan2((point.y - lastPoint.y), (point.x - lastPoint.x)) * 180 / Mathf.PI;
        Quaternion rot = Quaternion.Euler(0, 0, angleOfLine);
        Quaternion rotReverse = Quaternion.Euler(0, 0, -angleOfLine);
        //Matrix4x4 m = Matrix4x4.TRS(Vector3.zero, rot, Vector3.one);
        // 将四元数转换为矩阵
        Matrix4x4 matrix1 = Matrix4x4.Rotate(rotReverse);
        // 应用矩阵变换到世界坐标
        Vector2 leftPoint = new Vector2(_brushSize/2, _brushSize/2) - point;
        
        Vector3 newPosition = matrix1.MultiplyPoint(testPoint);
        Vector3 newPosition1 = newPosition.Addition(leftPoint);
        //Vector3 newPosition = Matrix4x4.Translate();
        Debug.Log("Paint TestMatrix:" + brushWid + " brushHei:" + brushHei + " angleOfLine:" + angleOfLine + " leftPoint:" + leftPoint + " newPos:" + newPosition + " newPos1:" + newPosition1);


        Matrix4x4 m4 = Matrix4x4.identity;
        Vector3 scale = new Vector3(_brushSize / _screenWidth, _brushSize / _screenHeight, 1);
        m4.SetTRS(new Vector3(-point.x, -point.y), rotReverse, scale);
        _paintBrushMat.SetMatrix("_BrushRotation", m4);

    }

    /// <summary>
    /// 平移矩阵对象
    /// </summary>
    /// <param name="pos"></param>
    public Matrix4x4 MyTranslate(Vector3 pos)
    {
        //按照当前位置进行位移
        //v = new Vector4(transform.position.x,transform.position.y,transform.position.z,1);

        //按照原点进行位移
        matrix = Matrix4x4.identity;

        //X、Y、Z移动因子
        matrix.m03 = pos.x;
        matrix.m13 = pos.y;
        matrix.m23 = pos.z;

        return matrix;
    }

    /// <summary>
    /// 缩放矩阵对象
    /// </summary>
    /// <param name="scale"></param>
    public Matrix4x4 MyScale(Vector3 scale)
    {
        //设置当前对象大小
        //v = new Vector4(transform.localScale.x, transform.localScale.y, transform.localScale.z, 1);

        //按照原点进行位移
        //matrix = Matrix4x4.identity;

        //X、Y、Z缩放因子
        matrix.m00 = scale.x;
        matrix.m11 = scale.y;
        matrix.m22 = scale.z;

        return matrix;
    }
    public Matrix4x4 MyRotation(Axle axle, float angle)
    {
        //matrix = Matrix4x4.identity;

        //对应 X、Y、Z的旋转
        switch (axle)
        {
            case Axle.X:
                matrix.m11 = Mathf.Cos(angle * Mathf.Deg2Rad);
                matrix.m12 = -Mathf.Sin(angle * Mathf.Deg2Rad);
                matrix.m21 = Mathf.Sin(angle * Mathf.Deg2Rad);
                matrix.m22 = Mathf.Cos(angle * Mathf.Deg2Rad);
                break;
            case Axle.Y:
                matrix.m00 = Mathf.Cos(angle * Mathf.Deg2Rad);
                matrix.m02 = Mathf.Sin(angle * Mathf.Deg2Rad);
                matrix.m20 = -Mathf.Sin(angle * Mathf.Deg2Rad);
                matrix.m22 = Mathf.Cos(angle * Mathf.Deg2Rad);
                break;
            case Axle.Z:
                matrix.m00 = Mathf.Cos(angle * Mathf.Deg2Rad);
                matrix.m01 = -Mathf.Sin(angle * Mathf.Deg2Rad);
                matrix.m10 = Mathf.Sin(angle * Mathf.Deg2Rad);
                matrix.m11 = Mathf.Cos(angle * Mathf.Deg2Rad);
                break;
            default:
                break;
        }

        float qw = Mathf.Sqrt(1f + matrix.m00 + matrix.m11 + matrix.m22) / 2;
        float w = 4 * qw;
        float qx = (matrix.m21 - matrix.m12) / w;
        float qy = (matrix.m02 - matrix.m20) / w;
        float qz = (matrix.m10 - matrix.m01) / w;

        //transform.rotation = new Quaternion(qx, qy, qz, qw);
        return matrix;
    }

    }

//C#中计算两点之间连线的角度
//https://www.cnblogs.com/ImNo1/p/4619123.html
//https://blog.csdn.net/qq_42194657/article/details/125871576