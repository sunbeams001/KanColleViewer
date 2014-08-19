using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Livet;

namespace Grabacr07.KanColleViewer.Models
{
    public class WindowOrientaionMode : NotificationObject, IOrientationMode
	{

        public OrientationType[] SupportedModes
		{
			get {
                OrientationType[] r = { OrientationType.Auto, OrientationType.Horizontal, OrientationType.Vertical };
                return r;
            }
		}

		#region Current 変更通知プロパティ

        private OrientationType _Current;

        public OrientationType Current
		{
			get { return this._Current; }
			private set
			{
                if (this._Current != value)
                {
                    this._Current = value;
                    this.RaisePropertyChanged();
                    this.RaisePropertyChanged("CurrentModeString");
                }
			}
		}

		#endregion

        #region CurrentMode 変更通知プロパティ

        private OrientationType _CurrentMode;

        public OrientationType CurrentMode
        {
            get { return this._CurrentMode; }
            set
            {
                if (this._CurrentMode != value)
                {
                    this._CurrentMode = value;
                    if (value.Equals(OrientationType.Auto))
                    {
                        System.Windows.SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
                        updateOrientationMode();
                    }
                    else
                    {
                        System.Windows.SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
                        this.Current = value;
                    }
                    this.RaisePropertyChanged("CurrentModeString");
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        public void updateOrientationMode()
        {
            if (!this.CurrentMode.Equals(OrientationType.Auto)) return;

            try
            {
                var window = System.Windows.Application.Current.MainWindow;
                if (System.Windows.SystemParameters.FullPrimaryScreenWidth >= System.Windows.SystemParameters.FullPrimaryScreenHeight)
                {
                    this.Current = OrientationType.Horizontal;

                    if (window != null && window.WindowState == System.Windows.WindowState.Normal)
                    {
                        window.Height = 0;
                        window.Width = 1440;
                    }
                }
                else
                {
                    this.Current = OrientationType.Vertical;

                    if (window != null && window.WindowState == System.Windows.WindowState.Normal)
                    {
                        window.Width = 0;
                        window.Height = 1000;
                    }
                }
            } catch {
                if (System.Windows.SystemParameters.FullPrimaryScreenWidth >= System.Windows.SystemParameters.FullPrimaryScreenHeight)
                {
                    this.Current = OrientationType.Horizontal;
                }
                else
                {
                    this.Current = OrientationType.Vertical;
                }
            }
        }

        private void SystemParameters_StaticPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals("FullPrimaryScreenHeight") || e.PropertyName.Equals("FullPrimaryScreenWidth"))
            {
                updateOrientationMode();
            }
        }

        public string CurrentModeString
        {
            get {
                if (this.CurrentMode == OrientationType.Auto)
                {
                    if (this.Current == OrientationType.Horizontal) return "Ah";
                    else return "Av";
                }
                else
                {
                    if (this.Current == OrientationType.Horizontal) return "H";
                    else return "V";
                }
            }
        }
    }
}
