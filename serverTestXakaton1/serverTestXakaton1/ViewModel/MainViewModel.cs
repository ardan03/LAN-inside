using GalaSoft.MvvmLight.Command;
using serverTestXakaton1.ViewModel;
using serverTestXakaton1.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace serverTestXakaton1.ViewModel
{
    public class MainViewModel : ViewModedBase
    {
        private Page _curPage = new MainPage();
        private Page _rulePage = new RulePage();
        private Page _mainPage = new MainPage();

        public Page CurPage
        {
            get => _curPage;
            set => Set(ref _curPage, value);
        }

        public ICommand RulePage
        {
            get
            {
                return new RelayCommand(() => CurPage = _rulePage);
            }
        }

        public ICommand MainPage
        {
            get
            {
                return new RelayCommand(() => CurPage = _mainPage);
            }
        }
        public MainViewModel()
        {
            MessageBox.Show("fgjvhbkjgvhbj");
        }
    }
}
