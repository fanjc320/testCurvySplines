// =====================================================================
// Copyright ?2013 ToolBuddy
// All rights reserved
// 
// http://www.toolbuddy.net
// =====================================================================

using FluffyUnderware.DevTools;
using JetBrains.Annotations;
using ToolBuddy.Pooling.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace FluffyUnderware.Curvy
{
    /// <summary>
    /// Given a <see cref="CurvySpline"/> and an input position, this class will provide you with the nearest point to that position on the given spline.
    /// The nearest position is provided either in an <see cref="OnUpdated"/> event, or as an override of the position of the <see cref="TargetPosition"/>
    /// </summary>
    /// <remarks>This script simply calls the <see cref="CurvySpline.GetNearestPoint"/> method. If you are a programmer, you don't need to go through this script, just call <see cref="CurvySpline.GetNearestPoint"/></remarks>
    [HelpURL(AssetInformation.DocsRedirectionBaseUrl + "nearestsplinepoint")]
    [AddComponentMenu("Curvy/Misc/Sector Spline Point Fjc")]
    [ExecuteAlways]
    public class SectorSplinePointFjc : DTVersionedMonoBehaviour
    {
        /// <summary>
        /// The <see cref="CurvySpline"/> on which the nearest position is searched for
        /// </summary>
        [Tooltip("The spline on which the nearest position is searched for")]
        public CurvySpline Spline;

        /// <summary>
        /// A transform which position will be used as the input position for the lookup
        /// </summary>
        [Tooltip("A transform which position will be used as the input position for the lookup")]
        public Transform SourcePosition;

        public Vector3 StartPosition;//fjc

        /// <summary>
        /// A transform which position will be updated with the nearest point on Spline to Source Position
        /// </summary>
        //[Tooltip("A transform which position will be updated with the nearest point on Spline to Source Position")]
        //public Transform TargetPosition;

        /// <summary>
        /// When to run the lookup
        /// </summary>
        [Tooltip("When to run the lookup")]
        public CurvyUpdateMethod UpdateIn;

        /// <summary>
        /// At each update, this event is called with the result of the lookup
        /// </summary>
        [Tooltip("At each update, this event is called with the result of the lookup")]
        public UnityEventEx<Vector3> OnUpdated = new UnityEventEx<Vector3>();
        public UnityEvent<Vector3, Vector3> OnUpdatedFjc = new UnityEvent<Vector3, Vector3>();

        public static float tf = 0;

        private void Process()
        {
            if (Spline && Spline.IsInitialized && Spline.Dirty == false)
            {
                tf += 0.023f;
                tf = tf % 1;
                Vector3 endPoint;
                CurvySplineSegment seg = null;
                int result = Spline.GetPointByTF(tf, out endPoint, out seg, Space.World);
                if (result != 0)
                {
                    Debug.Log("Process result is not 0!!!!!");
                }
                //if (TargetPosition)
                //    TargetPosition.position = endPoint;
                OnUpdated?.Invoke(endPoint);
                //OnUpdatedFjc?.Invoke(SourcePosition.position, destinationPosition);

                result = Spline.GetPointByTF(0, out StartPosition, out seg, Space.World);
                //Debug.Log("Process -------");
                OnUpdatedFjc?.Invoke(StartPosition, endPoint);
            }
        }

        #region ### Unity Callbacks ###

#if DOCUMENTATION___FORCE_IGNORE___UNITY == false


        [UsedImplicitly]
        private void Update()
        {
            if (UpdateIn == CurvyUpdateMethod.Update)
                Process();
        }

        [UsedImplicitly]
        private void LateUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.LateUpdate
                || (Application.isPlaying == false
                    && UpdateIn
                    == CurvyUpdateMethod.FixedUpdate)) // In edit mode, fixed updates are not called, so we update here instead
                Process();
        }

        [UsedImplicitly]
        private void FixedUpdate()
        {
            if (UpdateIn == CurvyUpdateMethod.FixedUpdate)
                Process();
        }

#if UNITY_EDITOR
        protected override void OnEnable()
        {
            base.OnEnable();
            EditorApplication.update += OnEditorUpdate;

            //CurvySplineSegment seg = null;
            //int result = Spline.GetPointByTF(0, out StartPosition, out seg, Space.World);
            //Debug.Log("OnEnable StartPosition:" + StartPosition);
        }

        //Ã»Ö´ÐÐ
        //void OnStart()
        //{
        //    CurvySplineSegment seg = null;
        //    int result = Spline.GetPointByTF(0, out StartPosition, out seg, Space.World);
        //    Debug.Log("Onstart StartPosition:" + StartPosition);
        //    if (result != 0)
        //    {
        //        Debug.Log("Onstart GetPointByTF result is not 0!!!!");
        //    }
        //}

        protected override void OnDisable()
        {
            base.OnDisable();
            EditorApplication.update -= OnEditorUpdate;
        }

        private void OnEditorUpdate()
        {
            if (Application.isPlaying == false)
                Process();
        }
#endif
#endif

        #endregion
    }
}