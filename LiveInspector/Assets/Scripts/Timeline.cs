using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Timeline
{
	const float minTimeForContinuous = 0.3f;
	
	public struct Stride
	{
		public float startTime, endTime;
		public GameObject gameObject;
		
		public Stride(float startTime, float endTime, GameObject gameObject)
		{
			this.startTime = startTime;
			this.endTime = endTime;
			this.gameObject = gameObject;
		}
	};
	
	// These are public for easy reading, better would be to properly encapsulate
	public Dictionary<ushort, List<Stride>> pastEvents = new Dictionary<ushort, List<Stride>>();
	public Dictionary<ushort, Stride> ongoingEvents = new Dictionary<ushort, Stride>();
	public Dictionary<ushort, ushort> setIdToEventTypeMap = new Dictionary<ushort, ushort>();
	
	// Return whether we should create a new panel
	public bool IsOngoingExtend(ushort setId)
	{
		Stride stride;
		if (ongoingEvents.TryGetValue(setId, out stride))
		{
			ongoingEvents[setId] = new Stride(stride.startTime, Time.time, stride.gameObject);
			return true;
		}
		else
			return false;
	}
	
	// Push an event, return whether it created a new stride
	public void PushEvent(ushort setId, ushort eventType, GameObject gameObject)
	{
		// add event type
		setIdToEventTypeMap[setId] = eventType;
		
		// we assume the event is not ongoing already, so we add it
		ongoingEvents[setId] = new Stride(Time.time, Time.time, gameObject);
	}
	
	// See whether we have uncommited outdated strides
	public void CommitPastEvents()
	{
		List<ushort> toRemove = new List<ushort>();
		foreach (KeyValuePair<ushort, Stride> entry in ongoingEvents)
		{
			if (entry.Value.endTime + minTimeForContinuous < Time.time)
			{
				if (pastEvents.ContainsKey(entry.Key))
					pastEvents[entry.Key].Add(entry.Value);
				else
					pastEvents[entry.Key] = new List<Stride> { entry.Value };
				toRemove.Add(entry.Key);
			}
		}
		foreach (ushort key in toRemove)
		{
			ongoingEvents.Remove(key);
			//Debug.Log(String.Format("Removing set {0}", key));
		}
	}
	
	public void Clear()
	{
		pastEvents.Clear();
		ongoingEvents.Clear();
		setIdToEventTypeMap.Clear();
	}
};