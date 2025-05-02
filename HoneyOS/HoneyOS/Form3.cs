using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Speech.Recognition;
using System.Windows.Forms;

namespace HoneyOS
{
    // Desktop Form
    public partial class Desktop : Form
    {
        List<string> phrases = new List<string>
        {
            /* command initializer */
            "honey",
            /* full commands */
            "open notepad please",          // create instance of notepad window
            "open file manager please",     // create instance of file manager window
            "close notepad please",         // close all existing instance of notepad window
            "close file manager please",    // close all existing instance of file manager window
            "goodbye",                // close the notepad
        };

        bool isListeningForAction;          // if true, that means "honey" is already heard and the speech engine is now listening for a command
        bool topmost;                       // if true, that means this slide is currently interacted
        bool isListening;                   // if true, the speech engine is active
        SpeechRecognitionEngine recognizer;

        List<Form7> notepads = new List<Form7>();
        List<Form5> file_managers = new List<Form5>();
        List<Form6> task_manager = new List<Form6>();
        List<Form8> menu = new List<Form8>();       //delete this after

        // Get the power status of the device
        PowerStatus ps = SystemInformation.PowerStatus;

        public Desktop()
        {
            // Initializes the form components
            InitializeComponent();
            isListeningForAction = false;
            isListening = false;
        }

        // Alex added global RecentFilePath 4/25/2025
        public static string RecentFilePath { get; set; } = ""; // Static field to store the recent file 

        private void Desktop_Load(object sender, EventArgs e)
        {
            // Disable the visibility of the notepad and filemanager icon on the taskbar
            notepadToolStripMenuItem.Visible = false;
            fileManagerToolStripMenuItem.Visible = false;

            
            BatteryTimer.Start();

            // Start a timer to call the update function periodically
            Timer updateTimer = new Timer();
            updateTimer.Interval = 1000; // 1000 milliseconds = 1 second
            updateTimer.Tick += (s, ev) => DesktopUpdate(); // Lambda expression to call the Update function
            updateTimer.Start();

            SpeechRecognition_Load();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            DesktopUpdate(); // Call the update function
        }
        public void DesktopUpdate()
        {
            // Update the current time displayed on the form
            label1.Text = DateTime.Now.ToShortTimeString();
            label2.Text = DateTime.Now.ToShortDateString();

            // Check whether Desktop is focused currently
            topmost = (Form.ActiveForm == this);
            if (topmost)
            {
                Desktop_GotFocus();
            }
            else
            {
                Desktop_LostFocus();
            }
        }
        private void Desktop_GotFocus()
        {
            // add stuff to do whenever the desktop is currently focused
            if (!isListening)
            {
                try
                {
                    isListening = true;
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);
                }
                catch (ObjectDisposedException)
                {

                }
            }
        }
        private void Desktop_LostFocus()
        {
            // add stuff to do whenever the desktop has lost focused ie another window is currently focused
            if (isListening)
            {
                try
                {
                    isListening = false;
                    recognizer.RecognizeAsyncStop();
                }
                catch (ObjectDisposedException)
                {

                }

            }
        }

        private void SpeechRecognition_Load()
        {
            //setup grammar
            Choices choices = new Choices(phrases.ToArray());
            GrammarBuilder builder = new GrammarBuilder(choices);
            Grammar grammar = new Grammar(builder);

            // initializing Speech Recognition
            recognizer = new SpeechRecognitionEngine();
            recognizer.SetInputToDefaultAudioDevice();
            recognizer.LoadGrammar(grammar);
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
        }

