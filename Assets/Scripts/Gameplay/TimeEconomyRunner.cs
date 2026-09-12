using UnityEngine;

namespace ChronoBlade.Gameplay
{
    // Drives TimeEconomyController.Tick every frame during Play mode. Kept separate from the
    // controller itself so spec 02's balancer can simulate drain outside Play mode without this.
    [RequireComponent(typeof(TimeEconomyController))]
    public class TimeEconomyRunner : MonoBehaviour
    {
        TimeEconomyController _controller;

        void Awake() => _controller = GetComponent<TimeEconomyController>();

        void Update()
        {
            if (!_controller.IsDepleted) _controller.Tick(Time.deltaTime);
        }
    }
}
