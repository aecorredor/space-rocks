using Godot;
using System;
using System.Linq;

enum PlayerState
{
  Init,
  Alive,
  Dead
}

public partial class player : RigidBody2D
{
  PlayerState state = PlayerState.Init;

  [Signal]
  public delegate void LivesChangedEventHandler();

  [Signal]
  public delegate void DeadEventHandler();

  [Signal]
  public delegate void ShieldChangedEventHandler();

  [Export]
  float maxShield = 100.0f;

  [Export]
  float shieldRegen = 2.50f;

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
  private float shield = 100.0f;

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
    Shield += shieldRegen * (float)delta;
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
    }
  }

  public float Shield
  {
    get { return shield; }
    set
    {
      shield = Math.Min(value, maxShield);
      EmitSignal(SignalName.ShieldChanged, shield / maxShield);

      if (shield <= 0)
      {
        Lives -= 1;
        explode();
        Shield = maxShield;
      }
    }
  }

  public void reset()
  {
    resetPos = true;
    GetNode<Sprite2D>("Sprite2D").Show();
    Lives = 3;
    changeState(PlayerState.Alive);
  }

  public void _on_gun_cooldown_timeout()
  {
    canShoot = true;
  }

  public void _on_body_entered(rock body)
  {
    if (body.IsInGroup("rocks") && body.HasMethod("explode"))
    {
      Shield -= body.size * 25;
      body.Call("explode");
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
    GetNode<CpuParticles2D>("Exhaust").Emitting = false;
    thrust = Vector2.Zero;
    PlayerState[] noOpStates = { PlayerState.Init, PlayerState.Dead };

    if (noOpStates.Contains(state))
    {
      return;
    }

    rotationDir = Input.GetAxis("rotate_left", "rotate_right");

    if (Input.IsActionPressed("thrust"))
    {
      GetNode<CpuParticles2D>("Exhaust").Emitting = true;
      thrust = Transform.X * enginePower;
      var EngineSound = GetNode<AudioStreamPlayer>("EngineSound");

      if (!EngineSound.Playing)
      {
        EngineSound.Play();
      }
    }

    if (Input.IsActionJustPressed("shoot") && canShoot)
    {
      GetNode<AudioStreamPlayer>("LaserSound").Play();
      canShoot = false;
      GetNode<Timer>("GunCooldown").Start();
      var bullet = BulletScene.Instantiate() as bullet;
      GetTree().Root.AddChild(bullet);
      bullet.start(GetNode<Marker2D>("Muzzle").GlobalTransform);
    }
  }

  void changeState(PlayerState newState)
  {
    state = newState;
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

      case PlayerState.Dead:
      {
        collisionShape.SetDeferred("disabled", true);
        sprite.Hide();
        LinearVelocity = Vector2.Zero;
        EmitSignal(SignalName.Dead);
        GetNode<AudioStreamPlayer>("EngineSound").Stop();
        break;
      }
    }
  }
}
