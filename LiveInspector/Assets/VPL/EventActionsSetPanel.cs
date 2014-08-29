using UnityEngine;
using System.Collections;

public class EventActionsSetPanel : MonoBehaviour
{
	// time to live of this object
	protected const float timeToLive = 1;

	// current time to live
	[SerializeField] protected float timeToLiveLeft;

	// Use this for initialization
	void Start ()
	{
		timeToLiveLeft = timeToLive;
		// note: Destroy(this, 10);
	}
	
	// Update is called once per frame
	void Update ()
	{
		// orient towards main camera
		transform.LookAt(Camera.main.transform);
		transform.rotation *= Quaternion.Euler(0, 180, 0);

		// check if it has to die
		timeToLiveLeft -= Time.deltaTime;
		if (timeToLiveLeft < 0)
			DestroyObject(gameObject);
	}
}
