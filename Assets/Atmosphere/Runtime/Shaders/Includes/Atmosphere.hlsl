#pragma once

// ShaderToy by Dimas Leenman under the MIT license: https://www.shadertoy.com/view/wlBXWK
// Modified and ported to HLSL by Kai Angulo
// Adjusted to use baked optical depth based on examples from Sebastian Lague's atmosphere rendering series : https://www.youtube.com/watch?v=DxfEbulyFcY


#define MAX_LOOP_ITERATIONS 30
#pragma shader_feature DIRECTIONAL_SUN 


TEXTURE2D(_BakedOpticalDepth);
float4 _BakedOpticalDepth_TexelSize;
SAMPLER(sampler_BakedOpticalDepth);


float3 _SunParams;
float3 _PlanetCenter;

// Celestial body radii
float _PlanetRadius;
float _AtmosphereRadius;
float _CutoffRadius;

// Scttering steps
int _NumInScatteringPoints;
int _NumOpticalDepthPoints;

// Rayleigh scattering coefficients
float3 _RayleighScattering;

// Mie scattering coeficcients
float3 _MieScattering;
float _MieG;

// Ozone absorbtion coefficients
float3 _AbsorbtionBeta; 

// Ambient atmosphere color
float3 _AmbientBeta;

// Density falloffs
float _RayleighFalloff;
float _MieFalloff;
float _HeightAbsorbtion;

// Light intensity
float _Intensity;



float3 DensityAtPoint(float3 position)
{
    float height = length(position) - _PlanetRadius;
    float height01 = height / (_AtmosphereRadius - _PlanetRadius);

    float2 scaleHeight = float2(_RayleighFalloff, _MieFalloff);

    // Rayleigh and Mie density falloffs are both calculated with the same equation
    float rayleighDensity = exp(-height01 * _RayleighFalloff) * (1 - height01);
    float mieDensity = exp(-height01 * _MieFalloff) * (1 - height01);

    // Absorption density. This is for ozone, which scales together with the rayleigh.
    float denom = (_HeightAbsorbtion + height01);
    float ozoneDensity = (1.0 / (denom * denom + 1.0)) * rayleighDensity;

    return float3(rayleighDensity, mieDensity, ozoneDensity);
}


// While slightly more cumbersome, baking optical depth beforehand with a compute shader + render texture provides a significant performance boost
// Take for example a non-baked fragment sample with 20 steps in the view direction and 10 steps in the sun direction -
// that's 10*20: 200 marches per pixel, not good due to the frequent use of transcendent functions like exp() and sqrt()

// When baked, this is reduced to only 1 call to OpticalDepthBaked, reducing the iterations to only 20, while keeping near identical visual quality.

float3 OpticalDepthBaked(float3 rayOrigin, float3 rayDir) 
{
	float rayLen = length(rayOrigin);
	float height = rayLen - _PlanetRadius;

	float height01 = saturate(height / (_AtmosphereRadius - _PlanetRadius));

    float3 normal = rayOrigin / rayLen;

	float uvX = 1 - (dot(normal, rayDir) * 0.5 + 0.5);

	return SAMPLE_TEXTURE2D(_BakedOpticalDepth, sampler_BakedOpticalDepth, float2(uvX, height01)).xyz;
}



float3 CalculateScattering(float3 start, float3 dir, float sceneDepth, float3 sceneColor) 
{
    // add an offset to the camera position, so that the atmosphere is in the correct position
    start -= _PlanetCenter;

    float2 cutoffHit = RaySphere(0, _CutoffRadius, start, dir);
    sceneDepth = min(sceneDepth, cutoffHit.x);

    float2 rayLength = RaySphere(0, _AtmosphereRadius, start, dir);
    rayLength.y = min(rayLength.x + rayLength.y, sceneDepth);

    // Did the ray miss the atmosphere?   
    if (rayLength.x > rayLength.y) 
    {
        return sceneColor;
    }

    // Get camera-relative sun direction
    #if !defined(DIRECTIONAL_SUN)
        float3 sunPos = _SunParams.xyz - _PlanetCenter;
        float3 dirToSun = -normalize(start - sunPos.xyz);
    #else
        float3 dirToSun = _SunParams.xyz;
    #endif


    // Clamp maximum ray length to proper values
    rayLength.y = min(rayLength.y, sceneDepth);
    rayLength.x = max(rayLength.x, 0.0);

    // Frequently used values
    float mu = dot(dir, dirToSun);
    float mumu = mu * mu;
    float gg = _MieG * _MieG;

    // Magic number is (pi * 16)
    float phaseRay = 3.0 / (50.2654824574) * (1.0 + mumu);

    // Magic number is (pi * 8)
    float phaseMie = 3.0 / (25.1327412287) * ((1.0 - gg) * (mumu + 1.0)) / (pow(abs(1.0 + gg - 2.0 * mu * _MieG), 1.5) * (2.0 + gg));

    // Does object block mie glow?
    phaseMie = sceneDepth > rayLength.y ? phaseMie : 0.0;


    float inScatterStepSize = (rayLength.y - rayLength.x) / float(_NumInScatteringPoints);

    float3 inScatterPoint = start + dir * (rayLength.x + inScatterStepSize * 0.5);

    // Scattered light accumulators
    float3 totalRay = 0;
    float3 totalMie = 0;
    float3 opticalDepth = 0;

    // Primary in-scatter loop
    [unroll(MAX_LOOP_ITERATIONS)]
    for (int i = 0; i < _NumInScatteringPoints; i++) 
    {
        // Particle density at sample position
        float3 density = DensityAtPoint(inScatterPoint) * inScatterStepSize;
        
        // Accumulate optical depth
        opticalDepth += density;

        // Get sample point relative sun direction
    #if !defined(DIRECTIONAL_SUN)
        dirToSun = -normalize(inScatterPoint - sunPos.xyz);
    #endif

        // Light ray optical depth - Original optical depth function can be found in OutScattering.compute
        //float3 lightOpticalDepth = OpticalDepth(inScatterPoint, dirToSun);
        float3 lightOpticalDepth = OpticalDepthBaked(inScatterPoint, dirToSun);

        // Attenuation calculation
        float3 attenuation = exp(
            -_RayleighScattering * (opticalDepth.x + lightOpticalDepth.x) 
            - _MieScattering * (opticalDepth.y + lightOpticalDepth.y) 
            - _AbsorbtionBeta * (opticalDepth.z + lightOpticalDepth.z)
        );

        // Accumulate scttered light
        totalRay += density.x * attenuation;
        totalMie += density.y * attenuation;

        inScatterPoint += dir * inScatterStepSize;
    }


    // Calculate how much light can pass through the atmosphere
    float3 opacity = exp(
        -(_MieScattering * opticalDepth.y
        + _RayleighScattering * opticalDepth.x 
        + _AbsorbtionBeta * opticalDepth.z)
    );


    // Calculate final scattering factors
    float3 rayleigh = phaseRay * _RayleighScattering * totalRay;
    float3 mie = phaseMie * _MieScattering * totalMie;

    float3 ambient = opticalDepth.x * _AmbientBeta * 0.00001 /* Fudge factor */;

	// Apply final color
    return (rayleigh + mie + ambient) * _Intensity + sceneColor * opacity;
}