        /* Speech Commands Functions */
        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (e.Result.Confidence < 0.7)
            {
                //indicate to UI that Beebot has heard something that is included in the grammar, but is not confident enough
                MessageBox.Show("I'm sorry honey, I'm not sure I heard you clearly", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (e.Result.Text.ToLower() == "honey" && !isListeningForAction)
            {
                // indicate to UI that Beebot is listening
                MessageBox.Show("Hello dear, what can I do for you?", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isListeningForAction = true;
            }
            else if (isListeningForAction)
            {
                switch (e.Result.Text.ToLower()) // for each case, create a corresponding function
                {
                    case "open notepad please":
                        MessageBox.Show("Sure, i'll open it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OpenNotepadFunction();
                        isListeningForAction = false;
                        break;
                    case "open file manager please":
                        MessageBox.Show("Sure, i'll open it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OpenFileManagerFunction();
                        isListeningForAction = false;
                        break;
                    case "close notepad please":
                        MessageBox.Show("Sure, i'll close it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CloseNotepadFunction();
                        isListeningForAction = false;
                        break;
                    case "close file manager please":
                        MessageBox.Show("Sure, i'll close it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CloseFileManagerFunction();
                        isListeningForAction = false;
                        break;
                    case "goodbye":
                        MessageBox.Show("Goodbye, honey", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ShutdownFunction();
                        isListeningForAction = false;
                        break;
                    default:
                        //indicate to UI that the command taken was not recognized
                        break;
                }
            }

        }
        // Function that opens the Notepad
        private void OpenNotepadFunction()
        {
            notepadToolStripMenuItem.Visible = true;
            // Create an instance of Form7
            Form7 form7 = new Form7(this);
            notepads.Add(form7);
            form7.Show();
        }

        // Function that opens the File Manager
        private void OpenFileManagerFunction()
        {
            fileManagerToolStripMenuItem.Visible = true;
            // Create an instance of Form5
            Form5 form5 = new Form5(this);
            file_managers.Add(form5);
            form5.Show();
        }

        // Function that closes the Notepad
        private void CloseNotepadFunction()
        {
            foreach(Form7 notepad in notepads)
            {
                if (notepad.Visible)
                {
                    notepad.Hide();
                    notepad.Dispose();
                }
            }
            notepads.Clear();
        }

        // Function that closes the File Manager
        private void CloseFileManagerFunction()
        {
            foreach (Form5 fm in file_managers)
            {
                if (fm.Visible)
                {
                    fm.Hide();
                    fm.Dispose();
                }
            }
            file_managers.Clear();
        }
        // Function that when the Shutdown is clicked
        private void ShutdownFunction()
        {
            recognizer.Dispose();
            Application.Exit();
        }



        /* Click / MouseEnter / MouseLeave Functions */
        // Event handler when the Notepad Button is clicked
        private void button1_Click(object sender, EventArgs e)
        {
            OpenNotepadFunction();
        }
        private void button5_click(object sender, EventArgs e)
        {
            MessageBox.Show("Button5 clicked"); 
        }

        // Event handler when the Notepad button in the taskbar is clicked
        private void notepadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenNotepadFunction();
        }

        // Event handler when the Shutdown button is clicked
        private void shutdownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShutdownFunction();
        }

        // Function that hides the notepad on the taskbar
        public void HideNotepadToolStripMenuItem()
        {
            notepadToolStripMenuItem.Visible = false;
        }

        // Function that hides the file manager on the taskbar
        public void HideFileManagerToolStripMenuItem()
        {
            fileManagerToolStripMenuItem.Visible = false;
        }

        // Event handler when the File Manager is clicked
        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileManagerFunction();
        }

        // Event handler when the Task Manager is clicked
        private void button3_Click(object sender, EventArgs e)
        {
            Form6 form6 = new Form6(this);
            task_manager.Add(form6);
            form6.Show();
        }
        
        // Event handler when the Menu for Task Manager is clicked
        private void button4_Click(object sender, EventArgs e)
        {
            Form8 form8 = new Form8(this);
            menu.Add(form8);
            form8.Show();
        }

        // Event handle that sets how much the battery life of the device
        private void BatteryTimer_Tick(object sender, EventArgs e)
        {
            BatteryLife.Value = (int)(ps.BatteryLifePercent * 100);
        }
    }
}
