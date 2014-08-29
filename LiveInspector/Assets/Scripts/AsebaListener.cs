using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Aseba;

public class AsebaListener : MonoBehaviour {

	public Aseba.Stream stream = null;

	public EventActionsSetPanel panelPrefab;
	
	Dictionary<ushort, Texture2D> eventActionsSetTextureCache = new Dictionary<ushort, Texture2D>();
	
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
			return;
		}

		// we are only interested in user event 0 for now
		if (type != 0)
			return;
			
		// deserialize data
		ushort[] wordPayload = new ushort[len/2];
		for (int i=0; i<len; i += 2)
			wordPayload[i/2] = BitConverter.ToUInt16(payload, i);
		
		// create texture if needed
		Texture2D tex;
		ushort setId = wordPayload[0];
		if (!eventActionsSetTextureCache.ContainsKey(setId))
			tex = eventActionsSetTextureCache[setId] = VPL.CreateTexture(wordPayload);
		else
			tex = eventActionsSetTextureCache[setId];
		
		// instanciate panel	
		EventActionsSetPanel panel = (EventActionsSetPanel) Instantiate(panelPrefab, transform.position, transform.rotation);
		panel.renderer.material.mainTexture = tex;
		const float scale = 0.05f / 256;
		panel.transform.localScale = new Vector3(tex.width * scale, tex.height * scale, 0);
		// log data
		//Debug.Log(String.Format("Received VPL event from {0} containing {1} words : {2}", source, wordPayload.Length, String.Join(", ", Array.ConvertAll<ushort, string>(wordPayload, Convert.ToString))));
	}
}
