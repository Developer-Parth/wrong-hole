using UnityEngine;

public static class Mission5Builder
{
    public static void BuildInto()
    {
        DinoBuilder.Build();
        GameObject player = GameObject.Find("Player");
        Mission2Manager m2 = player.GetComponent<Mission2Manager>();
        if (m2 != null) Object.DestroyImmediate(m2);
        Shader sh = Shader.Find("Universal Render Pipeline/Lit");
        if (sh == null) sh = Shader.Find("Standard");
        Material rockM = new Material(sh);
        rockM.color = new Color(0.4f, 0.37f, 0.33f);
        GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rock.name = "HideRock";
        rock.transform.position = new Vector3(16f, 1.4f, 36f);
        rock.transform.localScale = new Vector3(5f, 2.8f, 4f);
        rock.GetComponent<Renderer>().sharedMaterial = rockM;
        Vector3 look = new Vector3(16f, 0f, 36f) - new Vector3(player.transform.position.x, 0f, player.transform.position.z);
        player.transform.rotation = Quaternion.LookRotation(look);
        player.AddComponent<Mission5Manager>();
        Debug.Log("Mission 5 scene built");
    }
}
