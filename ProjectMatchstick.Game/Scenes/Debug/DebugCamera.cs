using Godot;

namespace ProjectMatchstick.Game.Debug;

public partial class DebugCamera : Camera2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		Transform = Transform.Translated(direction * 1500f * (float)delta);

		float zoom = Input.GetAxis("zoom_out", "zoom_in");

		this.Zoom += this.Zoom * zoom * (float)delta * 1f;
    }
}
