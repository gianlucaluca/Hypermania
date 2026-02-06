#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Design.Animation.Sorting.Editors
{
    [CustomEditor(typeof(SpriteSortGroup))]
    public sealed class SpriteSortGroupEditor : Editor
    {
        private SerializedProperty _sortingLayerIdProp;
        private SerializedProperty _baseOrderProp;
        private SerializedProperty _itemsProp;

        private ReorderableList _list;

        private const float ThumbSize = 48f;

        private void OnEnable()
        {
            _sortingLayerIdProp = serializedObject.FindProperty("_sortingLayerId");
            _baseOrderProp = serializedObject.FindProperty("_baseOrder");
            _itemsProp = serializedObject.FindProperty("_items");

            _list = new ReorderableList(
                serializedObject,
                _itemsProp,
                draggable: true,
                displayHeader: true,
                displayAddButton: false,
                displayRemoveButton: false
            );

            _list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Sprite Sort Items (drag to reorder)");
            };

            _list.elementHeightCallback = index => ThumbSize + 8f;

            _list.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                rect.y += 4f;
                rect.height = ThumbSize;

                var element = _itemsProp.GetArrayElementAtIndex(index);
                var item = element.objectReferenceValue as SpriteSortItem;

                // Thumbnail rect
                var thumbRect = new Rect(rect.x, rect.y, ThumbSize, ThumbSize);

                // Object field rect
                var fieldRect = new Rect(
                    rect.x + ThumbSize + 8f,
                    rect.y,
                    rect.width - (ThumbSize + 8f),
                    EditorGUIUtility.singleLineHeight
                );

                // Secondary info rect
                var infoRect = new Rect(
                    fieldRect.x,
                    rect.y + EditorGUIUtility.singleLineHeight + 4f,
                    fieldRect.width,
                    EditorGUIUtility.singleLineHeight
                );

                DrawThumb(thumbRect, item);

                EditorGUI.BeginChangeCheck();
                var newObj = EditorGUI.ObjectField(
                    fieldRect,
                    GUIContent.none,
                    item,
                    typeof(SpriteSortItem),
                    allowSceneObjects: true
                );
                if (EditorGUI.EndChangeCheck())
                    element.objectReferenceValue = newObj;

                if (item == null)
                {
                    EditorGUI.LabelField(infoRect, "Missing reference");
                }
                else
                {
                    var r = item.Renderer;
                    if (r == null)
                    {
                        EditorGUI.LabelField(infoRect, "No SpriteRenderer found");
                    }
                    else
                    {
                        EditorGUI.LabelField(infoRect, $"{r.gameObject.name}  •  SpriteRenderer");
                    }
                }
            };

            _list.onReorderCallback = _ =>
            {
                ApplySorting();
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Sorting layer popup (writes to _sortingLayerIdProp.intValue).
            int currentId = _sortingLayerIdProp.intValue;
            int newId = SortingLayerUtil.PopupSortingLayer("Sorting Layer", currentId);
            if (newId != currentId)
                _sortingLayerIdProp.intValue = newId;

            EditorGUILayout.PropertyField(_baseOrderProp);

            EditorGUILayout.Space(6);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Collect From Children"))
                {
                    CollectFromChildren();
                }

                if (GUILayout.Button("Apply Now"))
                {
                    ApplySorting();
                }
            }

            EditorGUILayout.Space(4);

            _list.DoLayoutList();

            if (serializedObject.ApplyModifiedProperties())
            {
                ApplySorting();
            }
        }

        private void CollectFromChildren()
        {
            var group = (SpriteSortGroup)target;

            var found = new List<SpriteSortItem>();
            group.GetComponentsInChildren(includeInactive: true, result: found);

            // Keep existing ordering when possible, append new ones at the end.
            var existing = new HashSet<SpriteSortItem>();
            for (int i = 0; i < _itemsProp.arraySize; i++)
            {
                var it = _itemsProp.GetArrayElementAtIndex(i).objectReferenceValue as SpriteSortItem;
                if (it != null)
                    existing.Add(it);
            }

            // Append any not already present.
            foreach (var it in found)
            {
                if (it == null)
                    continue;
                if (existing.Contains(it))
                    continue;

                _itemsProp.arraySize++;
                _itemsProp.GetArrayElementAtIndex(_itemsProp.arraySize - 1).objectReferenceValue = it;
            }

            serializedObject.ApplyModifiedProperties();
            ApplySorting();
        }

        private void ApplySorting()
        {
            var group = (SpriteSortGroup)target;

            // Ensure serialized changes are applied before we read group.Items via its backing list.
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            Undo.RecordObjects(GetAllRenderers(group), "Apply Sprite Sorting");
            group.ApplyToRenderers();

            // Mark renderers dirty so scene saves correctly.
            foreach (var r in GetAllRenderers(group))
                if (r != null)
                    EditorUtility.SetDirty(r);

            EditorUtility.SetDirty(group);
        }

        private static SpriteRenderer[] GetAllRenderers(SpriteSortGroup group)
        {
            return group.GetComponentsInChildren<SpriteRenderer>(includeInactive: true);
        }

        private static void DrawThumb(Rect rect, SpriteSortItem item)
        {
            Texture tex = null;

            if (item != null)
            {
                var thumbSprite = item.ThumbnailOverride;
                if (thumbSprite == null)
                {
                    var r = item.Renderer;
                    if (r != null)
                        thumbSprite = r.sprite;
                }

                if (thumbSprite != null)
                    tex = AssetPreview.GetAssetPreview(thumbSprite) ?? AssetPreview.GetMiniThumbnail(thumbSprite);
            }

            if (tex != null)
            {
                GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.HelpBox(rect, "No\nSprite", MessageType.None);
            }
        }
    }
}
#endif
