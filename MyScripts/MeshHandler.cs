/* Author: Davide Zhang
 * A script attached to the Oculus Quest right controller anchor (the local editing controller); 
 * This contains logic about creating, selecting, deleting, and modifying mesh.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MeshHandler : MonoBehaviour
{
    public Transform trackingSpace;
    public GameObject vertexBall;
    public GameObject targetMeshObj;

    public GameObject meshSpace;

    public GameObject leftHandAnchor;

    public GameObject colorPickerGameObj;

    private Vector3 RControllerPos;
    private GameObject vertClone;

    //baked mesh data
    private List<Vector3> vertices;
    private List<int> triangleIndices;

    //auxiliary mesh vertex gameObjects for latest position and rotation
    private List<GameObject> verticesGameObjects;

    private List<Color32> meshColors;

    //selected data
    private List<GameObject> selectedVertices;
    private List<int> selectedTriangleIndices;

    private int vertexCounter;
    private Stack<int> indexStack;


    private Mesh mesh;

    private GameObject highlighted;

 


    //For initialization
    private void Awake()
    {

        vertices = new List<Vector3>();
        triangleIndices = new List<int>();

        verticesGameObjects = new List<GameObject>();
        meshColors = new List<Color32>();

        selectedVertices = new List<GameObject>();
        selectedTriangleIndices = new List<int>();

        vertexCounter = 0;

        indexStack = new Stack<int>();


        highlighted = null;

    }

    // Start is called before the first frame update
    void Start()
    {
        vertexBall.SetActive(false);

        GetMesh();
    }

    // Update is called once per frame
    void Update()
    {
        //////APPLIES TO ALL 3 MODES//////
        //update mesh orientation when L trigger is released
        if (OVRInput.GetUp(OVRInput.RawButton.LIndexTrigger))
        {
            meshSpace.transform.SetParent(null, true);

            //deselect all before creating/updating mesh so as to not create new mesh in this orientation operation
            GameObject[] selected = GameObject.FindGameObjectsWithTag("selected");
            for (int i = 0; i < selected.Length; i++)
            {
                Deselect(selected[i]);
            }

            CreateMesh();

            //DEBUG
            //print("\nBAKED");
            //foreach (Vector3 v in mesh.vertices)
            //{
            //    print(transform.localToWorldMatrix.MultiplyPoint3x4(v).ToString());
            //}
            //print("\nGameObjects");
            //foreach (GameObject go in verticesGameObjects)
            //{
            //    print(transform.TransformPoint(go.transform.position).ToString());
            //}

        }





        //get controller position and convert it to world space from local space
        RControllerPos = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));


        //////CREATE MODE//////
        //if R controller index button is pushed
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) && GetGameMode() == "createMode")
        {
            //CREATE VERTEX if nothing is highlighted
            if (highlighted == null)
            {
                vertClone = Instantiate(vertexBall);
                vertClone.transform.position = RControllerPos;
                vertClone.SetActive(true);

                //move it under meshSpace gameObject for L controller manipulation
                vertClone.transform.SetParent(meshSpace.transform);

            }
            else
            {
                //select or deselect if vertex is highlighted
                if (highlighted.CompareTag("vertex") && GameObject.FindGameObjectsWithTag("selected").Length < 3)
                {
                    //SELECT vertex  
                    highlighted.tag = "selected";
                    highlighted.transform.GetComponent<Renderer>().material.color = Color.red;

                    if (indexStack.Count > 0)
                    {
                        int availableIndex = indexStack.Pop();
                        selectedVertices.Insert(availableIndex, highlighted);
                        selectedTriangleIndices.Insert(availableIndex, availableIndex);
                    }
                    else
                    {
                        selectedVertices.Add(highlighted);
                        selectedTriangleIndices.Add(vertexCounter);
                    }

                    //increase total vertex counter
                    vertexCounter++;

                }
                else if (highlighted.CompareTag("selected"))
                {
                    Deselect(highlighted);
                }



            }

        }

        //////CREATE MODE//////
        //MAKE TRIANGLE for selected vertices
        if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger) && GetGameMode() == "createMode")
        {
            CreateMesh();

            GameObject[] selected = GameObject.FindGameObjectsWithTag("selected");

            //deselect all and make those vertices
            for (int i = 0; i < selected.Length; i++)
            {
                selected[i].tag = "vertex";
                selected[i].transform.GetComponent<Renderer>().material.color = Color.white;
            }

            //clear selected vertices and indices
            selectedVertices.Clear();
            selectedTriangleIndices.Clear();
        }

        //////CREATE MODE//////
        //delete selected vertices
        if (OVRInput.GetDown(OVRInput.RawButton.A))
        {
            DeleteSelectedVertices();
        }


        //////SCULPT MODE//////
        //if R controller index trigger is pushed
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) && GetGameMode() == "sculptMode")
        {
            //SELECT vertex or vertices
            if (highlighted.CompareTag("vertex"))
            {
                //SELECT vertex  
                highlighted.tag = "sculptSelected";
                highlighted.transform.GetComponent<Renderer>().material.color = Color.blue;
            }
            //deselect
            else if (highlighted.CompareTag("sculptSelected"))
            {
                highlighted.tag = "vertex";
                highlighted.transform.GetComponent<Renderer>().material.color = Color.white;
            }
        }

        //////SCULPT MODE//////
        //translate selected vertices when index trigger is held
        if (OVRInput.Get(OVRInput.RawButton.RHandTrigger) && GetGameMode() == "sculptMode")
        {
            GameObject[] sculptSelected = GameObject.FindGameObjectsWithTag("sculptSelected");

            for (int i = 0; i < sculptSelected.Length; i++)
            {
                //change parent to right controller to follow translation
                sculptSelected[i].transform.SetParent(this.transform, true);

            }
        }

        //////SCULPT MODE//////
        //update mesh when vertex movement has ended (regardless if mesh vertex or outside vertex is moved)
        if (OVRInput.GetUp(OVRInput.RawButton.RHandTrigger) && GetGameMode() == "sculptMode")
        {
            GameObject[] sculptSelected = GameObject.FindGameObjectsWithTag("sculptSelected");

            for (int i = 0; i < sculptSelected.Length; i++)
            {
                //change parent back to meshspace to finish dragging
                sculptSelected[i].transform.SetParent(meshSpace.transform, true);
                
                //deselect every one
                sculptSelected[i].tag = "vertex";
                sculptSelected[i].transform.GetComponent<Renderer>().material.color = Color.white;

            }

            //DEBUG
            //print("\nHIGHLIGHTED NULL?");
            //print(highlighted);

            //make sure highlighted is null to avoid coloring bug
            highlighted = null;

            //update mesh
            CreateMesh();

        }

        //////COLOR MODE//////
        //Color vertex
        if (OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) && GetGameMode() == "colorMode")
        {
            for (int i = 0; i < verticesGameObjects.Count; i++)
            {
                if (verticesGameObjects[i] == highlighted)
                {
                    //DEBUG
                    print("HIGHLIGHT STILL THERE");
                    
                    Color32[] colors = new Color32[mesh.vertices.Length];
                    

                    if (mesh.colors32.Length == 0)
                    {
                        for (int j = 0; j < colors.Length; j++) 
                        {
                            colors[j] = Color.white;
                        }
                    } else
                    {
                        colors = mesh.colors32;
                    }

                    colors[i] = colorPickerGameObj.GetComponent<ColorPickerTriangle>().TheColor;
                    mesh.colors32 = colors;
                    meshColors = colors.ToList();
                    
                    //DEBUG
                    //print("\nMESH COLOR COUNT 1");
                    //print(meshColors.Count.ToString());
                }
            }
        }



    }

    private void OnTriggerEnter(Collider other)
    {

        //highlight when controller touches vertex
        if (other.gameObject.tag == "vertex" || other.gameObject.tag == "selected")
        {
           
            other.transform.GetComponent<Renderer>().material.color = Color.yellow;
            highlighted = other.gameObject;


        }

        if (other.gameObject.tag == "colorTriangle")
        {
            
            Vector3 collisionPt = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            colorPickerGameObj.GetComponent<ColorPickerTriangle>().HasIntersectionVRTri(collisionPt);
        }





    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "colorCircle" && OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && GetGameMode() == "colorMode")
        {

            Vector3 collisionPt = other.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            colorPickerGameObj.GetComponent<ColorPickerTriangle>().HasIntersectionVRCircle(collisionPt);
        }
    }

    private void OnTriggerExit(Collider other)
    {

        //un-highlight when controller does not touch the vertex
        if (other.gameObject.CompareTag("vertex"))
        {
            other.transform.GetComponent<Renderer>().material.color = Color.white;
            highlighted = null;
        }
        if (other.gameObject.CompareTag("selected"))
        {
            other.transform.GetComponent<Renderer>().material.color = Color.red;
            highlighted = null;
        }


    }

    private void GetMesh()
    {
        MeshFilter mf = targetMeshObj.AddComponent<MeshFilter>();
        MeshRenderer mr = targetMeshObj.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Custom/VertexColored"));

        mesh = mf.mesh;

    }

    private void CreateMesh()
    {
        mesh.Clear();

        //DEBUG
        //print("\nselected:");
        //printList(selectedVertices);
        //print("\nselectedIndices:");
        //printList(selectedTriangleIndices);

        //update vector position of vertices
        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = verticesGameObjects[i].transform.position;
        }

        //append both selected lists to mesh bake lists
        for (int i = 0; i < selectedVertices.Count; i++)
        {
            vertices.Add(selectedVertices[i].transform.position);
            meshColors.Add(Color.white);
        }

        
        verticesGameObjects.AddRange(selectedVertices);


        triangleIndices.AddRange(selectedTriangleIndices);

        //update values in the triangle index list
        for (int i = 0; i < triangleIndices.Count; i++)
        {
            if (triangleIndices[i] != i)
            {
                triangleIndices[i] = i;
            }
        }

        

        //DEBUG
        //print("\nfinal vertices");
        //printList(vertices);
        //print("\nfinal indices");
        //printList(triangleIndices);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangleIndices.ToArray();
        
        if (meshColors != null)
        {
            //DEBUG
            //print("\nMESH COLOR COUNT 2");
            //print(meshColors.Count.ToString());

            mesh.colors32 = meshColors.ToArray();
        }

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        //DEBUG
        print("\nvertices");
        print(mesh.vertices.Length.ToString());
        print("\ntriangles");
        print(mesh.triangles.Length.ToString()); ;
    }

    private void Deselect(GameObject gameObj)
    {
        gameObj.tag = "vertex";
        gameObj.transform.GetComponent<Renderer>().material.color = Color.white;

        int lastIndexToRemove = selectedVertices.LastIndexOf(gameObj);

        //remove the value in triangle indices
        selectedTriangleIndices.RemoveAt(lastIndexToRemove);

        //remove the value in vertex list
        selectedVertices.RemoveAt(lastIndexToRemove);

        indexStack.Push(lastIndexToRemove);


        vertexCounter--;
    }

    private void DeleteSelectedVertices()
    {
        Stack<int> toBeRemoved = new Stack<int>();

        //locate all occurrences of the vertex position and triangle index
        for (int i = 0; i < vertices.Count; i++)
        {
            for (int j = 0; j < selectedVertices.Count; j++)
            {
                if (vertices[i] == selectedVertices[j].transform.position)
                {
                    toBeRemoved.Push(i);
                }
            }
        }


        //remove all occurrences of the vertex position and triangle index
        while (toBeRemoved.Count > 0)
        {


            int toBeRemovedIndex = toBeRemoved.Pop();
 

            //remove related vertices and triangle indices to preserve existing mesh
            if (toBeRemovedIndex % 3 == 0)
            {
                //DEBUG
                print("\nvertices");
                print(vertices.Count.ToString());
                print("\ntobeREMOVEDINDEX");
                print(toBeRemovedIndex.ToString());

                vertices.RemoveRange(toBeRemovedIndex, 3);
                verticesGameObjects.RemoveRange(toBeRemovedIndex, 3);
                triangleIndices.RemoveRange(toBeRemovedIndex, 3);
                meshColors.RemoveRange(toBeRemovedIndex, 3);
            }
            else if (toBeRemovedIndex % 3 == 1)
            {
                vertices.RemoveRange(toBeRemovedIndex - 1, 3);
                verticesGameObjects.RemoveRange(toBeRemovedIndex - 1, 3);
                triangleIndices.RemoveRange(toBeRemovedIndex - 1, 3);
                meshColors.RemoveRange(toBeRemovedIndex - 1, 3);
            }
            else if (toBeRemovedIndex % 3 == 2)
            {
                vertices.RemoveRange(toBeRemovedIndex - 2, 3);
                verticesGameObjects.RemoveRange(toBeRemovedIndex - 2, 3);
                triangleIndices.RemoveRange(toBeRemovedIndex - 2, 3);
                meshColors.RemoveRange(toBeRemovedIndex - 2, 3);
            }
        }

        

        //deselect all selections; not calling Deselect() because need reference to destroy gameObjects
        GameObject[] selected = GameObject.FindGameObjectsWithTag("selected");

        //destroy those vertices
        for (int i = 0; i < selected.Length; i++)
        {
            Destroy(selected[i]);
            vertexCounter--;
        }

        //clear selections
        selectedVertices.Clear();
        selectedTriangleIndices.Clear();


        //update mesh with new vertices and indices
        CreateMesh();

        ////DEBUG
        //print("\nbaked vertices:");
        //printList(vertices);
        //print("\nbaked triangle indices:");
        //printList(triangleIndices);
    }

    private string GetGameMode()
    {
        return leftHandAnchor.GetComponent<SpaceHandler>().GetMode();
    }

    //to be called by SpaceHandler; clear selection when changing modes
    public void ClearSelection(string lastMode)
    {
        if (lastMode == "createMode")
        {
            //clear createMode selection
            GameObject[] selected = GameObject.FindGameObjectsWithTag("selected");
            for (int i = 0; i < selected.Length; i++)
            {
                Deselect(selected[i]);
            }
        } 
        else if (lastMode == "sculptMode")
        {
            //clear sculptMode selection
            GameObject[] sculptSelected = GameObject.FindGameObjectsWithTag("sculptSelected");
            for (int i = 0; i < sculptSelected.Length; i++)
            {
                sculptSelected[i].tag = "vertex";
                sculptSelected[i].transform.GetComponent<Renderer>().material.color = Color.white;
            }
        }
        
        

        //clear colorMode selection not needed?

    }

    //to be called by SpaceHandler to create relevant data for editing when importing meshes
    public void PrepareImportedMesh()
    {
        Mesh importedMesh = targetMeshObj.GetComponent<MeshFilter>().mesh;

        //DEBUG
        print("\nLENGTH 3");
        print((importedMesh.vertices.Length).ToString());
        print((importedMesh.triangles.Length).ToString());

        Vector3[] meshVertices = importedMesh.vertices;
        int[] meshTriangleIndices = importedMesh.triangles;

        //DEBUG
        print("\nLENGTH 4");
        print((meshVertices.Length).ToString());
        //print((meshTriangleIndices.Length).ToString());

        //populate vertices, triangle indices, vertices game objects, and vertex count
        for (int i = 0; i < meshVertices.Length; i++)
        {
            vertices.Add(meshVertices[i]);
            triangleIndices.Add(meshTriangleIndices[i]);

            //clone vertex instances; there should be only one vertex at same position
            
            bool samePos = false;
            for (int j = 0; j < verticesGameObjects.Count; j++)
            {
                if (verticesGameObjects[j].transform.position == meshVertices[i])
                {
                    
                    samePos = true;
                }
            }
            if (!samePos)
            {
                GameObject newVertex = Instantiate(vertexBall);
                newVertex.transform.position = meshVertices[i];
                newVertex.SetActive(true);
                newVertex.transform.SetParent(meshSpace.transform);

                verticesGameObjects.Add(newVertex);
            } else
            {
                GameObject toAdd = new GameObject();
                toAdd.transform.position = meshVertices[i];
                verticesGameObjects.Add(toAdd);
            }
            

            vertexCounter++;
        }

        //assign the imported mesh to private mesh variable
        mesh = importedMesh;


        //DEBUG
        //print("\nVERTICES-CHECK");
        //print(vertices.Count.ToString());

    }

    //FOR DEBUGGING
    private void printList<T>(List<T> l)
    {
        foreach (var x in l)
        {
            print(x.ToString());
        }
    }
}
