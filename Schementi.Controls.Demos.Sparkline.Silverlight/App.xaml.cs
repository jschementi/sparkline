using System;
using System.Windows;

namespace Schementi.Controls.Demos.Sparkline.Silverlight {
    public partial class App {

        public App() {
            Startup += ApplicationStartup;
            Exit += ApplicationExit;
            UnhandledException += ApplicationUnhandledException;

            InitializeComponent();
        }

        private void ApplicationStartup(object sender, StartupEventArgs e) {
            RootVisual = new MainPage();
        }

        private void ApplicationExit(object sender, EventArgs e) {
        }

        private static void ApplicationUnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e) {
            if (System.Diagnostics.Debugger.IsAttached) return;
            e.Handled = true;
            Deployment.Current.Dispatcher.BeginInvoke(() =>  ReportErrorToDom(e));
        }

        private static void ReportErrorToDom(ApplicationUnhandledExceptionEventArgs e) {
            try {
                var errorMsg = e.ExceptionObject.Message + e.ExceptionObject.StackTrace;
                errorMsg = errorMsg.Replace('"', '\'').Replace("\r\n", @"\n");

                System.Windows.Browser.HtmlPage.Window.Eval("throw new Error(\"Unhandled Error in Silverlight Application " + errorMsg + "\");");
            } catch {
                return;
            }
        }
    }
}
