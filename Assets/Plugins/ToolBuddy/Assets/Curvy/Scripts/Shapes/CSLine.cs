// =====================================================================
// Copyright ?2013 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using FluffyUnderware.DevTools;
using UnityEngine;

namespace FluffyUnderware.Curvy.Shapes
{
    /// <summary>
    /// Circle Shape (2D)
    /// </summary>
    [CurvyShapeInfo("2D/Line")]
    [RequireComponent(typeof(CurvySpline))]
    [AddComponentMenu("Curvy/Shapes/Line")]
    public class CSLine : CurvyShape2D
    {
        private const int MinCount = 2;

        [Positive(
            MinValue = MinCount,
            Tooltip = "Number of Control Points"
        )]
        [SerializeField]
        private int m_Count = 2;

        [Positive]
        [SerializeField]
        private Vector3 m_StartPt = Vector3.zero;
        [SerializeField]
        private Vector3 m_EndPt = Vector3.zero + new Vector3(1,0,0);

        public void setPoint(Vector3 startPt, Vector3 endPt)
        {
             m_StartPt = startPt;
             m_EndPt = endPt;
        }


        protected override void ApplyShape()
        {
            base.ApplyShape();
            PrepareSpline(CurvyInterpolation.Bezier);
            PrepareControlPoints(m_Count);

            Spline.ControlPointsList[0].transform.localPosition = m_StartPt;
            Spline.ControlPointsList[1].transform.localPosition = m_EndPt;

        }
    }
}