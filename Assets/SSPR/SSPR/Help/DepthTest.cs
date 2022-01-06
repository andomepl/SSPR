using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepthTest : MonoBehaviour
{

    private Camera cam;
    void Start()
    {
        cam = GetComponent<Camera>();

        cam.depthTextureMode |= DepthTextureMode.Depth;
    }

}
