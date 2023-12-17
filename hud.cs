using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

public partial class hud : CanvasLayer
{
  [Signal]
  public delegate void StartGameEventHandler();

  private TextureRect[] livesCounter;
  private Label scoreLabel;
  private Label message;
  private TextureButton startButton;
  private TextureProgressBar shieldBar;
  private Dictionary<string, Resource> barTextures =
    new Dictionary<string, Resource>();

  // Called when the node enters the scene tree for the first time.
  public override void _Ready()
  {
    livesCounter = GetNode<HBoxContainer>(
        "MarginContainer/HBoxContainer/LivesCounter"
      )
      .GetChildren()
      .OfType<TextureRect>()
      .ToArray();
    scoreLabel = GetNode<Label>("MarginContainer/HBoxContainer/ScoreLabel");
    message = GetNode<Label>("VBoxContainer/Message");
    startButton = GetNode<TextureButton>("VBoxContainer/StartButton");
    shieldBar = GetNode<TextureProgressBar>(
      "MarginContainer/HBoxContainer/ShieldBar"
    );
    barTextures["green"] = GD.Load("res://assets/bar_green_200.png");
    barTextures["yellow"] = GD.Load("res://assets/bar_yellow_200.png");
    barTextures["red"] = GD.Load("res://assets/bar_red_200.png");
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta) { }

  public void showMessage(string text)
  {
    message.Text = text;
    message.Show();
    GetNode<Timer>("Timer").Start();
  }

  public void updateScore(int value)
  {
    scoreLabel.Text = value.ToString();
  }

  public void updateLives(int value)
  {
    for (int i = 0; i < livesCounter.Length; i++)
    {
      livesCounter[i].Visible = i < value;
    }
  }

  public void updateShield(float value)
  {
    shieldBar.Value = value;
    shieldBar.TextureProgress = (Texture2D)barTextures["green"];

    if (value < 0.4)
    {
      shieldBar.TextureProgress = (Texture2D)barTextures["red"];
      return;
    }

    if (value < 0.7)
    {
      shieldBar.TextureProgress = (Texture2D)barTextures["yellow"];
      return;
    }
  }

  public async void gameOver()
  {
    showMessage("Game Over");
    await ToSignal(GetNode<Timer>("Timer"), "timeout");
    startButton.Show();
  }

  private void _on_start_button_pressed()
  {
    startButton.Hide();
    EmitSignal(SignalName.StartGame);
  }

  private void _on_timer_timeout()
  {
    message.Hide();
    message.Text = "";
  }
}
