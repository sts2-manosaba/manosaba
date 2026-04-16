shader_type canvas_item;

uniform vec4 key_color : source_color = vec4(0.0, 1.0, 0.0, 1.0);
uniform float threshold = 0.12;
uniform float softness = 0.10;
uniform float despill = 0.65;

void fragment() {
	vec4 col = texture(TEXTURE, UV);

	// Prefer green-dominance keying over raw RGB distance so green spill is removed
	// more aggressively while preserving non-green subjects.
	float green_dominance = col.g - max(col.r, col.b);
	float keep = 1.0 - smoothstep(threshold, threshold + softness, green_dominance);

	// Reduce green fringe on remaining visible pixels.
	float spill = max(0.0, green_dominance) * despill;
	vec3 corrected = col.rgb;
	corrected.g = max(corrected.g - spill, (corrected.r + corrected.b) * 0.5);

	COLOR = vec4(corrected, col.a * keep);
}
