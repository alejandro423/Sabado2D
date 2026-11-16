using System;
using System.IO;
using UnityEngine;
using Platformer;

GameManager manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
Goal goal = UnityEngine.Object.FindAnyObjectByType<Goal>();

if (manager == null) throw new Exception("no GameManager in the open scene");
if (player == null) throw new Exception("no PlayerController in the open scene");
if (goal == null) throw new Exception("no Goal in the open scene");

player.transform.position = goal.transform.position;
float deadline = Time.time + 3f;
await Wait.Until(() => (UnityEngine.Object.FindAnyObjectByType<GameManager>()?.HasWon ?? false) || Time.time > deadline);

manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
if (manager == null || !manager.HasWon) throw new Exception("player did not win after touching the goal");
if (manager.WinText == null || !manager.WinText.gameObject.activeSelf) throw new Exception("win text was not shown");

string winMessage = manager.WinText.text;

string path = "/tmp/opencode/win.png";
if (File.Exists(path)) File.Delete(path);
ScreenCapture.CaptureScreenshot(path);
await Wait.Until(() => File.Exists(path), 5);

manager.Restart();
await Wait.Until(() =>
{
    GameManager reloaded = UnityEngine.Object.FindAnyObjectByType<GameManager>();
    return reloaded != null && !reloaded.HasWon && reloaded.Score == 0;
}, 5);

return "win ok, restart ok: " + winMessage.Replace("\n", " | ");
