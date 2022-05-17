using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics.Display;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;


namespace ModernComponents.System
{
    /// <summary>
    ///     Represents an implementation of the system console for Windows Runtime applications. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         A console is a text-based interface where you can interact with the operating system or with text-based applications by 
    ///         entering text input through the computer keyboard, and reading text output from the computer screen. This Console class 
    ///         provides basic support for Windows 8 Metro applications that read characters from, and write characters to, a console.
    ///     </para>
    /// </remarks>
    /// <example>
    ///     <include file="Example.txt" path="//code[@id=0]" />
    ///     <include file="Example.txt" path="//code[@id=1]" />
    /// </example>
    public static class Console
    {
        private static StringBuilder OutputText;
        private static List<char> CurrentLine;
        private static List<List<char>> CurrentOutput;
        private static string InputText;
        private static TextBlock output;
        private static TextBox input;
        private static int windowWidth;
        private static int windowHeight;
        private static bool ReadingLine;
        private static int CharsInLine;
        private static int CurrentCharsInLine;
        private static int LinesInOutput;
        private static int CurrentLinesInOutput;
        
        /// <summary>
        ///     Gets or sets the output of the Console.
        /// </summary>
        /// <value>
        ///     <para>Type: <see cref="TextBlock"/></para>
        ///     <para>A <see cref="TextBlock"/> that represents the output of the console.</para>
        /// </value>
        /// <remarks>
        ///     <para>You need to set this property before calling <see cref="Write(object)"/> or <see cref="WriteLine()"/> methods. Currently this property 
        ///     only accepts a <see cref="TextBlock"/> as its value.</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=0]" />
        /// </example>
        [CLSCompliantAttribute(false)]
        public static TextBlock Out
        {
            get
            {
                return output;
            }
            set
            {
                output = value;
                output.SizeChanged += Out_SizeChanged;
                if(WindowWidth <= 0)
                {
                    WindowWidth = 80;
                }
                if (WindowHeight <= 0)
                {
                    WindowHeight = 25;
                }
                CurrentLinesInOutput = 0;
                CurrentOutput = new List<List<char>>();
                InsertNewLine();
                output.FontFamily = new FontFamily("Consolas");
                output.TextWrapping = TextWrapping.NoWrap;
                OutputText = new StringBuilder("");
                output.Text = OutputText.ToString();
                DisplayProperties.OrientationChanged += Page_OrientationChanged;
                
            }
        }

        /// <summary>
        ///     Gets or sets the input of the console.
        /// </summary>
        /// <value>
        ///     <para>Type: <see cref="TextBox"/></para>
        ///     <para>A <see cref="TextBox"/> that represents the input of the console.</para>
        /// </value>
        /// <remarks>
        ///     <para>You need to set this property before calling <see cref="ReadLine"/> method. Currently this property 
        ///     only accepts a <see cref="TextBox"/> as its value.</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=0]" />
        /// </example>
        [CLSCompliantAttribute(false)]
        public static TextBox In
        {
            get
            {
                return input;
            }
            set
            {
                input = value;
                input.Text = "";
                input.KeyDown += Input_KeyDown;
            }
        }

