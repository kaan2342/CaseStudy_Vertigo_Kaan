using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Vertigo.WheelGame.Presentation
{
    internal static class WheelViewUtility
    {
        internal static void UpdateButtonLabel(Button button, string label)
        {
            if (button == null)
            {
                return;
            }

            var text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
            }
        }

        internal static RectTransform EnsureRectTransform(Transform parent, string name)
        {
            var child = EnsureChild(parent, name);
            return EnsureComponent<RectTransform>(child);
        }

        internal static RectTransform EnsureRectTransform(Transform parent, string name, out bool wasCreated)
        {
            var child = EnsureChild(parent, name, out wasCreated);
            return EnsureComponent<RectTransform>(child);
        }

        internal static GameObject EnsureChild(Transform parent, string name)
        {
            var child = parent != null ? parent.Find(name) : null;
            if (child != null)
            {
                return child.gameObject;
            }

            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            return go;
        }

        internal static GameObject EnsureChild(Transform parent, string name, out bool wasCreated)
        {
            var child = parent != null ? parent.Find(name) : null;
            if (child != null)
            {
                wasCreated = false;
                return child.gameObject;
            }

            var go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            wasCreated = true;
            return go;
        }

        internal static T EnsureComponent<T>(GameObject gameObject) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            return gameObject.AddComponent<T>();
        }

        internal static T EnsureComponent<T>(GameObject gameObject, out bool wasCreated) where T : Component
        {
            var component = gameObject.GetComponent<T>();
            if (component != null)
            {
                wasCreated = false;
                return component;
            }

            wasCreated = true;
            return gameObject.AddComponent<T>();
        }

        internal static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 sizeDelta,
            Vector2 anchoredPosition)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;
        }

        internal static Vector2 ResolveOverlayPosition(RectTransform overlay, RectTransform source, Vector2 offset)
        {
            if (overlay == null || source == null)
            {
                return offset;
            }

            var worldCenter = source.TransformPoint(source.rect.center);
            var screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(overlay, screenPoint, null, out var overlayPoint);
            return overlayPoint + offset;
        }
    }
}
