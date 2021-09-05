using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawHelperLine : MonoBehaviour
{

    public LineRenderer LineRenderer;
    public Transform TransformOther;

    // Start is called before the first frame update
    void Start()
    {
        // set the color of the line
        LineRenderer.startColor = Color.white;
        LineRenderer.endColor = Color.white;

        // set width of the renderer
        LineRenderer.startWidth = 0.0005f;
        LineRenderer.endWidth = 0.0005f;

        
    }

    private void Update()
    {
        // set the position
        LineRenderer.SetPosition(0, transform.position);
        LineRenderer.SetPosition(1, TransformOther.position);
    }

}
