using ModernComponents.System;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ConsoleClassTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Console.WindowWidth = 160;
            Console.WindowHeight = 50;
            Console.Out = Output;
            Console.In = Input;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Main();
        }

        private async void Main()
        {
            string line;
            await Console.WriteLine("Enter one or more lines of text:");
            await Console.WriteLine();
            do
            {
                await Console.Write(" ");
                line = await Console.ReadLine();
                if (line != null)
                {
                    await Console.WriteLine("-> " + line + "\a");
                }
            } while (line != null);
        }
    }
}
