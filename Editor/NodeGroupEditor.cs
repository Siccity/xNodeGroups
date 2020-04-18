using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XNode;
using XNode.NodeGroups;

namespace XNodeEditor.NodeGroups {
	[CustomNodeEditor(typeof(NodeGroup))]
	public class NodeGroupEditor : NodeEditor {
		private NodeGroup group { get { return _group != null ? _group : _group = target as NodeGroup; } }
		private NodeGroup _group;
		public static Texture2D corner { get { return _corner != null ? _corner : _corner = Resources.Load<Texture2D>("xnode_corner"); } }
		private static Texture2D _corner;
		private bool isDragging;
		private Vector2 size;

		public override void OnBodyGUI() {
			Event e = Event.current;
			switch (e.type) {
				case EventType.MouseDrag:
					if (isDragging) {
						group.width = Mathf.Max(200, (int) e.mousePosition.x + 16);
						group.height = Mathf.Max(100, (int) e.mousePosition.y - 34);
						NodeEditorWindow.current.Repaint();
					}
					break;
				case EventType.MouseDown:
					// Ignore everything except left clicks
					if (e.button != 0) return;
					if (NodeEditorWindow.current.nodeSizes.TryGetValue(target, out size)) {
						// Mouse position checking is in node local space
						Rect lowerRight = new Rect(size.x - 34, size.y - 34, 30, 30);
						if (lowerRight.Contains(e.mousePosition)) {
							isDragging = true;
						}
					}
					break;
				case EventType.MouseUp:
					isDragging = false;
					// Select nodes inside the group
					if (Selection.Contains(target)) {
						List<Object> selection = Selection.objects.ToList();
						selection.AddRange(group.GetNodes());
						Selection.objects = selection.Distinct().ToArray();
					}
					break;
				case EventType.Repaint:
					// Move to bottom
					if (target.graph.nodes.IndexOf(target) != 0) {
						target.graph.nodes.Remove(target);
						target.graph.nodes.Insert(0, target);
					}
					// Add scale cursors
					if (NodeEditorWindow.current.nodeSizes.TryGetValue(target, out size)) {
						Rect lowerRight = new Rect(target.position, new Vector2(30, 30));
						lowerRight.y += size.y - 34;
						lowerRight.x += size.x - 34;
						lowerRight = NodeEditorWindow.current.GridToWindowRect(lowerRight);
						NodeEditorWindow.current.onLateGUI += () => AddMouseRect(lowerRight);
					}
					break;
			}

			// Control height of node
			GUILayout.Space(group.height);

			GUI.DrawTexture(new Rect(group.width - 34, group.height + 16, 24, 24), corner);
		}

		public override int GetWidth() {
			return group.width;
		}

		public override Color GetTint() {
			return group.color;
		}

		public static void AddMouseRect(Rect rect) {
			EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeUpLeft);
		}
	}
}