using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static bool MissionOver = false;
    public static bool Won = false;
    public static bool Epilogue = false;
    public static bool Frozen = false;
    private static string endMessage = "";
    private static bool ended = false;
    private static bool won = false;
    private static float loadAt = -1f;

    float introAt = -1f;
    bool introDone;
    float flash;
    Camera cam;
    PlayerController pc;
    Transform[] cheetahs;
    Quaternion camStartRot;
    bool fovInit;

    float epiAt = -1f;
    bool staged;
    bool boarded;
    bool departed;
    bool photoReady;
    bool photoTaken;
    float reloadAt = -1f;
    Transform bus;
    GameObject stone;
    Vector3 walkFrom;
    Vector3 walkTo;
    Vector3 stonePos;
    Vector3 camHold;
    float busSpeed;

    void Start()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        MissionOver = false;
        Won = false;
        ended = false;
        won = false;
        loadAt = -1f;
        endMessage = "";
        flash = 1f;
        cam = Camera.main;
        pc = FindFirstObjectByType<PlayerController>();
        cheetahs = new Transform[4];
        for (int i = 0; i < 4; i++)
        {
            GameObject c = GameObject.Find("Cheetah" + (i + 1));
            if (c != null) cheetahs[i] = c.transform;
        }
        Frozen = true;
        if (pc != null) pc.enabled = false;
        if (cam != null) camStartRot = cam.transform.rotation;
        if (Epilogue) epiAt = Time.time + 0.6f;
        else introAt = Time.time;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public static void Caught()
    {
        End("THE CHEETAHS CAUGHT YOU", false);
    }

    public static void ReachedWormhole()
    {
        End("MISSION 1 COMPLETE - THE WORMHOLE PULLED YOU IN", true);
    }

    private static void End(string msg, bool isWin)
    {
        if (ended) return;
        ended = true;
        won = isWin;
        Won = isWin;
        MissionOver = true;
        endMessage = msg;
        if (isWin) loadAt = Time.time + 4f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    static Material MakeMat(Color c, float smooth)
    {
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material m = new Material(sh);
        m.color = c;
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", smooth);
        return m;
    }

    static GameObject Part(PrimitiveType ty, string name, Vector3 localPos, Vector3 scale, Material m, Transform parent, Vector3 localEuler)
    {
        GameObject g = GameObject.CreatePrimitive(ty);
        g.name = name;
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(parent, false);
        g.transform.localPosition = localPos;
        g.transform.localScale = scale;
        g.transform.localRotation = Quaternion.Euler(localEuler);
        g.GetComponent<Renderer>().sharedMaterial = m;
        return g;
    }

    static Transform BuildBus(Vector3 pos, Vector3 away)
    {
        Material bodyM = MakeMat(new Color(0.85f, 0.6f, 0.1f), 0.25f);
        Material glassM = MakeMat(new Color(0.5f, 0.7f, 0.85f), 0.6f);
        Material darkM = MakeMat(new Color(0.1f, 0.1f, 0.1f), 0.1f);
        Material lightM = MakeMat(new Color(1f, 1f, 1f), 0f);
        lightM.EnableKeyword("_EMISSION");
        lightM.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.6f) * 2.5f);
        GameObject bus = new GameObject("Bus");
        bus.transform.position = pos;
        bus.transform.rotation = Quaternion.LookRotation(away) * Quaternion.Euler(0f, -90f, 0f);
        Part(PrimitiveType.Cube, "BusBody", new Vector3(0f, 1.4f, 0f), new Vector3(7f, 1.8f, 2.3f), bodyM, bus.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "BusTop", new Vector3(0f, 2.55f, 0f), new Vector3(7f, 0.6f, 2.3f), bodyM, bus.transform, Vector3.zero);
        for (int i = 0; i < 5; i++)
        {
            float wx = -2.4f + i * 1.2f;
            Part(PrimitiveType.Cube, "BusWindowL", new Vector3(wx, 1.95f, -1.16f), new Vector3(0.95f, 0.7f, 0.05f), glassM, bus.transform, Vector3.zero);
            Part(PrimitiveType.Cube, "BusWindowR", new Vector3(wx, 1.95f, 1.16f), new Vector3(0.95f, 0.7f, 0.05f), glassM, bus.transform, Vector3.zero);
        }
        Part(PrimitiveType.Cube, "BusWindshield", new Vector3(3.52f, 1.9f, 0f), new Vector3(0.05f, 0.75f, 1.9f), glassM, bus.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "BusRearWindow", new Vector3(-3.52f, 1.9f, 0f), new Vector3(0.05f, 0.7f, 1.8f), glassM, bus.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "BusDoor", new Vector3(2.9f, 1.3f, 1.17f), new Vector3(0.9f, 1.5f, 0.05f), darkM, bus.transform, Vector3.zero);
        for (int wx = -1; wx <= 1; wx += 2)
            for (int wz = -1; wz <= 1; wz += 2)
                Part(PrimitiveType.Cylinder, "BusWheel", new Vector3(2.3f * wx, 0.5f, 1.15f * wz), new Vector3(0.55f, 0.12f, 0.55f), darkM, bus.transform, new Vector3(90f, 0f, 0f));
        Part(PrimitiveType.Sphere, "BusLightL", new Vector3(3.55f, 0.9f, -0.75f), Vector3.one * 0.18f, lightM, bus.transform, Vector3.zero);
        Part(PrimitiveType.Sphere, "BusLightR", new Vector3(3.55f, 0.9f, 0.75f), Vector3.one * 0.18f, lightM, bus.transform, Vector3.zero);
        GameObject label = new GameObject("BusRearLabel");
        label.transform.SetParent(bus.transform, false);
        label.transform.localPosition = new Vector3(-3.56f, 2.35f, 0f);
        label.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        TextMesh tm = label.AddComponent<TextMesh>();
        tm.text = "Cheetahs\nForest";
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.characterSize = 0.24f;
        tm.fontSize = 48;
        tm.color = new Color(0.12f, 0.08f, 0.04f);
        return bus.transform;
    }

    static GameObject BuildStone(Vector3 pos)
    {
        Material stoneM = MakeMat(new Color(0.55f, 0.48f, 0.3f), 0.2f);
        Material darkM = MakeMat(new Color(0.2f, 0.14f, 0.08f), 0.1f);
        Material faceM = MakeMat(new Color(0.75f, 0.62f, 0.32f), 0.5f);
        GameObject st = new GameObject("EpilogueStone");
        st.transform.position = pos;
        Part(PrimitiveType.Cube, "StoneBase", new Vector3(0f, 0.08f, 0f), new Vector3(1.4f, 0.16f, 1.1f), darkM, st.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "StoneBody", new Vector3(0f, 1.15f, 0f), new Vector3(1f, 2.3f, 0.7f), stoneM, st.transform, Vector3.zero);
        Part(PrimitiveType.Cube, "StoneLid", new Vector3(0f, 1.15f, -0.42f), new Vector3(0.9f, 2.2f, 0.16f), stoneM, st.transform, Vector3.zero);
        Part(PrimitiveType.Sphere, "StoneFace", new Vector3(0f, 1.78f, -0.52f), new Vector3(0.52f, 0.62f, 0.2f), faceM, st.transform, Vector3.zero);
        return st;
    }

    void Update()
    {
        if (flash > 0f) flash -= Time.deltaTime * 1.6f;
        if (!Epilogue && introAt > 0f && !introDone)
        {
            float t = Time.time - introAt;
            if (cam != null)
            {
                if (!fovInit) { fovInit = true; cam.fieldOfView = 20f; }
                if (t < 2.4f)
                {
                    Vector3 mid = Vector3.zero;
                    int n = 0;
                    for (int i = 0; i < 4; i++) if (cheetahs[i] != null) { mid += cheetahs[i].position; n++; }
                    if (n > 0)
                    {
                        mid /= n;
                        Vector3 look = mid + Vector3.up * 1f;
                        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(look - cam.transform.position), 7f * Time.deltaTime);
                    }
                    cam.fieldOfView = Mathf.Lerp(20f, 55f, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 2.4f)));
                }
                else
                {
                    cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, camStartRot, 3.2f * Time.deltaTime);
                    cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, 4f * Time.deltaTime);
                }
                if (t >= 7f)
                {
                    introDone = true;
                    cam.transform.rotation = camStartRot;
                    cam.fieldOfView = 60f;
                }
            }
            else if (t >= 7f) introDone = true;
            if (introDone)
            {
                Frozen = false;
                if (pc != null) pc.enabled = true;
            }
        }
        if (Epilogue && !ended && epiAt > 0f && Time.time >= epiAt)
        {
            float t = Time.time - epiAt;
            if (!staged)
            {
                staged = true;
                Vector3 basePos = pc != null ? pc.transform.position : transform.position;
                Vector3 fwd = pc != null ? pc.transform.forward : Vector3.forward;
                fwd.y = 0f;
                if (fwd.sqrMagnitude < 0.01f) fwd = Vector3.forward;
                fwd.Normalize();
                Vector3 side = Vector3.Cross(Vector3.up, fwd);
                Vector3 busPos = basePos + fwd * 15f + side * 5f;
                busPos.y = 0f;
                Vector3 camPos = cam != null ? cam.transform.position : basePos + Vector3.up * 1.6f;
                Vector3 away = busPos - camPos;
                away.y = 0f;
                if (away.sqrMagnitude < 0.01f) away = fwd;
                bus = BuildBus(busPos, away.normalized);
                stonePos = basePos + fwd * 7f - side * 3.5f;
                stonePos.y = 0f;
                stone = BuildStone(stonePos);
                stone.transform.rotation = Quaternion.Euler(0f, 15f, 0f);
                walkFrom = basePos;
                walkTo = bus.TransformPoint(new Vector3(2.9f, 0f, 2.2f));
                walkTo.y = basePos.y;
            }
            if (bus != null && departed)
            {
                busSpeed = Mathf.Min(busSpeed + 7f * Time.deltaTime, 11f);
                bus.position += bus.right * busSpeed * Time.deltaTime;
            }
            if (pc != null && !boarded)
            {
                float k = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t / 3.2f));
                pc.transform.position = Vector3.Lerp(walkFrom, walkTo, k);
                Vector3 face = walkTo - walkFrom;
                face.y = 0f;
                if (face.sqrMagnitude > 0.01f) pc.transform.rotation = Quaternion.Slerp(pc.transform.rotation, Quaternion.LookRotation(face), 6f * Time.deltaTime);
                if (t >= 3.2f)
                {
                    boarded = true;
                    if (cam != null)
                    {
                        cam.transform.SetParent(null, true);
                        camHold = cam.transform.position;
                    }
                    pc.gameObject.SetActive(false);
                }
            }
            if (boarded && !departed && t >= 3.6f) departed = true;
            if (departed && !photoReady && bus != null && cam != null)
            {
                Vector3 look = bus.position + Vector3.up * 1.9f;
                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(look - cam.transform.position), 4f * Time.deltaTime);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 46f, 2f * Time.deltaTime);
            }
            if (!photoReady && t >= 8.4f) photoReady = true;
            if (photoReady && !photoTaken && cam != null && stone != null)
            {
                Vector3 toCam = camHold - stonePos;
                toCam.y = 0f;
                if (toCam.sqrMagnitude < 0.04f) toCam = Vector3.forward;
                Vector3 vantage = stonePos + toCam.normalized * 3.4f + Vector3.up * 1.55f;
                cam.transform.position = Vector3.Lerp(cam.transform.position, vantage, 2.6f * Time.deltaTime);
                Vector3 look2 = stonePos + Vector3.up * 1.35f;
                cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(look2 - cam.transform.position), 3.5f * Time.deltaTime);
                cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 34f, 3f * Time.deltaTime);
                if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
                {
                    photoTaken = true;
                    flash = 1f;
                    reloadAt = Time.time + 0.5f;
                }
            }
        }
        if (reloadAt > 0f && Time.time >= reloadAt)
        {
            reloadAt = -1f;
            Epilogue = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        if (loadAt > 0f && Time.time >= loadAt)
        {
            loadAt = -1f;
            try
            {
                SceneManager.LoadScene("Mission2");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("Mission2 load failed: " + e.Message);
            }
        }
        if (ended && Keyboard.current != null && (Keyboard.current.rKey.wasPressedThisFrame || Keyboard.current.homeKey.wasPressedThisFrame))
        {
            loadAt = -1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void OnGUI()
    {
        float k = Screen.width / 1024f;
        Matrix4x4 old = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(k, k, 1));
        GUIStyle h = new GUIStyle(GUI.skin.label);
        h.fontSize = 20;
        h.normal.textColor = Color.white;
        if (!Epilogue)
        {
            GUI.color = new Color(0, 0, 0, 0.55f);
            GUI.DrawTexture(new Rect(0, 0, 1024, 36), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(14, 6, 1000, 28), "MISSION 1: ESCAPE THE FOREST - REACH THE WORMHOLE", h);
        }
        if (Epilogue && !ended && epiAt > 0f && Time.time >= epiAt)
        {
            string prompt = null;
            if (staged && !boarded) prompt = "GET TO THE BUS";
            if (photoReady && !photoTaken) prompt = "TAKE A PHOTO OF THE CHEETAHS FOREST STONE    [ F ]";
            if (prompt != null)
            {
                GUIStyle p = new GUIStyle(GUI.skin.label);
                p.fontSize = 24;
                p.alignment = TextAnchor.MiddleCenter;
                p.normal.textColor = new Color(1f, 0.95f, 0.7f);
                float py = (Screen.height / k) * 0.78f;
                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.DrawTexture(new Rect(0, py - 8, 1024, 48), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(62, py, 900, 34), prompt, p);
            }
        }
        if (ended)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 30;
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = true;
            style.normal.textColor = Color.white;
            float cy = (Screen.height / k) * 0.5f;
            GUI.Label(new Rect(62, cy - 50, 900, 46), endMessage, style);
            GUIStyle sub = new GUIStyle(style);
            sub.fontSize = 20;
            GUI.Label(new Rect(62, cy + 4, 900, 30), won ? "Entering the dinosaur era..." : "Press R to restart", sub);
        }
        if (!Epilogue && introAt > 0f && !introDone)
        {
            float t = Time.time - introAt;
            if (t >= 4f)
            {
                int count = Mathf.Max(1, Mathf.CeilToInt(7f - t));
                GUIStyle o = new GUIStyle(GUI.skin.label);
                o.alignment = TextAnchor.MiddleCenter;
                o.fontSize = 42;
                o.normal.textColor = Color.white;
                float hy = Screen.height / k;
                GUI.color = new Color(0, 0, 0, 0.5f);
                GUI.DrawTexture(new Rect(0, hy * 0.5f - 78, 1024, 130), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(0, hy * 0.5f - 66, 1024, 56), "Escape from cheetahs", o);
                GUIStyle cd = new GUIStyle(o);
                cd.fontSize = 30;
                cd.normal.textColor = new Color(1f, 0.9f, 0.55f);
                GUI.Label(new Rect(0, hy * 0.5f - 6, 1024, 44), count.ToString(), cd);
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
