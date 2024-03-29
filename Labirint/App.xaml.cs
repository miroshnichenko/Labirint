﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Diagnostics;
using System.Windows.Controls.Primitives;

namespace Blasig.Labirint.GUI
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Получаем ссылку на текущий процесс
            Process thisProc = Process.GetCurrentProcess();
            // Проверить количество процессов с таким же именем
            if (Process.GetProcessesByName(thisProc.ProcessName).Length > 1)
            {
                MessageWindow wnd =
                    new MessageWindow("Программа уже запущена. Только одна копия программы может выполнятся в одно и то же время.", MessageWindowImage.Warning, true);
                wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                wnd.ShowDialog();
                Application.Current.Shutdown();
                return;
            }

            base.OnStartup(e);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            SimpleLogger.Log(e.Exception);
            MessageBox.Show("Было сгенерировано исключение, которое программа не смогла обработать. Сейчас произойдет аварийное завершение выполнения программы", 
                "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            this.Shutdown(-1);
        }
    }

    
}
