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
    private Vector2 _brushRectangle = new Vector2(100,100);
    private float brushProp = 1.0f;
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
        _brushRectangle.x = size;
        _brushRectangle.y = size * brushProp;
        _brushLerpSize = _brushRectangle.x;
        Debug.Log("_brushRectangle:" + _brushRectangle);
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
	
    //初始化数据
    void InitData()
    {
        _brushRectangle.x = _brushSize;
        float prop = _defaultBrushTex.height / _defaultBrushTex.width;
        _brushRectangle.y = _brushSize * prop;
        _brushLerpSize = _brushRectangle.x;
        _lastPoint = Vector2.zero;

        Debug.Log("PaintView InitData brushWid:" + _defaultBrushTex.width + " brushHei:" + _defaultBrushTex.height + " brushLerpSize:" + _brushLerpSize);

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
        //_paintBrushMat.SetFloat("_Size", _brushSize);
    }

    //插点
    private void LerpPaint(Vector2 point)
    {
        //Paint(point);

        if (_lastPoint == Vector2.zero)
        {
            _lastPoint = point;
            return;
        }

        float angleOfLine = Mathf.Atan2((point.y - _lastPoint.y), (point.x - _lastPoint.x)) * 180 / Mathf.PI;
        //Quaternion rot = Quaternion.Euler(0, 0, angleOfLine);
        Quaternion rotReverse = Quaternion.Euler(0, 0, -angleOfLine);

        Vector2 pointUV = new Vector2(point.x/_screenWidth, point.y/_screenHeight);
        Matrix4x4 matScaleRecover = Matrix4x4.Scale(new Vector3(_screenWidth, _screenHeight));
        //Vector2 pointRecover = matScaleRecover.MultiplyPoint3x4(pointUV);
        //Debug.Log("Paint BrushWid:" + brushWid + " brushHei:" + brushHei + " angleOfLine:" + angleOfLine + " pointUV:" + pointUV +" pointRecover:" + pointRecover);

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

        Vector3 brushScale = new Vector3(1.0f / _brushRectangle.x, 1.0f /_brushRectangle.y, 1);
        Matrix4x4 matScale = Matrix4x4.Scale(brushScale);
        Vector3 scaledPt = matScale.MultiplyPoint3x4(rotatedPt);

        Matrix4x4 mat4 = matScale * matRot * matTranslate;
        Vector3 uvPt = mat4.MultiplyPoint3x4(point);

        Matrix4x4 matFromUV = matScale * matRot * matTranslate * matScaleRecover;
        Vector3 uvPoint = matFromUV.MultiplyPoint3x4(pointUV);
        _paintBrushMat.SetMatrix("_Matrix", matFromUV);

        //三个结果都是(0.5,0),说明是对的
        Debug.Log("Paint point:" + point + " originPt:" + originPt + " traslatedPt:" + traslatedPt + " rotatedPt:" + rotatedPt + " scaledPt:" + scaledPt + " uvPt" + uvPt + " uvPoint:" + uvPoint);
        //Paint(point);

        float dis = Vector2.Distance(point, _lastPoint);
        if (dis > _brushLerpSize)
        {
            Vector2 dir = (point - _lastPoint).normalized;
            int num = (int)(dis / _brushLerpSize);
            for (int i = 0; i < num; i++)
            {
                Vector2 newPoint = _lastPoint + dir * (i + 1) * _brushLerpSize;
                //Paint(newPoint);
                Graphics.Blit(_renderTex, _canvasTex, _paintBrushMat);
                _lastPoint = point;
            }
        }
        //Debug.Log("PaintView LerpPaint brushLerpSize:" + _brushLerpSize + " dis:" + dis + " point:" + point + " _lastPoint:" + _lastPoint);
    }

    //画点
    private void Paint(Vector2 point)
    {
        if (point.x < 0 || point.x > _screenWidth || point.y < 0 || point.y > _screenHeight)
            return;

        Graphics.Blit(_renderTex, _canvasTex, _paintBrushMat);
        //Graphics.Blit(_canvasTex, _canvasTex, _paintBrushMat);
    }

    }

//C#中计算两点之间连线的角度
//https://www.cnblogs.com/ImNo1/p/4619123.html
//https://blog.csdn.net/qq_42194657/article/details/125871576