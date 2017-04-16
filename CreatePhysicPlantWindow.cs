using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using NUnit.Framework;


public class CreatePhysicPlantWindow : EditorWindow
{
    //CONFIGURABLE JOINT PROPERTIES
    private ConfigurableJointMotion xMotion = ConfigurableJointMotion.Locked;
    private ConfigurableJointMotion yMotion = ConfigurableJointMotion.Locked;
    private ConfigurableJointMotion zMotion = ConfigurableJointMotion.Locked;
    private ConfigurableJointMotion AngularXMotion = ConfigurableJointMotion.Limited;
    private ConfigurableJointMotion AngularYMotion = ConfigurableJointMotion.Limited;
    private ConfigurableJointMotion AngularZMotion = ConfigurableJointMotion.Limited;

    private float LinearLimitSprSpring = 0F;
    private float LinearLimitSprDamper = 0F;
    private float LinearLimitLimLimit = 0F;
    private float LinearLimitLimBounce = 0F;
    private float LinearLimitLimContactDist = 0F;

    private float AngXLimitSping = 10F;
    private float AngXLimitDamper = 5F;

    private float LowAngXLimitLim = -40F;
    private float LowAngXLimitBounce = 0.1F;
    private float LowAngXLimitContactDist = 0F;

    private float HighAngXLimitLim = 40F;
    private float HighAngXLimitBounce = 0.1F;
    private float HighAngXLimitContactDist = 0F;

    private float AngYZLimitSping = 10F;
    private float AngYZLimitDamper = 5F;

    private float AngYLimitLim = 40F;
    private float AngYLimitBounce = 0.1F;
    private float AngYLimitContactDist = 0F;

    private float AngZLimitLim = 40F;
    private float AngZLimitBounce = 0.1F;
    private float AngZLimitContactDist = 0F;

    private float XDrivePosSpring = 10F;
    private float XDrivePosDamper = 2F;

    private float YDrivePosSpring = 10F;
    private float YDrivePosDamper = 2F;

    private float ZDrivePosSpring = 10F;
    private float ZDrivePosDamper = 2F;

    private RotationDriveMode rotationDriveMode = RotationDriveMode.Slerp;

    private float AngXDrivePosSpring = 1000F;
    private float AngXDrivePosDamper = 50F;
    private float AngXDriveMaxForce = 1F;

    private float AngYZDrivePosSpring = 1000F;
    private float AngYZDrivePosDamper = 50F;
    private float AngYZDriveMaxForce = 1F;

    private float XYZDriveAngXSpring = 1F;
    private float XYZDriveAngXDamper = 0.5F;
    private float XYZDriveAngXMaxForce = 1F;

    private float XYZDriveAngYZSpring = 1F;
    private float XYZDriveAngYZDamper = 0.5F;
    private float XYZDriveAngYZMaxForce = 1F;

    private float SlerpDrivePosSpring = 1F;
    private float SlerpDrivePosDamper = 0.5F;
    private float SlerpDriveMaxForce = 1F;

    private JointProjectionMode ProjectionMode = JointProjectionMode.PositionAndRotation;

    private float ProjectionDistance = 0.1F;
    private float ProjectionAngle = 180F;

    private bool ConfiguredInWorldSpace = false;
    private bool SwapBodies = false;

    
    private float BreakForce = Mathf.Infinity;
    private float BreakTorque = Mathf.Infinity;
    private bool BreakOnFirstBone = false;


    private bool EnableCollision = false;
    private bool EnablePreProcessing = false;
    //END CONFIGURABLE JOINT PROPERTIES.

    //DEFAULT RIGIDBODY PROPERTIES
    private float RigidbodyMass = 0.01F;
    private float RigidbodyDrag = 0F;
    private float RigidbodyAngDrag = 0.05F;
    private bool RigidbodyUseGravity = false;
    private bool RigidbodyIsKinematic = false;
    private RigidbodyInterpolation RigidbodyInterpolationType = RigidbodyInterpolation.None;
    private CollisionDetectionMode RigidbodyCollisionType = CollisionDetectionMode.Discrete;
    //END RIGIDBODY PROPERTIES

