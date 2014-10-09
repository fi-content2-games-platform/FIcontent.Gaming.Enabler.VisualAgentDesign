using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Aseba;

public class AsebaListener : MonoBehaviour
{
	const float timelineHeight = 200;
	const float timelineCurrentViewHeight = 150;
	const float timelineGlobalViewHeight = 40;
	
	public EventActionsSetPanel panelPrefab;
	public GUISkin guiSkin;
	public Texture2D tex_Timeline_bg;
	public Texture2D tex_Timeline_event_visible;
	public Texture2D tex_Timeline_event_outOfView;
	public Texture2D tex_Timeline_event_selected_visible;
	public Texture2D tex_Timeline_event_selected_outOfView;
	public Texture2D tex_pause;
	public Texture2D tex_rec;
	public Texture2D tex_board_untracked;
	public Texture2D tex_board_tracked;
	public Texture2D tex_thymio_untracked;
	public Texture2D tex_thymio_tracked;
	
	protected EventActionsSetPanel selectedPanel = null;
	
	protected string targetName = "localhost";
	protected Aseba.Stream stream = null;
	
	Dictionary<ushort, Texture2D> eventActionsSetTextureCache = new Dictionary<ushort, Texture2D>();
	
	bool boardIsTracked = false;
	bool thymioIsTracked = false;
	
	Timeline timeline = new Timeline();
	float timelineViewStart = 0;
	float timelineViewStop = 0;
	bool recording = false;
	float recordingStartTime;
	float recordingDuration;
	
	// Connect to the Aseba network
	void Start ()
	{
		if (PlayerPrefs.HasKey("targetName"))
			targetName = PlayerPrefs.GetString("targetName");
		stream = new Aseba.Stream();
		stream.messageCallback = ReceiveEvent;
		stream.disconnectionCallback = NetworkDisconnected;
		InvokeRepeating("SendStats", 1, 1);
		TryToConnect();
	}
	
	// Look for messages from the Aseba network and update the timeline view to now
	void Update ()
	{
		if (recording)
		{
			timeline.CommitPastEvents();
			recordingDuration += Time.deltaTime;
		}
		
		stream.Step();
		
		if (stream.Connected)
		{
			ProcessTimelineInputs();
			UpdateTimelineView();
		}
	}

	// Disconnect from Aseba
	void OnDestroy()
	{
		stream.Disconnect();
	}
	
	// Display 2D overlay elements
	void OnGUI()
	{
		GUI.skin = guiSkin;
		
		ShowTrackingStatus();
		
		// show timeline
		if (stream.Connected)
		{
			ShowTimelineView();
			if (recording)
				ShowRecordingStatus();
			else
				ShowRecordButton();
		}
		else
			ShowConnectionDialog();
	}
	
	// Tracking found
	public void OnTrackingFound(int number)
	{
		switch (number)
		{
			case 0: boardIsTracked = true; break;
			case 1: thymioIsTracked = true; break;
			default: break;
		}
	}
	
	// Tracking lost
	public void OnTrackingLost(int number)
	{
		switch (number)
		{
			case 0: boardIsTracked = false; break;
			case 1: thymioIsTracked = false; break;
			default: break;
		}
	}
	
	// Try to connect, catch the exception if it fails
	protected void TryToConnect()
	{
		stream.Connect(targetName);
		if (stream.Connected)
		{
			PlayerPrefs.SetString("targetName", targetName);
			StartRecording();
		}
	}
	
	// Start the recording and reset the timeline
	protected void StartRecording()
	{
		recording = true;
		recordingStartTime = Time.time;
		recordingDuration = 0;
		
		timelineViewStart = Time.time;
		timelineViewStop = Time.time + 10;
		
		Clear();
	}
	
	// Stop the recording
	protected void StopRecording()
	{
		recording = false;
	}
	
	// Update the timeline view
	protected void UpdateTimelineView()
	{
		if (recording && Time.time > timelineViewStop)
		{
			float deltaTimeView = timelineViewStop - timelineViewStart;
			timelineViewStop = Time.time;
			timelineViewStart = timelineViewStop - deltaTimeView;
			TimelineViewUpdated();
		}
	}
	
