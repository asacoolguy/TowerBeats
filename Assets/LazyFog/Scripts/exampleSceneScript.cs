using UnityEngine;
using System.Collections;

public class exampleSceneScript : MonoBehaviour {

	// Use this for initialization
	
	public float scale = 0.6f;
	public float intensity = 0.8f;
	public float alpha = 0.45f;
	public float alphasub = 0.05f;
	public float pow = 1.2f;
	public Color color = new Color(1f, 0.95f, 0.9f, 1.0f);
	public Material fogMaterial;
	
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		fogMaterial.SetFloat("_Scale", scale);
		fogMaterial.SetFloat("_Intensity", intensity);
		fogMaterial.SetFloat("_Alpha", alpha);
		fogMaterial.SetFloat("_AlphaSub", alphasub);
		fogMaterial.SetFloat("_Pow", pow);
		fogMaterial.SetColor("_Color", color);
		
	}
	
}
