using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryScene : MonoBehaviour
{
    public static int State;
    public static bool Prompt;
    public static Vector3 Target;
    string sn;
    Transform egg, baby, mum;
    Transform[] items = new Transform[5];
    bool[] used = new bool[5];
    Vector3 mumGo, babyGo;
    bool lit;
    int got, fed;
    bool carrying;
    float tAt = -1f, flash;
    GameObject heart;
    float heartAt = -1f;
    Vector3 eggBase;

    static Transform Get(string n)
    {
        foreach (Transform t in Resources.FindObjectsOfTypeAll<Transform>())
            if (t.name == n && t.gameObject.scene.IsValid()) return t;
        return null;
    }

    static bool SceneExists(string name)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            if (SceneUtility.GetScenePathByBuildIndex(i).EndsWith("/" + name + ".unity")) return true;
        return false;
    }

    void Start()
    {
        sn = SceneManager.GetActiveScene().name;
        State = 0;
        Prompt = false;
        egg = Get("StoryEgg");
        baby = Get("adityosauras");
        mum = Get("parinosauras");
        for (int i = 0; i < 5; i++) items[i] = Get("Item" + i);
        if (sn == "Cave") { Transform m0 = Get("SettleMum"); Transform b0 = Get("SettleBaby"); if (m0 != null) mumGo = m0.position; if (b0 != null) babyGo = b0.position; }
        if (egg != null) eggBase = egg.position;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    static float Flat(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    void Face(Transform t, Vector3 p)
    {
        Vector3 d = p - t.position;
        d.y = 0f;
        if (d.sqrMagnitude > 0.01f) t.rotation = Quaternion.Slerp(t.rotation, Quaternion.LookRotation(d), 5f * Time.deltaTime);
    }

    void Next(string scene)
    {
        if (SceneExists(scene)) SceneManager.LoadScene(scene);
    }

    void Tune()
    {
        int sr = 22050;
        float[] notes = { 523.25f, 659.25f, 783.99f, 659.25f, 587.33f, 698.46f, 880f, 698.46f, 659.25f, 783.99f, 1046.5f, 783.99f, 880f, 783.99f, 659.25f, 523.25f };
        float len = 0.55f;
        float[] d = new float[(int)(sr * len * notes.Length)];
        for (int n = 0; n < notes.Length; n++)
        {
            int s0 = (int)(n * len * sr);
            int cnt = (int)(len * sr);
            for (int i = 0; i < cnt; i++)
            {
                float t = i / (float)sr;
                float env = Mathf.Exp(-3f * t) * Mathf.Clamp01(i / 200f);
                float v = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * 0.5f + Mathf.Sin(2f * Mathf.PI * notes[n] * 2f * t) * 0.15f;
                float bass = n % 4 == 0 ? Mathf.Sin(2f * Mathf.PI * notes[n] * 0.25f * t) * 0.3f * Mathf.Exp(-1.2f * t) : 0f;
                d[s0 + i] = (v * env + bass) * 0.6f;
            }
        }
        AudioClip c = AudioClip.Create("tune", d.Length, 1, sr, false);
        c.SetData(d, 0);
        AudioSource a = gameObject.AddComponent<AudioSource>();
        a.clip = c;
        a.spatialBlend = 0f;
        a.Play();
    }

    void Update()
    {
        Prompt = false;
        if (flash > 0f) flash -= Time.deltaTime * 1.6f;
        PlayerController pc = GetComponent<PlayerController>();
        UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
        if (sn == "Hatch") UpdateHatch();
        else if (sn == "Care") UpdateCare();
        else if (sn == "Cave") UpdateCave(kb);
        if (heart != null)
        {
            float k = (Time.time - heartAt) / 1.5f;
            heart.transform.position += Vector3.up * 1.2f * Time.deltaTime;
            heart.transform.localScale = Vector3.one * Mathf.Lerp(0.6f, 0.1f, k);
            if (k >= 1f) { Destroy(heart); heart = null; }
        }
    }

    void UpdateHatch()
    {
        if (egg == null) return;
        Target = eggBase;
        if (State == 0)
        {
            if (Flat(transform.position, eggBase) < 6.5f) { State = 1; tAt = Time.time; Debug.Log("STORY hatch begins"); }
        }
        else if (State == 1)
        {
            float t = Time.time - tAt;
            egg.position = eggBase + new Vector3(Mathf.Sin(t * 40f) * 0.08f * t, 0f, 0f);
            egg.rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 30f) * 6f * t);
            if (t > 3.2f)
            {
                State = 2;
                tAt = Time.time;
                flash = 1f;
                egg.position = eggBase;
                egg.rotation = Quaternion.identity;
                egg.gameObject.SetActive(false);
                for (int i = 0; i < 6; i++)
                {
                    GameObject s = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    s.name = "Shell" + i;
                    float a = i * 1.047f;
                    s.transform.position = eggBase + new Vector3(Mathf.Cos(a) * 0.9f, 0.25f, Mathf.Sin(a) * 0.9f);
                    s.transform.localScale = new Vector3(0.7f, 0.18f, 0.5f);
                    s.transform.rotation = Quaternion.Euler(Random.Range(-20f, 20f), a * 57f, Random.Range(-20f, 20f));
                    s.GetComponent<Renderer>().sharedMaterial = egg.GetComponent<Renderer>().sharedMaterial;
                }
                if (baby != null)
                {
                    baby.gameObject.SetActive(true);
                    baby.position = eggBase + new Vector3(0f, 0f, -1.2f);
                    baby.localScale = Vector3.one * 0.15f;
                }
                Debug.Log("STORY baby emerged");
            }
        }
        else if (State == 2)
        {
            float t = Time.time - tAt;
            if (baby != null)
            {
                baby.localScale = Vector3.one * Mathf.Lerp(0.15f, 1f, Mathf.Clamp01(t / 2.5f));
                Face(baby, transform.position);
            }
            if (t > 6f) { State = 3; Debug.Log("STORY hatch complete"); Next("Care"); }
        }
    }

    void UpdateCare()
    {
        if (baby == null) return;
        float db = Flat(transform.position, baby.position);
        if (db > 5f)
        {
            Face(baby, transform.position);
            baby.position += baby.forward * 3f * Time.deltaTime;
        }
        else Face(baby, transform.position);
        if (State == 0)
        {
            int nxt = -1;
            for (int i = 0; i < 3; i++) if (!used[i] && items[i] != null) { nxt = i; break; }
            if (!carrying && nxt >= 0)
            {
                Target = items[nxt].position;
                if (Flat(transform.position, items[nxt].position) < 1.8f)
                {
                    used[nxt] = true;
                    items[nxt].gameObject.SetActive(false);
                    carrying = true;
                }
            }
            if (carrying)
            {
                Target = baby.position;
                if (db < 4f)
                {
                    carrying = false;
                    fed++;
                    baby.localScale *= 1.14f;
                    heart = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    Destroy(heart.GetComponent<Collider>());
                    heart.transform.position = baby.position + Vector3.up * 3f;
                    Material m = new Material(egg != null ? egg.GetComponent<Renderer>().sharedMaterial : heart.GetComponent<Renderer>().sharedMaterial);
                    m.color = new Color(1f, 0.35f, 0.5f);
                    heart.GetComponent<Renderer>().sharedMaterial = m;
                    heartAt = Time.time;
                    Debug.Log("STORY fed " + fed);
                    if (fed >= 3) { State = 1; tAt = Time.time; Debug.Log("STORY care complete"); }
                }
            }
        }
        else if (State == 1)
        {
            if (Time.time - tAt > 9f) { State = 2; Next("Cave"); }
        }
    }

    void Debris(Vector3 p)
    {
        for (int i = 0; i < 7; i++)
        {
            GameObject d = GameObject.CreatePrimitive(PrimitiveType.Cube);
            d.transform.position = p + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(0.5f, 3f), Random.Range(-0.5f, 0.5f));
            d.transform.localScale = Vector3.one * Random.Range(0.25f, 0.6f);
            d.transform.rotation = Random.rotation;
            Renderer r = d.GetComponent<Renderer>();
            Transform src = Get("CaveRock");
            if (src != null) r.sharedMaterial = src.GetComponent<Renderer>().sharedMaterial;
            d.AddComponent<Rigidbody>();
            Destroy(d, 7f);
        }
    }

    void UpdateCave(UnityEngine.InputSystem.Keyboard kb)
    {
        if (State == 0)
        {
            int nxt = -1;
            for (int i = 0; i < 5; i++) if (!used[i] && items[i] != null) { nxt = i; break; }
            if (nxt >= 0)
            {
                Target = items[nxt].position;
                if (Flat(transform.position, items[nxt].position) < 4.2f)
                {
                    Prompt = true;
                    if (kb != null && kb.fKey.wasPressedThisFrame)
                    {
                        used[nxt] = true;
                        Debris(items[nxt].position);
                        items[nxt].gameObject.SetActive(false);
                        got++;
                        flash = 0.5f;
                        if (!lit)
                        {
                            lit = true;
                            for (int k = 0; k < 3; k++) { Transform bl = Get("BackLight" + k); if (bl != null) bl.gameObject.SetActive(true); }
                        }
                        Debug.Log("STORY dug " + got);
                    }
                }
            }
            if (got >= 5)
            {
                State = 1;
                tAt = Time.time;
                Tune(); Vantage();
                Debug.Log("STORY cave opened, family settling, tune playing");
            }
        }
        else if (State == 1)
        {
            float step = 2.2f * Time.deltaTime;
            if (mum != null)
            {
                Face(mum, mumGo);
                mum.position = Vector3.MoveTowards(mum.position, mumGo, step);
            }
            if (baby != null)
            {
                Face(baby, babyGo);
                baby.position = Vector3.MoveTowards(baby.position, babyGo, step * 1.2f);
            }
            if (Time.time - tAt > 12f) { State = 3; tAt = Time.time; StartNoHome(); }
        }
        else if (State == 3)
        {
            float t = Time.time - tAt;
            float k = Mathf.Clamp01(1f - t / 3.5f);
            if (wayHome != null) wayHome.transform.localScale = Vector3.one * Mathf.Max(0.01f, 2.2f * k);
            if (wayLight != null) wayLight.intensity = 7f * k;
            if (t > 3.5f && wayHome != null)
            {
                Debris(wayHome.transform.position);
                Destroy(wayHome);
                if (wayLight != null) Destroy(wayLight.gameObject);
                flash = 0.6f;
                Debug.Log("STORY way home collapsed");
            }
            if (t > 5f) { State = 2; Debug.Log("STORY cave complete"); Next("Mission6"); }
        }
    }

    GameObject wayHome;
    Light wayLight;

    void StartNoHome()
    {
        Transform fire = Get("Fire");
        if (fire == null) { State = 2; Next("Mission6"); return; }
        Vector3 cc0 = fire.position + new Vector3(0f, -0.5f, -9f);
        Vector3 gp = cc0 + new Vector3(0f, 2.2f, -10f);
        Material g = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        g.EnableKeyword("_EMISSION");
        g.SetColor("_EmissionColor", new Color(0.3f, 0.7f, 1f) * 3.5f);
        wayHome = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        wayHome.name = "WayHome";
        Collider col = wayHome.GetComponent<Collider>();
        if (col != null) Destroy(col);
        wayHome.transform.position = gp;
        wayHome.transform.localScale = Vector3.one * 2.2f;
        wayHome.GetComponent<Renderer>().sharedMaterial = g;
        GameObject lg = new GameObject("WayHomeLight");
        lg.transform.position = gp;
        wayLight = lg.AddComponent<Light>();
        wayLight.type = LightType.Point;
        wayLight.color = new Color(0.4f, 0.7f, 1f);
        wayLight.intensity = 7f;
        wayLight.range = 18f;
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = cc0 + new Vector3(0f, 1.2f, -2f);
        Vector3 d = gp - transform.position;
        d.y = 0f;
        transform.rotation = Quaternion.LookRotation(d);
        PlayerController pq = GetComponent<PlayerController>();
        if (pq != null && pq.cameraTransform != null) pq.cameraTransform.localRotation = Quaternion.Euler(10f, 0f, 0f);
        if (cc != null) cc.enabled = true;
        Debug.Log("STORY no way home beat");
    }

    void Vantage(){ CharacterController cc = GetComponent<CharacterController>(); if (cc != null) cc.enabled = false; transform.position = new Vector3(mumGo.x - 14f, transform.position.y, mumGo.z - 16f); Vector3 d = (mumGo + new Vector3(-3f, 0f, 1f)) - transform.position; d.y = 0f; transform.rotation = Quaternion.LookRotation(d); PlayerController pq = GetComponent<PlayerController>(); if (pq != null && pq.cameraTransform != null) pq.cameraTransform.localRotation = Quaternion.Euler(6f, 0f, 0f); if (cc != null) cc.enabled = true; } void OnGUI()
    {
        float k = Screen.width / 1024f;
        Matrix4x4 old = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(k, k, 1));
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.fontSize = 20;
        s.alignment = TextAnchor.MiddleCenter;
        s.normal.textColor = Color.white;
        GUI.color = new Color(0, 0, 0, 0.55f);
        GUI.DrawTexture(new Rect(0, 0, 1024, 36), Texture2D.whiteTexture);
        GUI.color = Color.white;
        string title = sn == "Hatch" ? "THE EGG" : (sn == "Care" ? "LOOK AFTER THE BABY" : "THE CAVE");
        string extra = "";
        if (sn == "Care") extra = "  FED " + fed + " / 3";
        if (sn == "Cave") extra = "  DUG " + got + " / 5";
        GUI.Label(new Rect(0, 4, 1024, 28), title + extra, s);
        if (sn == "Cave" && State == 3)
        {
            s.fontSize = 30;
            GUI.Label(new Rect(62, (Screen.height / k) * 0.45f, 900, 40), "THE WAY HOME IS GONE", s);
        }
if (sn == "Care" && State == 1) { s.fontSize = 24; GUI.Label(new Rect(62, (Screen.height / k) * 0.4f, 900, 70), "AARYAN: now I'm stuck here forever, I have to keep this as a happy family,\nand give them a safe place to live", s); }     
        if (Prompt)
        {
            s.fontSize = 26;
            GUI.Label(new Rect(62, (Screen.height / k) * 0.62f, 900, 34), "PRESS F TO DIG", s);
        }
        if (flash > 0f)
        {
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(flash));
            GUI.DrawTexture(new Rect(0, 0, 1024, Screen.height / k), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        GUI.matrix = old;
    }
}
