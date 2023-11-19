using Godot;
using System;

public partial class bullet : Area2D
{
    [Export]
    int speed = 100;

    Vector2 velocity = Vector2.Zero;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready() { }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Position += velocity * (float)delta;
    }

    public void start(Transform2D _transform)
    {
        Transform = _transform;
        velocity = Transform.X * speed;
    }

    public void _on_visible_on_screen_notifier_2d_screen_exited()
    {
        QueueFree();
    }

    public void _on_body_entered(Node body)
    {
        if (body.IsInGroup("rocks") && body.HasMethod("explode"))
        {
            body.Call("explode");
            QueueFree();
        }
    }
}
