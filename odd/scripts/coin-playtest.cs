using System;
using UnityEngine;
using Platformer;

GameManager manager = UnityEngine.Object.FindAnyObjectByType<GameManager>();
PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
Coin[] coins = UnityEngine.Object.FindObjectsByType<Coin>();

if (manager == null) throw new Exception("no GameManager in the open scene");
if (player == null) throw new Exception("no PlayerController in the open scene");
if (coins.Length == 0) throw new Exception("no coins in the open scene");

int before = manager.Score;
int coinsBefore = coins.Length;
Coin target = coins[0];
player.transform.position = target.transform.position;

float deadline = Time.time + 3f;
await Wait.Until(() => manager.Score > before || Time.time > deadline);

if (manager.Score <= before)
{
    throw new Exception("score did not increase after touching a coin");
}

string hud = manager.ScoreText != null ? manager.ScoreText.text : "<missing hud>";
return string.Format("score {0} -> {1}, coins {2} -> {3}, hud='{4}'", before, manager.Score, coinsBefore, UnityEngine.Object.FindObjectsByType<Coin>().Length, hud);
