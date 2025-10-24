
/* Jenova C++ Nested Editor Plugin Extension */

// Godot SDK
#include <Godot/godot.hpp>
#include <Godot/classes/editor_interface.hpp>
#include <Godot/classes/editor_plugin.hpp>
#include <Godot/classes/editor_plugin_registration.hpp>

// Namespaces
using namespace godot;
using namespace jenova::sdk;

// EditorTool Implementation
class $BASE_CLASS_NAME$ : public EditorPlugin
{
	GDCLASS($BASE_CLASS_NAME$, EditorPlugin);

private:
	/* Write Internal Values Here */

protected:
	static void _bind_methods()
	{
		/* Write Bindings Here */
	}

public:
	// Awake/Destroy
	void OnAwake()
	{
		// Register Tool Menu
		add_tool_menu_item("Open $BASE_CLASS_NAME$...", callable_mp(this, &$BASE_CLASS_NAME$::OnToolMenuItemClick));
	}
	void OnDestroy()
	{
	}

	// Events
	void OnToolMenuItemClick()
	{
		Alert("Editor Tool $BASE_CLASS_NAME$ Executed!");
	}
};
 
// Register/Unregister $BASE_CLASS_NAME$ Extension
void Register$BASE_CLASS_NAME$()
{
	// We Only Register This Extension In Editor
	if (GetEngineMode() == EngineMode::Editor)
	{
		// Register Class
		ClassDB::register_class<$BASE_CLASS_NAME$>();
		
		// Register Plugin
		EditorPlugins::add_by_type<$BASE_CLASS_NAME$>();
	}
}
void Unregister$BASE_CLASS_NAME$()
{
	if (ClassDB::class_exists("$BASE_CLASS_NAME$"))
	{
		// Unregister Plugin
		EditorPlugins::remove_by_type<$BASE_CLASS_NAME$>();
		
		// Release Class
		sakura::Dispose("$BASE_CLASS_NAME$");
	}
}

// $BASE_CLASS_NAME$ Extension Activator
JENOVA_ACTIVATOR($BASE_CLASS_NAME$, Register$BASE_CLASS_NAME$, Unregister$BASE_CLASS_NAME$)