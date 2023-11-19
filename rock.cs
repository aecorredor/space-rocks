using Godot;
using System;

public partial class rock : RigidBody2D
{
    public Vector2 screenSize = Vector2.Zero;
    int size;
    float radius;
    float scaleFactor = 0.2f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        screenSize = GetViewportRect().Size;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

    public override void _IntegrateForces(PhysicsDirectBodyState2D physicsState)
    {
        var xform = physicsState.Transform;
        xform.Origin.X = Mathf.Wrap(xform.Origin.X, 0 - radius, screenSize.X + radius);
        xform.Origin.Y = Mathf.Wrap(xform.Origin.Y, 0 - radius, screenSize.Y + radius);
        physicsState.Transform = xform;
    }

    public void start(Vector2 position, Vector2 velocity, int _size)
    {
        Position = position;
        size = _size;
        Mass = 1.5f * size;
        GetNode<Sprite2D>("Sprite2D").Scale = Vector2.One * scaleFactor * size;
        var sprite2D = GetNode<Sprite2D>("Sprite2D");
        radius = (int)sprite2D.Texture.GetSize().X / 2 * sprite2D.Scale.X;
        var shape = new CircleShape2D();
        shape.Radius = radius;
        GetNode<CollisionShape2D>("CollisionShape2D").Shape = shape;
        LinearVelocity = velocity;
        AngularVelocity = (float)GD.RandRange(-Math.PI, Math.PI);
    }
}
