// picking_shader.gdshader
shader_type spatial;
render_mode unshaded;

uniform vec4 object_id : source_color;

void fragment() {
    ALBEDO = object_id.rgb;
}
