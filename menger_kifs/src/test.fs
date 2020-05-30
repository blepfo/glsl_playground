#version 330 core

out vec4 FragColor;

uniform vec2 iResolution;
uniform vec3 eye;
uniform vec3 forward;
uniform vec3 up;
uniform vec3 right;


float iTime = 0.;


#define PI (3.141)
#define TWOPI (2.*PI)
#define RAND_SEED (87629.3453)

#define MARCH_MAX_STEPS (512)
#define MARCH_MAX_DIST (1000.)
#define MARCH_HIT_DIST (0.001)

#define AO_ITERATIONS (1)
#define GLOBAL_AO (0.327)

#define SHADE_AO_ONLY (false)

// Material IDs
// Used so we can assign materials to objects
// when scene SDF is defined, but we can defer material calculation
// until later when we can compute normal
#define MATERIAL_GROUND_PLANE (0)
#define MATERIAL_CHECKERBOARD (1)

struct Ray {
    vec3 o;			// origin
    vec3 d;			// direction
};

struct PointLight { 
    vec3 o;			// origin
    vec3 d;			// diffuse intensity
    vec3 s; 		// specular intensity
    vec3 a;			// ambient intensity
};

struct DirectionLight {
    vec3 dir;		// direction
    vec3 d;			// diffuse intensity
    vec3 s; 		// specular intensity
    vec3 a;			// ambient intensity
};

struct Material {
    vec3 d;			// diffuse
    vec3 s;			// specular
    vec3 a;			// ambient
    float shiny;	// shininess
};

struct SceneObj {
    float sdf;
	int matId;
};

struct Hit {
    vec3 p;
    vec3 n;
    SceneObj obj;
};

vec3 translate(vec3 p, vec3 t) {
    return p - t;
}

mat2 rotate2d(float theta) { 
    return mat2(cos(theta), -sin(theta), 
                sin(theta), cos(theta)); 
}

vec2 repeat(vec2 p, float c) {
    // Center around origin
    p = p + (c/2.);
    // Repeat after interval of c, recenter around origin
    return mod(p, c) - (c / 2.);
}

vec2 repeat(vec2 p, float c, float l) {
	// Center around origin
    p = p + (c/2.);
    // floor(p/c) == grid idxs for grid of size c
    // by clamping floor(p/c) to +/- l, we only alow 
    // repeating up to (2l+1) idxs
    return (p - c*clamp(floor(p/c), -l, l)) - (c / 2.);
}


/********* Signed Distance Functions (SDFs)

References for SDFs:
Sphere, Cube, Torus, Capsule, Cylinder - https://youtu.be/Ff0jJyyiVyw (ArtOfCode)
Rotate, Scale, Union, Intersection, Difference - https://youtu.be/AfKGMUDWfuE (ArtOfCode)
Onioning, Displacement Mapping, Twisting - https://youtu.be/Vmb7VGBVZJA (ArtOfCode)

https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm (Inigo Quilez)
**********/

float sdfSphere(vec3 p, float r) {
    return length(p) - r;
}

float sdfPlane(vec3 p, vec3 n) {
    return dot(p, n);
}

float sdfTorus(vec3 p, vec2 r) {
    // Distance to major circle
    float x = length(p.xz) - r[0];
    return length(vec2(x, p.y)) - r[1];
}

float sdfCube(vec3 p, vec3 halfSize) {
    vec3 edgeDist = abs(p) - halfSize;
    return length(max(edgeDist, 0.))
        + min(max(edgeDist.x, max(edgeDist.y, edgeDist.z)), 0.);
}

SceneObj objUnion(SceneObj s1, SceneObj s2) {
    if (s1.sdf < s2.sdf) {
        return s1;
    } else {
        return s2;
    }
}

vec3 fold(vec3 p, vec3 n) {
    n = normalize(n);
    float d = dot(p, n);
    return p - (2. * max(d, 0.) * n);
}

vec3 fold(vec3 p, float pitch, float yaw) {
	float cp = cos(pitch);
    vec3 n = vec3(cp*sin(yaw), sin(pitch), cp*cos(yaw));
    return fold(p, n);
}

