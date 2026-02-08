using UnityEngine;
using UnityEngine.UI;

namespace Game.View
{
    [RequireComponent(typeof(Slider))]
    public class BurstBarView : MonoBehaviour
    {
        public void SetMaxBurst(float burst)
        {
            Slider slider = GetComponent<Slider>();
            slider.maxValue = burst;
            slider.value = burst;
        }

        public void SetBurst(float burst)
        {
            Slider slider = GetComponent<Slider>();
            slider.value = burst;
        }
    }
}