    //CAPSULE COLLIDER PROPERTIES
    private PhysicMaterial CapsulePhysicMaterial = null;
    private Vector3 CapsuleCenter = Vector3.zero;
    private float CapsuleRadius = 0.01F;
    private float CapsuleHeight = 0.15F;
    private int CapsuleDirection = 0;
    private int[] CapsuleAllowedDirections = {0, 1, 2};
    private string[] CapsuleDirectionNames = new string[] {"X","Y","Z"};
    private float CapsuleHeightReductionAmount = 0.05F;
    private Vector3 CapsuleCenterReductionAmount = Vector3.zero;
    private bool CapsuleIsTrigger = false;

    //END CAPSULE COLLIDER PROPERTIES
    
        
   //BEGIN WINDOW VARS
    private bool configurableJointPropertyGroup = false;
    private bool rigidbodyPropertyGroup = false;
    private bool capsulePropertyGroup = false;
    private Vector2 scrollPosition;
    //EndWindowVars

    //Creation Vars
    private GameObject RootBone = null;

    private List<GameObject> RootBoneChildren = new List<GameObject>();

    //End Creation Vars

    //ShiftColliderVars

    private bool shiftBool = false;
    private bool shiftInWorldSpace = true;
    private float shiftAmount = 0F;
    private int shiftDirection = 0;
    private int[] shiftDirectionsInt = {0, 1, 2, 3};
    private string[] shiftDirectionNames = new string[] {"X","Y","Z","Away From Center"};
    //EndShiftColliderVars

