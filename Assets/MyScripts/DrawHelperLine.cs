using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawHelperLine : MonoBehaviour
{

    public LineRenderer LineRenderer;
    public Transform TransformOther;

    private Vector3 adjustedPos;

    // Start is called before the first frame update
    void Start()
    {
        // set the color of the line
        LineRenderer.startColor = Color.white;
        LineRenderer.endColor = Color.white;

        // set width of the renderer
        LineRenderer.startWidth = 0.0003f;
        LineRenderer.endWidth = 0.0003f;

        



    }

    private void Update()
    {
        adjustedPos = TransformOther.position + new Vector3(0f, 0f, -0.01f);
        // set the position
        LineRenderer.SetPosition(0, transform.position);
        LineRenderer.SetPosition(1, adjustedPos);
    }

}
