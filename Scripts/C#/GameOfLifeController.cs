using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameOfLifeController : MonoBehaviour
{
    public Button GabenBtn;
    public Camera MainCam;
    public ComputeShader GameOfLifeComputer;

    public Texture2D InitalTexture;
    public RenderTexture OutputTex;
    public RenderTexture RndTex1;
    public RenderTexture RndTex2;

    private int RenderTexResolution;
    private int ComputeId = -1;
    private bool Swap = false;

    public Material mainMat;
    void Start()
    {
        InitBoids();
    }
    void Update()
    {
        CameraZoom();
        mainMat.SetTexture("_MainTex", OutputTex);
    }

    public void StartGaben()
    {
        StartCoroutine(Gaben());
        GabenBtn.gameObject.SetActive(false);
    }

    private IEnumerator Gaben()
    {
        while(true)
        {
            ComputeBoids();
            yield return new WaitForSeconds(Time.deltaTime);
        }
        yield return null;
    }

    private Vector3 MousePosOld;
    private Vector3 MousePosNew;
    private float OrthoSize = 1f;
    public float CameraSpeed = 5f;
    public float CameraZoomSpeed = 200f;
    void CameraZoom()
    {
        ////drag za move
        //MousePosNew = Input.mousePosition;
        //if (Input.GetKey(KeyCode.Mouse0))
        //    MainCam.transform.Translate((MousePosOld - MousePosNew)* CameraSpeed * Time.deltaTime);
        //MousePosOld = MousePosNew;

        //mouse scroll za zoom
       // float Temp = Input.GetAxis("Mouse ScrollWheel");

        OrthoSize +=( ((!Input.GetKey(KeyCode.Mouse1))?1f:6f)-OrthoSize)*CameraZoomSpeed  * Time.deltaTime;
        OrthoSize = Mathf.Clamp(OrthoSize, 1f, 5f);

        MainCam.orthographicSize = OrthoSize;
    }




    private void InitBoids()
    {
        RenderTexResolution = InitalTexture.width;

        ComputeId = GameOfLifeComputer.FindKernel("ComputeGame");
        int flash = GameOfLifeComputer.FindKernel("FlashInput");

        //init RNDTX
        RndTex1 = new RenderTexture(RenderTexResolution, RenderTexResolution, 32);
        RndTex1.format = RenderTextureFormat.ARGB32;
        RndTex1.enableRandomWrite = true;
        RndTex1.Create();

        RndTex2 = new RenderTexture(RenderTexResolution, RenderTexResolution, 32);
        RndTex2.format = RenderTextureFormat.ARGB32;
        RndTex2.enableRandomWrite = true;
        RndTex2.Create();

        OutputTex = new RenderTexture(RenderTexResolution, RenderTexResolution, 32);
        OutputTex.format = RenderTextureFormat.ARGB32;
        OutputTex.enableRandomWrite = true;
        OutputTex.Create();


        //flash
        GameOfLifeComputer.SetTexture(flash, "OutputTex", OutputTex);
        GameOfLifeComputer.SetTexture(flash, "InputTex", InitalTexture);
        GameOfLifeComputer.SetTexture(flash, "RndTex1", RndTex1);
        GameOfLifeComputer.SetTexture(flash, "RndTex2", RndTex2);


        //////result
        GameOfLifeComputer.SetTexture(ComputeId, "InputTex", InitalTexture);
        GameOfLifeComputer.SetTexture(ComputeId, "OutputTex", OutputTex);
        GameOfLifeComputer.SetTexture(ComputeId, "RndTex1", RndTex1);
        GameOfLifeComputer.SetTexture(ComputeId, "RndTex2", RndTex2);

        //setparams
        GameOfLifeComputer.SetFloat("TextureSize", RenderTexResolution);

       
        GameOfLifeComputer.Dispatch(flash, RenderTexResolution / 8, RenderTexResolution/8, 1);

    }



    private void ComputeBoids()
    {
        Swap = !Swap;
        if (Swap)
        {
            GameOfLifeComputer.SetTexture(ComputeId, "RndTex1", RndTex1);
            GameOfLifeComputer.SetTexture(ComputeId, "RndTex2", RndTex2);
        }
        else
        {
            GameOfLifeComputer.SetTexture(ComputeId, "RndTex1", RndTex2);
            GameOfLifeComputer.SetTexture(ComputeId, "RndTex2", RndTex1);
        }

        GameOfLifeComputer.Dispatch(ComputeId, RenderTexResolution / 8, RenderTexResolution / 8, 1);
    }

}
