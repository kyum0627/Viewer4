namespace IGX.ViewControl
{
    public class AuxillaryDrawSetting
    {
        private bool _backFace = true;
        private bool _boxIsOOBB = true;
        private bool _clipPlanes = true;
        private bool _companyLogo = true;
        private bool _coordinates = true;
        private bool _normals = true;
        private bool _objectBox = true;

        /// <summary>
        /// 설정 변경 시 발생하는 이벤트
        /// </summary>
        public event EventHandler? SettingChanged;

        public bool BackFace
        {
            get => _backFace;
            set
            {
                if (_backFace != value)
                {
                    _backFace = value;
                    OnSettingChanged();
                }
            }
        }

        public bool BoxIsOOBB
        {
            get => _boxIsOOBB;
            set
            {
                if (_boxIsOOBB != value)
                {
                    _boxIsOOBB = value;
                    OnSettingChanged();
                }
            }
        }

        public bool ClipPlanes
        {
            get => _clipPlanes;
            set
            {
                if (_clipPlanes != value)
                {
                    _clipPlanes = value;
                    OnSettingChanged();
                }
            }
        }

        public bool CompanyLogo
        {
            get => _companyLogo;
            set
            {
                if (_companyLogo != value)
                {
                    _companyLogo = value;
                    OnSettingChanged();
                }
            }
        }

        public bool Coordinates
        {
            get => _coordinates;
            set
            {
                if (_coordinates != value)
                {
                    _coordinates = value;
                    OnSettingChanged();
                }
            }
        }

        public bool Normals
        {
            get => _normals;
            set
            {
                if (_normals != value)
                {
                    _normals = value;
                    System.Diagnostics.Debug.WriteLine($"[AuxillaryDrawSetting] Normals changed to: {value}");
                    OnSettingChanged();
                }
            }
        }

        public bool ObjectBox
        {
            get => _objectBox;
            set
            {
                if (_objectBox != value)
                {
                    _objectBox = value;
                    System.Diagnostics.Debug.WriteLine($"[AuxillaryDrawSetting] ObjectBox changed to: {value}");
                    OnSettingChanged();
                }
            }
        }

        protected virtual void OnSettingChanged()
        {
            SettingChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}