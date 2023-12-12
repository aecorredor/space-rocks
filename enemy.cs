using Godot;
using System;
using System.Linq;

public partial class enemy : Area2D
{
  [Export]
  PackedScene BulletScene;

  [Export]
  int speed = 150;

  [Export]
  int rotationSpeed = 120;

  [Export]
  int health = 3;

  PathFollow2D follow = new PathFollow2D();

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    GetNode<Sprite2D>("Sprite2D").Frame = GD.RandRange(0, 3);
    var enemyPaths = GetNode<Node>("EnemyPaths");
    var paths = enemyPaths.GetChildren().OfType<Path2D>().ToArray();
    var path = paths[GD.Randi() % enemyPaths.GetChildCount()];
    path.AddChild(follow);
    follow.Loop = false;
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) { }

  public override void _PhysicsProcess(double delta)
  {
    Rotation += Mathf.DegToRad(rotationSpeed) * (float)delta;
    follow.Progress += speed * (float)delta;
    Position = follow.GlobalPosition;

    if (follow.ProgressRatio >= 1)
    {
      QueueFree();
    }
  }

  private void _on_gun_cooldown_timeout() { }
}
