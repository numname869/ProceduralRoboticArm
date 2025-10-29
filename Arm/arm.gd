extends Node2D

const SMOOTHING = 0.2
const MIN_DIST = 15
const TOO_CLOSE_DIST = 10

@onready var joint1 = $joint1
@onready var joint2 = $joint1/joint2
@onready var hand = $joint1/joint2/hand

@export var flipped = true
@export var rotation_smoothing = 0.1

var len1 = 0
var len2 = 0
var target_pos = Vector2()

func _ready():
	len1 = joint2.position.length()
	len2 = hand.position.length()

func _process(delta):
	var mouse_pos = get_viewport().get_mouse_position()
	target_pos = target_pos.lerp(mouse_pos, SMOOTHING)
	update_ik(mouse_pos, delta)

func update_ik(look_at_pos: Vector2, _delta: float):
	var base_pos = joint1.global_position
	var distance_to_mouse = base_pos.distance_to(look_at_pos)
	
	# SAFETY - smooth transition to rest position
	if distance_to_mouse < TOO_CLOSE_DIST:
		var t = 1.0 - (distance_to_mouse / TOO_CLOSE_DIST)  # 0 when far, 1 when very close
		
		# Rest position angles
		var rest_angle1 = deg_to_rad(90) if flipped else deg_to_rad(-90)
		var rest_angle2 = deg_to_rad(-30) if flipped else deg_to_rad(30)
		
		# Interpolate to rest position
		joint1.global_rotation = lerp_angle(joint1.global_rotation, rest_angle1, t * rotation_smoothing)
		joint2.rotation = lerp_angle(joint2.rotation, rest_angle2, t * rotation_smoothing)
		
		# Hand gently looks toward the mouse
		var look_direction = (look_at_pos - hand.global_position).normalized()
		if look_direction.length() > 0.1:
			var target_hand_angle = look_direction.angle() - hand.get_parent().global_rotation
			hand.rotation = lerp_angle(hand.rotation, target_hand_angle, rotation_smoothing)
		return
	
	# NORMAL OPERATION
	var hand_to_mouse_dir = (look_at_pos - hand.global_position).normalized()
	var desired_joint2_pos = look_at_pos - hand_to_mouse_dir * len2
	
	var to_joint2 = desired_joint2_pos - base_pos
	var dist_to_joint2 = to_joint2.length()
	
	# Constraints
	var max_reach_joint1 = len1
	if dist_to_joint2 > max_reach_joint1:
		to_joint2 = to_joint2.normalized() * max_reach_joint1
	elif dist_to_joint2 < MIN_DIST:
		to_joint2 = to_joint2.normalized() * MIN_DIST
	
	var joint1_angle = to_joint2.angle()
	
	# Rotate joint2 to aim
	var joint2_to_hand = hand.global_position - joint2.global_position
	var joint2_to_mouse = look_at_pos - joint2.global_position
	
	if joint2_to_hand.length() > 0.1 and joint2_to_mouse.length() > 0.1:
		var joint2_angle_correction = joint2_to_hand.angle_to(joint2_to_mouse) * 0.3
		
		if flipped:
			joint1.global_rotation = lerp_angle(joint1.global_rotation, joint1_angle, rotation_smoothing)
			joint2.rotation = lerp_angle(joint2.rotation, joint2.rotation + joint2_angle_correction, rotation_smoothing)
		else:
			joint1.global_rotation = lerp_angle(joint1.global_rotation, -joint1_angle, rotation_smoothing)
			joint2.rotation = lerp_angle(joint2.rotation, joint2.rotation - joint2_angle_correction, rotation_smoothing)

func lerp_angle(from, to, weight):
	return from + short_angle_dist(from, to) * weight

func short_angle_dist(from, to):
	var max_angle = PI * 2
	var difference = fmod(to - from, max_angle)
	return fmod(2 * difference, max_angle) - difference
