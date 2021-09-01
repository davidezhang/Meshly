using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerTrigger : MonoBehaviour
{

    public MeshHandHandler meshHandHandler;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        meshHandHandler.FingerTriggerEnter(other, transform.position);
    }

    private void OnTriggerStay(Collider other)
    {
        meshHandHandler.FingerTriggerStay(other, transform.position);
    }

    private void OnTriggerExit(Collider other)
    {
        meshHandHandler.FingerTriggerExit(other);
    }
}
