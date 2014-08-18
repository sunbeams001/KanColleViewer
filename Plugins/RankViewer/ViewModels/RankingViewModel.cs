using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleWrapper.Models;
using Livet;

namespace Grabacr07.KanColleViewer.Plugins.ViewModels
{
    public class RankingViewModel : ViewModel
    {
        #region Id

        private int _Id;

        public int Id
        {
            get { return this._Id; }
            set
            {
                if (this._Id != value)
                {
                    this._Id = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region NickName

        private string _NickName;

        public string NickName
        {
            get { return this._NickName; }
            set
            {
                if (this._NickName != value)
                {
                    this._NickName = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Comment

        private string _Comment;

        public string Comment
        {
            get { return this._Comment; }
            set
            {
                if (this._Comment != value)
                {
                    this._Comment = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Rate

        private int _Rate;

        public int Rate
        {
            get { return this._Rate; }
            set
            {
                if (this._Rate != value)
                {
                    this._Rate = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Rank

        private int _Rank;

        public int Rank
        {
            get { return this._Rank; }
            set
            {
                if (this._Rank != value)
                {
                    this._Rank = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Level

        private int _Level;

        public int Level
        {
            get { return this._Level; }
            set
            {
                if (this._Level != value)
                {
                    this._Level = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Experience

        private int _Experience;

        public int Experience
        {
            get { return this._Experience; }
            set
            {
                if (this._Experience != value)
                {
                    this._Experience = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        #region Flag

        private int _Flag;

        public int Flag
        {
            get { return this._Flag; }
            set
            {
                if (this._Flag != value)
                {
                    this._Flag = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        #endregion

        public RankingViewModel(Ranking rank)
        {
            if (rank == null)
                return;

            this.Id         = rank.Id;
            this.NickName   = rank.NickName;
            this.Comment    = rank.Comment;
            this.Rate       = rank.Rate;
            this.Rank       = rank.Rank;
            this.Level      = rank.Level;
            this.Experience = rank.Experience;
            this.Flag       = rank.Flag;
        }

        public RankingViewModel() {}
    }
}