// Define all objects in the scene
// Use material Ids so we can wait until later to calculate Material colors
// Materials using textures need scene normal, which we can't get until after
// scene SDF is defined
SceneObj mapScene(vec3 p) {

    vec3 sphereP = p;
	float hPi = PI / 2.;
    hPi = iTime;
    
	sphereP.y *= -1.;
    sphereP /= 2.;
    #define iterations (4)
    sphereP = translate(sphereP, vec3(0., -0.25, 0.));
    for (int i = 0; i < iterations; i++) {
        sphereP *= 3.;
    	sphereP = fold(sphereP, vec3(0., 1., 0.));
    	sphereP = translate(sphereP, vec3(0., -2., 0.));
    	sphereP = fold(sphereP, vec3(-1., 0., 0.));
    	sphereP = translate(sphereP, vec3(2., 0., 0.));
		sphereP = fold(sphereP, 0., 0.);
    	sphereP = translate(sphereP, vec3(0., 0., -2.));
    	sphereP = fold(sphereP, -hPi/2., 0.);
    	sphereP = fold(sphereP, vec3(1., 1., 0.));
    	sphereP = translate(sphereP, vec3(-2., 0., 0.));
    	sphereP = fold(sphereP, vec3(1.,0.,0.));
    	sphereP = translate(sphereP, vec3(-1., 0., 0.));
    	sphereP = fold(sphereP, vec3(1.,0.,0.));
		sphereP = translate(sphereP, vec3(-1.,0.,0.));
    	sphereP = fold(sphereP, vec3(1., 1.,0.));
    }
    
    



    float sphere1Sdf = sdfCube(sphereP, vec3(1.));
    sphere1Sdf /= pow(3., float(iterations));
    SceneObj sphere1 = SceneObj(sphere1Sdf, MATERIAL_GROUND_PLANE);
    
    float groundPlaneSdf = sdfPlane(p, normalize(vec3(0.,1.,0.)));
    SceneObj groundPlane = SceneObj(groundPlaneSdf, MATERIAL_GROUND_PLANE);

    
    SceneObj s = groundPlane;
    s = objUnion(groundPlane, sphere1);
    s = sphere1;
    return s ;
}

float sdfScene(vec3 p) {
	SceneObj objAtP = mapScene(p);
    return objAtP.sdf;
}


// Scene normals using SDF gradient
// References for normal calculation: 
// http://www.michaelwalczyk.com/blog/2017/5/25/ray-marching (Michael Walczyk)
// https://www.iquilezles.org/www/articles/normalsSDF/normalsSDF.htm (Inigo Quilez)
vec3 sceneNormal(vec3 p) {
    vec2 epsilon = vec2(0.01, 0.);
    return normalize(vec3(
        sdfScene(p + epsilon.xyy) - sdfScene(p - epsilon.xyy),
        sdfScene(p + epsilon.yxy) - sdfScene(p - epsilon.yxy),
        sdfScene(p + epsilon.yyx) - sdfScene(p - epsilon.yyx)
    ));
}


/********** MATERIALS **********/

Material materialLookup(Hit hit) {
    int matId = hit.obj.matId;
    if (matId == MATERIAL_GROUND_PLANE) {
        // Plane material
        return Material(
        	vec3(0.535,0.535,0.535),
        	vec3(0.245,0.245,0.245), 
        	vec3(0.245,0.239,0.245),
        	256.
        );
    }
}


// References for raymarching:
// http://www.michaelwalczyk.com/blog/2017/5/25/ray-marching (Michael Walczyk)
// https://youtu.be/PGtv-dBi2wE (ArtofCode)
bool rayMarch(Ray r, out Hit hit) {
    // Accumulated distance to ray origin
    float dO = 0.;
    for (int i = 0; i < MARCH_MAX_STEPS; i++) {
        vec3 p = r.o + (dO * r.d);
        // Current distance to scene
        float dS = sdfScene(p);
        dO += dS;
        if (abs(dS) < MARCH_HIT_DIST) {
            hit = Hit(
                p,
                sceneNormal(p),
                mapScene(p)
            );
            return true;
        }
        if (dO >= MARCH_MAX_DIST) {
            return false;
        }
    }
    return false;
}


/********** LIGHTING **********/

// Reference for shadows: 
// https://youtu.be/2YZClgDWCaM (3dGraphicsFromScratch)
// http://www.polygonpi.com/?p=318 (Polygon Pi)
// https://www.iquilezles.org/www/articles/rmshadows/rmshadows.htm (Inigo Quilez)
float calcShadow(Ray shadowRay, float maxMarch, float k) {
    // Shadow increases when ray from surface to light gets closer to scene (dS gets smaller)
    // More distant objects cast darker shadows (dO gets larger)
    float shadowMultiplier = 1.;
    float dO = 0.;
    for (int i = 0; i < MARCH_MAX_STEPS; i++) {
        if (dO > maxMarch) { break; }
        vec3 p = shadowRay.o + (dO * shadowRay.d);
        float dS = sdfScene(p);
        if (abs(dS) < MARCH_HIT_DIST) {
            // Hit surface -- we are in a shadow
            return 0.;
        }
        dO += dS;
        shadowMultiplier = min(shadowMultiplier, k * dS / dO);
    }
	return clamp(shadowMultiplier, 0., 1.);
}

// Reference for ambient occlusion:
// https://youtu.be/6zYTrFRVGiU (3dGraphicsFromScratch)
float calcAO(vec3 p, vec3 normal, float epsilon) {
    float aoMultiplier = 0.;
    float weight = 0.5;
    for (int i = 1; i <= AO_ITERATIONS ; i++) {
        float t = epsilon * float(i);
        aoMultiplier += (weight * (1. - (t - sdfScene(p + t*normal))));
        weight /= 2.;
    }
    return aoMultiplier;
}


