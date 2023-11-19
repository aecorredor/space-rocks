using Godot;
using System;
using System.Linq;

public partial class hud : CanvasLayer
{
  [Signal]
  public delegate void StartGameEventHandler();

  private TextureRect[] livesCounter;
  private Label scoreLabel;
  private Label message;
  private TextureButton startButton;

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

  private void updateLives(int value)
  {
    for (int i = 0; i < livesCounter.Length; i++)
    {
      livesCounter[i].Visible = i < value;
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
