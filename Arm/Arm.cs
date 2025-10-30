using Godot;

public partial class Arm : Node2D
{
	private const float SMOOTHING = 0.2f;
	private const float MIN_DIST = 15f;
	private const float TOO_CLOSE_DIST = 10f;

	[Export] public bool Flipped { get; set; } = true;
	[Export] public float RotationSmoothing { get; set; } = 0.1f;

	private Node2D joint1;
	private Node2D joint2;
	private Node2D hand;

	private float len1;
	private float len2;
	private Vector2 targetPos;

	public override void _Ready()
	{
		// Automatyczne pobranie węzłów według hierarchii
		joint1 = GetNode<Node2D>("joint1");
		joint2 = joint1.GetNode<Node2D>("joint2");
		hand = joint2.GetNode<Node2D>("hand");

		len1 = joint2.Position.Length();
		len2 = hand.Position.Length();
		targetPos = joint1.GlobalPosition;
	}

	public override void _Process(double delta)
	{
		Vector2 mousePos = GetViewport().GetMousePosition();
		targetPos = targetPos.Lerp(mousePos, SMOOTHING);
		UpdateIK(mousePos, (float)delta);
	}

	private void UpdateIK(Vector2 lookAtPos, float delta)
	{
		Vector2 basePos = joint1.GlobalPosition;
		float distanceToMouse = basePos.DistanceTo(lookAtPos);

		// SAFETY - smooth transition to rest position
		if (distanceToMouse < TOO_CLOSE_DIST)
		{
			float t = 1f - (distanceToMouse / TOO_CLOSE_DIST);
			float restAngle1 = Flipped ? Mathf.DegToRad(90) : Mathf.DegToRad(-90);
			float restAngle2 = Flipped ? Mathf.DegToRad(-30) : Mathf.DegToRad(30);

			joint1.GlobalRotation = LerpAngle(joint1.GlobalRotation, restAngle1, t * RotationSmoothing);
			joint2.Rotation = LerpAngle(joint2.Rotation, restAngle2, t * RotationSmoothing);

			Vector2 lookDir = (lookAtPos - hand.GlobalPosition).Normalized();
			if (lookDir.Length() > 0.1f)
			{
				float targetHandAngle = lookDir.Angle() - hand.GetParent<Node2D>().GlobalRotation;
				hand.Rotation = LerpAngle(hand.Rotation, targetHandAngle, RotationSmoothing);
			}
			return;
		}

		// NORMAL OPERATION
		Vector2 handToMouseDir = (lookAtPos - hand.GlobalPosition).Normalized();
		Vector2 desiredJoint2Pos = lookAtPos - handToMouseDir * len2;

		Vector2 toJoint2 = desiredJoint2Pos - basePos;
		float distToJoint2 = toJoint2.Length();

		// Constraints
		float maxReachJoint1 = len1;
		if (distToJoint2 > maxReachJoint1)
			toJoint2 = toJoint2.Normalized() * maxReachJoint1;
		else if (distToJoint2 < MIN_DIST)
			toJoint2 = toJoint2.Normalized() * MIN_DIST;

		float joint1Angle = toJoint2.Angle();

		Vector2 joint2ToHand = hand.GlobalPosition - joint2.GlobalPosition;
		Vector2 joint2ToMouse = lookAtPos - joint2.GlobalPosition;

		if (joint2ToHand.Length() > 0.1f && joint2ToMouse.Length() > 0.1f)
		{
			float joint2AngleCorrection = joint2ToHand.AngleTo(joint2ToMouse) * 0.3f;

			if (Flipped)
			{
				joint1.GlobalRotation = LerpAngle(joint1.GlobalRotation, joint1Angle, RotationSmoothing);
				joint2.Rotation = LerpAngle(joint2.Rotation, joint2.Rotation + joint2AngleCorrection, RotationSmoothing);
			}
			else
			{
				joint1.GlobalRotation = LerpAngle(joint1.GlobalRotation, -joint1Angle, RotationSmoothing);
				joint2.Rotation = LerpAngle(joint2.Rotation, joint2.Rotation - joint2AngleCorrection, RotationSmoothing);
			}
		}
	}

	private float LerpAngle(float from, float to, float weight)
	{
		return from + ShortAngleDist(from, to) * weight;
	}

	private float ShortAngleDist(float from, float to)
	{
		float maxAngle = Mathf.Pi * 2f;
		float difference = Mathf.PosMod(to - from, maxAngle);
		return Mathf.PosMod(2f * difference, maxAngle) - difference;
	}
}
