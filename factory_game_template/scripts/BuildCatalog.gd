extends Node
class_name BuildCatalog

var _buildables := {
	"Conveyor": preload("res://scenes/buildables/Conveyor.tscn"),
	"Smelter": preload("res://scenes/buildables/Smelter.tscn"),
	"Storage": preload("res://scenes/buildables/Storage.tscn"),
}

func get_names() -> Array[String]:
	var names: Array[String] = _buildables.keys()
	names.sort()
	return names

func get_scene(name: String) -> PackedScene:
	return _buildables.get(name)

func has_buildable(name: String) -> bool:
	return _buildables.has(name)

