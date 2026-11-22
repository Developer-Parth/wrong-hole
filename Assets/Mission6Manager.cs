using UnityEngine;
using UnityEngine.SceneManagement;

public class Mission6Manager : MonoBehaviour
{
    public static int State;
    public static bool PromptVisible;
    Transform[] coffins;
    bool[] snapped;
    int photos;
    int state;
    float flash;
    float cineAt = -1f;
    bool noM1;
    Camera cam;
    PlayerController pc;
    Transform lastCoffin;
    float[] lidAt = new float[3] { -1f, -1f, -1f };
    float[] lidBase = new float[3];

    bool intro = true;
    float introAt;
    Vector3 spawnPos;
    Quaternion spawnRot;
    bool vistaSet;
    bool deadFlashed;
    bool interiorSet;
    bool arriveFlashed;

    void Start()
    {
        State = 0;
        PromptVisible = false;
        state = 0;
        photos = 0;
        flash = 0f;
        coffins = new Transform[3];
        snapped = new bool[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject g = GameObject.Find("Coffin" + (i + 1));
            if (g != null)
            {
                coffins[i] = g.transform;
                Transform lid0 = g.transform.Find("Lid");
                if (lid0 != null) lidBase[i] = lid0.position.y;
            }
        }
        cam = Camera.main;
        pc = GetComponent<PlayerController>();
        spawnPos = transform.position;
        spawnRot = transform.rotation;
        introAt = Time.time;
        if (pc != null) pc.enabled = false;
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = new Vector3(0f, -6f, 40f);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EnsureProps();
    }

