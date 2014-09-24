using UnityEngine;
using System.Collections;
using System;

public class EventActionsSetPanel : MonoBehaviour
{
	// time of creation
	public float creationTime;
	// time to live of this object
	//protected const float timeToLive = 120;
	
	// the aseba listener, to notify selection
	public AsebaListener asebaListener;

	// current time to live
	//[SerializeField] protected float timeToLiveLeft;

	// Use this for initialization
	void Start()
	{
		creationTime = Time.time;
		//timeToLiveLeft = timeToLive;
		// note: Destroy(this, 10);
	}
	
	// Update is called once per frame
	void Update()
	{
		// orient towards main camera
		//transform.LookAt(Camera.main.transform);
		//transform.rotation *= Quaternion.Euler(0, 180, 0);
		
		// set position
		if (transform.position.y > 0.005f)
			transform.position = transform.position - new Vector3(0, 0.001f, 0);
		
		// set local scale
		int tw = renderer.material.mainTexture.width;
		int th = renderer.material.mainTexture.height;
		const float scale = 0.025f / 256;
		float dist = Vector3.Distance(Camera.main.transform.position, transform.position);
		float factor = (float)(1.0f - Math.Atan(dist)/(Math.PI/2));
		transform.localScale = new Vector3(tw * scale * factor, th * scale * factor, 0);
		//renderer.material.color.a = factor;

		// check if it has to die
		/*timeToLiveLeft -= Time.deltaTime;
		if (timeToLiveLeft < 0)
			DestroyObject(gameObject);*/
	}

	void OnMouseDown()
	{
		//Debug.Log ("panel hit");
		asebaListener.SetSelectedPanel(this);
	}
}
