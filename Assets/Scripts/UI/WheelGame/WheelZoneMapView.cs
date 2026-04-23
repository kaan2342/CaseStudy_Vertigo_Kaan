using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Vertigo.WheelGame.Domain;

namespace Vertigo.WheelGame.Presentation
{
    public sealed class WheelZoneMapView : MonoBehaviour
    {
        private const int VisibleZoneCount = 13;

        [SerializeField] private RectTransform ui_group_zone_slots;
        [SerializeField] private Sprite ui_sprite_zone_current;
        [SerializeField] private Sprite ui_sprite_zone_current_white;
        [SerializeField] private Sprite ui_sprite_zone_super;
        [SerializeField] private Sprite ui_sprite_zone_white;
        [SerializeField] private Sprite ui_sprite_zone_coming;
        [SerializeField] private ZoneCell[] ui_zone_cells = new ZoneCell[VisibleZoneCount];

        private void Awake()
        {
            if (!ValidateReferences())
            {
                enabled = false;
            }
        }

        public void Render(int currentZone, int safeZoneInterval, int superZoneInterval)
        {
            if (ui_group_zone_slots == null || currentZone <= 0 || ui_zone_cells == null || ui_zone_cells.Length == 0)
            {
                return;
            }

            var halfWindow = VisibleZoneCount / 2;
            for (var i = 0; i < ui_zone_cells.Length; i++)
            {
                var zoneIndex = currentZone + i - halfWindow;
                var cell = ui_zone_cells[i];
                if (cell?.Root == null)
                {
                    continue;
                }

                var isVisible = zoneIndex > 0;
                cell.Root.gameObject.SetActive(isVisible);
                if (!isVisible)
                {
                    continue;
                }

                var isCurrent = zoneIndex == currentZone;
                var isPast = zoneIndex < currentZone;
                var zoneType = ZoneRules.GetZoneType(zoneIndex, safeZoneInterval, superZoneInterval);

                if (cell.Background != null)
                {
                    cell.Background.enabled = isCurrent;
                    cell.Background.raycastTarget = false;
                    if (isCurrent)
                    {
                        cell.Background.sprite = ui_sprite_zone_current_white != null
                            ? ui_sprite_zone_current_white
                            : ui_sprite_zone_current;
                        cell.Background.type = Image.Type.Sliced;
                        cell.Background.color = Color.white;
                    }
                }

                if (cell.Label != null)
                {
                    cell.Label.text = zoneIndex.ToString();
                    cell.Label.color = ResolveZoneColor(zoneType, isCurrent, isPast);
                }
            }
        }

        private static Color ResolveZoneColor(ZoneType zoneType, bool isCurrent, bool isPast)
        {
            if (isCurrent)
            {
                return new Color(0.08f, 0.08f, 0.08f, 1f);
            }

            var color = zoneType == ZoneType.Super
                ? new Color(0.63f, 0.53f, 0.12f, 1f)
                : (zoneType == ZoneType.Safe
                    ? new Color(0.45f, 1f, 0.15f, 1f)
                    : Color.white);

            if (isPast)
            {
                color *= 0.5f;
                color.a = 0.92f;
            }

            return color;
        }

        private bool ValidateReferences()
        {
            if (ui_group_zone_slots == null || ui_zone_cells == null || ui_zone_cells.Length != VisibleZoneCount)
            {
                Debug.LogError("WheelZoneMapView requires a serialized zone slot container and exactly " + VisibleZoneCount + " serialized zone cells.", this);
                return false;
            }

            for (var i = 0; i < ui_zone_cells.Length; i++)
            {
                if (ui_zone_cells[i] == null ||
                    ui_zone_cells[i].Root == null ||
                    ui_zone_cells[i].Background == null ||
                    ui_zone_cells[i].Label == null)
                {
                    Debug.LogError("WheelZoneMapView is missing serialized references for zone cell " + (i + 1).ToString("00") + ".", this);
                    return false;
                }
            }

            return true;
        }

        [Serializable]
        private sealed class ZoneCell
        {
            [SerializeField] private RectTransform root;
            [SerializeField] private Image background;
            [SerializeField] private TMP_Text label;

            public RectTransform Root => root;
            public Image Background => background;
            public TMP_Text Label => label;
        }
    }
}