    static Material Mat(Color c, float smooth)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        m.color = c;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
        return m;
    }

    static GameObject Prim(PrimitiveType ty, string name, Vector3 pos, Vector3 scale, Material m)
    {
        GameObject g = GameObject.CreatePrimitive(ty);
        g.name = name;
        Destroy(g.GetComponent<Collider>());
        g.transform.position = pos;
        g.transform.localScale = scale;
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }

    static Texture2D PlayerFaceTex()
    {
        int W = 128, H = 128;
        Color skin = new Color(0.87f, 0.67f, 0.53f);
        Color hair = new Color(0.08f, 0.06f, 0.05f);
        Color iris = new Color(0.3f, 0.18f, 0.1f);
        Texture2D t = new Texture2D(W, H, TextureFormat.RGBA32, false);
        for (int i = 0; i < W * H; i++) t.SetPixel(i % W, i / W, skin);
        System.Action<int, int, int, int, Color> disc = (cx, cy, rx, ry, c) =>
        {
            for (int y = cy - ry; y <= cy + ry; y++)
                for (int x = cx - rx; x <= cx + rx; x++)
                {
                    float dx = (x - cx) / (float)rx, dy = (y - cy) / (float)ry;
                    if (dx * dx + dy * dy <= 1f && x >= 0 && x < W && y >= 0 && y < H) t.SetPixel(x, y, c);
                }
        };
        System.Action<int, int, int, int, Color> rect = (x0, y0, w, h, c) =>
        {
            for (int y = y0; y < y0 + h; y++)
                for (int x = x0; x < x0 + w; x++)
                    if (x >= 0 && x < W && y >= 0 && y < H) t.SetPixel(x, y, c);
        };
        Color white = Color.white, black = new Color(0.05f, 0.04f, 0.04f);
        int ey = 64, ex = 19;
        foreach (int s in new int[] { -1, 1 })
        {
            int cx = 64 + s * ex;
            disc(cx, ey, 11, 9, white);
            disc(cx, ey, 5, 5, iris);
            disc(cx, ey, 2, 2, black);
            rect(cx - 9, ey + 10, 18, 3, black);
        }
        disc(64, 28, 12, 7, new Color(0.45f, 0.12f, 0.12f));
        disc(64, 31, 6, 3, black);
        rect(59, 46, 10, 8, new Color(skin.r * 0.85f, skin.g * 0.8f, skin.b * 0.8f));
        disc(64, 98, 44, 22, hair);
        disc(30, 84, 12, 16, hair);
        disc(98, 84, 12, 16, hair);
        t.Apply();
        return t;
    }

    void EnsureProps()
    {
        if (GameObject.Find("ShaftW1") == null)
        {
            Material shaftM = Mat(new Color(0.05f, 0.045f, 0.06f), 0f);
            Prim(PrimitiveType.Cube, "ShaftW1", new Vector3(3.5f, -51f, 40f), new Vector3(1f, 102f, 8f), shaftM);
            Prim(PrimitiveType.Cube, "ShaftW2", new Vector3(-3.5f, -51f, 40f), new Vector3(1f, 102f, 8f), shaftM);
            Prim(PrimitiveType.Cube, "ShaftW3", new Vector3(0f, -51f, 43.5f), new Vector3(8f, 102f, 1f), shaftM);
            Prim(PrimitiveType.Cube, "ShaftW4", new Vector3(0f, -51f, 36.5f), new Vector3(8f, 102f, 1f), shaftM);
            Prim(PrimitiveType.Cube, "ShaftBottom", new Vector3(0f, -102.5f, 40f), new Vector3(8f, 1f, 8f), shaftM);
            Material shardM = Mat(new Color(0.28f, 0.24f, 0.2f), 0f);
            Material glowShardM = Mat(new Color(0.95f, 0.75f, 0.2f), 0.6f);
            glowShardM.EnableKeyword("_EMISSION");
            glowShardM.SetColor("_EmissionColor", new Color(1.2f, 0.85f, 0.25f) * 1.6f);
            System.Random srng = new System.Random(11);
            for (int i = 0; i < 26; i++)
            {
                float sx = (float)(srng.NextDouble() * 4.4 - 2.2);
                float sz2 = 40f + (float)(srng.NextDouble() * 4.4 - 2.2);
                float sy = -8f - (float)srng.NextDouble() * 86f;
                float ss = 0.2f + (float)srng.NextDouble() * 0.45f;
                GameObject sh = Prim(PrimitiveType.Cube, "FallShard", new Vector3(sx, sy, sz2), Vector3.one * ss, i % 5 == 0 ? glowShardM : shardM);
                sh.transform.rotation = Quaternion.Euler((float)srng.NextDouble() * 90f, (float)srng.NextDouble() * 90f, 0f);
            }
        }
        if (GameObject.Find("DesertFloor") == null)
        {
            Material sand = Mat(new Color(0.55f, 0.46f, 0.3f), 0.1f);
            Material sandFlat = Mat(new Color(0.6f, 0.5f, 0.33f), 0.1f);
            Material deadTrunk = Mat(new Color(0.42f, 0.33f, 0.22f), 0f);
            Material deadLeaf = Mat(new Color(0.58f, 0.5f, 0.33f), 0f);
            Material desertRock = Mat(new Color(0.5f, 0.44f, 0.32f), 0f);
            Prim(PrimitiveType.Cube, "DesertFloor", new Vector3(0f, -0.5f, -50f), new Vector3(70f, 1f, 90f), sand);
            Prim(PrimitiveType.Cube, "PyramidStep1", new Vector3(0f, 3f, -9.5f), new Vector3(46f, 7f, 9f), sandFlat);
            Prim(PrimitiveType.Cube, "PyramidStep2", new Vector3(0f, 9f, -9.5f), new Vector3(36f, 6f, 8f), sandFlat);
            Prim(PrimitiveType.Cube, "PyramidStep3", new Vector3(0f, 14f, -9.5f), new Vector3(26f, 5f, 7f), sandFlat);
            Prim(PrimitiveType.Cube, "PyramidDoor", new Vector3(0f, 2.2f, -5.2f), new Vector3(5f, 4.4f, 0.6f), Mat(new Color(0.8f, 0.62f, 0.25f), 0.55f));
            System.Random drng = new System.Random(42);
            for (int i = 0; i < 8; i++)
            {
                float tx = (float)(drng.NextDouble() * 44.0 - 22.0);
                float tz = (float)(drng.NextDouble() * 32.0 - 48.0);
                float tilt = (float)(drng.NextDouble() * 14.0 - 7.0);
                GameObject dt = Prim(PrimitiveType.Cylinder, "DeadTreeTrunk", new Vector3(tx, 2.4f, tz), new Vector3(0.5f, 2.4f, 0.5f), deadTrunk);
                dt.transform.rotation = Quaternion.Euler(tilt, 0f, tilt * 0.6f);
                Prim(PrimitiveType.Sphere, "DeadTreeTop", new Vector3(tx, 6f, tz), new Vector3(3.4f, 2.8f, 3.4f), deadLeaf);
                Prim(PrimitiveType.Sphere, "DeadTreeTop2", new Vector3(tx, 8f, tz), Vector3.one * 2.2f, deadLeaf);
            }
            for (int i = 0; i < 9; i++)
            {
                float bx = (float)(drng.NextDouble() * 44.0 - 22.0);
                float bz = (float)(drng.NextDouble() * 34.0 - 50.0);
                float bs = 0.7f + (float)drng.NextDouble() * 0.8f;
                Prim(PrimitiveType.Sphere, "DeadBush", new Vector3(bx, bs * 0.25f, bz), new Vector3(bs * 1.6f, bs * 0.55f, bs * 1.6f), deadLeaf);
            }
            for (int i = 0; i < 7; i++)
            {
                float rx = (float)(drng.NextDouble() * 40.0 - 20.0);
                float rz = (float)(drng.NextDouble() * 30.0 - 48.0);
                float rs = 0.6f + (float)drng.NextDouble() * 1.2f;
                Prim(PrimitiveType.Sphere, "DesertRock", new Vector3(rx, rs * 0.2f, rz), new Vector3(rs * 1.5f, rs * 0.7f, rs * 1.2f), desertRock);
            }
            Material moonM = Mat(new Color(1f, 1f, 1f), 0f);
            moonM.EnableKeyword("_EMISSION");
            moonM.SetColor("_EmissionColor", new Color(0.9f, 0.95f, 1f) * 2.2f);
            Prim(PrimitiveType.Sphere, "DesertMoon", new Vector3(20f, 42f, 70f), Vector3.one * 9f, moonM);
        }
        if (transform.Find("PlayerHead") == null)
        {
            Shader hs = Shader.Find("Universal Render Pipeline/Lit");
            if (hs == null) hs = Shader.Find("Standard");
            Material hm = new Material(hs);
            Texture2D tex = PlayerFaceTex();
            hm.mainTexture = tex;
            if (hm.HasProperty("_BaseMap")) hm.SetTexture("_BaseMap", tex);
            if (hm.HasProperty("_BaseColor")) hm.SetColor("_BaseColor", Color.white);
            GameObject h = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            h.name = "PlayerHead";
            Destroy(h.GetComponent<Collider>());
            h.transform.SetParent(transform, false);
            h.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            h.transform.localScale = new Vector3(0.42f, 0.46f, 0.42f);
            h.GetComponent<Renderer>().sharedMaterial = hm;
        }
    }

    static float FlatDist(Vector3 a, Vector3 b)
    {
        float dx = a.x - b.x, dz = a.z - b.z;
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    static bool SceneExists(string name)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            if (SceneUtility.GetScenePathByBuildIndex(i).EndsWith("/" + name + ".unity")) return true;
        return false;
    }

    void Update()
    {
        State = state;
        if (flash > 0f) flash -= Time.deltaTime * 1.6f;
        for (int i = 0; i < 3; i++)
        {
            if (lidAt[i] < 0f || coffins[i] == null) continue;
            Transform lid = coffins[i].Find("Lid");
            if (lid == null) continue;
            float k2 = Mathf.Clamp01((Time.time - lidAt[i]) / 1.4f);
            Vector3 lp = lid.position;
            lp.y = lidBase[i] + k2 * 2.8f;
            lid.position = lp;
        }
        if (intro)
        {
            float t = Time.time - introAt;
            if (t < 1.4f)
            {
            }
            else if (t < 5f)
            {
                float k = (t - 1.4f) / 3.6f;
                float y = Mathf.Lerp(-6f, -88f, k * k * (0.55f + 0.45f * k));
                transform.position = new Vector3(0f, y, 40f);
                if (cam != null)
                {
                    float roll = Mathf.Sin(t * 1.3f) * 7f;
                    float yaw = Mathf.Sin(t * 0.6f) * 14f;
                    cam.transform.localRotation = Quaternion.Euler(Mathf.Lerp(18f, 58f, k), yaw, roll);
                }
            }
            else if (t < 5.8f)
            {
                if (!deadFlashed) { deadFlashed = true; flash = 1f; }
            }
            else if (t < 10.1f)
            {
                if (!vistaSet)
                {
                    vistaSet = true;
                    transform.position = new Vector3(0f, 2.6f, -52f);
                    transform.rotation = Quaternion.LookRotation(new Vector3(0f, 0f, 1f));
                    RenderSettings.fogStartDistance = 30f;
                    RenderSettings.fogEndDistance = 170f;
                }
                if (cam != null)
                {
                    float k = Mathf.Clamp01((t - 5.8f) / 4.3f);
                    Vector3 look = new Vector3(Mathf.Lerp(-14f, 10f, k), 9f, 20f);
                    cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(look - cam.transform.position), 2.2f * Time.deltaTime);
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 55f, 3f * Time.deltaTime);
                }
            }
            else if (t < 10.5f)
            {
                if (!arriveFlashed) { arriveFlashed = true; flash = 1f; }
            }
            else if (t < 14.5f)
            {
                if (!interiorSet)
                {
                    interiorSet = true;
                    transform.position = spawnPos;
                    transform.rotation = spawnRot;
                    RenderSettings.fogStartDistance = 10f;
                    RenderSettings.fogEndDistance = 55f;
                    if (cam != null)
                    {
                        cam.transform.localRotation = Quaternion.identity;
                        cam.fieldOfView = 60f;
                    }
                }
            }
            else
            {
                intro = false;
                CharacterController cc = GetComponent<CharacterController>();
                if (cc != null) cc.enabled = true;
                if (pc != null) pc.enabled = true;
            }
            return;
        }
        if (state == 0)
        {
            int near = -1;
            float best = 4f;
            for (int i = 0; i < 3; i++)
            {
                if (snapped[i] || coffins[i] == null) continue;
                float d = FlatDist(transform.position, coffins[i].position);
                if (d < best) { best = d; near = i; }
            }
            PromptVisible = near >= 0;
            UnityEngine.InputSystem.Keyboard kb = UnityEngine.InputSystem.Keyboard.current;
            if (near >= 0 && kb != null && kb.fKey.wasPressedThisFrame)
            {
                snapped[near] = true;
                photos++;
                flash = 1f;
                lastCoffin = coffins[near];
                Debug.Log("PLAYCHAIN M6 photo " + photos);
                lidAt[near] = Time.time;
                if (photos >= 3)
                {
                    state = 2;
                    cineAt = Time.time;
                    PromptVisible = false;
                    if (pc != null) pc.enabled = false;
                    Debug.Log("PLAYCHAIN M6 cinematic");
                }
            }
        }
        else if (state == 2)
        {
            float t = Time.time - cineAt;
            Transform reveal = coffins[2] != null ? coffins[2] : lastCoffin;
            if (cam != null)
            {
                if (t < 2.4f)
                {
                    Transform head = transform.Find("PlayerHead");
                    if (head != null)
                    {
                        Vector3 hp = head.position;
                        Vector3 camTarget = hp + transform.forward * 1.15f + Vector3.up * 0.03f;
                        cam.transform.position = Vector3.Lerp(cam.transform.position, camTarget, Mathf.Clamp01(t / 0.9f));
                        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(hp - cam.transform.position), 6f * Time.deltaTime);
                        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 38f, 4f * Time.deltaTime);
                    }
                }
                else
                {
                    cam.transform.localPosition = Vector3.Lerp(cam.transform.localPosition, new Vector3(0f, 1.65f, 0f), Mathf.Clamp01((t - 2.4f) / 0.8f));
                    if (reveal != null)
                    {
                        Vector3 look = reveal.position + new Vector3(0f, 2.1f, -0.4f);
                        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(look - cam.transform.position), 3f * Time.deltaTime);
                        cam.fieldOfView = Mathf.Lerp(60f, 17f, Mathf.Clamp01((t - 2.4f) / 3.5f));
                    }
                }
            }
            if (t > 10f)
            {
                state = 3;
                Debug.Log("PLAYCHAIN M6 complete");
                GameManager.Epilogue = true;
                if (SceneExists("Mission1")) SceneManager.LoadScene("Mission1");
                else
                {
                    noM1 = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            }
        }
        if (state == 3 && noM1 && UnityEngine.InputSystem.Keyboard.current != null && UnityEngine.InputSystem.Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnGUI()
    {
        float k = Screen.width / 1024f;
        Matrix4x4 old = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(k, k, 1));
        GUIStyle s = new GUIStyle(GUI.skin.label);
        s.fontSize = 20;
        s.alignment = TextAnchor.MiddleCenter;
        s.normal.textColor = Color.white;
        GUI.color = new Color(0f, 0f, 0f, state == 2 ? 0f : 0.55f);
        GUI.DrawTexture(new Rect(0, 0, 1024, 36), Texture2D.whiteTexture);
        GUI.color = Color.white;
        GUI.Label(new Rect(0, 4, 1024, 28), (state == 2 ? "" : "MISSION 6: THE PYRAMID - PHOTOGRAPH THE COFFINS"), s);
        if (state == 0 && !intro)
        {
            GUIStyle c = new GUIStyle(s);
            c.fontSize = 18;
            c.normal.textColor = new Color(1f, 0.9f, 0.6f);
            GUI.Label(new Rect(0, 42, 1024, 24), "COFFINS: " + photos + " / 3", c);
            if (PromptVisible)
            {
                GUIStyle p = new GUIStyle(s);
                p.fontSize = 26;
                p.normal.textColor = Color.white;
                float cy = (Screen.height / k) * 0.62f;
                GUI.Label(new Rect(62, cy, 900, 34), "PRESS F TO PHOTOGRAPH", p);
            }
        }
        if (state == 2)
        {
            float t = Time.time - cineAt;
            string msg = null;
            if (t > 2.9f && t <= 5.2f) msg = "THE THIRD COFFIN IS DIFFERENT...";
            else if (t > 7.6f) msg = "SAME FOREST. TONIGHT.";
            if (msg != null)
            {
                GUIStyle b = new GUIStyle(GUI.skin.label);
                b.fontSize = 24;
                b.alignment = TextAnchor.MiddleCenter;
                b.normal.textColor = new Color(1f, 0.95f, 0.75f);
                float cy = (Screen.height / k) * 0.82f;
                GUI.Label(new Rect(62, cy, 900, 60), msg, b);
            }
        }
        if (state == 3 && noM1)
        {
            GUIStyle b = new GUIStyle(s);
            b.fontSize = 28;
            GUI.Label(new Rect(62, 300, 900, 40), "Press R to restart", b);
        }
        if (intro)
        {
            float t = Time.time - introAt;
            string card = t < 2.6f ? "some years later" : t < 5f ? null : t < 5.8f ? "Dead!" : t < 10.1f ? "millions of years later" : "For your photographs you are visiting egypt mummy's coffins";
            if (card != null)
            {
                GUIStyle cd = new GUIStyle(GUI.skin.label);
                cd.fontSize = 34;
                cd.alignment = TextAnchor.MiddleCenter;
                cd.normal.textColor = Color.white;
                GUI.color = new Color(0f, 0f, 0f, 0.45f);
                GUI.DrawTexture(new Rect(0, 240, 1024, 96), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(0, 258, 1024, 60), card, cd);
            }
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
