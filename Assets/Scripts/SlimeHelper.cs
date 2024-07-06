using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeHelper : MonoBehaviour
{
    public GameObject obj;
    Renderer ren;
    RenderTexture TrailMapTex, ProcessedTrailMap;
    public ComputeShader shader;
    public int width, height, agentNumber;

    [Header("AGENTS")]
    public float moveSpeed = 1f;
    public float turnSpeed;
    public float sensorAngleSpacing;
    public float sensorOffsetDst;
    public int sensorSize;
    
    public Agent[] agents;

    bool init;

    public useMap MapUsed;

    bool t;

    [Header("Procesing")][SerializeField]
    float evaporateSpeed, diffuseSpeed;

    public enum useMap
    {
        TrailMapTex, ProcessedMap
    }

    ComputeBuffer agentBuffer;
    [System.Serializable]
    public struct Agent
    {
        public Vector2 position;
        public float angle;
    }

    private void OnEnable()
    {
        Init();
    }

    void Init()
    {
        int posSize = sizeof(float) * 2;
        int angleSize = sizeof(float);
        int stride = posSize + angleSize;

        agentBuffer = new ComputeBuffer(agentNumber, stride);

        //SETTINGS
        //SETTINGS
        shader.SetInt("width", width);
        shader.SetInt("height", height);

        shader.SetFloat("moveSpeed", moveSpeed);
        shader.SetFloat("turnSpeed", turnSpeed);
        shader.SetFloat("sensorAngleSpacing", sensorAngleSpacing);
        shader.SetInt("sensorSize", sensorSize);
        shader.SetFloat("sensorOffsetDst", sensorOffsetDst);

        shader.SetFloat("evaporateSpeed", evaporateSpeed);
        shader.SetFloat("diffuseSpeed", diffuseSpeed);

        TrailMapTex = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);
       
                
        ren = obj.GetComponent<Renderer>();
        TrailMapTex.enableRandomWrite = true;
        TrailMapTex.filterMode = FilterMode.Point;

        if (MapUsed == useMap.TrailMapTex)
        {
            ren.material.mainTexture = TrailMapTex;
        }
        TrailMapTex.Create();
        shader.SetTexture(0, "TrailMap", TrailMapTex);
        shader.SetTexture(1, "TrailMap", TrailMapTex);
        
        ProcessedTrailMap = new RenderTexture(width, height, 16, RenderTextureFormat.ARGB32);

        ProcessedTrailMap.enableRandomWrite = true;
        ProcessedTrailMap.filterMode = FilterMode.Point;

        if (MapUsed == useMap.ProcessedMap)
        {
            ren.material.mainTexture = ProcessedTrailMap;
        }

        ProcessedTrailMap.Create();
        shader.SetTexture(1, "ProcessedTrailMap", ProcessedTrailMap);
        shader.SetTexture(0, "ProcessedTrailMap", ProcessedTrailMap);


        agents = new Agent[agentNumber];

        popAgents();
    }

    void popAgents()
    {
        for (int i = 0; i < agentNumber; i++)
        {
            
            Vector2 position = new Vector2(width /2,height/2);
            float angle = Random.value * Mathf.PI * 2f;

            agents[i] = new Agent()
            {
                position = position,
                angle = angle,
            };
        }

        
        
        agentBuffer.SetData(agents);

        shader.SetBuffer(0, "agents", agentBuffer);
        shader.SetInt("numAgents", agentNumber);
        init = true;
        //agentBuffer.Dispose();        
    }
    
    private void Update()
    {
        //SETTINGS
        shader.SetInt("width", width);
        shader.SetInt("height", height);
       
        shader.SetFloat("moveSpeed", moveSpeed);
        shader.SetFloat("turnSpeed", turnSpeed);
        shader.SetFloat("sensorAngleSpacing", sensorAngleSpacing);
        shader.SetInt("sensorSize", sensorSize);
        shader.SetFloat("sensorOffsetDst", sensorOffsetDst);

        shader.SetFloat("evaporateSpeed", evaporateSpeed);
        shader.SetFloat("diffuseSpeed", diffuseSpeed);
        


        if (!init) { return; }
        //Debug.Log("dispatching");
        shader.SetFloat("deltaTime", Time.deltaTime);
        shader.SetFloat("time", Time.time);

        shader.Dispatch(0, agentNumber / 16, 1, 1);

        

        shader.Dispatch(shader.FindKernel("ProcessTrailMap"), width / 8, height / 8, 1);

        Graphics.Blit(ProcessedTrailMap, TrailMapTex);
        //Graphics.Blit(TrailMapTex, ProcessedTrailMap);

    }

    //void LateUpdate()
    //{
    //    shader.Dispatch(shader.FindKernel("ProcessTrailMap"), width / 8, height / 8, 1);
    //
    //    Graphics.Blit(ProcessedTrailMap, TrailMapTex);
    //}

    void OnDisable()
    {
        agentBuffer.Release();
    }
}
