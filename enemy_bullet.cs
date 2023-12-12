using Godot;
using System;

public partial class enemy_bullet : Area2D
{
  [Export]
  int speed = 1000;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready() { }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
  {
    Position += Transform.X * speed * (float)delta;
  }

  public void Start(Vector2 _Position, Vector2 _Direction)
  {
    Position = _Position;
    Rotation = _Direction.Angle();
  }

  public void _on_body_entered(Node node)
  {
    QueueFree();
  }

  public void _on_visible_on_screen_notifier_2d_screen_exited()
  {
    QueueFree();
  }
}