    [MenuItem("Window/CreatePhysicsPlant")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof (CreatePhysicPlantWindow));
    }

    /// <summary>
    /// Attempts to create the physics interactive plant from a skinned plant mesh.
    /// Uses parameters set for capsule colliders, rigidbodies, and configurable joints set in editor window.
    /// </summary>
    /// <param name="rootBone">Root bone of skinned plant mesh in hierarchy</param>
    void AttemptToCreatePhysicsPlant(GameObject rootBone)
    {
       FindBonesOfMesh(rootBone);
       AddAndConfigureJoints();
       AddAndConfigureRigidbodies();
       AddAndConfigureCapsuleColliders();
       SetConnectedRigidbodies();
        //SET CONIFGURABLE JOINT CONNECTED BODIES.
    }

    void SetConnectedRigidbodies()
    {
        foreach (GameObject bone in RootBoneChildren)
        {
            ConfigurableJoint configurableJoint = bone.GetComponent<ConfigurableJoint>();
            if (configurableJoint != null)
            {
                configurableJoint.connectedBody = bone.transform.parent.GetComponent<Rigidbody>();
            }
        }
    }

    void UpdateConfigurableJoints(GameObject rootBone)
    {
        FindBonesOfMesh(rootBone);
        AddAndConfigureJoints();
    }

    void UpdateRigidbodies(GameObject rootBone)
    {
        FindBonesOfMesh(rootBone);
        AddAndConfigureRigidbodies();
    }

    void UpdateCapsuleColliders(GameObject rootBone)
    {
        FindBonesOfMesh(rootBone);
        AddAndConfigureCapsuleColliders();
    }

    void AddAndConfigureCapsuleColliders()
    {
        CapsuleCollider capsuleCollider = null;
        foreach (GameObject bone in RootBoneChildren)
        {
            capsuleCollider = bone.GetComponent<CapsuleCollider>();
            if (capsuleCollider == null)
            {
                capsuleCollider = bone.AddComponent<CapsuleCollider>();
            }
            UpdateCapsuleColliderParams(bone,capsuleCollider);
            capsuleCollider = null;
        }

    }

    void UpdateCapsuleColliderParams(GameObject gameObject, CapsuleCollider collider)
    {
        collider.direction = CapsuleDirection;
        collider.radius = CapsuleRadius;
        collider.isTrigger = CapsuleIsTrigger;
        collider.material = CapsulePhysicMaterial;
        GameObject baseParent = RootBone.transform.parent.gameObject;
        if (baseParent == null)
        {
            Debug.Log("RootBone must be child of base parent gameobject.");
        }
        CalculateColliderHeightAndCenter(collider, gameObject);
    }

    /// <summary>
    /// Calculates height of capsule collider from self to child
    /// if no child exists, uses default value in capsule collider properties.
    /// </summary>
    /// <param name="collider"></param>
    /// <param name="gameObject"></param>
    /// <returns></returns>
    void CalculateColliderHeightAndCenter(CapsuleCollider collider, GameObject gameObject)
    {
        if (gameObject.transform.childCount > 0)
        {
            float maxdist = 0;
            int furthestChild = 0;
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                float dist =
                    (gameObject.transform.GetChild(i).transform.position - gameObject.transform.position).magnitude;
                if (dist > maxdist)
                {
                    maxdist = dist;
                    furthestChild = i;
                }
            }
            Transform child = gameObject.transform.GetChild(furthestChild);
            Vector3 distance = child.transform.position - gameObject.transform.position;
            Vector3 localDistance = child.transform.localPosition/2;
            collider.center = localDistance - CapsuleCenterReductionAmount;
            collider.height = distance.magnitude - CapsuleHeightReductionAmount;   
        }
        else
        {
            collider.height = CapsuleHeight;
            collider.center = CapsuleCenter;
        }
    }



    void AddAndConfigureRigidbodies()
    {
        Rigidbody rigidBody = RootBone.GetComponent<Rigidbody>();
        if (rigidBody == null)
        {
            RootBone.AddComponent<Rigidbody>();
        }
        UpdateRigidbodyValues(rigidBody);
        rigidBody = null;
        foreach (GameObject bone in RootBoneChildren)
        {
            rigidBody = bone.GetComponent<Rigidbody>();
            if (rigidBody == null)
            {
                rigidBody = bone.AddComponent<Rigidbody>();
            }
            UpdateRigidbodyValues(rigidBody);
            rigidBody = null;
        }
    }

    void UpdateRigidbodyValues(Rigidbody rigidBody)
    {
        rigidBody.mass = RigidbodyMass;
        rigidBody.drag = RigidbodyDrag;
        rigidBody.angularDrag = RigidbodyAngDrag;
        rigidBody.useGravity = RigidbodyUseGravity;
        rigidBody.isKinematic = RigidbodyIsKinematic;
        rigidBody.interpolation = RigidbodyInterpolationType;
        rigidBody.collisionDetectionMode = RigidbodyCollisionType;
    }


    void ShiftCapsuleColliders(GameObject gameObj)
    {
        List<CapsuleCollider> CapsuleList = new List<CapsuleCollider>();
        foreach (GameObject bone in RootBoneChildren)
        {
            CapsuleCollider collider = bone.GetComponent<CapsuleCollider>();
            if (collider != null)
            {
                CapsuleList.Add(collider);
            }
        }
        foreach (CapsuleCollider collider in CapsuleList)
        {
            Vector3 colliderRelative = collider.transform.TransformPoint(collider.center);
            if (shiftDirection == 0)
            {
                colliderRelative += new Vector3(shiftAmount, 0, 0);
                collider.center = collider.transform.InverseTransformPoint(colliderRelative); 
            }
            else if (shiftDirection == 1)
            {
                colliderRelative+= new Vector3(0,shiftAmount,0);
                collider.center = collider.transform.InverseTransformPoint(colliderRelative);
            }
            else if (shiftDirection == 2)
            {
                colliderRelative+= new Vector3(0,0,shiftAmount);
                collider.center = collider.transform.InverseTransformPoint(colliderRelative);
            }
            else if (shiftDirection == 3)
            {
                if (Mathf.Abs(collider.center.x) > 0.01F)
                {
                    collider.center -= new Vector3(shiftAmount, 0, 0);
                }
            }
        }
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label("Tools to create basic physics plant using skinned plant meshes.");

        RootBone = (GameObject) EditorGUILayout.ObjectField("Root Bone:", RootBone, typeof(GameObject), true);
        
        if (RootBone != null)
        {
            if (GUILayout.Button("Attempt to Create Physics Plant"))
            {
                AttemptToCreatePhysicsPlant(RootBone);
            }
            if (GUILayout.Button("Update Configurable Joints"))
            {
                UpdateConfigurableJoints(RootBone);
            }
            if (GUILayout.Button("Update Rigidbodies"))
            {
                UpdateRigidbodies(RootBone);
            }
            if (GUILayout.Button("Update Capsule Colliders"))
            {
                UpdateCapsuleColliders(RootBone);
            }
            GUILayout.Label("-------------", EditorStyles.boldLabel);
            capsulePropertyGroup = EditorGUILayout.BeginToggleGroup("Capsule Collider Properties", capsulePropertyGroup);
            if (capsulePropertyGroup)
            {
                GUILayout.Label("See Unity manual online for description of properties.");
          //      RootBone = (GameObject)EditorGUILayout.ObjectField("Root Bone:", RootBone, typeof(GameObject), true);
                CapsulePhysicMaterial =
                    (PhysicMaterial)
                        EditorGUILayout.ObjectField("Physic Material:", CapsulePhysicMaterial, typeof (PhysicMaterial),
                            false);
                CapsuleIsTrigger = EditorGUILayout.Toggle("Is Trigger", CapsuleIsTrigger);
                CapsuleCenter = EditorGUILayout.Vector3Field("Center:", CapsuleCenter);
                CapsuleRadius = EditorGUILayout.FloatField("Radius:", CapsuleRadius);
                CapsuleHeight = EditorGUILayout.FloatField("Height:", CapsuleHeight);
                CapsuleDirection = EditorGUILayout.IntPopup("Direction:", CapsuleDirection, CapsuleDirectionNames,
                    CapsuleAllowedDirections);
                CapsuleHeightReductionAmount = EditorGUILayout.FloatField("Reduce Calc Height by: ",
                    CapsuleHeightReductionAmount);
                CapsuleCenterReductionAmount = EditorGUILayout.Vector3Field("Reduce Calc Center by:",
                    CapsuleCenterReductionAmount);
            }
            EditorGUILayout.EndToggleGroup();

            rigidbodyPropertyGroup = EditorGUILayout.BeginToggleGroup("Rigidbody Properties", rigidbodyPropertyGroup);
            if (rigidbodyPropertyGroup)
            {
                GUILayout.Label("See Unity manual online for description of properties.");
                RigidbodyMass = EditorGUILayout.FloatField("Mass:", RigidbodyMass);
                RigidbodyDrag = EditorGUILayout.FloatField("Drag:", RigidbodyDrag);
                RigidbodyAngDrag = EditorGUILayout.FloatField("Angular Drag:", RigidbodyAngDrag);
                RigidbodyUseGravity = EditorGUILayout.Toggle("Use Gravity:", RigidbodyUseGravity);
                RigidbodyIsKinematic = EditorGUILayout.Toggle("Is Kinematic:", RigidbodyIsKinematic);
                RigidbodyInterpolationType =
                    (RigidbodyInterpolation)
                        EditorGUILayout.EnumPopup("Interpolation Type:", RigidbodyInterpolationType);
                RigidbodyCollisionType =
                    (CollisionDetectionMode)
                        EditorGUILayout.EnumPopup("Collision Detection Type:", RigidbodyCollisionType);
            }
            EditorGUILayout.EndToggleGroup();
            configurableJointPropertyGroup = EditorGUILayout.BeginToggleGroup("Configurable Joint Properties",
                configurableJointPropertyGroup);
            if (configurableJointPropertyGroup)
            {
                GUILayout.Label("See Unity manual online for description of properties.");
                //xyz motions.
                xMotion = (ConfigurableJointMotion) EditorGUILayout.EnumPopup("X Motion:", xMotion);
                yMotion = (ConfigurableJointMotion) EditorGUILayout.EnumPopup("Y Motion:", yMotion);
                zMotion = (ConfigurableJointMotion) EditorGUILayout.EnumPopup("Z Motion:", zMotion);
                AngularXMotion =
                    (ConfigurableJointMotion) EditorGUILayout.EnumPopup("Angular X Motion:", AngularXMotion);
                AngularYMotion =
                    (ConfigurableJointMotion) EditorGUILayout.EnumPopup("Angular Y Motion:", AngularYMotion);
                AngularZMotion =
                    (ConfigurableJointMotion) EditorGUILayout.EnumPopup("Angular Z Motion:", AngularZMotion);



                if (xMotion != 0 || yMotion != 0 || zMotion != 0)
                {
                    GUILayout.Label("Linear Limit Spring:");
                    LinearLimitSprSpring = EditorGUILayout.FloatField("     Spring:", LinearLimitSprSpring);
                    LinearLimitSprDamper = EditorGUILayout.FloatField("     Damper:", LinearLimitSprDamper);

                    GUILayout.Label("Linear Limit:");
                    LinearLimitLimLimit = EditorGUILayout.FloatField("     Limit:", LinearLimitLimLimit);
                    LinearLimitLimBounce = EditorGUILayout.FloatField("     Bounciness:", LinearLimitLimBounce);
                    LinearLimitLimContactDist = EditorGUILayout.FloatField("     Contact Distance:",
                        LinearLimitLimContactDist);
                }
                if (AngularXMotion != 0 || AngularYMotion != 0 || AngularZMotion != 0)
                {
                    GUILayout.Label("Angular X Limit Spring:");
                    AngXLimitSping = EditorGUILayout.FloatField("    Spring:", AngXLimitSping);
                    AngXLimitDamper = EditorGUILayout.FloatField("    Damper:", AngXLimitDamper);

                    GUILayout.Label("Low Angular X Limit:");
                    LowAngXLimitLim = EditorGUILayout.FloatField("    Limit:", LowAngXLimitLim);
                    LowAngXLimitBounce = EditorGUILayout.FloatField("    Bounciness:", LowAngXLimitBounce);
                    LowAngXLimitContactDist = EditorGUILayout.FloatField("    Contact Distance:", LowAngXLimitContactDist);

                    GUILayout.Label("High Angular X Limit:");
                    HighAngXLimitLim = EditorGUILayout.FloatField("    Limit:", HighAngXLimitLim);
                    HighAngXLimitBounce = EditorGUILayout.FloatField("    Bounciness:", HighAngXLimitBounce);
                    HighAngXLimitContactDist = EditorGUILayout.FloatField("    Contact Distance:", HighAngXLimitContactDist);

                    GUILayout.Label("Angular YZ Limit Spring:");
                    AngYZLimitSping = EditorGUILayout.FloatField("    Spring:", AngYZLimitSping);
                    AngYZLimitDamper = EditorGUILayout.FloatField("    Damper:", AngYZLimitDamper);

                    GUILayout.Label("Angular Y Limit:");
                    AngYLimitLim = EditorGUILayout.FloatField("    Limit:", AngYLimitLim);
                    AngYLimitBounce = EditorGUILayout.FloatField("    Bounciness:", AngYLimitBounce);
                    AngYLimitContactDist = EditorGUILayout.FloatField("    Contact Distance:", AngYLimitContactDist);

                    GUILayout.Label("Angular Z Limit:");
                    AngZLimitLim = EditorGUILayout.FloatField("    Limit:", AngZLimitLim);
                    AngZLimitBounce = EditorGUILayout.FloatField("    Bounciness:", AngZLimitBounce);
                    AngZLimitContactDist = EditorGUILayout.FloatField("    Contact Distance:", AngZLimitContactDist);
                }

                if (xMotion != 0 || yMotion != 0 || zMotion != 0)
                {
                    GUILayout.Label("X Drive:");
                    XDrivePosSpring = EditorGUILayout.FloatField("    Position Spring:", XDrivePosSpring);
                    XDrivePosDamper = EditorGUILayout.FloatField("    Position Damper:", XDrivePosDamper);

                    GUILayout.Label("Y Drive:");
                    YDrivePosSpring = EditorGUILayout.FloatField("    Position Spring:", YDrivePosSpring);
                    YDrivePosDamper = EditorGUILayout.FloatField("    Position Damper:", YDrivePosDamper);

                    GUILayout.Label("Z Drive:");
                    ZDrivePosSpring = EditorGUILayout.FloatField("    Position Spring:", ZDrivePosSpring);
                    ZDrivePosDamper = EditorGUILayout.FloatField("    Position Damper:", ZDrivePosDamper);
                }

                rotationDriveMode =
                    (RotationDriveMode) EditorGUILayout.EnumPopup("Rotation Drive Mode:", rotationDriveMode);
                if (rotationDriveMode == RotationDriveMode.Slerp)
                {
                    GUILayout.Label("Slerp Drive:");
                    SlerpDrivePosSpring = EditorGUILayout.FloatField("    Position Spring:", SlerpDrivePosSpring);
                    SlerpDrivePosDamper = EditorGUILayout.FloatField("    Position Damper:", SlerpDrivePosDamper);
                    SlerpDriveMaxForce = EditorGUILayout.FloatField("    Maximum Force:", SlerpDriveMaxForce);
                }
                else if (rotationDriveMode == RotationDriveMode.XYAndZ)
                {
                    GUILayout.Label("Angular X Drive:");
                    XYZDriveAngXSpring = EditorGUILayout.FloatField("    Position Spring:", XYZDriveAngXSpring);
                    XYZDriveAngXDamper = EditorGUILayout.FloatField("    Position Damper:", XYZDriveAngXDamper);
                    XYZDriveAngXMaxForce = EditorGUILayout.FloatField("    Maximum Force:", XYZDriveAngXMaxForce);
                    GUILayout.Label("Angular YZ Drive:");
                    XYZDriveAngYZSpring = EditorGUILayout.FloatField("    Position Spring:", XYZDriveAngYZSpring);
                    XYZDriveAngYZDamper = EditorGUILayout.FloatField("    Position Damper:", XYZDriveAngYZDamper);
                    XYZDriveAngYZMaxForce = EditorGUILayout.FloatField("    Maximum Force:", XYZDriveAngYZMaxForce);
                }


                //PROJECTION MODE HERE POS/ROT
                GUILayout.Label("Do not use position mode only below.");
                ProjectionMode = (JointProjectionMode) EditorGUILayout.EnumPopup("Projection Mode:", ProjectionMode);

                ProjectionDistance = EditorGUILayout.FloatField("Projection Distance:", ProjectionDistance);
                ProjectionAngle = EditorGUILayout.FloatField("Projection Angle:", ProjectionAngle);

                ConfiguredInWorldSpace = EditorGUILayout.Toggle("Configured In World Space", ConfiguredInWorldSpace);
                SwapBodies = EditorGUILayout.Toggle("Swap Bodies:", SwapBodies);

                BreakForce = EditorGUILayout.FloatField("Break Force:", BreakForce);
                BreakTorque = EditorGUILayout.FloatField("Break Torque:", BreakTorque);
                BreakOnFirstBone = EditorGUILayout.Toggle("Break On First Bone:", BreakOnFirstBone);

                EnableCollision = EditorGUILayout.Toggle("Enable Collision:", EnableCollision);
                EnablePreProcessing = EditorGUILayout.Toggle("EnablePreProcessiong:", EnablePreProcessing);
            }
            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.LabelField("------");
            EditorGUILayout.LabelField("Other Tools:");
            shiftBool = EditorGUILayout.BeginToggleGroup("Shift Colliders Tools", shiftBool);
            if (shiftBool)
            {
                shiftInWorldSpace = EditorGUILayout.Toggle("Shift In World Space:", shiftInWorldSpace);
                shiftDirection = EditorGUILayout.IntPopup("Direction:", shiftDirection, shiftDirectionNames,
                    shiftDirectionsInt);
                shiftAmount = EditorGUILayout.FloatField("Shift Amount", shiftAmount);
                if (GUILayout.Button("Shift"))
                {
                    ShiftCapsuleColliders(RootBone);
                }
                 //CapsuleDirection = EditorGUILayout.IntPopup("Direction:", CapsuleDirection, CapsuleDirectionNames,
                  //  CapsuleAllowedDirections);

            }
            EditorGUILayout.EndToggleGroup();
            if (GUILayout.Button("Reset To Origin"))
            {
                ResetPlantToOrigin(RootBone);
            }

        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
    }

    void ResetPlantToOrigin(GameObject root)
    {
        FindBonesOfMesh(root);
        RootBone.transform.localPosition = Vector3.zero;
        Transform parent = RootBone.transform.parent;
        int numChildren = parent.transform.childCount;
        for(int i = numChildren-1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.name == parent.name)
            {
                child.transform.localPosition =  Vector3.zero;
                break;
            }
            if (child.GetComponent<SkinnedMeshRenderer>() != null)
            {
                child.transform.localPosition = Vector3.zero;
                break;
            }
        }
    }

    /// <summary>
    /// Empties root bone children list and recalculates it.
    /// </summary>
    /// <param name="rootBone"></param>
    void FindBonesOfMesh(GameObject rootBone)
    {
        EnsureRootBone(rootBone);
        RootBoneChildren.Clear();
        RootBoneChildren = GetAndAddChildrenToList(RootBoneChildren, RootBone.transform);
    }

    void EnsureRootBone(GameObject rootBone)
    {
        //if (rootBone.name.Contains("Bone"))
        //{
        //    RootBone = rootBone;
        //}
        //else
        //{
        //    int childCount = rootBone.transform.childCount;
        //    for (int i = 0; i < childCount; i++)
        //    {
        //        EnsureRootBone(rootBone.transform.GetChild(i).gameObject);
        //    }
        //}
        //if (rootBone.GetComponent<Animator>() != null)
        //{
        //    GameObject child = rootBone.transform.GetChild(0).gameObject;
        //    Debug.Log("anim " + child.name);
        //    EnsureRootBone(child);
        //}
        //if (rootBone.GetComponent<SkinnedMeshRenderer>() != null)
        //{
        //    Debug.Log("skinmeshrend");
        //}
        //else
        //{
        //    RootBone = rootBone;
        //}
        if (rootBone.GetComponent<SkinnedMeshRenderer>() == null && rootBone.GetComponent<Animator>() == null)
        {
            return;
        }
        int childCount = rootBone.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Debug.Log(childCount);
            Debug.Log(rootBone.transform.GetChild(i).name);
            
            if (rootBone.GetComponent<SkinnedMeshRenderer>() == null)
            {
                GameObject child = rootBone.transform.GetChild(i).gameObject;
                if (child.name.Contains("Bone") || child.name.Contains("ro"))
                {
                    RootBone = child;
                    break;
                }
            }
        }
    }
    /// <summary>
    /// Adds children of root bone to list in order in which they appear in scene view hierarchy.
    /// </summary>
    /// <param name="baseList">The List to which you are adding children.</param>
    /// <param name="parentTransform">Root transform from which to start looking for children. (Should be root bone)</param>
    /// <returns>Returns List of children in which they appear in hierarchy.</returns>
    List<GameObject> GetAndAddChildrenToList(List<GameObject> baseList, Transform parentTransform)
    {
        int childrenTotal = parentTransform.childCount;
        if (childrenTotal > 0)
        {
            for (int i = 0; i < childrenTotal; i++)
            {
               
                GameObject childTransform = parentTransform.GetChild(i).gameObject;
                if (childTransform.transform.childCount > 0)
                {
                    baseList.Add(childTransform);
                }
                GetAndAddChildrenToList(baseList, childTransform.transform);
            }
        }
        return baseList;
    }

    void AddAndConfigureJoints()
    {
        FixedJoint rootJoint = RootBone.GetComponent<FixedJoint>();
        if (rootJoint == null)
        {
            rootJoint = RootBone.AddComponent<FixedJoint>();
        }
        foreach (GameObject bone in RootBoneChildren)
        {
            ConfigurableJoint childJoint = bone.GetComponent<ConfigurableJoint>();
            if (childJoint == null)
            {
                childJoint = bone.AddComponent<ConfigurableJoint>();
            }
            int childNum = RootBoneChildren.IndexOf(bone);
            UpdateConfigurableJointParams(childJoint, childNum);
        }
    }

    void UpdateConfigurableJointParams(ConfigurableJoint joint, int jointNum)
    {
        
        joint.xMotion = xMotion;
        joint.yMotion = yMotion;
        joint.zMotion = zMotion;

        joint.angularXMotion = AngularXMotion;
        joint.angularYMotion = AngularYMotion;
        joint.angularZMotion = AngularZMotion;

        SoftJointLimitSpring tempSoftJointLimitSpring = new SoftJointLimitSpring();

        tempSoftJointLimitSpring.spring = AngXLimitSping;
        tempSoftJointLimitSpring.damper = AngXLimitDamper;
        joint.angularXLimitSpring = tempSoftJointLimitSpring;

        SoftJointLimit tempSoftJointLimit = new SoftJointLimit();

        if (xMotion != 0 || yMotion != 0 || zMotion != 0)
        {
            tempSoftJointLimit.bounciness = LinearLimitLimBounce;
            tempSoftJointLimit.limit = LinearLimitLimLimit;
            tempSoftJointLimit.contactDistance = LinearLimitLimContactDist;
            joint.linearLimit = tempSoftJointLimit;
            tempSoftJointLimitSpring.spring = LinearLimitSprSpring;
            tempSoftJointLimitSpring.damper = LinearLimitSprDamper;
            joint.linearLimitSpring = tempSoftJointLimitSpring;
        }


        tempSoftJointLimit.bounciness = LowAngXLimitBounce;
        tempSoftJointLimit.limit = LowAngXLimitLim;
        tempSoftJointLimit.contactDistance = LowAngXLimitContactDist;
        joint.lowAngularXLimit = tempSoftJointLimit;

        tempSoftJointLimit.bounciness = HighAngXLimitBounce;
        tempSoftJointLimit.limit = HighAngXLimitLim;
        tempSoftJointLimit.contactDistance = HighAngXLimitContactDist;
        joint.highAngularXLimit = tempSoftJointLimit;

        tempSoftJointLimitSpring.spring = AngYZLimitSping;
        tempSoftJointLimitSpring.damper = AngYZLimitDamper;
        joint.angularYZLimitSpring = tempSoftJointLimitSpring;

        tempSoftJointLimit.bounciness = AngYLimitBounce;
        tempSoftJointLimit.limit = AngYLimitLim;
        tempSoftJointLimit.contactDistance = AngYLimitContactDist;
        joint.angularYLimit = tempSoftJointLimit;

        tempSoftJointLimit.bounciness = AngZLimitBounce;
        tempSoftJointLimit.limit = AngZLimitLim;
        tempSoftJointLimit.contactDistance = AngZLimitContactDist;
        joint.angularZLimit = tempSoftJointLimit;

        JointDrive tempJointDrive = new JointDrive(); ;
        tempJointDrive.positionSpring = XDrivePosSpring;
        tempJointDrive.positionDamper = XDrivePosDamper;
        tempJointDrive.maximumForce = Mathf.Infinity;
        joint.xDrive = tempJointDrive;

        tempJointDrive.positionSpring = YDrivePosSpring;
        tempJointDrive.positionDamper = YDrivePosDamper;
        tempJointDrive.maximumForce = Mathf.Infinity;
        joint.yDrive = tempJointDrive;

        tempJointDrive.positionSpring = ZDrivePosSpring;
        tempJointDrive.positionDamper = ZDrivePosDamper;
        tempJointDrive.maximumForce = Mathf.Infinity;
        joint.zDrive = tempJointDrive;

        joint.rotationDriveMode = rotationDriveMode;

        tempJointDrive.positionSpring = SlerpDrivePosSpring;
        tempJointDrive.positionDamper = SlerpDrivePosDamper;
        tempJointDrive.maximumForce = SlerpDriveMaxForce;
        joint.slerpDrive = tempJointDrive;

        joint.projectionMode = ProjectionMode;
        joint.projectionDistance = ProjectionDistance;
        joint.projectionAngle = ProjectionAngle;

        joint.configuredInWorldSpace = ConfiguredInWorldSpace;
        joint.swapBodies = SwapBodies;
        if (jointNum != 0 || BreakOnFirstBone)
        {
            joint.breakForce = BreakForce;
            joint.breakTorque = BreakTorque;
        }
        else
        {
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
        }
        joint.enableCollision = EnableCollision;
        joint.enablePreprocessing = EnablePreProcessing;

        joint.autoConfigureConnectedAnchor = true;

    }
}

