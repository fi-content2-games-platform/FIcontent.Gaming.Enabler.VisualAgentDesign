using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathRenderer : MonoBehaviour
{
		public GameObject lineRendererPrefab;
		public float minDistance = .01f;
		public float cutDistance = .2f;
		public float height = .01f;
		public Transform robot;
		LineRenderer lineRenderer;
		LineRenderer selectedLineRenderer;
		List<Vector3> positions = new List<Vector3> ();
		Vector3 lastPosition;
		private List<LineRenderer> lineRenderers;
		int idx;

		void AddLineRenderer ()
		{
				var go = Instantiate (lineRendererPrefab) as GameObject;
				go.transform.parent = this.transform;
				this.lineRenderer = go.GetComponent<LineRenderer> ();
		}

		void Start ()
		{
				selectedLineRenderer = GetComponent<LineRenderer> ();
				AddLineRenderer ();
				idx = -1;
				lastPosition = this.robot.position;
				
		}

		public void AddPosition (Vector3 newPos)
		{
				float d = Vector3.Distance (newPos, lastPosition);

				if (d > minDistance) {
						newPos.y = height;
						positions.Add (newPos);
						idx++;
				}

				lastPosition = newPos;

				
				if (positions.Count > 0) {

						if (positions.Count == 1) {
								lineRenderer.SetVertexCount (positions.Count);
								
						} else {
								d = Vector3.Distance (positions [idx - 1], positions [idx]);
								if (d < cutDistance) {

										lineRenderer.SetVertexCount (positions.Count);
								} else {

										AddLineRenderer ();
										idx = 0;
										positions.RemoveRange (0, positions.Count - 1);
										lineRenderer.SetVertexCount (1);
								}
						}

						lineRenderer.SetPosition (idx, positions [idx]);
				}

		}

		public void SetSelected (List<Vector3> selectedList)
		{

				if (selectedList.Count < 2)
						return;

				Vector3 tmpPos;
				selectedLineRenderer.SetVertexCount (selectedList.Count);
				for (int i = 0; i < selectedList.Count; i++) {
						tmpPos = selectedList [i];
						tmpPos.y = height;
						selectedLineRenderer.SetPosition (i, tmpPos);
				}
		}

		public void Clear ()
		{
				foreach (Transform t in this.transform)
						Destroy (t.gameObject);

				selectedLineRenderer.SetVertexCount (0);
				idx = -1;
				positions.Clear ();
				AddLineRenderer ();

		}
}
