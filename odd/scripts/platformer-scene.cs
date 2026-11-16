using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PlatformerTools
{
    /// <summary>
    /// Builds and verifies the platformer scene.
    /// Build:  unity command script run --file odd/scripts/platformer-scene.cs --entry PlatformerTools.PlatformerScene.Build
    /// Verify: unity command script run --file odd/scripts/platformer-scene.cs --entry PlatformerTools.PlatformerScene.Verify
    /// </summary>
    public static class PlatformerScene
    {
        private const string ScenePath = "Assets/Scenes/Platformer.unity";

        public static string Build()
        {
            Scene current = EditorSceneManager.GetActiveScene();
            if (current.isDirty && !string.IsNullOrEmpty(current.path))
            {
                EditorSceneManager.SaveScene(current);
            }

            Sprite playerSprite = LoadSprite("player");
            Sprite groundSprite = LoadSprite("ground");
            Sprite wallSprite = LoadSprite("wall");
            Sprite coinSprite = LoadSprite("coin");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Platformer";

            // Camera
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(-5f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.13f, 0.16f, 0.23f);
            cameraObject.AddComponent<AudioListener>();
            AddUrpCameraData(cameraObject);

            // 2D global light, required by the URP 2D renderer.
            GameObject lightObject = new GameObject("Global Light 2D");
            Light2D light = lightObject.AddComponent<Light2D>();
            light.lightType = Light2D.LightType.Global;
            light.color = Color.white;
            light.intensity = 1f;

            // Level layout: ground from x -8..22, top edge at y -3.
            Transform level = new GameObject("Level").transform;
            MakeRect("Ground", level, groundSprite, new Vector2(7f, -3.5f), new Vector2(30f, 1f));
            MakeRect("Wall Left", level, wallSprite, new Vector2(-8.5f, 0.5f), new Vector2(1f, 8f));
            MakeRect("Wall Right", level, wallSprite, new Vector2(22.5f, 0.5f), new Vector2(1f, 8f));
            MakeRect("Platform A", level, groundSprite, new Vector2(0f, -2f), new Vector2(5f, 1f));
            MakeRect("Platform B", level, groundSprite, new Vector2(6f, -0.25f), new Vector2(4f, 1f));
            MakeRect("Platform C", level, groundSprite, new Vector2(11.5f, 1.25f), new Vector2(4f, 1f));

            MakeCoin("Coin A", level, coinSprite, new Vector2(-1f, -1.1f));
            MakeCoin("Coin B", level, coinSprite, new Vector2(6f, 0.65f));
            MakeCoin("Coin C", level, coinSprite, new Vector2(11.5f, 2.15f));

            // HUD
            Canvas hud = new GameObject("HUD").AddComponent<Canvas>();
            hud.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = hud.gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            hud.gameObject.AddComponent<GraphicRaycaster>();

            GameObject scoreObject = new GameObject("Score Text");
            scoreObject.transform.SetParent(hud.transform, false);
            Text scoreText = scoreObject.AddComponent<Text>();
            scoreText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            scoreText.fontSize = 48;
            scoreText.color = Color.white;
            scoreText.alignment = TextAnchor.UpperLeft;
            scoreText.text = "Monedas: 0";
            RectTransform scoreRect = scoreObject.GetComponent<RectTransform>();
            scoreRect.anchorMin = new Vector2(0f, 1f);
            scoreRect.anchorMax = new Vector2(0f, 1f);
            scoreRect.pivot = new Vector2(0f, 1f);
            scoreRect.anchoredPosition = new Vector2(40f, -30f);
            scoreRect.sizeDelta = new Vector2(500f, 80f);

            Platformer.GameManager manager = new GameObject("Game Manager").AddComponent<Platformer.GameManager>();
            manager.ScoreText = scoreText;

            // Player body; movement arrives in the player-controller step.
            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.transform.position = new Vector3(-5f, -2.5f, 0f);
            SpriteRenderer renderer = player.AddComponent<SpriteRenderer>();
            renderer.sprite = playerSprite;
            renderer.sortingOrder = 10;
            BoxCollider2D collider = player.AddComponent<BoxCollider2D>();
            collider.size = Vector2.one;
            Rigidbody2D body = player.AddComponent<Rigidbody2D>();
            body.gravityScale = 2.5f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            player.AddComponent<Platformer.PlayerController>();
            player.AddComponent<Platformer.KeyboardInput>();

            Platformer.CameraFollow follow = cameraObject.AddComponent<Platformer.CameraFollow>();
            follow.Target = player.transform;

            EditorSceneManager.MarkSceneDirty(scene);
            bool saved = EditorSceneManager.SaveScene(scene, ScenePath);
            AddToBuildSettings();

            int roots = scene.GetRootGameObjects().Length;
            return string.Format("saved={0} path={1} roots={2}", saved, ScenePath, roots);
        }

        public static string Verify()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            List<string> problems = new List<string>();

            GameObject cameraObject = GameObject.FindWithTag("MainCamera");
            if (cameraObject == null)
            {
                problems.Add("missing Main Camera tag");
            }

            Camera camera = cameraObject != null ? cameraObject.GetComponent<Camera>() : null;
            if (camera == null)
            {
                problems.Add("missing Camera component");
            }
            else if (!camera.orthographic || Mathf.Abs(camera.orthographicSize - 5f) > 0.01f)
            {
                problems.Add("camera is not orthographic size 5");
            }

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                problems.Add("missing Player tag");
            }

            if (player != null)
            {
                Rigidbody2D body = player.GetComponent<Rigidbody2D>();
                if (body == null || body.bodyType != RigidbodyType2D.Dynamic)
                {
                    problems.Add("player has no dynamic Rigidbody2D");
                }

                if (player.GetComponent<BoxCollider2D>() == null)
                {
                    problems.Add("player has no BoxCollider2D");
                }

                if (player.GetComponent<Platformer.PlayerController>() == null)
                {
                    problems.Add("player has no PlayerController");
                }

                if (player.GetComponent<Platformer.KeyboardInput>() == null)
                {
                    problems.Add("player has no KeyboardInput");
                }
            }

            Platformer.CameraFollow follow = cameraObject != null ? cameraObject.GetComponent<Platformer.CameraFollow>() : null;
            if (follow == null)
            {
                problems.Add("camera has no CameraFollow");
            }
            else if (player == null || follow.Target != player.transform)
            {
                problems.Add("CameraFollow target is not the player");
            }

            if (Object.FindAnyObjectByType<Light2D>() == null)
            {
                problems.Add("missing Light2D global light");
            }

            int colliders = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                foreach (BoxCollider2D box in root.GetComponentsInChildren<BoxCollider2D>())
                {
                    colliders++;
                }
            }

            if (colliders < 7)
            {
                problems.Add("expected at least 7 colliders, found " + colliders);
            }

            bool inBuildSettings = false;
            foreach (EditorBuildSettingsScene entry in EditorBuildSettings.scenes)
            {
                if (entry.path == ScenePath && entry.enabled)
                {
                    inBuildSettings = true;
                }
            }

            if (!inBuildSettings)
            {
                problems.Add("scene is not enabled in build settings");
            }

            Platformer.Coin[] coins = Object.FindObjectsByType<Platformer.Coin>();
            if (coins.Length != 3)
            {
                problems.Add("expected 3 coins, found " + coins.Length);
            }

            foreach (Platformer.Coin coin in coins)
            {
                Collider2D trigger = coin.GetComponent<Collider2D>();
                if (trigger == null || !trigger.isTrigger)
                {
                    problems.Add("coin " + coin.name + " has no trigger collider");
                }
            }

            Platformer.GameManager manager = Object.FindAnyObjectByType<Platformer.GameManager>();
            if (manager == null)
            {
                problems.Add("missing GameManager");
            }
            else if (manager.ScoreText == null)
            {
                problems.Add("GameManager has no score text");
            }

            if (Object.FindAnyObjectByType<Canvas>() == null)
            {
                problems.Add("missing HUD canvas");
            }

            if (problems.Count > 0)
            {
                throw new System.Exception("scene verification failed: " + string.Join("; ", problems));
            }

            return string.Format("scene ok: {0} roots, {1} box colliders, build settings enabled", scene.GetRootGameObjects().Length, colliders);
        }

        private static void MakeCoin(string name, Transform parent, Sprite sprite, Vector2 position)
        {
            GameObject coin = new GameObject(name);
            coin.transform.SetParent(parent, false);
            coin.transform.position = position;
            SpriteRenderer renderer = coin.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 5;
            CircleCollider2D trigger = coin.AddComponent<CircleCollider2D>();
            trigger.radius = 0.25f;
            trigger.isTrigger = true;
            coin.AddComponent<Platformer.Coin>();
        }

        private static void MakeRect(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.drawMode = SpriteDrawMode.Tiled;
            renderer.size = size;
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = size;
        }

        private static Sprite LoadSprite(string name)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/" + name + ".png");
            if (sprite == null)
            {
                throw new System.Exception("missing sprite Assets/Art/" + name + ".png");
            }

            return sprite;
        }

        private static void AddUrpCameraData(GameObject cameraObject)
        {
            System.Type dataType = System.Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, Unity.RenderPipelines.Universal.Runtime");
            if (dataType != null && cameraObject.GetComponent(dataType) == null)
            {
                cameraObject.AddComponent(dataType);
            }
        }

        private static void AddToBuildSettings()
        {
            // The EditorBuildSettings.scenes setter only updates memory; writing the serialized
            // asset is what makes the entry survive an editor restart.
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/EditorBuildSettings.asset");
            if (assets.Length == 0)
            {
                throw new System.Exception("cannot load EditorBuildSettings.asset");
            }

            SerializedObject settings = new SerializedObject(assets[0]);
            SerializedProperty scenes = settings.FindProperty("m_Scenes");
            string guid = AssetDatabase.AssetPathToGUID(ScenePath);

            for (int i = 0; i < scenes.arraySize; i++)
            {
                if (scenes.GetArrayElementAtIndex(i).FindPropertyRelative("path").stringValue == ScenePath)
                {
                    return;
                }
            }

            scenes.InsertArrayElementAtIndex(scenes.arraySize);
            SerializedProperty entry = scenes.GetArrayElementAtIndex(scenes.arraySize - 1);
            entry.FindPropertyRelative("enabled").boolValue = true;
            entry.FindPropertyRelative("path").stringValue = ScenePath;
            entry.FindPropertyRelative("guid").stringValue = guid;
            settings.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}
