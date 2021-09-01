/* Author: Davide Zhang
 * A script attached to the Oculus Quest left controller anchor (the menu and global editing controller); 
 * This contains logic about mesh rotation, menu toggle, import and export.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xamin;

public class SpaceHandler : MonoBehaviour
{

    public GameObject meshSpace;

    public CircleSelector pieMenu;

    public GameObject rightHandAnchor;

    public GameObject targetMeshObj;

    public GameObject colorPickerGameObj;

    private string gameMode;

    private Quaternion originalOrientation;

    // Start is called before the first frame update
    void Start()
    {
        originalOrientation = meshSpace.transform.rotation;

        pieMenu.controlType = CircleSelector.ControlType.customVector;

        gameMode = "createMode";

        colorPickerGameObj.SetActive(false);

        
    }

    // Update is called once per frame
    void Update()
    {


        //ROTATION
        //rotate entire mesh (including free vertices) when L index trigger is pushed
        if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger))
        {
            //Archived
            //Quaternion offset = Quaternion.Inverse(controllerOrientation) * OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            // controllerOrientation = OVRInput.GetLocalControllerRotation(OVRInput.Controller.LTouch);
            //meshSpace.transform.rotation = meshSpace.transform.rotation * offset;

            meshSpace.transform.SetParent(this.transform, true);


        }

        //OPEN MENU if thumbstick is moved
        if (OVRInput.Get(OVRInput.RawAxis2D.LThumbstick) != Vector2.zero)
        {

            if (!pieMenu.isOpen())
            {
                pieMenu.Open();
            }
            
            pieMenu.CustomInputVector = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);

            


        } else
        //SELECT and CLOSE MENU when thumbstick is released
        {


            if (pieMenu.isOpen())
            {
                //select menu item by pressing thumbstick
                pieMenu.confirmSelect();
                pieMenu.Close();
            }


        }



        //Export mesh when X is pushed
        if (OVRInput.GetDown(OVRInput.RawButton.X))
        {

            string savePath = "C:/Users/Davide/Desktop/export.obj";
            MeshFilter mf = targetMeshObj.GetComponent<MeshFilter>();

            //add UVs
            Vector2[] uvs = UvCalculator.CalculateUVs(mf.mesh.vertices, 1.0f);

            mf.mesh.uv = uvs;

            ObjExporter.MeshToFile(mf, savePath);
        }


        //Import mesh when Y is pushed
        //This should only be called at the beginning of the game (no custom mesh created) for now
        if (OVRInput.GetDown(OVRInput.RawButton.Y))
        {
            string loadPath = "C:/Users/Davide/Desktop/export.obj";
            Mesh m = new Mesh();
            ObjImporter importer = new ObjImporter();
            m = importer.ImportFile(loadPath);

            //DEBUG
            //print("\nLENGTH 1");
            //print((m.vertices.Length).ToString());
            //print((m.triangles.Length).ToString());

            //a hack to correct the doubling of triangles caused by import script
            int[] correctTriangles = new int[m.vertices.Length];
            for (int i = 0; i < m.vertices.Length; i++)
            {
                correctTriangles[i] = i;
            }
            m.triangles = correctTriangles;

            Material mat = new Material(Shader.Find("Custom/VertexColored"));
            if (targetMeshObj.GetComponent<MeshRenderer>() == null) { targetMeshObj.AddComponent<MeshRenderer>(); }
            targetMeshObj.GetComponent<MeshFilter>().mesh = m;
            targetMeshObj.GetComponent<MeshRenderer>().material = mat;

            //scale
            //targetMeshObj.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
            //targetMeshObj.transform.Translate(0, 0, 2f);

            //DEBUG
            //print("\nLENGTH 2");
            //print((m.vertices.Length).ToString());
            //print((m.triangles.Length).ToString());

            //create vertices and related behind-the-scenes data for editing
            rightHandAnchor.GetComponent<MeshHandler>().PrepareImportedMesh();

        }

    }

    public void CreateMode()
    {
        print("SWITCHING TO CREATE MODE");
        string lastMode = gameMode;
        gameMode = "createMode";
        rightHandAnchor.GetComponent<MeshHandler>().ClearSelection(lastMode);
        colorPickerGameObj.SetActive(false);
    }

    public void SculptMode()
    {
        print("SWITCHING TO SCULPT MODE");
        string lastMode = gameMode;
        gameMode = "sculptMode";
        rightHandAnchor.GetComponent<MeshHandler>().ClearSelection(lastMode);
        colorPickerGameObj.SetActive(false);
    }

    public void ColorMode()
    {
        print("SWITCHING TO COLOR MODE");
        string lastMode = gameMode;
        gameMode = "colorMode";
        rightHandAnchor.GetComponent<MeshHandler>().ClearSelection(lastMode);
        colorPickerGameObj.SetActive(true);
    }

    public string GetMode()
    {
        return gameMode;
    }

}
