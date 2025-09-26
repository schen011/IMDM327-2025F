// InteractiveBody Starter Code
// Fall 2025. IMDM 327
// Instructor. Myungin Lee
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class ThreeBody : MonoBehaviour
{
    public float G = 1f; // Gravity constant https://en.wikipedia.org/wiki/Gravitational_constant
    private GameObject[] body;
    BodyProperty[] bp;
    private int numberOfSphere = 200;
    public float fastforwardConst = 1f;
    TrailRenderer[] trailRenderer;

    // private GameObject interactivePoint;
    // public Vector3 interactPoint, previousInteractivePoint; // where to interact 
    // public float interactiveMass = 5f; // how much to interact

    struct BodyProperty // why struct?
    {                   // https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct
        public float mass;
        public Vector3 velocity;
        public Vector3 acceleration;
    }


    void Start()
    {
        // // interactive point 
        // interactivePoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // interactivePoint.transform.position = new Vector3(0f, 0f, 180f);

        
        // Just like GO, computer should know how many room for struct is required:
        bp = new BodyProperty[numberOfSphere];
        body = new GameObject[numberOfSphere];
        trailRenderer = new TrailRenderer[numberOfSphere];
        // Loop generating the gameobject and assign initial conditions (type, position, (mass/velocity/acceleration)
        for (int i = 0; i < numberOfSphere; i++)
        {
            // Our gameobjects are created here:
            body[i] = GameObject.CreatePrimitive(PrimitiveType.Cube); // why sphere? try different options.
            // https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html

            // initial conditions
            float r = 100f;
            // position is (x,y,z). In this case, I want to plot them on the circle with r
            // ******** Fill in this part ******** // Initialization of the position
            body[i].transform.position = new Vector3(r * Mathf.Sin(i * 2f * Mathf.PI / numberOfSphere),
                                                      r * Mathf.Cos(i * 2f * Mathf.PI / numberOfSphere),
                                                      180f + Random.Range(-10f, 10f));
            // z = 180 to see this happen in front of me. Try something else (randomize) too.

            bp[i].velocity = new Vector3(0, 0, 0); // Try different initial condition
            bp[i].mass = Random.Range(1f, 5f); // Simplified. Try different initial condition
            body[i].GetComponent<MeshRenderer>().enabled = false;

            // + This is just pretty trails
            trailRenderer[i] = body[i].AddComponent<TrailRenderer>();
            // Configure the TrailRenderer's properties
            trailRenderer[i].time = 20.0f;  // Duration of the trail
            trailRenderer[i].startWidth = 0.7f;  // Width of the trail at the start
            trailRenderer[i].endWidth = 0.1f;    // Width of the trail at the end
            // a material to the trail
            trailRenderer[i].material = new Material(Shader.Find("Sprites/Default"));
            // Set the trail color
            Gradient gradient = new Gradient();
            float h = (i / (float)numberOfSphere) % 1f;
            float s = 0.45f;        
            float v = 0.98f;        
            Color c = Color.HSVToRGB(h, s, v); 

            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(c, 0.0f),
                                        new GradientColorKey(c, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trailRenderer[i].colorGradient = gradient;

        }
    }

    void Update()
    {
        // // Draw interactivePoint
        // interactivePoint.transform.position = interactPoint;

        // Loop for N-body gravity
        // How should we design the loop?
        // initailize 
        for (int i = 0; i < numberOfSphere; i++)
        {
            bp[i].acceleration = Vector3.zero; // important
        }

        // Acceleration (Force)  
        for (int i = 0; i < numberOfSphere; i++)
        {
            for (int j = i + 1; j < numberOfSphere; j++)
            {
                // Vector from i to j body. Make sure which vector you are getting.
                Vector3 distance = body[j].transform.position - body[i].transform.position;
                // Gravity
                Vector3 gravity = CalculateGravity(distance, bp[i].mass, bp[j].mass);
                // Apply Gravity
                // F = ma -> a = F/m
                // Gravity is push and pull with same amount. Force: m1 <-> m2
                bp[i].acceleration += gravity / bp[i].mass; // why is this +?
                bp[j].acceleration -= gravity / bp[j].mass; // why is this -? What decided the direction?
            }
        }

        // // (Force) Hesitation: randomly hover the space for natural behavior  
        // for (int i = 0; i < numberOfSphere; i++)
        // {
        //     float randomScale = 3f;
        //     if (Random.Range(0f, 1.05f) > 1f)
        //     {
        //         bp[i].acceleration += new Vector3(randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f), randomScale * Random.Range(-1f, 1f));
        //     }
        // }

        // // (Force) Interactive Acceleration : reacts to the actuation of the interactive point
        // float actuation = (previousInteractivePoint - interactPoint).sqrMagnitude;
        // for (int i = 0; i < numberOfSphere; i++)
        // {
        //     // pull via interactPoint
        //     Vector3 distance = interactPoint - body[i].transform.position;
        //     // calculate additional acceleration
        //     bp[i].acceleration += CalculateGravity(distance, bp[i].mass, interactiveMass) / bp[i].mass * actuation;
        // }
        // previousInteractivePoint = interactPoint;
        // G = 1f + actuation * 0.01f;


        // Apply acceleration to velocity, to position
        for (int i = 0; i < numberOfSphere; i++)
        {
            // velocity is sigma(Acceleration*time)
            bp[i].velocity += bp[i].acceleration * Time.deltaTime * fastforwardConst;
            // Prevent extra ordinary speed
            body[i].transform.position += bp[i].velocity * Time.deltaTime * fastforwardConst;
            body[i].transform.LookAt(body[i].transform.position + bp[i].velocity);
        }

        // Color update
        for (int i = 0; i < numberOfSphere; i++)
        {
            // + This is just pretty trails
            Gradient gradient = new Gradient();
            float h = (i / (float)numberOfSphere) % 1f;
            float s = 0.3f + bp[i].acceleration.sqrMagnitude / 1000f;
            float v = 0.3f + bp[i].acceleration.sqrMagnitude / 1000f;
            Color c = Color.HSVToRGB(h, s, v);

            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(c, 0.0f),
                                        new GradientColorKey(c, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
            );
            trailRenderer[i].colorGradient = gradient;
        }

    }

    // Gravity Fuction
    private Vector3 CalculateGravity(Vector3 distanceVector, float m1, float m2)
    {
        Vector3 gravity = new Vector3(0f, 0f, 0f); // note this is also Vector3
                                                   // **** Fill in the function below.
        float eps = 0.1f;                                                   
        gravity = G * m1 * m2 / (distanceVector.magnitude + eps) * distanceVector.normalized;
        return gravity;
    }
}
