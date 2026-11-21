using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class ChainBuilder
{
    [MenuItem("WrongHole/Build All Scenes")]
    public static void BuildAll()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Scene s1 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(s1, "Assets/Scenes/Mission1.unity");
        MissionBuilder.BuildMission();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/Mission1.unity");
        Scene s2 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(s2, "Assets/Scenes/Mission2.unity");
        DinoBuilder.Build();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/Mission2.unity");
        Scene s3 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(s3, "Assets/Scenes/Mission3.unity");
        Mission3Builder.BuildInto();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/Mission3.unity");
        Scene s4 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(s4, "Assets/Scenes/Mission4.unity");
        Mission4Builder.BuildInto();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/Mission4.unity");
        Scene s5 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(s5, "Assets/Scenes/Mission5.unity");
        Mission5Builder.BuildInto();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/Mission5.unity");
        StoryBuilder.BuildAll(); Scene s6 = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        EditorSceneManager.SaveScene(s6, "Assets/Scenes/Mission6.unity");
        Mission6Builder.BuildInto();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), "Assets/Scenes/Mission6.unity");
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Mission1.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Mission2.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Mission3.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Mission4.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Mission5.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Hatch.unity", true), new EditorBuildSettingsScene("Assets/Scenes/Care.unity", true), new EditorBuildSettingsScene("Assets/Scenes/Cave.unity", true), new EditorBuildSettingsScene("Assets/Scenes/Mission6.unity", true)
        };
        EditorSceneManager.OpenScene("Assets/Scenes/Mission1.unity");
        Debug.Log("All scenes built: Mission1, Mission2, Mission3, Mission4, Mission5, Mission6");
    }
}