	// Handling pinching on timeline
	protected void ProcessTimelineInputs()
	{
		float deltaTimeView = timelineViewStop - timelineViewStart;
		
		// simple scroll
		if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
		{
			float delta = -Input.GetTouch(0).deltaPosition.x * deltaTimeView / Screen.width;
			if (timelineViewStart + delta < recordingStartTime)
				delta = recordingStartTime - timelineViewStart;
			if (timelineViewStop + delta > recordingStartTime + recordingDuration)
				delta = recordingStartTime + recordingDuration - timelineViewStop;
			
			timelineViewStart += delta;
			timelineViewStop += delta;
			TimelineViewUpdated();
			
			return;
		}
		
		// now we need two touches...
		if (Input.touchCount != 2)
			return;
		// ...with one moving
		if (Input.GetTouch(0).phase != TouchPhase.Moved && Input.GetTouch(1).phase != TouchPhase.Moved)
			return;
		if (Input.GetTouch(0).position.y > timelineHeight && Input.GetTouch(1).position.y > timelineHeight)
			return;
		
		// get factors, assumes touch 1 is right of touch 0
		float moveFactor = (Input.GetTouch(1).deltaPosition.x + Input.GetTouch(0).deltaPosition.x) / 2;
		float posRatio = (Input.GetTouch(1).position.x + Input.GetTouch(0).position.x) / (2.0f * Screen.width);
		float zoomFactor = Input.GetTouch(1).deltaPosition.x - Input.GetTouch(0).deltaPosition.x;
		zoomFactor /= (Input.GetTouch(1).position.x - Input.GetTouch(0).position.x);
		// safety
		if (zoomFactor > 5.0f)
			zoomFactor = 5.0f;
		if (zoomFactor < -5.0f)
			zoomFactor = -5.0f;
		if (moveFactor > Screen.width)
			moveFactor = Screen.width;
		if (moveFactor < -Screen.width)
			moveFactor = -Screen.width;
			
		// adjust time
		timelineViewStart -= moveFactor * deltaTimeView / Screen.width;
		timelineViewStop -= moveFactor * deltaTimeView / Screen.width;
		timelineViewStart += deltaTimeView * 0.5f * zoomFactor * posRatio;
		timelineViewStop -= deltaTimeView * 0.5f * zoomFactor * (1.0f-posRatio);
		
		// clamp time
		timelineViewStart = Math.Max(timelineViewStart, recordingStartTime);
		timelineViewStop = Math.Min(timelineViewStop, recordingStartTime + recordingDuration);
		
		TimelineViewUpdated();
	}
	
	// Update panels visibility
	protected void TimelineViewUpdated()
	{
		var panels = GameObject.FindGameObjectsWithTag("EventActionsSetPanel");
		foreach (GameObject go in panels)
		{
			EventActionsSetPanel panel = go.GetComponent<EventActionsSetPanel>();
			float alpha = (panel.creationTime > timelineViewStart && panel.creationTime < timelineViewStop) ? 1.0f : 0.3f;
			bool isSelected = (panel == selectedPanel);
			
			if (isSelected)
				go.GetComponent<MeshRenderer>().materials[0].color = new Color(1.0f,1.0f,0.70588235294118f,alpha);
			else
				go.GetComponent<MeshRenderer>().materials[0].color = new Color(1.0f,1.0f,1.0f,alpha);
		}
	}
	
	// Show the tracking status
	protected void ShowTrackingStatus()
	{
		// board
		if (boardIsTracked)
			GUI.DrawTexture(new Rect(32, Screen.height/2-16-64, 64, 64), tex_board_tracked);
		else
			GUI.DrawTexture(new Rect(32, Screen.height/2-16-64, 64, 64), tex_board_untracked);
		// thymio
		if (thymioIsTracked)
			GUI.DrawTexture(new Rect(32, Screen.height/2+16, 64, 64), tex_thymio_tracked);
		else
			GUI.DrawTexture(new Rect(32, Screen.height/2+16, 64, 64), tex_thymio_untracked);
	}
	
	// Show the timeline
	protected void ShowTimelineView()
	{
		// current view
		GUI.DrawTexture(new Rect(0, Screen.height - timelineHeight, Screen.width, timelineCurrentViewHeight), tex_Timeline_bg);
		// past events
		foreach(KeyValuePair<ushort, List<Timeline.Stride>> strides in timeline.pastEvents)
		{
			foreach (Timeline.Stride stride in strides.Value)
			{
				DrawTimelineEvent(stride);
			}
		}
		// recent ones
		foreach(KeyValuePair<ushort, Timeline.Stride> entry in timeline.ongoingEvents)
			DrawTimelineEvent(entry.Value);
		// global view
		if (recordingDuration > 0)
		{
			GUI.DrawTexture(new Rect(0, Screen.height - timelineGlobalViewHeight, Screen.width, timelineGlobalViewHeight), tex_Timeline_bg);
			float x = (timelineViewStart - recordingStartTime) / recordingDuration;
			float w = (timelineViewStop - timelineViewStart) / recordingDuration;
			GUI.DrawTexture(new Rect(x * Screen.width, Screen.height - timelineGlobalViewHeight, w * Screen.width, timelineGlobalViewHeight), tex_Timeline_event_outOfView);
		}
	}
	
