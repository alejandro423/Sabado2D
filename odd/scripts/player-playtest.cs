using System;
using UnityEngine;
using Platformer;

PlayerController player = UnityEngine.Object.FindAnyObjectByType<PlayerController>();
if (player == null)
{
    throw new Exception("no PlayerController in the open scene");
}

await Wait.Until(() => player.IsGrounded);
float startX = player.transform.position.x;
float startY = player.transform.position.y;

// Move right for a moment.
player.MoveInput = 1f;
float moveDeadline = Time.time + 2f;
await Wait.Until(() => player.transform.position.x > startX + 0.5f || Time.time > moveDeadline);
float movedBy = player.transform.position.x - startX;
player.MoveInput = 0f;

if (movedBy <= 0.5f)
{
    throw new Exception("player did not move right, delta=" + movedBy);
}

await Wait.Until(() => player.IsGrounded);
float takeoffY = player.transform.position.y;
player.QueueJump();

float jumpDeadline = Time.time + 2f;
await Wait.Until(() => player.transform.position.y > takeoffY + 0.3f || Time.time > jumpDeadline);
float jumpedBy = player.transform.position.y - takeoffY;

if (jumpedBy <= 0.3f)
{
    throw new Exception("player did not jump, delta=" + jumpedBy);
}

return string.Format("ok movedRight={0:F2} units jumped={1:F2} units", movedBy, jumpedBy);
