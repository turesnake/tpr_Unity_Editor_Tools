using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;
using System;
using System.Collections.Generic;



// 演示如何在一个 EditorWindow 中绘制一组 ReorderableList objs;


public class Reorderable_Window : EditorWindow
{


    public class ReorderableCache : ScriptableObject
    {
        [Serializable]
        public class Node
        {
            public UnityEngine.Object obj;
            public Node()
            {
                obj = null;
            }
        }

        [SerializeField]
        List<Node> m_nodes = new List<Node>();

        public List<Node> Nodes
        {
            get { return m_nodes; }
        }
    }


    // ---------------------------------------
    ReorderableCache m_reorderableCache; // m_objs 的本地缓存, 方便 EditorWindow 再次打开时, 显示上一次配置的信息;
    ReorderableList m_objs;

    Vector2 m_objs_scrollPos; // 在 m_objs 右侧维护一个 滑动条
    static string assetCachePath = TprIO.NormalizePathSeparator( "Assets/_Tmp_/Reorderable_Window.asset" );// m_reorderableCache 本地缓存地址



    [MenuItem("_Tools_/001_ReorderableList 单Obj元素 (Window)", false, 701)]
    static void ShowWindow()
    {
        GetWindow<Reorderable_Window>(); // 一个可缩放的窗口
    }


    void OnGUI()
    {
        Init();

        // 绘制 上部 oths...
        // ...

        // 绘制 m_objs:
        if (m_objs != null)
        {
            EditorGUI.BeginChangeCheck();
            m_objs_scrollPos = GUILayout.BeginScrollView(m_objs_scrollPos);
            m_objs.DoLayoutList();
            GUILayout.EndScrollView();
            bool isDirty_objs = EditorGUI.EndChangeCheck();
            if (isDirty_objs)
            {
                EditorUtility.SetDirty(m_reorderableCache);
                AssetDatabase.SaveAssets();
                Repaint();
            } 
        }

        // 绘制 下部 oths...
        // ...

        GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(1)); // draw a line
        if(GUILayout.Button("测试", GUILayout.Width(150), GUILayout.Height(40) ))
        {
            
        }
    }

    void Init()
    {
        if (m_reorderableCache == null)
        {
            TprIO.CheckAndCreateDirectory( TprIO.GetDirectoryName( assetCachePath ));
            m_reorderableCache = AssetDatabase.LoadAssetAtPath<ReorderableCache>( assetCachePath );
            if (m_reorderableCache == null)
            {
                m_reorderableCache = (ReorderableCache)ScriptableObject.CreateInstance(typeof(ReorderableCache));
                m_reorderableCache.Nodes.Add(new ReorderableCache.Node());
                AssetDatabase.CreateAsset(m_reorderableCache, assetCachePath );
            }
        }

        if (m_objs == null)
        {
            m_objs = new ReorderableList(m_reorderableCache.Nodes, null, true, false, true, true);
            m_objs.drawHeaderCallback = (Rect rect) => { EditorGUI.LabelField(rect, "KOKO"); }; // draw list header 好像没啥用, 没找到画在哪了
            m_objs.elementHeightCallback = (int index) => { return EditorGUIUtility.singleLineHeight + 1; }; // 每一行元素的高度
            m_objs.drawElementCallback = OnDrawElementCallback;
        }
    }


    // 最简单: 一行只有一个 obj slot;
    void OnDrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
    {
        if (m_reorderableCache == null || m_reorderableCache.Nodes.Count < index)
        {
            return;
        }
        var info = m_reorderableCache.Nodes[index];
        rect.height = EditorGUIUtility.singleLineHeight;
        info.obj = (UnityEngine.Object)EditorGUI.ObjectField(rect, info.obj, typeof(UnityEngine.Object), true );
    }


}