// Reference for lighting:
// Basics - http://www.michaelwalczyk.com/blog/2017/5/25/ray-marching (Michael Walczyk)
// Diffuse - https://youtu.be/9VJReTr7YXY (3dGraphicsFromScratch)
// Diffuse - shadows - https://youtu.be/PGtv-dBi2wE (ArtOfCode)
vec3 illuminateSingleLight(
    Hit hit, 
    vec3 lDir, 
    vec3 lDiffuse, 
    vec3 lSpec, 
    vec3 lAmbient, 
    vec3 eye,
    float maxMarch
) {
    Material mat = materialLookup(hit);
    // DIRECT LIGHTING
    // Diffuse illumination
    float diffuse = dot(hit.n, lDir);
    vec3 diffuseIllum = diffuse * lDiffuse * mat.d;
    // Specular illumination
    vec3 r = reflect(lDir, hit.n);
    vec3 viewDir = normalize(eye - hit.p);
    float specular = dot(r, viewDir);
    vec3 specularIllum = pow(specular, mat.shiny) * mat.s * lSpec;
    vec3 directLight = diffuseIllum + specularIllum;
    // Estimate shadow
    vec3 pNearSurface = hit.p + (hit.n * MARCH_HIT_DIST * 2.);
    float shadow = calcShadow(Ray(pNearSurface, lDir), maxMarch, 3.);
    directLight *= shadow;
    // INDIRECT LIGHTING
    vec3 ambientIllum = lAmbient * mat.a;
    float ambientOcclusion = calcAO(hit.p, hit.n, GLOBAL_AO);
    vec3 indirectLight = ambientIllum * ambientOcclusion;
    
    vec3 illum = directLight + indirectLight;
    
    if (SHADE_AO_ONLY) {
        return vec3(ambientOcclusion);
    }

    return clamp(illum, 0., 1.);
}


// DirectionalLight
vec3 illuminateSingleLight(Hit hit, DirectionLight l, vec3 eye, float maxMarch) {
    vec3 illum = illuminateSingleLight(hit, l.dir, l.d, l.s, l.a, eye, maxMarch);
    return illum;
}


// Point light
vec3 illuminateSingleLight(Hit hit, PointLight l, vec3 eye) {
    vec3 lDir = normalize(l.o - hit.p);
    // Light intensity decayed by 1/(dist^2)
    vec3 lDiff = (hit.p - l.o);
    float falloff = 1. / dot(lDiff, lDiff);
    vec3 illum = illuminateSingleLight(hit, DirectionLight(lDir, l.d*falloff, l.s*falloff, l.a), eye, distance(hit.p, l.o));

    return illum;
}


// Define all lights in the scene
vec3 illuminateScene(Hit hit, vec3 eye) {
    vec3 l1Pos = vec3(0.769,4.652,4.080);
    vec3 l1Intensity = vec3(20.);
    PointLight l1 = PointLight(
        l1Pos, 			
        l1Intensity, 
        l1Intensity, 
        vec3(0.090,0.090,0.090)
    );
    
    vec3 l2Pos = vec3(6., 4., -7.);
    vec3 l2Intensity = vec3(20.);
    PointLight l2 = PointLight(
        l2Pos, 			
        l2Intensity, 
        l2Intensity, 
        vec3(0.075,0.075,0.075)
    );
    
    DirectionLight l3 = DirectionLight(
        vec3(0.000,0.000,0.835), 
        vec3(1.),
        vec3(1.),
        vec3(0.1)
    );

    vec3 i1 = illuminateSingleLight(hit, l1, eye);
    vec3 i2 = illuminateSingleLight(hit, l2, eye);
    vec3 i3 = illuminateSingleLight(hit, l3, eye, 100.);
    return i1 + i2 + i3;
}


/********* RENDER SCENE *********/

Ray viewRayFromAxes(vec2 uv, vec3 eye, vec3 f, vec3 u, vec3 r, float fovDegrees, float roll) {
    // Image plane
    float zoom = 1. / tan(radians(fovDegrees) / 2.);
    vec3 c = eye + f*zoom;
    uv *= rotate2d(roll);
    vec3 i = c + (uv.x*r) + (uv.y*u);
    // Ray from camera origin to intersection with image plane
    return Ray(eye, normalize(i - eye));
}

vec3 renderScene(Ray viewRay) {
    Hit sceneHit;
    // rayMarch returns false if no scene intersection
    if(!rayMarch(viewRay, sceneHit)) return vec3(0.012,0.025,0.006);
    else return illuminateScene(sceneHit, viewRay.o);
}



void main() {
    vec2 uv = gl_FragCoord.xy/iResolution.xy;
    // Move origin to center of viewing plane
    uv -= 0.5;
    // Normalize aspect ratio
    uv.x *= iResolution.x/iResolution.y;
    // Remap to [-1, -1] x [1, 1]
    uv *= 2.;
    

    // CAMERA SETUP
	Ray viewRay = viewRayFromAxes(
		uv, 
        eye, 
        forward,
        up,
        right,
        60.,
        0.
    );

    // CALCULATE PIXEL COLOR
    vec3 color = vec3(0.);
    color = renderScene(viewRay);
    
	//color *= (1. / (1. + color));
    
    // Gamma correction
    color = pow(color, vec3(0.6));

    FragColor = vec4(color,1.0);
}
