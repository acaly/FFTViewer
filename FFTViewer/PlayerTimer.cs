using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class PlayerTimer
    {
        private Stopwatch _Clock = new Stopwatch();
        private float _TimeOffset = 0;

        public PlayerTimer(float totalTimeMs)
        {
            TotalTimeMs = totalTimeMs;
        }

        public float TotalTimeMs { get; private set; }

        public void Start()
        {
            _Clock.Start();
            StateChanged?.Invoke();
        }

        public void Pause()
        {
            _Clock.Stop();
            StateChanged?.Invoke();
        }

        public void Stop()
        {
            _Clock.Reset();
            _TimeOffset = 0;
            StateChanged?.Invoke();
        }

        public void SetPosition(float ms)
        {
            if (_Clock.IsRunning)
            {
                _Clock.Restart();
            }
            else
            {
                _Clock.Reset();
            }
            _TimeOffset = ms;
            StateChanged?.Invoke();
        }

        public float TimeMs => (_Clock.ElapsedMilliseconds) + _TimeOffset;

        public void Update()
        {
            var val = TimeMs / TotalTimeMs;
            if (val > 1)
            {
                val = 1;
                _Clock.Reset();
                _TimeOffset = TotalTimeMs;
                StateChanged?.Invoke();
            }
        }

        public bool IsRunning => _Clock.IsRunning;
        public event Action StateChanged;
    }
}
