﻿/*
 * @Author: fasthro
 * @Date: 2019-05-27 17:37:12
 * @Description: 选择器窗口
 */
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CubeWorldEditor
{
    public class SelectorWindow : AbstractSceneWindow, ISceneWindow
    {
        // 位置偏移
        private Vector3 m_positionOffset;
        // 角度偏移
        private Vector3 m_angleOffset;

        // 选择器工具
        private SelectorTools m_selectorTools;

        private int selectedSnapping;
        private float snapValue;
        private readonly string[] Options = { "Custom", "0.25", "0.5", "1.0" };
        private readonly float[] OptionValues = { 0.0f, 0.25f, 0.5f, 1.0f };
        private readonly Color backgroundRed = new Color(1.0f, 0.467f, 0.465f);
        private readonly Color backgroundGreen = new Color(0.467f, 1.0f, 0.514f);
        private readonly Color backgroundBlue = new Color(0.467f, 0.67f, 1.0f);



        protected override void OnInitialize()
        {
            title = "Selecter Setting";
            w = 330;
            h = 175;
            windowId = EUI.GetWindowId();

            selectedSnapping = 1;

            m_selectorTools = CubeWorldEditorWindow.Inst.sceneWindow.selectorTools;
            if (m_selectorTools.selectGrid != null)
            {
                m_positionOffset = m_selectorTools.selectGrid.positionOffset;
                m_angleOffset = m_selectorTools.selectGrid.angleOffset;
            }
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            if (!isShow) return;
            if (m_selectorTools.selectGrid == null) return;

            x = sceneView.position.width - w - 2;
            y = sceneView.position.height - h - 5;
            GUI.Window(windowId, new Rect(x, y, w, h), OnGUI, title);
        }

        protected override void OnGUI(int id)
        {
            GUILayout.Label("-Rotation");
            using (new HorizontalCenteredScope())
            {
                var originalColour = GUI.backgroundColor;
                GUI.backgroundColor = backgroundRed;
                if (GUILayout.Button(EUI.GetTextureContent("iconX90"), GUILayout.Width(50), GUILayout.Height(50)))
                    m_angleOffset.x += 90;
                if (GUILayout.Button(EUI.GetTextureContent("iconX180"), GUILayout.Width(50), GUILayout.Height(50)))
                    m_angleOffset.x += 180;

                GUI.backgroundColor = backgroundGreen;
                if (GUILayout.Button(EUI.GetTextureContent("iconY90"), GUILayout.Width(50), GUILayout.Height(50)))
                    m_angleOffset.y += 90;
                if (GUILayout.Button(EUI.GetTextureContent("iconY180"), GUILayout.Width(50), GUILayout.Height(50)))
                    m_angleOffset.y += 180;


                GUI.backgroundColor = backgroundBlue;
                if (GUILayout.Button(EUI.GetTextureContent("iconZ90"), GUILayout.Width(50), GUILayout.Height(50)))
                    m_angleOffset.z += 90;
                if (GUILayout.Button(EUI.GetTextureContent("iconZ180"), GUILayout.Width(50), GUILayout.Height(50)))
                    m_angleOffset.z += 180;

                GUI.backgroundColor = originalColour;
            }

            GUILayout.Label("-Position");
            selectedSnapping = GUILayout.SelectionGrid(selectedSnapping, Options, Options.Length, EditorStyles.miniButton);
            if (selectedSnapping < OptionValues.Length && selectedSnapping > 0)
            {
                snapValue = OptionValues[selectedSnapping];
            }
            else
            {
                snapValue = EditorGUILayout.FloatField("Custom Value", snapValue);
            }

            using (new HorizontalCenteredScope())
            {
                var originalColour = GUI.backgroundColor;
                GUI.backgroundColor = backgroundRed; ;

                if (GUILayout.Button(EUI.GetTextContent("+X"), GUILayout.Width(50)))
                    m_positionOffset.x += snapValue;

                if (GUILayout.Button(EUI.GetTextContent("-X"), GUILayout.Width(50)))
                    m_positionOffset.x -= snapValue;

                GUI.backgroundColor = backgroundGreen;
                if (GUILayout.Button(EUI.GetTextContent("+Y"), GUILayout.Width(50)))
                    m_positionOffset.y += snapValue;
                if (GUILayout.Button(EUI.GetTextContent("-Y"), GUILayout.Width(50)))
                    m_positionOffset.y -= snapValue;

                GUI.backgroundColor = backgroundBlue;
                if (GUILayout.Button(EUI.GetTextContent("+Z"), GUILayout.Width(50)))
                    m_positionOffset.z += snapValue;
                if (GUILayout.Button(EUI.GetTextContent("-Z"), GUILayout.Width(50)))
                    m_positionOffset.z -= snapValue;

                GUI.backgroundColor = originalColour;
            }

            // 设置笔刷
            m_selectorTools.selectGrid.positionOffset = m_positionOffset;
            m_selectorTools.selectGrid.angleOffset = m_angleOffset;

            m_selectorTools.selectGrid.Refresh();
        }
    }
}
