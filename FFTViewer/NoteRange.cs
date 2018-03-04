using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFTViewer
{
    class NoteRange
    {
        private float _Base;
        public float Base
        {
            get => _Base;
            set
            {
                _Base = value;
                UpdateCache();
            }
        }

        private int _OffsetMin;
        public int OffsetMin
        {
            get => _OffsetMin;
            set
            {
                _OffsetMin = value;
                UpdateCache();
            }
        }

        private int _OffsetMax;
        public int OffsetMax
        {
            get => _OffsetMax;
            set
            {
                _OffsetMax = value;
                UpdateCache();
            }
        }

        private float[] _CachedBase;
        private float _CachedLogScale0, _CachedLogScaleK;

        private void UpdateCache()
        {
            _CachedBase = new float[OffsetMax - OffsetMin + 1];
            for (int i = OffsetMin; i <= OffsetMax; ++i)
            {
                _CachedBase[i - OffsetMin] = Base * (float)Math.Pow(2, i);
            }
            //For log scale calculation
            _CachedLogScaleK = 1 / (float)(Math.Log(MaxX) - Math.Log(MinX));
            _CachedLogScale0 = (float)Math.Log(MinX);
        }
        
        public float MinX => _CachedBase[0];
        public float MaxX => _CachedBase[_CachedBase.Length - 1];

        public int GroupCount => OffsetMax - OffsetMin + 1;

        public float GroupBase(int i)
        {
            return _CachedBase[i];
        }

        public float CalculateLogScale(float val)
        {
            if (val < MinX) return 0;
            if (val > MaxX) return 1;
            return ((float)Math.Log(val) - _CachedLogScale0) * _CachedLogScaleK;
        }
    }
}
