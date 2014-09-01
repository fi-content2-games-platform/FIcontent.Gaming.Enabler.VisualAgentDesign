using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Aseba;

public class Timeline
{
	const float minTimeForContinuous = 0.2f;
	
	public struct Stride
	{
		public float startTime, endTime;
		
		public Stride(float startTime, float endTime)
		{
			this.startTime = startTime;
			this.endTime = endTime;
		}
	};
	
	Dictionary<ushort, List<Stride>> pastEvents = new Dictionary<ushort, List<Stride>>();
	
	Dictionary<ushort, Stride> ongoingEvents = new Dictionary<ushort, Stride>();
	
	// Push an event, return whether it created a new stride
	public bool PushEvent(ushort setId)
	{
		// TODO: use timer to commit old events even though we did not receive new ones
		// see whether we have uncommited outdated strides
		List<ushort> toRemove = new List<ushort>();
		foreach (KeyValuePair<ushort, Stride> entry in ongoingEvents)
		{
			if (entry.Value.endTime + minTimeForContinuous < Time.time)
			{
				pastEvents[entry.Key] = new List<Stride> { entry.Value };
				toRemove.Add(entry.Key);
			}
		}
		foreach (ushort key in toRemove)
		{
			ongoingEvents.Remove(key);
			Debug.Log(String.Format("Removing set {0}", key));
		}
		
		// check if already ongoing, and update time or add
		bool newStride;
		Stride stride;
		if (ongoingEvents.TryGetValue(setId, out stride))
		{
			ongoingEvents[setId] = new Stride(stride.startTime, Time.time);
			newStride = false;
		}
		else
		{
			ongoingEvents[setId] = new Stride(Time.time, Time.time);
			newStride = true;
		}
		
		// TODO: call delegates if any
		
		return newStride;
	}
	
	public void Clear()
	{
		pastEvents.Clear();
		ongoingEvents.Clear();
	}
};

public class AsebaListener : MonoBehaviour
{

	public Aseba.Stream stream = null;

	public EventActionsSetPanel panelPrefab;
	
	Dictionary<ushort, Texture2D> eventActionsSetTextureCache = new Dictionary<ushort, Texture2D>();
	
	Timeline timeline = new Timeline();
	
	// Connect to the Aseba network
	void Start ()
	{
		stream = new Aseba.Stream("localhost");
		stream.messageCallback = ReceiveEvent; 
	}
	
	// Look for messages from the Aseba network
	void Update ()
	{
		if (stream != null)
			stream.Step ();
		// TODO: catch exception and show GUI to enter address if connection fails?
	}

	// Disconnect from Aseba
	void OnDestroy()
	{
		if (stream != null)
		{
			stream.Disconnect();
			stream = null;
		}
	}

	// Unity delegate for messages
	public void ReceiveEvent(ushort len, ushort source, ushort type, byte[] payload)
	{
		// check if we have to clear code
		if (type == 0xA001)
		{
			//Debug.Log("New code, clearing cache");
			eventActionsSetTextureCache.Clear();
			timeline.Clear();
			return;
		}

		// we are only interested in user event 0 for now
		if (type != 0)
			return;
			
		// deserialize data
		ushort[] wordPayload = new ushort[len/2];
		for (int i=0; i<len; i += 2)
			wordPayload[i/2] = BitConverter.ToUInt16(payload, i);
		
		// get set ID, see whether we have to show it
		ushort setId = wordPayload[0];
		if (timeline.PushEvent(setId))
		{
			// create texture if needed
			Texture2D tex;
			if (!eventActionsSetTextureCache.ContainsKey(setId))
				tex = eventActionsSetTextureCache[setId] = VPL.CreateTexture(wordPayload);
			else
				tex = eventActionsSetTextureCache[setId];
			
			// instanciate panel	
			EventActionsSetPanel panel = (EventActionsSetPanel) Instantiate(panelPrefab, transform.position, transform.rotation);
			panel.renderer.material.mainTexture = tex;
			const float scale = 0.05f / 256;
			panel.transform.localScale = new Vector3(tex.width * scale, tex.height * scale, 0);
		}
		
		// log data
		//Debug.Log(String.Format("Received VPL event from {0} containing {1} words : {2}", source, wordPayload.Length, String.Join(", ", Array.ConvertAll<ushort, string>(wordPayload, Convert.ToString))));
	}
}
