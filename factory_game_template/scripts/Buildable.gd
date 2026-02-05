extends StaticBody3D
class_name Buildable

@export var is_ghost := false
@export var ghost_alpha := 0.35

func _ready() -> void:
	_apply_ghost_state()

func set_ghost(value: bool) -> void:
	is_ghost = value
	_apply_ghost_state()

func _apply_ghost_state() -> void:
	if is_ghost:
		collision_layer = 0
		collision_mask = 0
	else:
		collision_layer = 1
		collision_mask = 1

	for child in get_children():
		if child is GeometryInstance3D:
			child.transparency = ghost_alpha if is_ghost else 0.0
			child.cast_shadow = not is_ghost

