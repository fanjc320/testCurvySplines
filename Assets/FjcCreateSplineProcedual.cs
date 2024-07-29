using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FluffyUnderware.Curvy;
using FluffyUnderware.Curvy.Shapes;
using FluffyUnderware.Curvy.Generator;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class FjcCreateSplineProcedual : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       
    }

    public void onGetPoint(Vector3 start, Vector3 end)
    {
        CreateLine(start, end);
    }
    //public Component Create(CGModule cgModule, string context)
    public Component CreateLine(Vector3 startPt, Vector3 endPt)
    {
        CurvySpline spl = CurvySpline.Create();
        spl.transform.parent = transform;
        spl.name = "fjcLine";
        spl.GizmoColor = Color.green;
        spl.transform.position = Vector3.zero;
        spl.RestrictTo2D = true;
        spl.Closed = true;
        spl.Orientation = CurvyOrientation.None;
        spl.gameObject.AddComponent<CSLine>().setPoint(startPt, endPt);
        spl.gameObject.GetComponent<CSLine>().Refresh();
        return spl;
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
