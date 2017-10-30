/* Created by Patrick Murphy
 * Free for personal use.
 * Contact for any commercial use.
 * Contact if you would like to see additional features addded as well.
 * I will continue to add features as I personally find a need in my own development, but would love to hear what others want.
*/

using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class RenameAndCreatePrefabs : EditorWindow
{

    [MenuItem("Window/RenameAndCreatePrefabs")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof (RenameAndCreatePrefabs));
    }

    private bool resetXforms;

    private string materialsFolder;
    private int numMaterials=1;
    private Material[] materials;
    private Material material;
    private string textToReplace;
    private string newText;
    private string suffixTextIdentifier;

    private bool prefixFromMaterial;
    private bool inverseSuffix;
    private int prefixFromMatLength;

    private bool matchFolderByGameObjectPrefix;
    private string prefixFolderIdentifier;
    private bool matchFolderByGameObjectSuffix;
    private int numPrefabFolders=1;
    private string[] prefabLocation;
    private string prefabFolderPrefix = "Assets/Prefabs/";
    private bool showAllMaterials;

    
    private bool matchMatBySuffix;
    private string matchMatBySuffixIdentifier;

    private bool subFolderByMaterial;
    private bool usePrefixFromMatAsFolder;

    private bool isSkinned;
    private bool hasLODS;
    private string originalName;
    void OnGUI()
    {
        EditorGUIUtility.labelWidth = 0;
        EditorStyles.label.fontStyle = FontStyle.Normal;
        
        if (prefabLocation == null)
        {
            prefabLocation=new string[0];
        }
        if (materials == null)
        {
            materials = new Material[0];
        }

        GUILayout.Label("Tools to rename a bunch of selected gameobjects, apply material, and create prefabs into folders");
        EditorGUILayout.LabelField("-");
        EditorGUILayout.LabelField("Material Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;

       
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Materials Folder:");
        EditorGUILayout.TextField(materialsFolder);
        if (GUILayout.Button("Choose Material Folder"))
        {
            materialsFolder = EditorUtility.OpenFolderPanel("Material Folder:", "", "");
            if (!string.IsNullOrEmpty(materialsFolder))
            {
                int index = materialsFolder.IndexOf("Assets", StringComparison.CurrentCulture);
                if (index >= 0)
                {
                    materialsFolder = materialsFolder.Substring(index, materialsFolder.Length - index);
                }
                Debug.Log(materialsFolder);
                string[] files = Directory.GetFiles(materialsFolder, "*.mat", SearchOption.AllDirectories);
                int materialsFound = 0;
                List<Material> materialList = new List<Material>();
                foreach (string file in files)
                {

                    if (file.EndsWith(".mat"))
                    {
                        Material matLoad = (Material)AssetDatabase.LoadAssetAtPath(file, typeof(Material));
                        if (matLoad != null)
                        {
                            materialList.Add(matLoad);
                            materialsFound += 1;
                        }
                    }
                }
                numMaterials = materialsFound;
                materials = materialList.ToArray();
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        numMaterials = EditorGUILayout.IntField("# Materials:", numMaterials);

        matchMatBySuffix = EditorGUILayout.Toggle("Match by suffix", matchMatBySuffix);
        if (matchMatBySuffix)
        {
            matchMatBySuffixIdentifier = EditorGUILayout.TextField("Suffix Identifier:", matchMatBySuffixIdentifier);
        }
        EditorGUILayout.EndHorizontal();
        if (numMaterials != materials.Length)
        {
            Material[] tempMaterial = new Material[materials.Length];
            tempMaterial = materials;
            materials = new Material[numMaterials];
            for (int i = 0; i < materials.Length && i < tempMaterial.Length; i++)
            {
                materials[i] = tempMaterial[i];
            }
        }
        showAllMaterials = EditorGUILayout.Foldout(showAllMaterials, "Materials:");
        if (showAllMaterials)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = (Material)EditorGUILayout.ObjectField("Material", materials[i], typeof(Material));
            }

        }


        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("-");
        EditorGUILayout.LabelField("Name Editing Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        textToReplace = EditorGUILayout.TextField("Text To Replace", textToReplace);
        newText = EditorGUILayout.TextField("New Text", newText);
        EditorGUILayout.BeginHorizontal();
        prefixFromMaterial = EditorGUILayout.Toggle("apnd matID prefx2sufx", prefixFromMaterial);
        if (prefixFromMaterial)
        {
            prefixFromMatLength = EditorGUILayout.IntField("prefix in matID length", prefixFromMatLength);
            inverseSuffix = EditorGUILayout.Toggle("Start at mat end?", inverseSuffix);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("-");
        EditorGUILayout.LabelField("Prefab Folder Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.LabelField("From Object Name");
        EditorGUILayout.BeginHorizontal();
        matchFolderByGameObjectSuffix = EditorGUILayout.Toggle("1st Folder by Suffix?:", matchFolderByGameObjectSuffix);
        if (matchFolderByGameObjectSuffix)
        {
            suffixTextIdentifier = EditorGUILayout.TextField("Suffix identifier:", suffixTextIdentifier);
        }
        else
        {
            matchFolderByGameObjectPrefix = EditorGUILayout.Toggle("1st Folder by Prefix?:", matchFolderByGameObjectPrefix);
        }
        if (matchFolderByGameObjectPrefix)
        {
            prefixFolderIdentifier = EditorGUILayout.TextField("Prefix identifier", prefixFolderIdentifier);
        }
        EditorGUILayout.EndHorizontal();
   
        EditorGUILayout.BeginHorizontal();
        subFolderByMaterial = EditorGUILayout.Toggle("SubFolder from MatID?:", subFolderByMaterial);
        if (subFolderByMaterial)
        {
            usePrefixFromMatAsFolder = EditorGUILayout.Toggle("Mat prefix as SubFolder?", usePrefixFromMatAsFolder);
            EditorGUILayout.LabelField("(from apnd in Material Settings)");
        }
        EditorGUILayout.EndHorizontal();
        prefabFolderPrefix = EditorGUILayout.TextField("Prefab folder prefix:", prefabFolderPrefix);
        //numPrefabFolders = EditorGUILayout.IntField("#seperate prefab folders:", numPrefabFolders);
        //if (numPrefabFolders!=prefabLocation.Length)
        //{
        //    string[] TempPrefabLocation = new string[this.prefabLocation.Length];
        //    if (prefabLocation.Length > 0)
        //    {
        //        for (int i = 0; i<prefabLocation.Length; i++)
        //        {
        //            TempPrefabLocation[i] = prefabLocation[i];
        //        }
        //    }
        //    prefabLocation = new string[numPrefabFolders];
        //    for (int i = 0; i < prefabLocation.Length && i<TempPrefabLocation.Length; i++)
        //    {
        //        prefabLocation[i] = TempPrefabLocation[i];
        //    }
        //}
        //for (int i = 0; i < prefabLocation.Length; i++)
        //{
        //    prefabLocation[i] = EditorGUILayout.TextField("PrefabFolder:", prefabLocation[i]);
        //}

        EditorGUILayout.LabelField("-");
        EditorGUI.indentLevel--;
        EditorGUILayout.LabelField("Additional Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        EditorGUILayout.BeginHorizontal();
        isSkinned = EditorGUILayout.Toggle("Skinned Object?", isSkinned);
        hasLODS = EditorGUILayout.Toggle("LODS?", hasLODS);
        resetXforms = EditorGUILayout.Toggle("Reset Transforms?", resetXforms);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Create Prefabs"))
        {
            GameObject[] selection = Selection.gameObjects;
            foreach (GameObject gameObj in selection)
            {
                string name = gameObj.name;
                if (hasLODS)
                {
                    foreach (Material mat in materials)
                    {
                        
                        if (matchMatBySuffix)
                        {
                            string MatName = mat.name.ToLower();
                            int SubStringIdex = name.LastIndexOf(matchMatBySuffixIdentifier, StringComparison.CurrentCulture);
                            string MatchToSuffix = name.Substring(SubStringIdex+1);
                            if (MatchToSuffix != null)
                            {
                                MatchToSuffix = MatchToSuffix.ToLower();
                                if (!MatName.Contains(MatchToSuffix))
                                {
                                    continue; //if the material does not contain the suffix specified on the gameobject to match with, then continue to the next material.
                                }
                            }
                        }
                        int ChildCount = gameObj.transform.childCount;
                        for (int i = 0; i < ChildCount; i++)
                        {
                            Transform child = gameObj.transform.GetChild(i);
                            Renderer renderer = child.GetComponent<Renderer>();
                            if (renderer == null)
                            {
                                continue; //if no renderer, go to the next child.
                            }
                            else
                            {
                                renderer.material = mat;
                            }
                        }
                        CreatePrefab(gameObj, mat);
                    }
                   
                } //OLD SETTINGS FOR CREATING PLANTS BELOW. UNCHANGED FOR PREFIXS/SUFFIX/MAT MATCHING AND SUCH AS OF NOW
                else //handles a single material to a single obj.
                {
                    if (!isSkinned)
                    {
                        Renderer rend = gameObj.GetComponent<Renderer>();
                        foreach (Material mat in materials)
                        {
                            rend.material = mat;
                            CreatePrefab(gameObj, mat);
                        }
                    }
                    else
                    {
                        SkinnedMeshRenderer skinnedMeshRenderer = gameObj.GetComponentInChildren<SkinnedMeshRenderer>();
                        if (skinnedMeshRenderer != null)
                        {
                            foreach (Material mat in materials)
                            {
                                skinnedMeshRenderer.material = mat;
                                CreatePrefab(gameObj, mat);
                            }
                        }
                    }
                }
            }

        }


        GUILayout.Label("Location format: Assets/Folder/Prefabs/");
    }

    public void CreatePrefab(GameObject gameObj, Material mat)
    {
        Vector3 originalPos = gameObj.transform.position;
        Vector3 originalScale = gameObj.transform.localScale;
        Quaternion originalRotation = gameObj.transform.rotation;
        originalName = gameObj.name;
        string name = gameObj.name;
        if (!String.IsNullOrEmpty(textToReplace) && !String.IsNullOrEmpty(newText))
        {
            name = name.Replace(textToReplace, newText);
        }
        if (prefixFromMaterial)
        {
            if (!inverseSuffix)
            {
                string suffixToAdd = mat.name.Substring(0, prefixFromMatLength);
                name += "_" + suffixToAdd;
            }
        }
        gameObj.name = name;
        string prefabCreationLocation = prefabFolderPrefix;

            if (matchFolderByGameObjectSuffix || matchFolderByGameObjectPrefix)
            {
                int indexOfSubString = 0;
                if (matchFolderByGameObjectSuffix)
                {
                     indexOfSubString = gameObj.name.LastIndexOf(suffixTextIdentifier);
                    if (indexOfSubString + 1 < gameObj.name.Length)
                    {
                        prefabCreationLocation += gameObj.name.Substring(indexOfSubString + 1) + '/';
                    }
                }
                else
                {
                    indexOfSubString = gameObj.name.IndexOf(prefixFolderIdentifier);
                    if (indexOfSubString - 1 < gameObj.name.Length)
                    {
                        prefabCreationLocation += gameObj.name.Substring(0, indexOfSubString) + '/';
                    }
                }
            }
        if (subFolderByMaterial)
        {
            if (usePrefixFromMatAsFolder)
            {
                string subFolder = mat.name.Substring(0, prefixFromMatLength);
                prefabCreationLocation += subFolder + '/';
            }
        }
        if (resetXforms)
        {
            gameObj.transform.position = Vector3.zero;
            gameObj.transform.rotation = Quaternion.identity;
            gameObj.transform.localScale = new Vector3(1,1,1);
        }
        Directory.CreateDirectory(prefabCreationLocation);
        UnityEngine.Object newPrefab = PrefabUtility.CreateEmptyPrefab(prefabCreationLocation + name + ".prefab");
        PrefabUtility.ReplacePrefab(gameObj, newPrefab, ReplacePrefabOptions.ConnectToPrefab);
        gameObj.name = originalName;
        gameObj.transform.position = originalPos;
        gameObj.transform.rotation = originalRotation;
        gameObj.transform.localScale = originalScale;
    }

}
