using UnityEngine;

[System.Serializable]
public struct BodyProperty
{
    public float mass;
    public float distance;
    public float initial_velocity;
    public Vector3 acceleration;
    public Vector3 velocity;
    public Vector3 position;
}

public class DataCSV : MonoBehaviour
{
    public BodyProperty[] bp;
    public GameObject[] body;

    // [Range(0.000000001f, 1f)]
    public float G = 6.6743e-11f;
    public float fastforwardConst = 10000f;

    TrailRenderer trailRenderer;


    void Start()
    {
        G = 6.6743e-11f;
        fastforwardConst = 50000f;

        // sets up bp
        LoadIntoArray();
        Debug.Log($"bp length :{bp.Length}");

        // SPAWNING PLANETS        
        if (bp == null) Debug.LogError("bp not yet initalized.");

        body = new GameObject[bp.Length];

        for (int i = 0; i < bp.Length; i++)
        {
            // spawning spheres
            body[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);

            // setting initial position 
            float theta = Random.Range(0, 2 * Mathf.PI);
            float x = bp[i].distance * Mathf.Cos(theta);
            float y = bp[i].distance * 0;
            float z = bp[i].distance * Mathf.Sin(theta);

            bp[i].position = new Vector3(x, y, z);
            ApplyScaledPosition(i);

            //intial velocity
            bp[i].velocity = new Vector3(bp[i].initial_velocity * Mathf.Cos(theta + Mathf.PI/2), 0,
                                         bp[i].initial_velocity * Mathf.Sin(theta + Mathf.PI/2));

            // Debug.Log($"Theta: {theta}\nBody {i}: x:{x}, y:{y}, z:{z}; {bp[i].distance}");

            // change scale
            float scale = Mathf.Sqrt(bp[i].mass / 1e28f);
            body[i].transform.localScale = new Vector3(scale, scale, scale);
            // body[i].transform.localScale = new Vector3(10,10,10);

            // Trail Renderer
            {
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
                    new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(Mathf.Cos(Mathf.PI * 2 / bp.Length * i), Mathf.Sin(Mathf.PI * 2 / bp.Length * i), Mathf.Tan(Mathf.PI * 2 / bp.Length * i)), 0.80f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) }
                );
                trailRenderer.colorGradient = gradient;
            }
        }

    }

    void Update()
    {
        // How to make them move over the time

        for (int i = 0; i < bp.Length; i++)
            bp[i].acceleration = Vector3.zero; 


        for (int i = 0; i < bp.Length; i++)
        {
            //calculate acceleration
            for (int j = i + 1; j < bp.Length; j++)
            {
                Vector3 diff = bp[j].position - bp[i].position; 

                float dist = diff.magnitude + 0.001f;
                Vector3 direction = diff.normalized;

                float m1 = bp[i].mass;
                float m2 = bp[j].mass;

                // F = G * m1 * m2 / r^2
                float gravity = G * m1 * m2 / (dist * dist);

                // a = F/m
                bp[i].acceleration += (gravity * direction) / m1;
                bp[j].acceleration -= (gravity * direction) / m2;
            }

            bp[i].velocity += bp[i].acceleration * fastforwardConst;
            bp[i].position += bp[i].velocity * fastforwardConst;

            // applying scaled positions to body objects
            ApplyScaledPosition(i);
            
            Debug.Log($"Body {i} Pos - x: {body[i].transform.position.x}, y: {body[i].transform.position.y}, z: {body[i].transform.position.z}");
        }
    }

    void ApplyScaledPosition(int i)
    {   
        // scale distances and multiply by normalized vector (direction)
        float scaledDistance = Mathf.Sqrt(bp[i].position.magnitude / 1e8f);
        Vector3 directionForScale = bp[i].position.normalized;

        body[i].transform.position = scaledDistance * directionForScale;
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

            if (float.TryParse(cols[2].Trim(), out float mass) && //////////////////////////////////////error
                float.TryParse(cols[4].Trim(), out float dist) &&
                float.TryParse(cols[5].Trim(), out float vel))
            {
                // assignment into array
                bp[i].mass = mass * 1e22f;
                bp[i].distance = dist;
                bp[i].initial_velocity = vel;
            }
        }
    }
}
