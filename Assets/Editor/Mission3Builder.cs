using UnityEngine;

public static class Mission3Builder
{
    public static void BuildInto()
    {
        DinoBuilder.Build();
        GameObject player = GameObject.Find("Player");
        Mission2Manager m2 = player.GetComponent<Mission2Manager>();
        if (m2 != null) Object.DestroyImmediate(m2);
        player.AddComponent<Mission3Manager>();
        Debug.Log("Mission 3 scene built");
    }
}
