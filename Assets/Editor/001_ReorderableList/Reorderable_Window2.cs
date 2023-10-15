using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditorInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SceneManagement;



public class Reorderable_Window2 : EditorWindow
{


    public class WindowParamsCache : ScriptableObject
    {
        // 必须使用此结构, 才能被 ReorderableList 正确处理
        [Serializable]
        public class Node
        {
            public UnityEngine.Object obj;
            public Node()
            {
                obj = null;
            }
        }
        public List<Node> nodes = new List<Node>();
    }


    // ---------------------------------------
    WindowParamsCache windowParams; // 参数的本地缓存, 方便 EditorWindow 再次打开时, 显示上一次配置的信息;
    ReorderableList reorderableNodes; // 仅仅是个 list管理器, 管理 windowParams.nodes;
    Vector2 reorderableNodes_scrollPos; // 在 reorderableNodes 右侧维护一个 滑动条


    static readonly string assetCachePath = TprIO.NormalizePathSeparator( "Assets/_Tmp_/Reorderable_Window_2.asset" );// windowParams 本地缓存地址



    [MenuItem("_Tools_/001_ReorderableList 单Obj元素 -2- (Window)", false, 701)]
    static void ShowWindow()
    {
        GetWindow<Reorderable_Window2>("单Obj元素"); // 一个可缩放的窗口
    }



    void Init()
    {
        //---
        if (windowParams == null)
        {
            TprIO.CheckAndCreateDirectory( TprIO.GetDirectoryName( assetCachePath ));
            windowParams = AssetDatabase.LoadAssetAtPath<WindowParamsCache>( assetCachePath );
            if (windowParams == null)
            {

                windowParams = (WindowParamsCache)ScriptableObject.CreateInstance(typeof(WindowParamsCache));
                windowParams.nodes.Add(new WindowParamsCache.Node());
                AssetDatabase.CreateAsset(windowParams, assetCachePath );
            }
        }
        if (reorderableNodes == null)
        {
            reorderableNodes = new ReorderableList(windowParams.nodes, null, true, false, true, true);
            reorderableNodes.elementHeightCallback = (int index) => { return EditorGUIUtility.singleLineHeight + 1; }; // 每一行元素的高度
            reorderableNodes.drawElementCallback = OnDrawElementCallback;
        }
    }

    
    void OnGUI()
    {
        Init();
        // =========================================================
        reorderableNodes_scrollPos = GUILayout.BeginScrollView(reorderableNodes_scrollPos); // 给整个窗口搞个右侧滑动条, 避免窗口太扁

        if (reorderableNodes != null)
        {
            reorderableNodes.DoLayoutList();
        }


        if(GUILayout.Button("打印 list", GUILayout.Width(170), GUILayout.Height(40) ))
        {
            PrintList();
        }
       
        GUILayout.EndScrollView();
    }
    


    // 最简单: 一行只有一个 obj slot;
    void OnDrawElementCallback(Rect rect, int index, bool isactive, bool isfocused)
    {
        if (windowParams == null || windowParams.nodes.Count < index)
        {
            return;
        }
        var info = windowParams.nodes[index];
        rect.height = EditorGUIUtility.singleLineHeight;
        info.obj = (UnityEngine.Object)EditorGUI.ObjectField(rect, info.obj, typeof(UnityEngine.Object), true );
    }


    void PrintList() 
    {
        Debug.Log("共有元素: " + windowParams.nodes.Count + " 个;" );

        for( int i=0; i<windowParams.nodes.Count; i++ )
        {
            var node = windowParams.nodes[i];
            Debug.Assert( node != null );
            if( node.obj == null )
            {
                Debug.Log( "-" + i + "-: null"  );
            }
            else 
            {
                var path = AssetDatabase.GetAssetPath( node.obj.GetInstanceID() );
                Debug.Log( "-" + i + "-: " + path );
            }
        }
    }


}




