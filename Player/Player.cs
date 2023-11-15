using Godot;
using System;
using System.Diagnostics;
using System.Linq;

enum PlayerState
{
    Init,
    Alive,
    Invulnerable,
    Dead
}

public partial class Player : RigidBody2D
{
    PlayerState state = PlayerState.Init;

    [Export]
    int enginePower = 500;
    int spinPower = 8000;

    Vector2 thrust = Vector2.Zero;
    float rotationDir = 0;
    Vector2 screenSize = Vector2.Zero;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        screenSize = GetViewportRect().Size;
        changeState(PlayerState.Alive);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        getInput();
    }

    public override void _PhysicsProcess(double delta)
    {
        ConstantForce = thrust;
        ConstantTorque = rotationDir * spinPower;
        Debug.WriteLine("ConstantTorque: " + ConstantTorque);
        Debug.WriteLine("ConstantForce: " + ConstantForce);
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D physicsState)
    {
        var xform = physicsState.Transform;
        xform.Origin.X = Mathf.Wrap(xform.Origin.X, 0, screenSize.X);
        xform.Origin.Y = Mathf.Wrap(xform.Origin.Y, 0, screenSize.Y);
        physicsState.Transform = xform;
    }

    private void getInput()
    {
        thrust = Vector2.Zero;
        PlayerState[] invalidStates = { PlayerState.Init, PlayerState.Dead };

        if (invalidStates.Contains(state))
        {
            return;
        }

        // check if player state array includes state
        if (Input.IsActionPressed("thrust"))
        {
            // log thrust
            Debug.WriteLine("Thrusting");
            thrust = Transform.X * enginePower;
        }

        rotationDir = Input.GetAxis("rotate_left", "rotate_right");
    }

    private void changeState(PlayerState newState)
    {
        CollisionShape2D collisionShape = GetNode<CollisionShape2D>("CollisionShape2D");

        switch (state)
        {
            case PlayerState.Init:
                collisionShape.SetDeferred("disabled", true);
                break;

            case PlayerState.Alive:
                collisionShape.SetDeferred("disabled", false);
                break;

            case PlayerState.Invulnerable:
                collisionShape.SetDeferred("disabled", true);
                break;

            case PlayerState.Dead:
                collisionShape.SetDeferred("disabled", true);
                break;
        }

        this.state = newState;
    }
}
