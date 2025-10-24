
/* Jenova C++ Nested Extension */

// Godot SDK
#include <Godot/godot.hpp>
#include <Godot/classes/node.hpp>
#include <Godot/classes/$BASE_TYPE_HEADER$>

// Namespaces
using namespace godot;
using namespace jenova::sdk;

// $BASE_CLASS_NAME$ Implementation
class $BASE_CLASS_NAME$ : public $BASE_TYPE$
{
	GDCLASS($BASE_CLASS_NAME$, $BASE_TYPE$);

private:
	/* Write Internal Values Here */

protected:
	static void _bind_methods()
	{
		/* Write Bindings Here */
	}
	
public:
	/* Write Setters/Getters Here */
	
public:
	/* Write  Signals Values Here */
	
public:
	// Routines
	void _enter_tree() override
	{
		// Called When Node Enters Scene Tree
	}
	void _exit_tree() override
	{
		// Called When Node Exits Scene Tree
	}
	void _ready() override 
	{
		// Called When Node and All It's Children Entered Scene Tree
	}
	void _process(double p_delta) override 
	{
		// Called On Every Frame
	}
};
 
// Register/Unregister
void Register$BASE_CLASS_NAME$()
{
	// Register Class
	ClassDB::register_class<$BASE_CLASS_NAME$>();
	
	// Finish Reload
	sakura::FinishReload("$BASE_CLASS_NAME$");
}
void Unregister$BASE_CLASS_NAME$()
{
	// Prepare for Reload
	sakura::PrepareReload("$BASE_CLASS_NAME$");
	
	// Release Class
	sakura::Dispose("$BASE_CLASS_NAME$");
}

// Activator
JENOVA_ACTIVATOR($BASE_CLASS_NAME$, Register$BASE_CLASS_NAME$, Unregister$BASE_CLASS_NAME$)