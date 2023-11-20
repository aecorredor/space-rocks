using Godot;
using System;
using System.Linq;

public partial class main : Node2D
{
  [Export]
  PackedScene RockScene;

  Vector2 screenSize = Vector2.Zero;

  int level = 0;
  int score = 0;
  bool playing = false;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    screenSize = GetViewportRect().Size;
  }

  public override void _Process(double delta)
  {
    if (!playing)
    {
      return;
    }

    if (GetTree().GetNodesInGroup("rocks").Count() == 0)
    {
      newLevel();
    }
  }

  private void newGame()
  {
    // remove any old rocks from previous game
    GetTree().CallGroup("rocks", "queue_free");
    level = 0;
    score = 0;
    GetNode<hud>("HUD").updateScore(score);
    GetNode<hud>("HUD").showMessage("Get Ready!");
    GetNode<player>("Player").reset();
    GetNode<Timer>("HUD/Timer").Start();
    playing = true;
  }

  private void gameOver()
  {
    playing = false;
    GetNode<hud>("HUD").gameOver();
  }

  private void newLevel()
  {
    level += 1;
    GetNode<hud>("HUD").showMessage($"Wave {level}");
    for (int i = 0; i < level; i++)
    {
      spawnRock(3);
    }
  }

  void spawnRock(
    int size,
    Nullable<Vector2> pos = null,
    Nullable<Vector2> vel = null
  )
  {
    if (pos == null)
    {
      var rockSpawn = GetNode<PathFollow2D>("RockPath/RockSpawn");
      rockSpawn.Progress = new Random().Next(0, 100);
      pos = rockSpawn.Position;
    }

    if (vel == null)
    {
      vel =
        Vector2.Right.Rotated((float)GD.RandRange(0, Math.PI))
        * GD.RandRange(50, 125);
    }

    var rock = RockScene.Instantiate() as rock;
    rock.screenSize = screenSize;
    rock.start((Vector2)pos, (Vector2)vel, size);
    CallDeferred("add_child", rock);

    rock.Connect(
      rock.SignalName.Exploded,
      new Callable(this, nameof(onRockExploded))
    );
  }

  void onRockExploded(int size, float radius, Vector2 pos, Vector2 velocity)
  {
    if (size <= 1)
    {
      return;
    }

    int[] offsets = { -1, 1 };
    // Loop through the array using foreach
    foreach (int offset in offsets)
    {
      var dir =
        GetNode<player>("Player").Position.DirectionTo(pos).Orthogonal()
        * offset;
      var newpos = pos + dir * radius;
      var newvel = dir * velocity.Length() * 1.1f;
      spawnRock(size - 1, newpos, newvel);
    }
  }
}
