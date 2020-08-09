using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

// HeadChopper Script
//
// A simple script to remove the head of an avatar so that you can use it for a first person 
// embodiment in a VR system. Derived from similar code written for other systems, but customised
// to use the conventions with the RocketBox avatars. This is a crude method that doesn't
// delete the vertices of the head and stitch the hole, but just collapses those triangles to
// the bone location of the head.
//
// Written by Anthony Steed, a.steed@ucl.ac.uk/anthonysteed@gmail.com, August 2020 
// Given to the public domain.

// How to use:
//    1. Drag one of the RocketBox avatars to your scene, say, "Business_Male_01", from the 
//       directory "Assets/Avatars/Professions/Business_Male_01/Export"
//    2. You will need to alter the import settings for this avatar, click on the "Business_Male_01" 
//       in that directory, to get the inspector called "Business_Male_01 Import Settings". Under 
//       the "Model" (the default) tab, make sure "Read/Write Enabled" under Meshes is ticked
//    3. Add this script to the object with the SkinnedMeshRender, "m005_hipoly_81_bones_opacity" 
//       for this example, but all the avatars are laid out the same way.

[RequireComponent(typeof(SkinnedMeshRenderer))]

public class HeadChopper : MonoBehaviour
{
    void Start()
    {
        // We need to clone the mesh as otherwise it alters the Prefab
        SkinnedMeshRenderer skin = GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = skin.sharedMesh = (Mesh)Instantiate(skin.sharedMesh);

        // Find all the bones that we want to collapse
        Transform[] bones = skin.bones;
        Transform headBone=null;
        int headBoneIndex=-1;

        bool[] headBoneFlag = new bool[bones.Length];

        for (int i = 0; i < bones.Length; i++)
        {
            if (bones[i].name.IndexOf("head", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                headBone = bones[i];
                headBoneIndex = i;
                break;
            }
        }

        if (headBoneIndex < 0)
        {
            Debug.Log("Failed to find head object");
            return;
        }

        for (int i = 0; i < bones.Length; i++)
        {
            headBoneFlag[i] = false;
            Transform cur = bones[i];
            while (cur!=null)
            {
                if (cur == headBone)
                {
                    headBoneFlag[i] = true;
                    break;
                }
                cur = cur.parent;
            }
        }

        headBoneFlag[headBoneIndex] = true;

        // Now move all vertices that have a weight > 0 for a tagged bone to the vertex position of the head bone
        // Vector3 headPos = headBone.transform.localPosition; // Attempt 1

        Vector3 headPos = GetComponent<Transform>().InverseTransformPoint(headBone.transform.position);

        BoneWeight[] meshBoneweights = mesh.boneWeights;
        Vector3[] vertices = mesh.vertices;
        int count = 0;
        for (int i=0;i< meshBoneweights.Length;i++)
        {
            if ((headBoneFlag[meshBoneweights[i].boneIndex0] &&
                meshBoneweights[i].weight0 > 0) ||
                (headBoneFlag[meshBoneweights[i].boneIndex1] &&
                meshBoneweights[i].weight1 > 0) ||
                (headBoneFlag[meshBoneweights[i].boneIndex2] &&
                meshBoneweights[i].weight2 > 0) ||
                (headBoneFlag[meshBoneweights[i].boneIndex3] &&
                meshBoneweights[i].weight3 > 0))
            {
                vertices[i] = headPos;

                // We remove smooth blending for anything that was partly linked to the head only
                meshBoneweights[i].boneIndex0 = headBoneIndex;
                meshBoneweights[i].weight0 = 1.0f;
                meshBoneweights[i].boneIndex1 = meshBoneweights[i].boneIndex2 = meshBoneweights[i].boneIndex3 = 0;
                meshBoneweights[i].weight1 = meshBoneweights[i].weight2 = meshBoneweights[i].weight3 = 0f;

                count++;
            }
        }
        mesh.vertices = vertices;
        mesh.boneWeights = meshBoneweights;
        Debug.Log("Headchopper chopped " + count);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
