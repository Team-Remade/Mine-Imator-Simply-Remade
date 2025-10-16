extends MarginContainer

@export var textLabel: Label

func _process(_delta: float) -> void:
	if not visible:
		return
	textLabel.text = str(Engine.get_frames_per_second()) + " FPS"
