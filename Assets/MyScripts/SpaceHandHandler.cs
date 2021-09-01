using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xamin;
using UniRx;

public class SpaceHandHandler : MonoBehaviour
{

    public Hand handRecognizer;
    public OVRHand OVRHand;

    public GameObject meshSpace;

    public CircleSelector pieMenu;

    public MeshHandHandler meshHandHandler;

    public GameObject targetMeshObj;

    public GameObject colorPickerGameObj;

    private string gameMode;

    // Start is called before the first frame update
    void Start()
    {

        pieMenu.controlType = CircleSelector.ControlType.customVector;

        gameMode = "createMode";

        colorPickerGameObj.SetActive(false);

        //Subscribe to grasp to orient mesh
        handRecognizer.OnStartGrasping.Subscribe(_ => {
            meshSpace.transform.SetParent(OVRHand.transform, true);
        });

        //Subscribe to open grasp to update mesh
        handRecognizer.OnStopGrasping.Subscribe(_ => {
            meshHandHandler.UpdateOrientedMesh();
        });

        //Subscrive to index pinch to select createMode
        handRecognizer.OnPinchStarted(OVRHand.HandFinger.Index).Subscribe(_ => {
            if (!pieMenu.isOpen())
            {
                pieMenu.Open();
            }
            pieMenu.CustomInputVector = new Vector2(0.5f, 0.5f);
        });

        //Close menu
        handRecognizer.OnPinchEnded(OVRHand.HandFinger.Index).Subscribe(_ => {
            if (pieMenu.isOpen())
            {
                if (gameMode != "createMode")
                {
                    pieMenu.confirmSelect();
                }
                
                pieMenu.Close();
            }
        });

        //Subscrive to index pinch to select colorMode
        handRecognizer.OnPinchStarted(OVRHand.HandFinger.Middle).Subscribe(_ => {
            if (!pieMenu.isOpen())
            {
                pieMenu.Open();
            }
            pieMenu.CustomInputVector = new Vector2(-0.5f, 0.5f);
        });

        //Close menu
        handRecognizer.OnPinchEnded(OVRHand.HandFinger.Middle).Subscribe(_ => {
            if (pieMenu.isOpen())
            {
                if (gameMode != "colorMode")
                {
                    pieMenu.confirmSelect();
                }
                pieMenu.Close();
            }
        });

        //Subscrive to index pinch to select sculptMode
        handRecognizer.OnPinchStarted(OVRHand.HandFinger.Ring).Subscribe(_ => {
            if (!pieMenu.isOpen())
            {
                pieMenu.Open();
            }
            pieMenu.CustomInputVector = new Vector2(0f, -1f);
        });

        //Close menu
        handRecognizer.OnPinchEnded(OVRHand.HandFinger.Ring).Subscribe(_ => {
            if (pieMenu.isOpen())
            {
                if (gameMode != "sculptMode")
                {
                    pieMenu.confirmSelect();
                }
                pieMenu.Close();
            }
        });


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateMode()
    {
        print("SWITCHING TO CREATE MODE");
        string lastMode = gameMode;
        gameMode = "createMode";
        meshHandHandler.ClearSelection(lastMode);
        colorPickerGameObj.SetActive(false);
    }

    public void SculptMode()
    {
        print("SWITCHING TO SCULPT MODE");
        string lastMode = gameMode;
        gameMode = "sculptMode";
        meshHandHandler.ClearSelection(lastMode);
        colorPickerGameObj.SetActive(false);
    }

    public void ColorMode()
    {
        print("SWITCHING TO COLOR MODE");
        string lastMode = gameMode;
        gameMode = "colorMode";
        meshHandHandler.ClearSelection(lastMode);
        colorPickerGameObj.SetActive(true);
    }

    public string GetMode()
    {
        return gameMode;
    }

}
