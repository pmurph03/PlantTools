using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;


public class PutPrefabsIntoGrid : EditorWindow {
    [MenuItem("Window/PutObjectsIntoGrid")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(PutPrefabsIntoGrid));
    }

    private float gridSpacing;
    private Vector3 startGridLocation;
    private string baseFolder;
    void OnGUI()
    {
        gridSpacing = EditorGUILayout.FloatField("GridSpacing", gridSpacing);
        startGridLocation = EditorGUILayout.Vector3Field("StartGridLocation", startGridLocation);
        baseFolder = EditorGUILayout.TextField("Base Folder", baseFolder);
        if (GUILayout.Button("Create Grided Objects"))
        {
            string[] subDirs = System.IO.Directory.GetDirectories(baseFolder);
            if (subDirs.Length == 0)
            {
                string[] subFiles = System.IO.Directory.GetFiles(baseFolder, "*.prefab");
                int i = 0;
                int y = 0;
                int z = 0;
                string currentStartString = "";
                for (int j = 0; j < subFiles.Length; j++)
                {
                    if (currentStartString == "")
                    {
                        currentStartString = subFiles[j].Substring(baseFolder.Length+1, 8).ToLower();

                        Debug.Log(currentStartString);
                    }
                    else
                    {
                        string newSubString = subFiles[j].Substring(baseFolder.Length + 1, 8).ToLower();
                        if (newSubString != currentStartString)
                        {
                            i += 1;
                            z = 0;
                            currentStartString = newSubString;
                            Debug.Log(currentStartString);
                        }
                    }
                    float l = 0;
                    float k = 0;
                    
                    //if (j > 9 && j <= 19)
                    //{
                    //    l = gridSpacing / 3;
                    //    k = 10;
                    //}
                    //else if (j > 19)
                    //{
                    //    l = (gridSpacing / 3) * 2;
                    //    k = 20;
                    //}

                    subFiles[j] = subFiles[j].Replace(@"\", "/");
                    GameObject test = (GameObject)AssetDatabase.LoadAssetAtPath(subFiles[j], typeof(GameObject));
                    test = (GameObject)PrefabUtility.InstantiatePrefab(test);
                    test.transform.position = new Vector3(i + 1 + startGridLocation.x, 0,
                        z*gridSpacing - k + startGridLocation.y);
            //        test = (GameObject)Instantiate(test, new Vector3(i+1 + startGridLocation.x, 0, z * gridSpacing - k + startGridLocation.y), Quaternion.identity);
              //      test.transform.name = test.transform.name.Remove(test.transform.name.Length - 7);
                    z += 1;
                    //   GameObject gameObj = (GameObject) Instantiate(prefab, Vector3.zero, Quaternion.identity);
                    //   gameObj.transform.position = new Vector3(i*gridSpacing, j*gridSpacing,0);
                }
            }

            for (int i = 0; i < subDirs.Length; i++)
            {
                string[] subFiles = System.IO.Directory.GetFiles(subDirs[i], "*.prefab");
                int p = 0;
                string currentStartString = "";
                for (int j = 0; j < subFiles.Length; j++)
                {

                    if (currentStartString == "")
                    {
                        currentStartString = subFiles[j].Substring(0, 4);
                        Debug.Log(currentStartString);
                    }
                    else
                    {
                        string newSubString = subFiles[j].Substring(0, 4);
                        if (newSubString != currentStartString)
                        {
                            i += 1;
                            currentStartString = newSubString;
                            Debug.Log(currentStartString);
                        }
                    }
                    float l = 0;
                    float k = 0;
                   
                    if (j > 9 && j<=19)
                    {
                        l = gridSpacing/3;
                        k = 10;
                    }
                    else if (j > 19)
                    {
                        l = (gridSpacing/3)*2;
                        k = 20;
                    }

                    subFiles[j] = subFiles[j].Replace(@"\", "/");
                    GameObject test = (GameObject) AssetDatabase.LoadAssetAtPath(subFiles[j], typeof(GameObject));
       
                    test = (GameObject)Instantiate(test, new Vector3(p+l+startGridLocation.x, 0, j*gridSpacing-k+startGridLocation.y), Quaternion.identity);
                    test.transform.name = test.transform.name.Remove(test.transform.name.Length - 7);

                    //   GameObject gameObj = (GameObject) Instantiate(prefab, Vector3.zero, Quaternion.identity);
                    //   gameObj.transform.position = new Vector3(i*gridSpacing, j*gridSpacing,0);
                }

            }
        }
    }
}