        /// <summary>
        ///     Gets or sets the width of the console.
        /// </summary>
        /// <value>
        ///     <para>Type: <see cref="int"/></para>
        ///     <para>The width of the console measured in columns.</para>
        /// </value>
        /// <remarks>
        ///     <para>The WindowWidth property defines the number of columns that are actually displayed in the console output 
        ///     control at any particular time.</para>
        ///     <para>If the <see cref="BufferWidth"/> property is not set, it will take the value of this property.</para>
        /// </remarks>
        /// <example>
        ///     <para>This example demonstrates the WindowWidth and WindowHeight properties.</para>
        ///     <para>The default dimensions of the console (80 columns x 25 rows) are doubled.</para>
        ///     <include file="Example.txt" path="//code[@id=0]" />
        /// </example>
        public static int WindowWidth
        {
            get
            {
                return windowWidth;
            }
            set
            {
                if(value > 0)
                {
                    windowWidth = value;
                }else{
                    windowWidth = 80;
                }
                
                if (BufferWidth <= 0)
                {
                    BufferWidth = windowWidth;
                }
                try
                {
                    SetFontSize(Out.RenderSize.Width, Out.RenderSize.Height);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        ///     Gets or sets the height of the console area.
        /// </summary>
        /// <value>
        ///     <para>Type: <see cref="int"/></para>
        ///     <para>The height of the console measured in rows.</para>
        /// </value>
        /// <remarks>
        ///     <para>The WindowHeight property defines the number of rows that are actually displayed in the console output 
        ///     control at any particular time.</para>
        ///     <para>If the <see cref="BufferHeight"/> property is not set, it will take the value of this property.</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=0]" />
        /// </example>
        public static int WindowHeight
        {
            get
            {
                return windowHeight;
            }
            set
            {
                if (value > 0)
                {
                    windowHeight = value;
                }
                else
                {
                    windowHeight = 25;
                }
                if (BufferHeight <= 0)
                {
                    BufferHeight = windowHeight;
                }
                try
                {
                    SetFontSize(Out.RenderSize.Width, Out.RenderSize.Height);
                }
                catch
                {
                }
            }
        }

        /// <summary>
        ///     Gets or sets the width of the console's buffer area.
        /// </summary>
        /// <value>
        ///     <para>Type: <see cref="int"/></para>
        ///     <para>The width of the console's buffer area measured in columns.</para>
        /// </value>
        /// <remarks>
        ///     <para>This property defines the number of columns stored in the buffer of the console output control. In 
        ///     contrast, the WindowWidth property defines the number of columns that are actually displayed in the console output 
        ///     control at any particular time. If the number of columns actually written to the buffer exceeds the number of columns 
        ///     defined by the WindowWidth property, the window can be scrolled horizontally so that it displays a contiguous number 
        ///     of columns that are equal to the WindowWidth property and are located anywhere in the buffer.</para>
        ///     
        ///     <para>Note, that to allow scrolling of the output control, it must be inside a ScrollViewer with its 
        ///     HorizontalScrollBarVisibility property set to Visible</para>
        /// 
        ///     <para>If the BufferWidth property is decreased, the rightmost columns are removed. For example, 
        ///     if the number of columns is reduced from 80 to 60, columns 60 through 79 of each row are removed.</para>
        /// </remarks>
        /// <example>
        ///     <para>This example demonstrates the BufferWidth and BufferHeight properties.</para>
        ///     <para>The default dimensions of the console buffer (80 columns x 25 rows) are doubled.</para>
        ///     <include file="Example.txt" path="//code[@id=3]" />
        /// </example>
        public static int BufferWidth
        {
            get
            {
                return CharsInLine;
            }
            set
            {
                if (value > 0)
                {
                    CharsInLine = value;
                }
                else
                {
                    CharsInLine = 80;
                }
                try
                {
                    CreateOutputText();
                    Out.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, (() => { WriteDelegate(null, null); }));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        ///     Gets or sets the height of the console's buffer area area.
        /// </summary>
        /// <value>
        ///     <para>Type: <see cref="int"/></para>
        ///     <para>The height of the console's buffer area measured in rows.</para>
        /// </value>
        /// <remarks>
        ///     <para>This property defines the number of rows (or lines) stored in the buffer of the console output control. In 
        ///     contrast, the WindowHeight property defines the number of rows that are actually displayed in the console output 
        ///     control at any particular time. If the number of rows actually written to the buffer exceeds the number of rows 
        ///     defined by the WindowHeight property, the window can be scrolled vertically so that it displays a contiguous number 
        ///     of rows that are equal to the WindowHeight property and are located anywhere in the buffer.</para>
        ///     
        ///     <para>Note, that to allow scrolling of the output control, it must be inside a ScrollViewer with its VerticalScrollBarVisibility 
        ///     property set to Visible</para>
        ///     
        ///     <para>If a set operation decreases the value of the BufferHeight property, the uppermost lines are removed. For example, 
        ///     if the number of lines is reduced from 300 to 250, lines 0 through 49 are removed, and the existing lines 50 through 299 
        ///     become lines 0 through 249.</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=3]" />
        /// </example>
        public static int BufferHeight
        {
            get
            {
                return LinesInOutput;
            }
            set
            {
                if (value > 0)
                {
                    LinesInOutput = value;
                }
                else
                {
                    LinesInOutput = 25;
                }
                try
                {
                    CreateOutputText();
                    Out.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, (() => { WriteDelegate(null, null); }));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        ///     Terminates the current line and inserts a new one to the output of the console.
        /// </summary>
        /// <returns>
        ///     <para>Type: <see cref="Task"/></para>
        ///     <para>An awaitable <see cref="Task"/></para>
        /// </returns>
        /// <remarks>
        ///     The line terminator currently is a string whose value is a line feed ("\n"). Currently this value can't be changed.
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=1]" />
        /// </example>
        public async static Task WriteLine()
        {
            await WriteLine("");
        }

        /// <summary>
        ///     Writes the text representation of the specified object, then terminates the current line and inserts a new one to the output 
        ///     of the console.
        /// </summary>
        /// <param name="value">
        ///     <para>The value to write</para>
        /// </param>
        /// <returns>
        ///     <para>Type: <see cref="Task"/></para>
        ///     <para>An awaitable <see cref="Task"/></para>
        /// </returns>
        /// <remarks>
        ///     <para>If value is null, the current line is terminated. Otherwise, the ToString method of value is called to produce its 
        ///     string representation, and the resulting string is written to the output of the console.</para>
        ///     <para>For more information about the line terminator, see the Remarks section of the <see cref="WriteLine()"/> method without 
        ///     parameters.</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=1]" />
        /// </example>
        public async static Task WriteLine(object value)
        {
            await WriteLine("{0}", value);
        }

        /// <summary>
        ///     Writes the specified string value, then terminates the current line and inserts a new one to the output 
        ///     of the console.
        /// </summary>
        /// <param name="value">
        ///     <para>The value to write</para>
        /// </param>
        /// <returns>
        ///     <para>Type: <see cref="Task"/></para>
        ///     <para>An awaitable <see cref="Task"/></para>
        /// </returns>
        /// <remarks>
        ///     <para>If value is null, the current line is terminated.</para>
        ///     <para>For more information about the line terminator, see the Remarks section of the <see cref="WriteLine()"/> method without 
        ///     parameters.</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=1]" />
        /// </example>
        public async static Task WriteLine(string value)
        {
            await WriteLine("{0}", value);
        }

        /// <summary>
        ///     Writes the text representation of the specified array of objects, then terminates the current line and inserts a new one to 
        ///     the output of the console, using the specified format information.
        /// </summary>
        /// <param name="format">
        ///     <para>A composite format string (see Remarks).</para>
        /// </param>
        /// <param name="args">
        ///     <para>An array of objects to write using <paramref name="format"/>.</para>
        /// </param>
        /// <returns>
        ///     <para>Type: <see cref="Task"/></para>
        ///     <para>An awaitable <see cref="Task"/></para>
        /// </returns>
        /// <remarks>
        ///     <para>This method uses the composite formatting feature of the .NET Framework to convert the value of an object to its text representation and embed that representation in a string. The resulting string is written to the output stream.</para>
        ///     
        ///     <para>The format parameter consists of zero or more runs of text intermixed with zero or more indexed placeholders, called format items, that correspond to an object in the parameter list of this method. The formatting process replaces each format item with the text representation of the value of the corresponding object.</para>
        ///     
        ///     <para>The syntax of a format item is {index[,alignment][:formatString]}, which specifies a mandatory index, the optional length and alignment of the formatted text, and an optional string of format specifier characters that govern how the value of the corresponding object is formatted.</para>
        ///     
        ///     <para>The .NET Framework provides extensive formatting support, which is described in greater detail in the following formatting topics.</para>
        ///     
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 For more information about the composite formatting feature supported by methods such as Format, AppendFormat, and some overloads of WriteLine, see Composite Formatting.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 For more information about numeric format specifiers, see Standard Numeric Format Strings and Custom Numeric Format Strings.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 For more information about date and time format specifiers, see Standard Date and Time Format Strings and Custom Date and Time Format Strings.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 For more information about enumeration format specifiers, see Enumeration Format Strings.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 For more information about formatting, see Formatting Types.
        ///             </description>
        ///         </item>
        ///     </list>
        ///         
        ///     <para>For more information about the line terminator, see the Remarks section of the <see cref="WriteLine()"/> method without 
        ///     parameters.</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=1]" />
        /// </example>
        public async static Task WriteLine(string format, params object[] args)
        {
            await Write(format + "\n", args);
        }

        /// <summary>
        ///     Writes the text representation of the specified object to the output 
        ///     of the console.
        /// </summary>
        /// <param name="value">
        ///     <para>The value to write</para>
        /// </param>
        /// <returns>
        ///     <para>Type: <see cref="Task"/></para>
        ///     <para>An awaitable <see cref="Task"/></para>
        /// </returns>
        /// <remarks>
        ///     <para>If value is null, nothing is written. Otherwise, the ToString method of value is called to produce its 
        ///     string representation, and the resulting string is written to the output of the console.</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=1]" />
        /// </example>
        public async static Task Write(object value)
        {
            await Write("{0}", value);
        }

        /// <summary>
        ///     Writes the text representation of the specified array of objects using the specified format information.
        /// </summary>
        /// <param name="format">
        ///     <para>A composite format string (see Remarks).</para>
        /// </param>
        /// <param name="args">
        ///     <para>An array of objects to write using <paramref name="format"/>.</para>
        /// </param>
        /// <returns>
        ///     <para>Type: <see cref="Task"/></para>
        ///     <para>An awaitable <see cref="Task"/></para>
        /// </returns>
        /// <remarks>
        ///     <para>This method uses the composite formatting feature of the .NET Framework to convert the value of an object to its text representation and embed that representation in a string. The resulting string is written to the output stream.</para>
        ///     
        ///     <para>The format parameter consists of zero or more runs of text intermixed with zero or more indexed placeholders, called format items, that correspond to an object in the parameter list of this method. The formatting process replaces each format item with the text representation of the value of the corresponding object.</para>
        ///     
        ///     <para>The syntax of a format item is {index[,alignment][:formatString]}, which specifies a mandatory index, the optional length and alignment of the formatted text, and an optional string of format specifier characters that govern how the value of the corresponding object is formatted.</para>
        ///     
        ///     <para>The .NET Framework provides extensive formatting support, which is described in greater detail in the following formatting topics.</para>
        ///     
        ///     <list type="bullet">
        ///         <item>
        ///             <description>
        ///                 For more information about the composite formatting feature supported by methods such as Format, AppendFormat, and some overloads of WriteLine, see Composite Formatting.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 For more information about numeric format specifiers, see Standard Numeric Format Strings and Custom Numeric Format Strings.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 For more information about date and time format specifiers, see Standard Date and Time Format Strings and Custom Date and Time Format Strings.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 For more information about enumeration format specifiers, see Enumeration Format Strings.
        ///             </description>
        ///         </item>
        ///         <item>
        ///             <description>
        ///                 For more information about formatting, see Formatting Types.
        ///             </description>
        ///         </item>
        ///     </list>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=2]" />
        /// </example>
        public async static Task Write(string format, params object[] args)
        {
            lock (OutputText)
            {
                char[] TempText = string.Format(format, args).ToCharArray();
                for (int i = 0; i < TempText.Length; i++)
                {
                    /*if (TempText[i] == 0) //Null Character (\0)
                    {

                    }
                    else*/
                    if (TempText[i] == 7) //Bell (\a)
                    {
                        RingBell();
                    }
                    else if (TempText[i] == 8) //Backspace (\b)
                    {
                        CurrentCharsInLine--;
                    }
                    else if (TempText[i] == 9) //Horizontal Tab (\t)
                    {
                        int CurrentTabPosition = CurrentCharsInLine / 8;
                        CurrentTabPosition++;
                        CurrentCharsInLine = CurrentTabPosition * 8;
                    }
                    else if (TempText[i] == 10) //Line Feed (\n)
                    {
                        CurrentCharsInLine = CharsInLine;
                    }
                    else if (TempText[i] == 11) //Vertical Tab (\v)
                    {
                        int CurrentTabLines = CurrentLinesInOutput % 6;
                        int NumberOfNewLines = 6 - CurrentTabLines;
                        for (int j = 0; j <= NumberOfNewLines; j++)
                        {
                            InsertNewLine();
                        }
                        CurrentCharsInLine--;
                    }
                    else if (TempText[i] == 12) //Form Feed (\f)
                    {
                        int NumberOfNewLines = LinesInOutput - CurrentLinesInOutput;
                        for (int j = 0; j <= NumberOfNewLines; j++)
                        {
                            InsertNewLine();
                        }
                        CurrentCharsInLine = 0;
                    }
                    else if (TempText[i] == 13) //Carriage Return (\r)
                    {
                        CurrentCharsInLine = 0;
                    }
                    /*else if (TempText[i] == 27) //Escape (\e)
                    {
                        
                    }*/
                    else
                    {
                        try
                        {
                            CurrentLine.Add(TempText[i]);
                        }
                        catch
                        {
                        }
                        CurrentCharsInLine++;
                    }
                    if (CurrentCharsInLine >= CharsInLine)
                    {
                        InsertNewLine();
                        CurrentCharsInLine = 0;
                    }
                }
                CreateOutputText();
            }
            try
            {
                await Out.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, (() => { WriteDelegate(format, args); }));
            }
            catch
            {
                //ExceptionOnWrite = true;
            }
        }

        /// <summary>
        ///     Clears the console buffer and corresponding console output control of display information.
        /// </summary>
        /// <returns>
        ///     <para>Type: <see cref="Task"/></para>
        ///     <para>An awaitable <see cref="Task"/></para>
        /// </returns>
        /// <remarks>
        ///     <para>When the Clear method is called, the cursor automatically scrolls to the top-left corner of the window and the 
        ///     contents of the screen buffer are set to blank.</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=1]" />
        /// </example>
        public async static Task Clear()
        {
            lock (OutputText)
            {
                CurrentLinesInOutput = 0;
                CurrentLine.Clear();
                CurrentOutput.Clear();
                OutputText.Clear();
                InsertNewLine();
                CurrentCharsInLine = 0;
            }
            try
            {
                await Out.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, (() => { WriteDelegate(null, null); }));
            }
            catch
            {
                //ExceptionOnWrite = true;
            }
        }

        /// <summary>
        ///     Plays the sound of a beep through the default sound device.
        /// </summary>
        /// <returns>
        ///     <para>Type: <see cref="Task"/></para>
        ///     <para>An awaitable <see cref="Task"/></para>
        /// </returns>
        /// <remarks>
        ///     <para>The beep plays at a frequency of 800 hertz for a duration of 400 milliseconds.</para>
        /// </remarks>
        /// <example>
        ///     <para>The following example demonstrates the Beep method. The example beeps once to warn the user that it's 
        ///     accepting input and beeps once the program has been finished and no longer accepts any user's input.</para>
        ///     <include file="Example.txt" path="//code[@id=2]" />
        /// </example>
        public static async Task Beep()
        {
            await RingBell();
        }

        private static void CreateOutputText()
        {
            OutputText.Clear();
            for (int i = (CurrentOutput.Count > LinesInOutput ? CurrentOutput.Count - LinesInOutput : 0); i < CurrentOutput.Count; i++)
            {
                List<char> TempLine = CurrentOutput.ElementAt(i);
                StringBuilder OutputLine = new StringBuilder();
                for (int j = 0; j < TempLine.Count && j < CharsInLine; j++)
                {
                    OutputLine.Append(new string(TempLine.ElementAt(j), 1));
                }
                OutputText.AppendLine(OutputLine.ToString());
            }
        }

        private static void WriteDelegate(string format, params object[] args)
        {
            Out.Text = OutputText.ToString();
        }

        /// <summary>
        ///     Reads the next line of characters from the input.
        /// </summary>
        /// <returns>
        ///     <para>Type: <see cref="Task{string}"/></para>
        ///     <para>An awaitable <see cref="Task"/>, who includes the next line of text from the input, or an empty string ("") if there is no 
        ///     text on the input.</para>
        /// </returns>
        /// <remarks>
        ///     <para>A line is defined as a sequence of characters followed by the press of the Enter Key. The returned string does not contain 
        ///     the equivalent character of the ending key.</para>
        ///     
        ///     <para>If no text is entered when the method is reading from the input control, the method returns an empty string ("").</para>
        /// </remarks>
        /// <example>
        ///     <include file="Example.txt" path="//code[@id=1]" />
        /// </example>
        public static async Task<string> ReadLine()
        {
            ReadingLine = true;
            In.Focus(Windows.UI.Xaml.FocusState.Programmatic);

            do
            {
                await Task.Delay(100);
            } while (ReadingLine);
            return InputText;
        }

        private static void Input_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (ReadingLine)
            {
                /*if (ControlPressed)
                {
                    if (e.Key == Windows.System.VirtualKey.Z)
                    {
                        In.Text += "^z";
                    }
                }*/
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    InputText = In.Text;
                    if (ReadingLine)
                    {
                        WriteLine(InputText);
                    }
                    else
                    {
                        Write(InputText);
                    }
                    ReadingLine = false;
                    In.Text = "";
                }
                /*if (e.Key == Windows.System.VirtualKey.Control)
                {
                    ControlPressed = true;
                }*/
            }
        }

        /*private void Input_KeyUp(object sender, Windows.UI.Xaml.Input.KeyEventArgs e)
        {
            if (ReadingLine)
            {
                if (e.Key == Windows.System.VirtualKey.Control)
                {
                    ControlPressed = false;
                }
            }
        }*/

        private static void Page_OrientationChanged(object sender)
        {
            SetFontSize(Out.ActualWidth, Out.ActualHeight);
        }

        private static void Out_SizeChanged(object sender, SizeChangedEventArgs args)
        {
            double Width = args.NewSize.Width;
            double Height = args.NewSize.Height;
            try
            {
                Control ParentViewer = (Control)Out.Parent;
                if (ParentViewer.RenderSize.Width < Width)
                {
                    Width = ParentViewer.RenderSize.Width;
                }
                if (ParentViewer.RenderSize.Height < Height)
                {
                    Height = ParentViewer.RenderSize.Height;
                }
            }
            catch
            {
            }
            SetFontSize(Width, Height);
        }

        private static void SetFontSize(double Width, double Height)
        {
            
            DisplayOrientations Orientation = DisplayProperties.CurrentOrientation;
            double newFontSize = 0;
            double aspectRatio = 0;
            double textAspectRatio = (double)WindowWidth / (double)WindowHeight;
            switch (Orientation)
            {
                case DisplayOrientations.None:
                case DisplayOrientations.Landscape:
                case DisplayOrientations.LandscapeFlipped:
                    aspectRatio = Width / Height;
                    if (aspectRatio >= 1.6)
                    {
                        if (textAspectRatio >= 3.8)
                        {
                            newFontSize = ((23 * 95) / (double)WindowWidth) * (Height / 730);
                        }
                        else
                        {
                            newFontSize = ((23 * 25) / (double)WindowHeight) * (Height / 730);
                        }
                    }
                    else
                    {
                        if(textAspectRatio >= 3.2)
                        {
                            newFontSize = ((23 * 80) / (double)WindowWidth) * (Width / 1024);
                        }else{
                            newFontSize = ((23 * 25) / (double)WindowHeight) * (Width / 1024);
                        }
                        
                    }
                    output.FontSize = (newFontSize) == 0 ? 23 : (int)newFontSize;
                    break;
                case DisplayOrientations.Portrait:
                case DisplayOrientations.PortraitFlipped:
                    aspectRatio = Width / Height;
                    if (aspectRatio >= 0.7)
                    {
                        if (textAspectRatio >= 1.7)
                        {
                            newFontSize = ((17 * 80) / (double)WindowWidth) * (Width / 768);
                        }
                        else
                        {
                            newFontSize = ((17 * 45) / (double)WindowHeight) * (Width / 768);
                        }
                    }
                    else
                    {
                        if (textAspectRatio >= 1.4)
                        {
                            newFontSize = ((17 * 80) / (double)WindowWidth) * (Width / 768);
                        }
                        else
                        {
                            newFontSize = ((17 * 55) / (double)WindowHeight) * (Width / 768);
                        }
                    }
                    output.FontSize = (newFontSize) == 0 ? 17 : newFontSize;
                    break;
                default:
                    output.FontSize = 23;
                    break;
            }
        }

        private static void InsertNewLine()
        {
            CurrentLine = new List<char>();
            CurrentOutput.Add(CurrentLine);
            if (CurrentOutput.Count > LinesInOutput)
            {
                CurrentOutput.RemoveAt(0);
            }
            CurrentLinesInOutput++;
            if (CurrentLinesInOutput > LinesInOutput)
            {
                CurrentLinesInOutput = 1;
            }
        }

        private async static Task RingBell()
        {
            MediaElement bellPlayer = new MediaElement();
            bellPlayer.AutoPlay = false;
            
            InMemoryRandomAccessStream audioStream = new InMemoryRandomAccessStream();
            Stream resourceStream = typeof(Console).GetTypeInfo().Assembly.GetManifestResourceStream("ModernComponents.System.sounds.beep.wav");
            await resourceStream.CopyToAsync(audioStream.AsStreamForWrite());
            audioStream.Seek(0);
            bellPlayer.SetSource(audioStream, "audio/wav");
            
            do
            {
                await Task.Delay(100);
            } while (bellPlayer.CurrentState == MediaElementState.Opening);
            bellPlayer.Play();
        }
    }
}
