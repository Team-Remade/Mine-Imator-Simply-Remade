## Copyright (c) 2025-present squirrel-bat under the MIT License.

@tool
extends EditorPlugin

const SETTING_NAME: StringName = "run/window_placement/screen"
var options: Dictionary[String, int]
var button := OptionButton.new()


func _enter_tree() -> void:
	var property_list: Array[Dictionary] = (
		get_editor_interface().get_editor_settings().get_property_list()
	)
	var idx: int = property_list.find_custom(is_screen_setting)
	assert(idx > -1, 'Failed to load EditorSetting from path "%s".' % SETTING_NAME)

	var screen_setting: Dictionary = property_list[idx]
	options = hint_string_to_options(screen_setting.hint_string)
	for label in options:
		button.add_item(label, options[label])
	# select before connect to prevent superfluous call
	button.select(
		button.get_item_index(
			get_editor_interface().get_editor_settings().get_setting(SETTING_NAME)
		)
	)
	button.item_selected.connect(_on_item_selected)
	add_control_to_container(EditorPlugin.CONTAINER_TOOLBAR, button)


func is_screen_setting(el) -> bool:
	return el.name == SETTING_NAME


func hint_string_to_options(input: String) -> Dictionary[String, int]:
	var result: Dictionary[String, int]
	for option in input.split(",", false):
		var option_key_value: PackedStringArray = option.split(":", false)
		result[tr(option_key_value[0])] = int(option_key_value[1])
	return result


func _exit_tree() -> void:
	remove_control_from_container(EditorPlugin.CONTAINER_TOOLBAR, button)
	if button:
		button.queue_free()


func _on_item_selected(item_index) -> void:
	get_editor_interface().get_editor_settings().set_setting(
		SETTING_NAME, options.values()[item_index]
	)
