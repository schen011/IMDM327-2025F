using UnityEngine;

[System.Serializable]
public struct BodyProperty
{
    public float mass;
    public float distance;
    public float initial_velocity;
    public Vector3 acceleration;
    public Vector3 velocity;
}

public class DataCSV : MonoBehaviour
{
    public BodyProperty[] bp;
    public GameObject[] body;

    // [Range(0.000000001f, 1f)]
    public float scale;
    // public float massScale = 1f;
    public float G;

    TrailRenderer trailRenderer;


    void Start()
    {
        scale = 1e-10f;
        G = 10f;
        // massScale = 1e-5f;
        // sets up bp
        LoadIntoArray();
        for (int i = 0; i < bp.Length; i++)
        {
            bp[i].acceleration = new Vector3(0f, 0f, 0f);
        }
        SpawnPlanets();

    }

    void Update()
    {
        // How to make them move over the time

    //    for (int i = 0; i < bp.Length; i++)
            // bp[i].acceleration = new Vector3(0f, 0f, 0f); /// fix this?
        
            
        for (int i = 0; i < bp.Length; i++)
        {
            //calculate acceleration
                for (int j = i + 1; j < bp.Length; j++)
                {
                    Vector3 diff = body[j].transform.position - body[j].transform.position;
                    float dist = diff.magnitude + 0.001f;
                    Vector3 direction = diff.normalized;
                    
                    float m1 = bp[i].mass;
                    float m2 = bp[j].mass;

                    // F = G * m1 * m2 / r^2
                    float gravity = G * m1 * m2 / (dist * dist);

                    bp[i].acceleration += (gravity * direction) / m1;
                    bp[j].acceleration -= (gravity * direction) / m2;
                    
                }
            
            bp[i].velocity += bp[i].acceleration * 5000;
            body[i].transform.position += bp[i].velocity * 5000;

        }
    }

    void SpawnPlanets()
    {
        if (bp == null) Debug.LogError("bp not yet initalized.");


        body = new GameObject[bp.Length];

        for (int i = 0; i < bp.Length; i++)
        {
            body[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // initial position
            float theta = Random.Range(0, 2 * Mathf.PI);
            float x = bp[i].distance * Mathf.Cos(theta) * scale;
            float y = bp[i].distance * 0;
            float z = bp[i].distance * Mathf.Sin(theta) * scale;
            Vector3 pos = new Vector3(x, y, z);

            body[i].transform.position = pos;

            Debug.Log($"Theta: {theta}\nBody {i}: {x},{y},{z}; {bp[i].distance}\n");

            // change scale
            // body[i].transform.localScale = new Vector3(bp[i].mass * massScale,
            //                                            bp[i].mass * massScale,
            //                                            bp[i].mass * massScale);

            // initial color
            {
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
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(Mathf.Cos(Mathf.PI * 2 / bp.Length * i), Mathf.Sin(Mathf.PI * 2 / bp.Length * i), Mathf.Tan(Mathf.PI * 2 / bp.Length * i)), 0.80f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                trailRenderer.colorGradient = gradient;
            }
        }
    }

    void LoadIntoArray()
    {
        // Load Assets/Resources/solar.csv (omit extension)
        // https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Resources.Load.html
        TextAsset csv = Resources.Load<TextAsset>("solar");
        if (csv == null) // - Safer
        {
            Debug.LogError("Resources/solar.csv not found.");
            bp = new BodyProperty[0];
            return;
        }
        string[] lines = csv.text.Split('\n'); // \n = line feed

        // Allocate array with read values
        bp = new BodyProperty[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            // - Safer: Trim() is used to remove whitespace or specific characters
            string[] cols = line.Split(',');

            if (float.TryParse(cols[2].Trim(), out float mass) &&
                float.TryParse(cols[4].Trim(), out float dist) &&
                float.TryParse(cols[5].Trim(), out float vel))
            {
                // assignment into array
                bp[i].mass = mass;
                bp[i].distance = dist;
                bp[i].initial_velocity = vel;
            }
        }
    }
}
