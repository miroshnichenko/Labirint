using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Timers;
using System.Collections;
using System.Windows.Threading;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.EntityClient;
using System.Data;
using System.Collections.Specialized;
using Blasig.Labirint.DataModel;
using System.IO;
using System.Windows.Resources;

namespace Blasig.Labirint.GUI
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private LabirintEntities ctx;
        private DispatcherTimer refreshTimer;
        private VisitorItems items;
        private CollectionViewSource viewSource;

        /// <summary>
        /// Конструктор окна
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            StringBuilder builder = new StringBuilder();
            builder.Append(this.Title);
            builder.Append(" (ver.: ");
            builder.Append(assembly.GetName().Version);
            builder.Append(") Render capability: ");
            switch (RenderCapability.Tier >> 16)
            { 
                case 0:
                    builder.Append("no h/w accel.");
                    break;
                case 1:
                    builder.Append("low perf.");
                    break;
                case 2:
                    builder.Append("high perf.");
                    break;
            }

            this.Title = builder.ToString();
            try
            {
                ctx = new LabirintEntities(GetConnectionString());
                items = (VisitorItems)((ObjectDataProvider)this.TryFindResource("Data")).Data;
                viewSource = (CollectionViewSource)this.TryFindResource("OrderedData");
                int tokens = 100;
                try
                {
                    tokens = GetTokenNumber();
                }
                catch (ApplicationException ex)
                {
                    this.GenerateErrorWindow(ex);
                }
                items.Initialize(ctx, tokens);
                ((ListCollectionView)viewSource.View).CustomSort = new LeftTimeSorter();
                refreshTimer = new DispatcherTimer();
                refreshTimer.Interval = new TimeSpan(0, 0, 1);
                refreshTimer.Tick += (o, a) =>
                {
                    viewSource.View.Refresh();
                    ((DispatcherTimer)o).Stop();
                };
                viewSource.View.Refresh();
                this.SetHelpCommandBinding();
            }
            catch (ApplicationException ex)
            {
                this.GenerateErrorWindow(ex);
                this.Close();
            }
            
        }

        /// <summary>
        /// Добавление команды вызова справки
        /// </summary>
        private void SetHelpCommandBinding()
        {
            CommandBinding binding = new CommandBinding(ApplicationCommands.Help);
            binding.CanExecute += (o, e) => { e.CanExecute = true; };
            binding.Executed += delegate 
            {
                try
                {
                    Uri resource = new Uri("./Resources/About.txt", UriKind.Relative);
                    StreamResourceInfo info = Application.GetResourceStream(resource);
                    StreamReader reader = new StreamReader(info.Stream);
                    string text = reader.ReadToEnd();
                    MessageWindow wnd = new MessageWindow(text,
                        MessageWindowImage.Info, true);
                    wnd.Owner = this;
                    wnd.ShowDialog();
                }
                catch (IOException ex)
                {
                    this.GenerateErrorWindow(ex);
                }
            };
            CommandBindings.Add(binding);
        }

        /// <summary>
        /// Генерация сообщения об ошибке из объекта исключения
        /// </summary>
        /// <param name="ex">Исключение</param>
        /// <returns></returns>
        private string ErrorMessage(Exception ex)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Произошла непредвиденная ошибка. \n\n");
            stringBuilder.Append(ex.Message + "\n\n");
            if (ex.InnerException != null)
            {
                stringBuilder.Append("Дополнительные сведения:\n");
                stringBuilder.Append(ex.InnerException.Message);
                stringBuilder.Append("\n\n");
            }
            stringBuilder.Append("Пожалуйста, обратитесь к системному администратору.");
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Показывает окно с текстом ошибки
        /// </summary>
        /// <param name="ex"></param>
        private void GenerateErrorWindow(Exception ex)
        {
            string errorMsg = ErrorMessage(ex);
            MessageWindow wnd = new MessageWindow(errorMsg, MessageWindowImage.Error, true);
            if (this.IsLoaded)
            {
                wnd.Owner = this;
            }
            else
            {
                wnd.WindowStartupLocation = 
                    System.Windows.WindowStartupLocation.CenterScreen;
            }
            wnd.ShowDialog();
        }

        /// <summary>
        /// Добавление клиента в базу
        /// </summary>
        /// <param name="visitor">Клиент</param>
        private void AddVisitor(Visitor visitor)
        {
            try
            {
                visitor.TimeStart = DateTime.Now;
                ctx.Visitors.AddObject(visitor);
                ctx.SaveChanges();
            }
            catch (DataException ex)
            {
                throw new ApplicationException("Ошибка добавления пользователя в базу данных",
                    ex.InnerException);
            }
        }

        /// <summary>
        /// Закрытие номерка клиента
        /// </summary>
        /// <param name="visitor"></param>
        private void CloseVisitor(Visitor visitor)
        {
            try
            {
                visitor.TimeEnd = DateTime.Now;
                ctx.SaveChanges();
            }
            catch (DataException ex)
            {
                throw new ApplicationException("Ошибка сохранения данных",
                    ex.InnerException);
            }
        }

        /// <summary>
        /// Продление времени пребывания клиента
        /// </summary>
        /// <param name="visitor">Клиент</param>
        /// <param name="time">Время в минутах</param>
        private void VisitorTimeProlongation(Visitor visitor, int time)
        {
            try
            {
                visitor.Paid += time;
                ctx.SaveChanges();
            }
            catch (DataException ex)
            {
                throw new ApplicationException("Ошибка сохранения данных",
                    ex.InnerException);
            }
        }

        /// <summary>
        /// Обработчик события нажатия кнопки 
        /// </summary>
        /// <param name="sender">Виновник поднятия события</param>
        /// <param name="e">Аргументы события</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = e.Source as FrameworkElement;
            if (element != null)
            {
                VisitorItem item = element.DataContext as VisitorItem;
                if (item != null)
                {
                    if (item.Value == null)
                    {
                        AddVisitorWindow dialog = new AddVisitorWindow();
                        dialog.Owner = this;
                        dialog.DataContext = new Visitor()
                        {
                            Check = item.Key,
                            ClientName = string.Empty,
                            Telephone = string.Empty,
                            Age = -1,
                            Paid = 60
                        };
                        bool? result = dialog.ShowDialog();
                        if (result == true)
                        {
                            Visitor visitor = dialog.DataContext as Visitor;
                            if (visitor != null)
                            {
                                try
                                {
                                    this.AddVisitor(visitor);
                                    item.Value = visitor;
                                    refreshTimer.Start();
                                }
                                catch (ApplicationException ex)
                                {
                                    this.GenerateErrorWindow(ex);
                                }
                            }
                        }
                    }
                    else
                    {
                        EditVisitorWindow dialog = new EditVisitorWindow();
                        dialog.Owner = this;
                        dialog.DataContext = item.Value.Check;
                        bool? result = dialog.ShowDialog();
                        if (result == true)
                        {
                            if (dialog.ClosingDecision.IsChecked == true)
                            {
                                try
                                {
                                    this.CloseVisitor(item.Value);
                                    item.Value = null;
                                    refreshTimer.Start();
                                }
                                catch (ApplicationException ex)
                                {
                                    this.GenerateErrorWindow(ex);
                                }
                            }
                            else if (dialog.ProlongationDecision.IsChecked == true)
                            {
                                if (dialog.Combo.SelectedValue != null)
                                {
                                    try
                                    {
                                        int add = int.Parse((string)dialog.Combo.SelectedValue);
                                        this.VisitorTimeProlongation(item.Value, add);
                                        refreshTimer.Start();
                                    }
                                    catch (ApplicationException ex)
                                    {
                                        this.GenerateErrorWindow(ex);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Генерация строки подключения
        /// </summary>
        /// <returns></returns>
        private static string GetConnectionString()
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            string server = ConfigurationManager.AppSettings["server"];
            string db = ConfigurationManager.AppSettings["db"];
            if (server == null)
                throw new ApplicationException("В файле конфигурации не задан адрес сервера.");
            if(db == null)
                throw new ApplicationException("В файле конфигурации не задан каталог БД.");
            builder.DataSource = server;
            builder.InitialCatalog = db;
            builder.IntegratedSecurity = true;

            EntityConnectionStringBuilder entityConnection = new EntityConnectionStringBuilder();
            entityConnection.Provider = "System.Data.SqlClient";
            entityConnection.ProviderConnectionString = builder.ConnectionString;
            entityConnection.Metadata = @"res://*/Model.csdl|
                                          res://*/Model.ssdl|
                                          res://*/Model.msl";

            return entityConnection.ConnectionString;
        }

        /// <summary>
        /// Получение количества номерков из файла конфигурации
        /// </summary>
        /// <returns>Количество номерков</returns>
        private static int GetTokenNumber()
        {
            string data = ConfigurationManager.AppSettings["tokens"];
            if (data == null)
                throw new ApplicationException("В файле конфигурации нет настройки tokens, которая указывает количество номерков.\n"
                    + "Будет использовано значение по умолчанию.");
            else
                try
                {
                    int tokens = Int32.Parse(data);
                    if (tokens < 20 || tokens > 100)
                        throw new ApplicationException("Количество номерков, указанное в файле конфигурации, должно быть в пределах от 20 до 100.\n"
                            + "Будет использовано значение по умолчанию");
                    return tokens;
                }
                catch (FormatException ex)
                {
                    throw new ApplicationException("Неправильно указан формат числа количества номерков в файле конфигурации.\n"
                        + "Будет использовано значение по умолчанию", ex);
                }
                catch (OverflowException ex)
                {
                    throw new ApplicationException("Значение количества номерков в файле конфигурации является недопустимым.\n"
                        + "Будет использовано значение по умолчанию", ex);
                }

        }

        /// <summary>
        /// Обработчик события закрытия окна
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (this.IsLoaded)
            {
                MessageWindow window = new MessageWindow("Вы действительно хотите завершить работу программы?", MessageWindowImage.Exit, false);
                window.Owner = this;
                window.ShowDialog();
                if (window.DialogResult == true)
                {
                    e.Cancel = false;
                    if (ctx != null)
                    {
                        ctx.Dispose();
                    }
                }
                else
                    e.Cancel = true;
            }
        }
    }

    /// <summary>
    /// Представляет номерк и клиента, его арендовавшего
    /// </summary>
    public class VisitorItem : INotifyPropertyChanged
    {
        private TimeSpan? _LeftTime;
        private Visitor _Value;
        private Timer Timer = new Timer(500);

        public VisitorItem()
        {
            Timer.Elapsed += delegate
                {
                    if(Value != null)
                        LeftTime = Value.TimeStart.AddMinutes((double)Value.Paid)
                            .Subtract(DateTime.Now);
                };
        }

        public int Key
        { get; set; }

        public TimeSpan? LeftTime
        {
            get { return _LeftTime; }
            protected set
            {
                if (_LeftTime != value)
                {
                    if (value.HasValue && value.Value < new TimeSpan())
                        _LeftTime = new TimeSpan();
                    else
                        _LeftTime = value;
                    OnPropertyChanged("LeftTime");
                }
            }
        }

        public Visitor Value
        {
            get { return _Value; }
            set 
            {
                if (_Value != value)
                {
                    _Value = value;
                    OnPropertyChanged("Value");
                    if (Value == null && Timer.Enabled == true)
                    {
                        Timer.Stop();
                        LeftTime = null;
                    }
                    else if (Timer.Enabled == false)
                        Timer.Start();
                    
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        { 
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    /// <summary>
    /// Представляет коллекцию номерков и соответсвующих клиентов
    /// </summary>
    public class VisitorItems : ObservableCollection<VisitorItem>
    {
        private bool _initialized = false;

        public void Initialize(LabirintEntities ctx, int tokens)
        {
            try
            {
                if (!_initialized)
                {
                    var ActiveVisitors = ctx.Visitors.Where(i => i.TimeEnd == null);
                    for (int i = 1; i <= tokens; i++)
                    {
                        VisitorItem item = new VisitorItem()
                        {
                            Key = i,
                            Value = ActiveVisitors.SingleOrDefault(it => it.Check == i)
                        };
                        this.Add(item);
                    }
                    _initialized = true;
                }
                else
                    throw new ApplicationException("Данные уже были инициализированы");
            }
            catch (DataException ex)
            {
                throw new ApplicationException("Не удалось выполнить инициализацию данных.",
                    ex.InnerException);
            }
        }
    }

    /// <summary>
    /// Сортировщик номерков
    /// </summary>
    public class LeftTimeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            VisitorItem itemA = x as VisitorItem;
            VisitorItem itemB = y as VisitorItem;
            if(itemA.Value != null && itemB.Value == null)
                return -1;
            else if(itemA.Value == null && itemB.Value != null)
                return 1;
            else if (itemA.Value != null && itemB.Value != null)
            {
                return Nullable.Compare<TimeSpan>(itemA.LeftTime, itemB.LeftTime);
            }
            else
            {
                if (itemA.Key < itemB.Key)
                    return -1;
                else if (itemA.Key > itemB.Key)
                    return 1;
                else return 0;
            }
        }
    }
}
