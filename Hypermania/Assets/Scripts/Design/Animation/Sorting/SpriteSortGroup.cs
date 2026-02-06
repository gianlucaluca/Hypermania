using System.Collections.Generic;
using UnityEngine;

namespace Design.Animation.Sorting
{
    [DisallowMultipleComponent]
    public sealed class SpriteSortGroup : MonoBehaviour
    {
        [SerializeField]
        private int _sortingLayerId;

        [SerializeField]
        private int _baseOrder = 0;

        [SerializeField]
        private List<SpriteSortItem> _items = new();

        public int SortingLayerId => _sortingLayerId;
        public int BaseOrder => _baseOrder;

        public void SetSortingLayerId(int id) => _sortingLayerId = id;

        public void SetItems(List<SpriteSortItem> items) => _items = items;

        public void ApplyToRenderers()
        {
            int order = _baseOrder;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (item == null)
                {
                    continue;
                }

                var r = item.Renderer;
                if (r == null)
                {
                    continue;
                }

                r.sortingLayerID = _sortingLayerId;
                r.sortingOrder = order;
                order++;
            }
        }
    }
}
