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

  [Export]
  float bulletSpread = 0.2f;

  PathFollow2D follow = new PathFollow2D();

  public RigidBody2D player;

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

  public void shoot()
  {
    GetNode<AudioStreamPlayer>("ShootSound").Play();
    var dir = GlobalPosition.DirectionTo(player.GlobalPosition);
    dir = dir.Rotated((float)GD.RandRange(-bulletSpread, bulletSpread));
    var b = BulletScene.Instantiate() as enemy_bullet;
    GetTree().Root.AddChild(b);
    b.Start(GlobalPosition, dir);
  }

  public void takeDamage(int amount)
  {
    health -= amount;
    GetNode<AnimationPlayer>("AnimationPlayer").Play("flash");

    if (health <= 0)
    {
      explode();
    }
  }

  private async void explode()
  {
    GetNode<AudioStreamPlayer>("ExplosionSound").Play();
    speed = 0;
    GetNode<Timer>("GunCooldown").Stop();
    GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
    GetNode<Sprite2D>("Sprite2D").Hide();
    GetNode<Sprite2D>("Explosion").Show();
    GetNode<AnimationPlayer>("Explosion/AnimationPlayer").Play("explosion");
    GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred("disabled", true);
    var explosionSprite = GetNode<Sprite2D>("Explosion");
    explosionSprite.Show();
    var explosionAnimation = explosionSprite.GetNode<AnimationPlayer>(
      "AnimationPlayer"
    );
    explosionAnimation.Play("explosion");
    await ToSignal(explosionAnimation, "animation_finished");
    QueueFree();
  }

  public void _on_body_entered(Node body)
  {
    if (body.IsInGroup("rocks"))
    {
      return;
    }

    explode();

    if (body.Name == "Player")
    {
      (body as player).Shield -= 50;
    }
  }

  private void _on_gun_cooldown_timeout()
  {
    shoot();
  }
}
