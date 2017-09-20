namespace ForeignExchange.ViewModels
{
    using GalaSoft.MvvmLight.Command;
    using Services;
    using System.ComponentModel;
    using System.Windows.Input;
    using Models;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using Helpers;
    using System.Threading.Tasks;

    public class MainViewModel: INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Services
        ApiService apiService;
        DataService dataService;
        DialogService dialogService;
        #endregion

        #region Attributes
        bool _isEnabled;
        bool _isRunning;
        List<Rate> rates;
        ObservableCollection<Rate> _rates;
        Rate _sourceRate;
        Rate _targetRate;
        string _amount;
        string _result;
        string _status;
        #endregion

        #region Properties

        public string Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status != value)
                {
                    _status = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(Status)));
                }
            }
        }

        public ObservableCollection<Rate> Rates
        {
            get
            {
                return _rates;
            }
            set
            {
                if (_rates != value)
                {
                    _rates = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(Rates)));
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(IsEnabled)));
                }
            }
        }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(IsRunning)));
                }
            }
        }

        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                if (_result != value)
                {
                    _result = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(Result)));
                }
            }
        }

        public Rate SourceRate
        {
            get
            {
                return _sourceRate;
            }
            set
            {
                if (_sourceRate != value)
                {
                    _sourceRate = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(SourceRate)));
                }
            }
        }

        public Rate TargetRate
        {
            get
            {
                return _targetRate;
            }
            set
            {
                if (_targetRate != value)
                {
                    _targetRate = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(TargetRate)));
                }
            }
        }

        public string Amount
        {
            get
            {
                return _amount;
            }
            set
            {
                if (_amount != value)
                {
                    _amount = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(Amount)));
                }
            }
        }
        #endregion

        #region Constructors 
        public MainViewModel()
        {
            apiService = new ApiService();
            dataService = new DataService();
            dialogService = new DialogService();
            LoadRates();
        }
        #endregion

        #region Methods
        async void LoadRates()
        {
            IsRunning = true;
            Result = Lenguages.Loading;

            var connection = await apiService.CheckConnection();

            if (!connection.IsSuccess)
            {
                LoadLocalData();
            }
            else
            {
                await LoadDataFromAPI();
            }

            if (rates.Count == 0)
            {
                IsRunning = false;
                IsEnabled = false;
                Result = Lenguages.Connection;
                Status = Lenguages.Message;
                return;
            }

            Rates = new ObservableCollection<Rate>(rates);

            IsRunning = false;
            IsEnabled = true;
            Result = Lenguages.Ready;
        }

        void LoadLocalData()
        {
            rates = dataService.Get<Rate>(false);
            Status =Lenguages.Status2;
        }

        async Task LoadDataFromAPI()
        {
            var url = "http://apiexchangesrates.azurewebsites.net"; //Application.Current.Resources["URLAPI"].ToString();

            var response = await apiService.GetList<Rate>(
                url,
                "/api/Rates");

            if (!response.IsSuccess)
            {
                LoadLocalData();
                return;
            }

            // Storage data local
            rates = (List<Rate>)response.Result;
            dataService.DeleteAll<Rate>();
            dataService.Save(rates);

            Status = Lenguages.Status;
        }
        #endregion

        #region Commands

        public ICommand ChangeCommand { get { return new RelayCommand(Change); } }

        void Change()
        {
            var aux = SourceRate;
            SourceRate = TargetRate;
            TargetRate = aux;
            Convert();
        }

        public ICommand ConvertCommand { get { return new RelayCommand(Convert); } }

        async void Convert()
        {
            if (string.IsNullOrEmpty(Amount))
            {
                await dialogService.ShowMessage(
                    Lenguages.Error,
                    Lenguages.AmountValidation,
                    Lenguages.Accept);
                return;
            }

            decimal amount = 0;
            if (!decimal.TryParse(Amount, out amount))
            {
                await dialogService.ShowMessage(
                   Lenguages.Error,
                   Lenguages.AmountNumericValidation,
                   Lenguages.Accept);
                return;
            }

            if (SourceRate == null)
            {
                await dialogService.ShowMessage(
                    Lenguages.Error,
                    Lenguages.SourceRateValidation,
                    Lenguages.Accept);
                return;
            }

            if (TargetRate == null)
            {
                await dialogService.ShowMessage(
                    Lenguages.Error,
                    Lenguages.TargetRateValidation,
                    Lenguages.Accept);
                return;
            }

            var amountConverted = amount /
                                  (decimal)SourceRate.TaxRate *
                                  (decimal)TargetRate.TaxRate;

            Result = string.Format(
                "{0:C2} {1} = {2:C2} {3}",
                amount,
                SourceRate.Code,
                amountConverted,
                TargetRate.Code);
        }
        #endregion
    }
}

