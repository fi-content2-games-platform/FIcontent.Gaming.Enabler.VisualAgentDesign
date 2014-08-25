using UnityEngine;
using System.Collections;
using System;

public class AsebaListener : MonoBehaviour {

	public Aseba.Stream stream = null;

	// Use this for initialization
	void Start ()
	{
		stream = new Aseba.Stream("localhost");
		stream.messageCallback = ReceiveEvent; 
	}
	
	// Update is called once per frame
	void Update ()
	{
		stream.Step ();
	}

	void OnDestroy()
	{
		stream.Disconnect();
	}

	// Unity delegate for messages
	public void ReceiveEvent(ushort len, ushort source, ushort type, byte[] payload)
	{
		// we are only interested in user event 0
		if (type != 0)
			return;


		ushort[] wordPayload = new ushort[len/2];
		for (int i=0; i<len; i += 2)
			wordPayload[i/2] = BitConverter.ToUInt16(payload, i);
		Debug.Log(String.Format("Received VPL event from {0} containing {1} words : {2}", source, wordPayload.Length, String.Join(", ", Array.ConvertAll<ushort, string>(wordPayload, Convert.ToString))));
	}
}
