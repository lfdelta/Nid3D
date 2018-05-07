#if (UNITY_EDITOR)

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// code modified from a Unity tutorial: https://docs.unity3d.com/ScriptReference/GL.html

public class NodeDebug : MonoBehaviour {
  public float height = 1;

  public bool bisectors = true;
  public bool bisBoth = false;
  public float bisRadius = 100;
  public float Rbis=1, Gbis=1, Bbis=1;

  public bool segments = true;
  public float segRadius = 100;
  public float Rseg=1, Gseg=1, Bseg=1;

  private WorldNodeScript node;

  void Start() {
    node = GetComponent<WorldNodeScript> ();
  }

  static Material lineMaterial;
  static void CreateLineMaterial()
  {
    if (!lineMaterial)
    {
      // Unity has a built-in shader that is useful for drawing simple colored things.
      Shader shader = Shader.Find("Hidden/Internal-Colored");
      lineMaterial = new Material(shader);
      lineMaterial.hideFlags = HideFlags.HideAndDontSave;
      // Turn on alpha blending
      lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
      lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
      // Turn backface culling off
      lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
      // Turn off depth writes
      lineMaterial.SetInt("_ZWrite", 0);
    }
  }

  // Will be called after all regular rendering is done
  public void OnRenderObject()
  {
    CreateLineMaterial();
    // Apply the line material
    lineMaterial.SetPass(0);

    GL.PushMatrix();
    // Set transformation matrix for drawing to match our transform
    GL.MultMatrix(transform.localToWorldMatrix);

    // Draw lines
    GL.Begin(GL.LINES);

    Vector3 hvector = new Vector3 (0, height, 0);

    if (bisectors) {
      GL.Color (new Color (Rbis, Gbis, Bbis, 1));
      GL.Vertex (bisBoth ? (-bisRadius * node.bisectorHat + hvector) : Vector3.zero);
      GL.Vertex (bisRadius * node.bisectorHat + hvector);
    }
    if (segments) {
      GL.Color (new Color (Rseg, Gseg, Bseg, 1));
      GL.Vertex (hvector);
      GL.Vertex (segRadius * node.segmentHat + hvector);
      GL.Vertex (hvector);
      GL.Vertex (segRadius * -node.prevSegmentHat + hvector);
    }

    GL.End();
    GL.PopMatrix();
  }
}


// https://docs.unity3d.com/ScriptReference/Editor.html

[CustomEditor(typeof(NodeDebug))]
[CanEditMultipleObjects]
public class NodeDebugInspector : Editor {
  SerializedProperty height;
  SerializedProperty bisectors, bisBoth, bisRadius, Rbis, Gbis, Bbis;
  SerializedProperty segments, segRadius, Rseg, Gseg, Bseg;

  void OnEnable () {
    height = serializedObject.FindProperty ("height");
    bisectors = serializedObject.FindProperty ("bisectors");
    bisBoth = serializedObject.FindProperty ("bisBoth");
    bisRadius = serializedObject.FindProperty ("bisRadius");
    Rbis = serializedObject.FindProperty ("Rbis");
    Gbis = serializedObject.FindProperty ("Gbis");
    Bbis = serializedObject.FindProperty ("Bbis");

    segments = serializedObject.FindProperty ("segments");
    segRadius = serializedObject.FindProperty ("segRadius");
    Rseg = serializedObject.FindProperty ("Rseg");
    Gseg = serializedObject.FindProperty ("Gseg");
    Bseg = serializedObject.FindProperty ("Bseg");
  }

  public override void OnInspectorGUI () {
    serializedObject.Update ();

    EditorGUILayout.PropertyField(height, new GUIContent("Draw Height"));
    EditorGUILayout.PropertyField(bisectors, new GUIContent("Bisectors"));
    EditorGUILayout.PropertyField(bisBoth, new GUIContent("Both Directions"));
    EditorGUILayout.PropertyField(bisRadius, new GUIContent("Radius"));
    EditorGUILayout.PropertyField(Rbis, new GUIContent("R"));
    EditorGUILayout.PropertyField(Gbis, new GUIContent("G"));
    EditorGUILayout.PropertyField(Bbis, new GUIContent("B"));

    EditorGUILayout.PropertyField(segments, new GUIContent("Segments"));
    EditorGUILayout.PropertyField(segRadius, new GUIContent("Radius"));
    EditorGUILayout.PropertyField(Rseg, new GUIContent("R"));
    EditorGUILayout.PropertyField(Gseg, new GUIContent("G"));
    EditorGUILayout.PropertyField(Bseg, new GUIContent("B"));

    serializedObject.ApplyModifiedProperties ();
  }
}

#endif