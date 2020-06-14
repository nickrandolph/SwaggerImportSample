using BuildIt;
using Newtonsoft.Json;
using SwaggerImport.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
#if __WASM__
using Uno.UI.Wasm;
#endif 
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SwaggerImport.Uno
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainPage()
        {
            this.InitializeComponent();

            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            VisualStateManager.GoToState(this, "NotLoaded", false);

           
        }

        public async  void LoadDataClick(object sender, RoutedEventArgs args)
        {
            var data = await ViewModel.LoadSessions();

            if (data == 0)
            {
                VisualStateManager.GoToState(this, "NotLoaded", false);

            }
        }
    }

    public class MainViewModel : NotifyBase
    {
        private bool isLoading;
        private ICollection<Session> sessions;
        private bool isDataLoaded;

        public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }

        public bool IsDataLoaded { get => isDataLoaded; set => SetProperty(ref isDataLoaded ,value); }

        public ICollection<Session> Sessions { get => sessions; set => SetProperty(ref sessions, value); }

        public ICollection<string> Types { get; set; }

        public async Task<int> LoadSessions()
        {
            IsLoading = true;
            try
            {
                IsDataLoaded = false;
#if __WASM__
                var handler = new WasmHttpHandler();
                var http = new System.Net.Http.HttpClient(handler);
#else
           var http = new System.Net.Http.HttpClient();
#endif
                Debug.WriteLine("This works - retrieve session types using raw request");
                var sessionData = await http.GetStringAsync("https://build2020.azurewebsites.net/Sessions/sessiontypes");
                Types= JsonConvert.DeserializeObject<string[]>(sessionData);
                Debug.WriteLine($"Retrieve {Types.Count} session types");

                Debug.WriteLine("This doesn't works - retrieve sessions using raw request");
                var sessionRawData = await http.GetStringAsync("https://build2020.azurewebsites.net/sessions");
                Sessions = JsonConvert.DeserializeObject<Session[]>(sessionRawData);
                Debug.WriteLine($"Retrieve {Sessions.Count} sessions");

                Debug.WriteLine("This doesn't work - retrieve session types using swagger generated code");
                var client = new swaggerClient("https://build2020.azurewebsites.net/", http);
                Types = await client.SessiontypesAsync();
                Debug.WriteLine($"Retrieve {Types.Count} session types");

                Debug.WriteLine("This doesn't work - retrieve sessions using swagger generated code");
                Sessions = await client.SessionsAsync();
                Debug.WriteLine($"Retrieve {Sessions.Count} sessions");
                
                IsDataLoaded = true;
                return Types.Count;

            }
            catch(Exception ex)
            {
                IsDataLoaded = false;
                return 0;
            }
            finally
            {
                IsLoading = false;

            }
        }
    }


    public class BooleanDataTrigger : StateTriggerBase
    {
#region DataValue
        public static Boolean GetDataValue(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(DataValueProperty);
        }
        public static void SetDataValue(DependencyObject obj, Boolean value)
        {
            obj.SetValue(DataValueProperty, value);
        }
        public static readonly DependencyProperty DataValueProperty =
            DependencyProperty.RegisterAttached("DataValue", typeof(Boolean),
                typeof(BooleanDataTrigger), new PropertyMetadata(false, DataValueChanged));
        private static void DataValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Boolean triggerValue = (Boolean)target.GetValue(BooleanDataTrigger.TriggerValueProperty);
            TriggerStateCheck(target, (Boolean)e.NewValue, triggerValue);
        }
#endregion
#region TriggerValue
        public static Boolean GetTriggerValue(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(TriggerValueProperty);
        }
        public static void SetTriggerValue(DependencyObject obj, Boolean value)
        {
            obj.SetValue(TriggerValueProperty, value);
        }
        // Using a DependencyProperty as the backing store for TriggerValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TriggerValueProperty =
            DependencyProperty.RegisterAttached("TriggerValue", typeof(Boolean),
                typeof(BooleanDataTrigger), new PropertyMetadata(false, TriggerValueChanged));
        private static void TriggerValueChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            Boolean dataValue = (Boolean)target.GetValue(BooleanDataTrigger.DataValueProperty);
            TriggerStateCheck(target, dataValue, (Boolean)e.NewValue);
        }
#endregion

        private static void TriggerStateCheck(DependencyObject target, Boolean dataValue, Boolean triggerValue)
        {
            BooleanDataTrigger trigger = target as BooleanDataTrigger;
            if (trigger == null) return;
            trigger.SetActive(triggerValue == dataValue);
        }

    }

}
