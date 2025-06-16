using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelEdit : EditorWindow
{
    int __width = 1;
    int __height = 1;
    int __depth = 1;

    bool __hollow = false;
    bool __group = true;
    bool __nonzero = false;
    bool __showTool = true;

    string __parentName = "Group";
    string __checkTag = "Ground";

    Object __ObjectType = null;
    Vector3 StartingVector = Vector3.zero;
    Vector3 PivotPoint = new Vector3(0.5f, 0.5f, 0.5f);

    Object __randPrefab = null;
    Material __groundA = null;
    Material __groundB = null;
    Material __groundC = null;
    Material __groundD = null;

    GameObject widget;
    LECompanion tool;

    [MenuItem("Window/Level Editor")]

    static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(LevelEdit));
    }

    private void Awake()
    {
        CreateTool();
    }

    void CreateTool()
    {
        if (widget != null) Destroy(widget);

        tool = GameObject.FindObjectOfType<LECompanion>();
        if (tool == null)
        {
            GameObject t = new GameObject("## Editor Tool ##");
            tool = t.AddComponent<LECompanion>();
        }
        widget = tool.gameObject;
    }

    void OnGUI()
    {
        if (tool != null && widget != null)
        {
            StartingVector = tool.gameObject.transform.position;
        }
        else
        {
            CreateTool();
            StartingVector = widget.transform.position;
        }

        GUILayout.Label("Instantiate objects", EditorStyles.boldLabel);
        __ObjectType = EditorGUILayout.ObjectField("Object", __ObjectType, typeof(Object), false);
        __width = EditorGUILayout.IntField("Width", __width);
        __height = EditorGUILayout.IntField("Height", __height);
        __depth = EditorGUILayout.IntField("Depth", __depth);
        __hollow = EditorGUILayout.Toggle("Make Hollow", __hollow);
        __group = EditorGUILayout.BeginToggleGroup("Group Objects", __group);
        __parentName = EditorGUILayout.TextField("Parent Name", __parentName);
        EditorGUILayout.EndToggleGroup();
        PivotPoint = EditorGUILayout.Vector3Field("Object Pivot", PivotPoint);
        widget.transform.position = EditorGUILayout.Vector3Field("Helper Coordinates", widget.transform.position);

        __showTool = EditorGUILayout.Toggle("Show Helper", __showTool);

        if (StartingVector != widget.transform.position)
        {
            StartingVector.x = (float)Mathf.RoundToInt(widget.transform.position.x);
            StartingVector.y = (float)Mathf.RoundToInt(widget.transform.position.y);
            StartingVector.z = (float)Mathf.RoundToInt(widget.transform.position.z);
            widget.transform.position = StartingVector;
        }

        // Dimensions cannot be less than 1 in any direction!
        if (__width < 1) __width = 1;
        if (__height < 1) __height = 1;
        if (__depth < 1) __depth = 1;

        if (GUILayout.Button("Create"))
        {
            if (__ObjectType != null)
            {
                if (__hollow)
                {
                    if (!CreateNewBox()) Debug.LogError("Error creating new objects");
                }
                else
                {
                    if (!CreateNewGrid()) Debug.LogError("Error creating new objects");
                }
            }
        }

        /*
        GUILayout.Label("Texture Randomizer", EditorStyles.boldLabel);
        __randPrefab = EditorGUILayout.ObjectField("Prefab", __randPrefab, typeof(Object), false);
        __checkTag = EditorGUILayout.TagField("Tag", __checkTag);

        __groundA = EditorGUILayout.ObjectField("Material 1", __groundA, typeof(Material), false) as Material;
        __groundB = EditorGUILayout.ObjectField("Material 2", __groundB, typeof(Material), false) as Material;
        __groundC = EditorGUILayout.ObjectField("Material 3", __groundC, typeof(Material), false) as Material;
        __groundD = EditorGUILayout.ObjectField("Material 4", __groundD, typeof(Material), false) as Material;
       

        if (GUILayout.Button("Randomize Blocks"))
        {
            RandomizeAll();
        }

        */

        if (__showTool)
        {
            if (tool != null) tool.DrawBox(new Vector3(__width, __height, __depth));
        }
        else
        {
            if (tool != null) tool.Clear();
        }
    }

    bool CreateNewGrid()
    {
        GameObject __parent = null;
        GameObject newPrefab;
        Vector3 sVector = Vector3.zero;
        if (__nonzero) sVector = StartingVector;
        if (__group) { __parent = new GameObject(__parentName); __parent.transform.position = sVector; }

        if (widget == null) CreateTool();

        for (int x = 0; x < __width; x++)
        {
            for (int y = 0; y < __height; y++)
            {
                for (int z = 0; z < __depth; z++)
                {
                    newPrefab = PrefabUtility.InstantiatePrefab(__ObjectType) as GameObject;
                    if (newPrefab == null) { Debug.Log("Prefab: " + __ObjectType); return false; }
                    newPrefab.transform.position = new Vector3(x, y, z) + sVector;
                    if (__group)
                    {
                        newPrefab.transform.parent = __parent.transform;
                    }
                    else
                    {
                        newPrefab.transform.position += widget.transform.position;
                    }
                }
            }
        }

        if(__group)
        {
            __parent.transform.position = widget.transform.position;
        }

        return true;
    }

    bool CreateNewBox()
    {
        GameObject __parent = null;
        GameObject newPrefab;
        Vector3 sVector = Vector3.zero;
        if (__nonzero) sVector = StartingVector;
        if (__group) { __parent = new GameObject(__parentName);  __parent.transform.position = sVector; }
        bool _create;

        if (widget == null) CreateTool();

        for (int x = 0; x < __width; x++)
        {
            for (int y = 0; y < __height; y++)
            {
                for (int z = 0; z < __depth; z++)
                {
                    _create = false;
                    if (x == 0 || x == __width - 1) _create = true;
                    if (y == 0 || y == __height-1) _create = true;
                    if (z == 0 || z == __depth - 1) _create = true;

                    if (_create)
                    {
                        newPrefab = PrefabUtility.InstantiatePrefab(__ObjectType) as GameObject;
                        if (newPrefab == null) { Debug.Log("Prefab: " + __ObjectType); return false; }
                        newPrefab.transform.position = new Vector3(x, y, z) + sVector;
                        if (__group)
                        {
                            newPrefab.transform.parent = __parent.transform;
                        }
                        else
                        {
                            newPrefab.transform.position += widget.transform.position;
                        }
                    }
                }
            }
        }

        if (__group)
        {
            __parent.transform.position = widget.transform.position;
        }

        return true;
    }

    void RandomizeAll()
    {
        /*
        Vector3 xyz = widget.transform.position;
        int lastBlock = 1;
        int x;
        int y;
        int z;
        int x2;
        int y2;
        int z2;
        bool oob = false;

        GameObject[] _blocks = GameObject.FindGameObjectsWithTag(__checkTag);
        GameObject[,,] grid = new GameObject[__width, __height, __height];

        foreach(GameObject b in _blocks)
        {
            x = (int)b.transform.position.x;
            y = (int)b.transform.position.y;
            z = (int)b.transform.position.z;
            if (PrefabUtility.GetCorrespondingObjectFromSource(b) == __randPrefab)
            {
                if(b.transform.parent.gameObject.tag != __checkTag) continue;

                if (x >= xyz.x && x <= (xyz.x + __width) )
                {
                    if (y >= xyz.y && y <= (xyz.y + __height))
                    {
                        if (z >= xyz.z && z <= (xyz.z + __depth))
                        {
                            x2 = x - (int)xyz.x;
                            y2 = y - (int)xyz.y;
                            z2 = z - (int)xyz.z;

                            if (x2 >= __width) oob = true;
                            if (x2 < 0) oob = true;
                            if (y2 >= __height) oob = true;
                            if (y2 < 0) oob = true;
                            if (z2 >= __height) oob = true;
                            if (z2 < 0) oob = true;

                            if (oob) { Debug.LogError("Out of Bounds: X:" + x2 + " Y: " + y2 + " Z: " + z2); continue; }

                            if (grid[x2,y2,z2] == null)
                            {
                                grid[x2, y2, z2] = b;
                            }
                        }
                    }
                }
            }
        }


        for (x = 0; x < __width; x++)
        {
            for (y = 0; y < __height; y++)
            {
                for (z = 0; y < __depth; z++)
                {

                }
            }
        }
        */
    }
    
}
