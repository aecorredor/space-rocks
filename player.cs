using Godot;
using System;
using System.Linq;

enum PlayerState
{
  Init,
  Alive,
  Invulnerable,
  Dead
}

public partial class player : RigidBody2D
{
  PlayerState state = PlayerState.Init;

  [Signal]
  public delegate void LivesChangedEventHandler();

  [Signal]
  public delegate void DeadEventHandler();

  [Export]
  int enginePower = 500;

  [Export]
  int spinPower = 8000;

  [Export]
  PackedScene BulletScene;

  [Export]
  float fireRate = 0.25f;

  bool canShoot = true;

  Vector2 thrust = Vector2.Zero;
  float rotationDir = 0;
  Vector2 screenSize = Vector2.Zero;

  bool resetPos = false;
  private int lives = 0;

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    screenSize = GetViewportRect().Size;
    changeState(PlayerState.Alive);
    GetNode<Timer>("GunCooldown").WaitTime = fireRate;
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
  }

  public override void _IntegrateForces(PhysicsDirectBodyState2D physicsState)
  {
    var xform = physicsState.Transform;
    xform.Origin.X = Mathf.Wrap(xform.Origin.X, 0, screenSize.X);
    xform.Origin.Y = Mathf.Wrap(xform.Origin.Y, 0, screenSize.Y);

    if (resetPos)
    {
      xform.Origin = screenSize / 2;
      resetPos = false;
    }

    physicsState.Transform = xform;
  }

  public int Lives
  {
    get { return lives; }
    set
    {
      lives = value;
      EmitSignal(SignalName.LivesChanged, lives);

      if (lives <= 0)
      {
        changeState(PlayerState.Dead);
      }
      else
      {
        changeState(PlayerState.Invulnerable);
      }
    }
  }

  public void reset()
  {
    resetPos = true;
    GetNode<Sprite2D>("Sprite2D").Show();
    lives = 3;
    changeState(PlayerState.Alive);
  }

  public void _on_gun_cooldown_timeout()
  {
    canShoot = true;
  }

  public void _on_invulnerability_timer_timeout()
  {
    changeState(PlayerState.Alive);
  }

  public void _on_body_entered(Node body)
  {
    if (body.IsInGroup("rocks") && body.HasMethod("explode"))
    {
      body.Call("explode");
      Lives -= 1;
      explode();
    }
  }

  private async void explode()
  {
    var explosionSprite = GetNode<Sprite2D>("Explosion");
    explosionSprite.Show();
    var explosionAnimation = explosionSprite.GetNode<AnimationPlayer>(
      "AnimationPlayer"
    );
    explosionAnimation.Play("explosion");
    await ToSignal(explosionAnimation, "animation_finished");
    explosionSprite.Hide();
  }

  void getInput()
  {
    thrust = Vector2.Zero;
    PlayerState[] noOpStates = { PlayerState.Init, PlayerState.Dead };

    if (noOpStates.Contains(state))
    {
      return;
    }

    rotationDir = Input.GetAxis("rotate_left", "rotate_right");

    if (Input.IsActionPressed("thrust"))
    {
      thrust = Transform.X * enginePower;
    }

    if (Input.IsActionJustPressed("shoot") && canShoot)
    {
      if (state == PlayerState.Invulnerable)
      {
        return;
      }

      canShoot = false;
      GetNode<Timer>("GunCooldown").Start();
      var bullet = BulletScene.Instantiate() as bullet;
      GetTree().Root.AddChild(bullet);
      bullet.start(GetNode<Marker2D>("Muzzle").GlobalTransform);
    }
  }

  void changeState(PlayerState newState)
  {
    CollisionShape2D collisionShape = GetNode<CollisionShape2D>(
      "CollisionShape2D"
    );
    var sprite = GetNode<Sprite2D>("Sprite2D");

    switch (state)
    {
      case PlayerState.Init:
      {
        collisionShape.SetDeferred("disabled", false);
        var modulate = sprite.Modulate;
        modulate.A = 0.5f;
        sprite.Modulate = modulate;
        break;
      }

      case PlayerState.Alive:
      {
        collisionShape.SetDeferred("disabled", false);
        var modulate = sprite.Modulate;
        modulate.A = 1.0f;
        sprite.Modulate = modulate;
        break;
      }

      case PlayerState.Invulnerable:
      {
        collisionShape.SetDeferred("disabled", true);
        var modulate = sprite.Modulate;
        modulate.A = 0.5f;
        sprite.Modulate = modulate;
        GetNode<Timer>("InvulnerabilityTimer").Start();
        break;
      }

      case PlayerState.Dead:
        collisionShape.SetDeferred("disabled", true);
        sprite.Hide();
        LinearVelocity = Vector2.Zero;
        EmitSignal(SignalName.Dead);
        break;
    }

    state = newState;
  }
}
