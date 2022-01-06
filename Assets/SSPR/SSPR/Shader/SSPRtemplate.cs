using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SSPRtemplate : MonoBehaviour
{



    [Header("Setting")]
    [Range(-0.01f, 0.01f)]
    public float Intensity = 0.005f;
    [Range(-0.5f, 0.1f)]
    public float Threshold = 0.0f;


    [Range(0.0f, 0.001f)]
    public float OffsetMin = 0.001f;

    [Header("Open Holes Fill")]
    public bool IsFill=true;


    private int[] TexSize;



    public float WaterHeight;


    public ComputeShader cs;

    private RenderTexture refTex;

    private RenderTexture CurTex;

    private RenderTexture HashTex;


  



   

        



    Matrix4x4 _VPMatrix;
    Matrix4x4 I_VPMatrix;


    private int Clear;
    private int WriteHashRT;
    private int csMain;
    private int FillHoles;


    bool flag = false;


    Camera cam;



    private void Start()
    {
        cam = GetComponent<Camera>();



        cam.depthTextureMode |= DepthTextureMode.Depth;

        TexSize = new int[2] { cam.pixelWidth , cam.pixelHeight };


        SetComputeShader();


        Shader.SetGlobalTexture("_ReflectTex", refTex);


        print("Clear is kernel:" + Clear);
        print("WriteHashRT is kernel:" + WriteHashRT);
        print("csMain is kernel:" + csMain);

    }

    private void Update()
    {
        TickComputeShaderBind();



    }



    void SetComputeShader()
    {

        refTex = new RenderTexture(TexSize[0], TexSize[1], 24, RenderTextureFormat.ARGB32);

        refTex.enableRandomWrite = true;

       //refTex.autoGenerateMips = false;

       //refTex.useMipMap = true;

        refTex.Create();



        CurTex = new RenderTexture(TexSize[0], TexSize[1], 24, RenderTextureFormat.ARGB32);

        //CurTex.Create();
       

        HashTex = new RenderTexture(TexSize[0], TexSize[1], 24, RenderTextureFormat.RFloat);

        HashTex.enableRandomWrite = true;

        HashTex.Create();



        csMain = cs.FindKernel("CSMain");

        WriteHashRT = cs.FindKernel("WriteHashRT");

        Clear = cs.FindKernel("Clear");

        FillHoles = cs.FindKernel("FillHoles");




        float[] camSize = new float[] { TexSize[0], TexSize[1] };

        cs.SetFloats("_CameraSize", camSize);

      


        cs.SetTexture(FillHoles,"ResultColor", refTex);
        cs.SetTexture(FillHoles, "HashBuffer", HashTex);


        cs.SetTexture(csMain, "ResultColor", refTex);
        cs.SetTexture(csMain, "HashBuffer", HashTex);
        cs.SetTexture(csMain, "_CurrentTexture", CurTex);


        cs.SetTexture(Clear, "ResultColor", refTex);
        cs.SetTexture(Clear, "HashBuffer", HashTex);


        

        cs.SetTexture(WriteHashRT, "HashBuffer", HashTex);
        cs.SetTexture(WriteHashRT, "ResultColor", refTex);




    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, CurTex);




        cs.Dispatch(Clear, TexSize[0]/8, TexSize[1]/8, 1);


        //RenderTexture depth = new RenderTexture(TexSize[0], TexSize[1], 24, RenderTextureFormat.RFloat);

        //Texture tmpDepth = Shader.GetGlobalTexture("_CameraDepthTexture");

        //Graphics.Blit(tmpDepth, depth);

        //cs.SetTexture(WriteHashRT, "_DepthTexture", depth);


        cs.SetTextureFromGlobal(WriteHashRT, "_DepthTexture", "_CameraDepthTexture");


        cs.Dispatch(WriteHashRT, TexSize[0] / 8, TexSize[1] / 8, 1);


        cs.Dispatch(csMain, TexSize[0] / 8, TexSize[1] / 8, 1);


       cs.Dispatch(FillHoles, TexSize[0] / 8, TexSize[1] / 8, 1);


        Graphics.Blit(source, destination);








    }


    void TickComputeShaderBind()
    {

        cs.SetFloat("_PlaneHeight", WaterHeight);

        _VPMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;

        cs.SetMatrix("_VPMatrix", _VPMatrix);


        cs.SetMatrix("I_VPMatrix", _VPMatrix.inverse);


        cs.SetVector("_CameraDir", cam.transform.forward);

        float[] _CameraTransformForward = new float[]
        {
            cam.transform.forward.x,
            cam.transform.forward.y,
            cam.transform.forward.z,
            0.0f
        };

        cs.SetFloats("_CameraDir", _CameraTransformForward);

        cs.SetFloat("_Threshold", Threshold);

        cs.SetFloat("_Intensity", Intensity);


        cs.SetFloat("_offsetMin", OffsetMin);

        cs.SetBool("_IsFill", IsFill);










    }
}
