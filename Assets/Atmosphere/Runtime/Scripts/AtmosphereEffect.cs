using UnityEngine;
using UnityEngine.Rendering;
using System;


[ExecuteAlways]
public class AtmosphereEffect : MonoBehaviour 
{
	public AtmosphereProfile profile;

	public Transform sun;
	public bool directional = true;

	[Min(1f)] public float planetRadius = 1000.0f;
	[Min(1f)] public float oceanRadius = 950.0f;
	[Min(0.025f)] public float atmosphereScale = 0.25f;

	public float AtmosphereSize => (1 + atmosphereScale) * planetRadius;


	private Material material;
	private ComputeShader computeInstance;
	private RenderTexture opticalDepthTexture;


	// Values to check if optical depth texture is up to date or not. This method is a little messy but does the job.
	private int _width, _points;
	private float _size, _scale, _rayFalloff, _mieFalloff, _hAbsorbtion;


	private void OnEnable() 
	{
		AtmosphereRenderPass.RegisterEffect(this);
	}


	// NOTE : Since Atmosphere shader has no defined properties, values must be updated every frame as they aren't stored in the property sheet.
	private void LateUpdate() 
	{
		if (material == null || sun == null || profile == null) 
		{
			return;
		}

		profile.SetProperties(material);
		ValidateOpticalDepth();

		material.SetTexture("_BakedOpticalDepth", opticalDepthTexture);
		material.SetVector("_PlanetCenter", transform.position);

    	if (directional)
      	{	
			// For directional sun
			material.SetVector("_SunParams", -sun.forward);
			material.EnableKeyword("DIRECTIONAL_SUN");
  		} 
    	else
      	{
			// For positional sun
			material.SetVector("_SunParams", sun.position);
			material.DisableKeyword("DIRECTIONAL_SUN");
  		}

		material.SetFloat("_AtmosphereRadius", AtmosphereSize);
		material.SetFloat("_PlanetRadius", planetRadius);
		material.SetFloat("_OceanRadius", oceanRadius);
	}


	private void OnDisable() 
	{
		AtmosphereRenderPass.RemoveEffect(this);

		// Probably not needed. Do it just in case anyway
		if (computeInstance != null) 
		{
			DestroyImmediate(computeInstance);
		}

		if (opticalDepthTexture != null)
		{
			opticalDepthTexture.Release();
			DestroyImmediate(opticalDepthTexture);
		}
	}


	private void ValidateOpticalDepth()
	{
		if (profile == null)
		{
			return;
		}

		bool upToDate = profile.IsUpToDate(ref _width, ref _points, ref _rayFalloff, ref _mieFalloff, ref _hAbsorbtion);
		bool sizeChange = _size != planetRadius || _scale != atmosphereScale;
		bool textureExists = opticalDepthTexture != null && opticalDepthTexture.IsCreated();

		if (!upToDate || sizeChange || !textureExists) 
		{
			if (computeInstance == null) 
			{
				// Create an instance per effect so multiple effects can bake their optical depth simultaneously
				computeInstance = Instantiate(profile.OpticalDepthCompute);
			}

			profile.BakeOpticalDepth(ref opticalDepthTexture, computeInstance, planetRadius, AtmosphereSize);
			
			_size = planetRadius;
			_scale = atmosphereScale;
		}
	}

	
	/// <summary>
	/// Gets or creates a new material with the provided shader
	/// </summary>
	public Material GetMaterial(Shader atmosphereShader) 
	{
		if (material == null)
		{
			material = new Material(atmosphereShader);
		}

		return material;
	}


	/// <summary>
	/// Is the effect visible to the provided camer frustum planes?
	/// </summary>
	public bool IsVisible(Plane[] cameraPlanes) 
	{
		if (profile == null || sun == null) 
		{
			return false;
		}

		Vector3 pos = transform.position;
		float radius = AtmosphereSize;

		// Cull spherical bounds, ignoring camera far plane at index 5
		for (int i = 0; i < cameraPlanes.Length - 1; i++) 
		{
			float distance = cameraPlanes[i].GetDistanceToPoint(pos);

			if (distance < 0 && Mathf.Abs(distance) > radius) 
			{
				return false;
			}
		}

		return true;
	}


	/// <summary>
	/// Returns absolute distance from point to atmosphere shell
	/// </summary>
	public float DistToAtmosphere(Vector3 pos) 
	{
		return Math.Abs((pos - transform.position).magnitude - AtmosphereSize);
	}


	private void OnDrawGizmosSelected() 
	{
		if (sun != null) 
		{
			Gizmos.color = Color.green;
			Vector3 sunDir = directional ? -sun.forward : (sun.position - transform.position).normalized;
			Gizmos.DrawRay(transform.position, sunDir * planetRadius);
		}
	}
}