	// Draw an event on the timeline
	protected void DrawTimelineEvent(Timeline.Stride stride)
	{
		float pos;
		if (IsEventVisible(stride.startTime, out pos))
		{
			Texture2D tex;
			if (stride.gameObject.GetComponent<EventActionsSetPanel>() == selectedPanel)
				tex = stride.gameObject.renderer.isVisible ? tex_Timeline_event_selected_visible : tex_Timeline_event_selected_outOfView;
			else
				tex = stride.gameObject.renderer.isVisible ? tex_Timeline_event_visible : tex_Timeline_event_outOfView;
			GUI.DrawTexture(new Rect(pos, Screen.height - timelineHeight, Screen.width/100, timelineCurrentViewHeight), tex);
		}
	}
	
	// Return whether an event is visible in timeline
	protected bool IsEventVisible(float time, out float pos)
	{
		// check whether event is visible
		if (time < timelineViewStart || time > timelineViewStop)
		{
			pos = 0;
			return false;
		}
		
		// compute x-pos
		float deltaTimeView = timelineViewStop - timelineViewStart;
		pos = (time - timelineViewStart) * Screen.width / deltaTimeView;
		
		return true;
	}
	
	// Display connection dialog if needed
	protected void ShowConnectionDialog()
	{
		GUILayout.BeginArea (new Rect (100, Screen.height / 3 - 50, Screen.width - 200, 100));
		GUILayout.BeginHorizontal();
		targetName = GUILayout.TextField(targetName, GUILayout.ExpandWidth(true) );
		if (GUILayout.Button("Connect", GUILayout.ExpandWidth(false) ))
			TryToConnect();
		GUILayout.EndHorizontal();
		GUILayout.EndArea ();
	}
	
	// Show the current recording status and a stop button
	protected void ShowRecordingStatus()
	{
		GUI.DrawTexture(new Rect(64,64,64,64), tex_rec);
		GUI.Label(new Rect(128+8,64-8,256,64+16), String.Format("{0:N1}", recordingDuration));
		if (GUI.Button(new Rect(Screen.width-64-80,64-6,80,80), tex_pause))
			StopRecording();
	}
	
	// Show a button to start recording 
	protected void ShowRecordButton()
	{
		if (GUI.Button(new Rect(Screen.width-64-80,64-6,80,80), tex_rec))
			StartRecording();
	}
	
	// Clear sets textures and timeline
	protected void Clear()
	{
		eventActionsSetTextureCache.Clear();
		timeline.Clear();
		var panels = GameObject.FindGameObjectsWithTag("EventActionsSetPanel");
		foreach (GameObject panel in panels)
			Destroy(panel);
		selectedPanel = null;
	}
	
	// Set a panel to selected
	public void SetSelectedPanel(EventActionsSetPanel panel)
	{
		selectedPanel = panel;
		TimelineViewUpdated();
	}

	// Delegate for disconnections
	public void NetworkDisconnected()
	{
		Clear();
	}
	
	// Every second, send stats to the client
	public void SendStats()
	{
		if (stream.Connected)
		{
			ushort[] data = new ushort[14];
			for (ushort i=0; i<14; ++i)
				data[i] = i;
			stream.SendAsebaMessage(0, 1, data); 
		}
	}
	
	// Delegate for messages
	public void ReceiveEvent(ushort len, ushort source, ushort type, byte[] payload)
	{
		// check if we have to clear code
		if (type == 0xA001)
		{
			//Debug.Log("New code, clearing cache");
			Clear();
			return;
		}
		
		// if we do not record, ignore event
		if (!recording)
			return;

		// we are only interested in user event 0 for now
		if (type != 0)
			return;
			
		// deserialize data
		ushort[] wordPayload = new ushort[len/2];
		for (int i=0; i<len; i += 2)
			wordPayload[i/2] = BitConverter.ToUInt16(payload, i);
		
		// get set ID, see whether we have to show it
		ushort setId = wordPayload[0];
		
		// check if not already in the timeline
		if (timeline.IsOngoingExtend(setId))
			return;
			
		// check if we are tracking correctly
		if (!boardIsTracked || !thymioIsTracked)
			return;
		
		// create texture if needed
		Texture2D tex;
		if (!eventActionsSetTextureCache.ContainsKey(setId))
			tex = eventActionsSetTextureCache[setId] = VPL.CreateTexture(wordPayload);
		else
			tex = eventActionsSetTextureCache[setId];
		
		// instantiate panel
		GameObject generator = GameObject.Find("PanelGenerator");
		EventActionsSetPanel panel = (EventActionsSetPanel) Instantiate(
			panelPrefab, generator.transform.position, generator.transform.rotation
		);
		panel.renderer.material.mainTexture = tex;
		const float scale = 0.025f / 256;
		panel.transform.localScale = new Vector3(tex.width * scale, tex.height * scale, 0);
		panel.asebaListener = this;
		
		// push into timeline
		timeline.PushEvent(setId, (ushort)(wordPayload[2] & (ushort)0xf), panel.gameObject);
	}
}
