using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForDraw : MonoBehaviour
{
    struct BodyProperty
    {
        public float mass;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 acceleration;
    }
    GameObject[] body;
    public Material[] material;
    private int numberOfSphere = 100;
    float mass = 1f;
    float radius = 0.2f;
    float G = 0.5f;
    BodyProperty[] b;
    TrailRenderer trailRenderer;



    void Start()
    {
        // placing camera
        Transform camera = Camera.main.transform;

        camera.position = new Vector3(0, 30, 0);
        camera.rotation = Quaternion.Euler(new Vector3(90,0,0));

        // spawning bodies
        body = new GameObject[numberOfSphere];
        b = new BodyProperty[numberOfSphere];

        // Loop generating the gameobject and assign initial conditions 
        for (int i = 0; i < numberOfSphere; i++)
        {
            // Our gameobjects are created here:
            body[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere); // why sphere? try different options.
                                                                        // https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html

            // initial position
            float x = radius * Random.Range(-50f, 50f);
            float y = radius * Random.Range(-5f, 5f);
            float z = radius * Random.Range(-50f, 50f);

            Vector3 position = new Vector3(x, y, z);
            body[i].transform.position = position;

            // initializing b fields
            b[i].position = position;
            b[i].mass = mass;

            // initial color
            var meshRenderer = body[i].GetComponent<Renderer>();
            meshRenderer.material.SetColor("_Color", new Color(Random.Range(0f, 255f) / 255f,
                                                               Random.Range(0f, 255f) / 255f,
                                                               Random.Range(0f, 255f) / 255f));

            // + This is just pretty trails
            trailRenderer = body[i].AddComponent<TrailRenderer>();
            // Configure the TrailRenderer's properties
            trailRenderer.time = 100.0f;  // Duration of the trail
            trailRenderer.startWidth = 0.5f;  // Width of the trail at the start
            trailRenderer.endWidth = 0.1f;    // Width of the trail at the end
            // a material to the trail
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            // Set the trail color over time
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(Mathf.Cos(Mathf.PI * 2 / numberOfSphere * i), Mathf.Sin(Mathf.PI * 2 / numberOfSphere * i), Mathf.Tan(Mathf.PI * 2 / numberOfSphere * i)), 0.80f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trailRenderer.colorGradient = gradient;
        }
    }


    void Update()
    {
        // initailize acceleration
        for (int i = 0; i < numberOfSphere; i++)
        {
            b[i].acceleration = Vector3.zero; // important
        }

        // ACCELERATION
        for (int i = 0; i < numberOfSphere; i++)
        {
            for (int j = i + 1; j < numberOfSphere; j++)
            {
                Vector3 distance = body[j].transform.position - body[i].transform.position;
                // Gravity
                Vector3 gravity = CalculateGravity(distance, b[i].mass, b[j].mass);
                // Apply Gravity
                // F = ma -> a = F/m
                // Gravity is push and pull with same amount. Force: m1 <-> m2
                b[i].acceleration += gravity / b[i].mass; // why is this +?
                b[j].acceleration -= gravity / b[j].mass; 

                // Debug.Log($"V: {b[i].velocity}\naccel: {b[i].acceleration}\n del time: {Time.deltaTime}");
            }
            b[i].velocity += b[i].acceleration * Time.deltaTime;
            body[i].transform.position += b[i].velocity * Time.deltaTime;

        }
    }
    
    private Vector3 CalculateGravity(Vector3 distanceVector, float m1, float m2)
    {
        Vector3 gravity = Vector3.zero; // note this is also Vector3
                                                   // **** Fill in the function below.
        float eps = 0.1f;                                                   
        gravity = G * m1 * m2 / (distanceVector.magnitude + eps) * distanceVector.normalized;
        return gravity;
    }
}


