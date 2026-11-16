using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.VisualScripting;

namespace PlatformerTools
{
    /// <summary>
    /// Builds the visual-scripting graph that moves and jumps the player.
    /// Run: unity command script run --file odd/scripts/vs-player-graph.cs --entry PlatformerTools.VsPlayerGraph.Build
    /// </summary>
    public static class VsPlayerGraph
    {
        private const float Force = 30f;
        private const float JumpImpulse = 20f;

        public static string Build()
        {
            FlowGraph graph = new FlowGraph();

            AddForceChain(graph, KeyCode.D, new Vector2(Force, 0f), new Vector2(0f, 120f));
            AddForceChain(graph, KeyCode.RightArrow, new Vector2(Force, 0f), new Vector2(0f, -140f));
            AddForceChain(graph, KeyCode.A, new Vector2(-Force, 0f), new Vector2(0f, -400f));
            AddForceChain(graph, KeyCode.LeftArrow, new Vector2(-Force, 0f), new Vector2(0f, -660f));
            AddJumpChain(graph, new Vector2(0f, -920f));
            AddGroundedChain(graph, true, new Vector2(0f, -1180f));
            AddGroundedChain(graph, false, new Vector2(0f, -1400f));

            ScriptGraphAsset asset = ScriptableObject.CreateInstance<ScriptGraphAsset>();
            asset.graph = graph;

            if (!AssetDatabase.IsValidFolder("Assets/Graphs"))
            {
                AssetDatabase.CreateFolder("Assets", "Graphs");
            }

            AssetDatabase.DeleteAsset("Assets/Graphs/PlayerMovement.asset");
            AssetDatabase.CreateAsset(asset, "Assets/Graphs/PlayerMovement.asset");

            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                throw new System.Exception("no Player in the open scene");
            }

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            body.linearDamping = 4f;

            ScriptMachine machine = player.GetComponent<ScriptMachine>();
            if (machine == null)
            {
                machine = player.AddComponent<ScriptMachine>();
            }

            machine.nest.SwitchToMacro(asset);

            RemoveProbe("VS Probe");
            RemoveProbe("VS Probe 2");
            AssetDatabase.DeleteAsset("Assets/Graphs/VisualSpike.asset");
            AssetDatabase.DeleteAsset("Assets/Graphs/ForceSpike.asset");
            AssetDatabase.DeleteAsset("Assets/Graphs/StartProbe.asset");
            AssetDatabase.DeleteAsset("Assets/Graphs/ForceVerify.asset");
            AssetDatabase.DeleteAsset("Assets/Scripts/Gameplay/PlayerController.cs");
            AssetDatabase.DeleteAsset("Assets/Scripts/Gameplay/PlayerController.cs.meta");
            AssetDatabase.DeleteAsset("Assets/Scripts/Gameplay/KeyboardInput.cs");
            AssetDatabase.DeleteAsset("Assets/Scripts/Gameplay/KeyboardInput.cs.meta");

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            AssetDatabase.SaveAssets();

            return "ok player graph: " + graph.units.Count + " units";
        }

        private static void AddForceChain(FlowGraph graph, KeyCode key, Vector2 force, Vector2 position)
        {
            OnKeyboardInput onKey = new OnKeyboardInput();
            onKey.position = position;
            graph.units.Add(onKey);
            onKey.defaultValues["key"] = key;
            onKey.defaultValues["action"] = PressState.Hold;

            InvokeMember invoke = new InvokeMember(AddForceMember());
            invoke.position = position + new Vector2(300f, 0f);
            graph.units.Add(invoke);

            Literal forceValue = new Literal(typeof(Vector2), force);
            forceValue.position = position + new Vector2(0f, -110f);
            graph.units.Add(forceValue);

            Literal modeValue = new Literal(typeof(ForceMode2D), ForceMode2D.Force);
            modeValue.position = position + new Vector2(0f, -200f);
            graph.units.Add(modeValue);

            graph.controlConnections.Add(new ControlConnection(onKey.trigger, invoke.enter));
            graph.valueConnections.Add(new ValueConnection(forceValue.output, invoke.valueInputs["%force"]));
            graph.valueConnections.Add(new ValueConnection(modeValue.output, invoke.valueInputs["%mode"]));
        }

        private static void AddJumpChain(FlowGraph graph, Vector2 position)
        {
            OnKeyboardInput onKey = new OnKeyboardInput();
            onKey.position = position;
            graph.units.Add(onKey);
            onKey.defaultValues["key"] = KeyCode.Space;
            onKey.defaultValues["action"] = PressState.Down;

            GetVariable grounded = new GetVariable();
            grounded.kind = VariableKind.Object;
            grounded.position = position + new Vector2(0f, -120f);
            graph.units.Add(grounded);
            grounded.defaultValues["name"] = "Grounded";

            If branch = new If();
            branch.position = position + new Vector2(300f, 0f);
            graph.units.Add(branch);

            InvokeMember jump = new InvokeMember(AddForceMember());
            jump.position = position + new Vector2(620f, 60f);
            graph.units.Add(jump);

            Literal jumpForce = new Literal(typeof(Vector2), new Vector2(0f, JumpImpulse));
            jumpForce.position = position + new Vector2(300f, -120f);
            graph.units.Add(jumpForce);

            Literal instant = new Literal(typeof(ForceMode2D), ForceMode2D.Impulse);
            instant.position = position + new Vector2(300f, -210f);
            graph.units.Add(instant);

            graph.controlConnections.Add(new ControlConnection(onKey.trigger, branch.enter));
            graph.valueConnections.Add(new ValueConnection(grounded.value, branch.condition));
            graph.controlConnections.Add(new ControlConnection(branch.ifTrue, jump.enter));
            graph.valueConnections.Add(new ValueConnection(jumpForce.output, jump.valueInputs["%force"]));
            graph.valueConnections.Add(new ValueConnection(instant.output, jump.valueInputs["%mode"]));
        }

        private static void AddGroundedChain(FlowGraph graph, bool grounded, Vector2 position)
        {
            EventUnit<Collision2D> collisionEvent = grounded
                ? (EventUnit<Collision2D>)new OnCollisionEnter2D()
                : new OnCollisionExit2D();
            collisionEvent.position = position;
            graph.units.Add(collisionEvent);

            SetVariable setVariable = new SetVariable();
            setVariable.kind = VariableKind.Object;
            setVariable.position = position + new Vector2(320f, 0f);
            graph.units.Add(setVariable);
            setVariable.defaultValues["name"] = "Grounded";

            Literal value = new Literal(typeof(bool), grounded);
            value.position = position + new Vector2(0f, -110f);
            graph.units.Add(value);

            graph.controlConnections.Add(new ControlConnection(collisionEvent.trigger, setVariable.assign));
            graph.valueConnections.Add(new ValueConnection(value.output, setVariable.valueInputs["input"]));
        }

        private static Member AddForceMember()
        {
            return new Member(typeof(Rigidbody2D), "AddForce", new[] { typeof(Vector2), typeof(ForceMode2D) });
        }

        private static void RemoveProbe(string name)
        {
            GameObject probe = GameObject.Find(name);
            if (probe != null)
            {
                Object.DestroyImmediate(probe);
            }
        }
    }
}
