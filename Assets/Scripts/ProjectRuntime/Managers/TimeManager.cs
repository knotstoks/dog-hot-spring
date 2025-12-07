using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectRuntime.Managers
{
    public class TimeManager
    {
        private static readonly Lazy<TimeManager> s_lazy = new(() => new TimeManager());

        private DateTime _startTimeObject;

        private TimeManager()
        {
            var timeManagerObject = new GameObject
            {
                name = nameof(TimeManager)
            };
            Object.DontDestroyOnLoad(timeManagerObject);
        }

        public void SetStartTime()
        {
            this._startTimeObject = DateTime.Now;
        }

        public TimeSpan GetTimeFromStart()
        {
            return DateTime.Now - this._startTimeObject;
        }
    }
}