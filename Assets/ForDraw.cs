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
    private float timeflow = 0;
    float mass = 1f;
    float radius = 0.2f;
    float G = 0.5f;
    BodyProperty[] b;
    TrailRenderer trailRenderer;



    void Start()
    {
        body = new GameObject[numberOfSphere];
        b = new BodyProperty[numberOfSphere];

        // Loop generating the gameobject and assign initial conditions 
        for (int i = 0; i < numberOfSphere; i++)
        {
            // Our gameobjects are created here:
            body[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere); // why sphere? try different options.
                                                                        // https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html

            // initial position
            float x = radius * Random.Range(-50f, 20f);
            float y = radius * Random.Range(-5f, 0f);
            float z = radius * Random.Range(-50f, 20f);
            Vector3 pos = new Vector3(x, y, z);

            body[i].transform.position = pos;

            b[i].position = pos;
            b[i].mass = mass;

            // initial color
            var meshRenderer = body[i].GetComponent<Renderer>();
            meshRenderer.material.SetColor("_Color", new Color(Random.Range(0f, 255f) / 255f, 1f, .5f));
            
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
                new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color (Mathf.Cos(Mathf.PI * 2 / numberOfSphere * i), Mathf.Sin(Mathf.PI * 2 / numberOfSphere * i), Mathf.Tan(Mathf.PI * 2 / numberOfSphere * i)), 0.80f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trailRenderer.colorGradient = gradient;
        }
    }

    
    void Update()
    {
        timeflow += Time.deltaTime;
        // How to make them move over the time

       for (int i = 0; i < numberOfSphere; i++)
            b[i].acceleration = new Vector3(0f, 0f, 0f);
        
            
        for (int i = 0; i < numberOfSphere; i++)
        {

            //calculate acceleration
                for (int j = i+1; j < numberOfSphere; j++)
                {

                    // F = G * m1 * m2 / r^2
                    float distance = Vector3.Distance(body[i].transform.position, body[j].transform.position);
                    float gravity = G * b[i].mass * b[j].mass / (distance * distance); 
                    // Vector3 direction = Vector3.Normalize(body[j].transform.position - body[i].transform.position);
                    b[i].acceleration += gravity/b[i].mass * Vector3.Normalize(body[j].transform.position - body[i].transform.position);
                    b[j].acceleration += gravity/b[j].mass * Vector3.Normalize(body[i].transform.position - body[j].transform.position);

                    Debug.Log($"V: {b[i].velocity}\naccel: {b[i].acceleration}\n del time: {Time.deltaTime}");
                }
            
            b[i].velocity += b[i].acceleration * Time.deltaTime;
            // b[i].position += b[i].velocity * Time.deltaTime;
            body[i].transform.position += b[i].velocity;


        }

    }
}


