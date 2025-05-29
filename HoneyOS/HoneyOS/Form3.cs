using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
            "open recycle bin please",              // create instance of recycle bin window
            "close notepad please",         // close all existing instance of notepad window
            "close file manager please",    // close all existing instance of file manager window
            "goodbye",                // close the notepad
        };

        bool isListeningForAction;          // if true, that means "honey" is already heard and the speech engine is now listening for a command
        bool topmost;                       // if true, that means this slide is currently interacted
        bool isListening;                   // if true, the speech engine is active
        SpeechRecognitionEngine recognizer;

        // Visual feedback components
        private PictureBox microphoneIcon;
        private Label speechTextLabel;
        private Panel speechPanel;
        private Timer micAnimationTimer;
        private bool micAnimationState = false;

        List<Form7> notepads = new List<Form7>();
        List<Form5> file_managers = new List<Form5>();
        List<Form6> task_manager = new List<Form6>();
        List<Form8> menu = new List<Form8>();       //delete this after
        List<Form4> recycle_bin = new List<Form4>();

        // Get the power status of the device
        PowerStatus ps = SystemInformation.PowerStatus;

        public Desktop()
        {
            // Initializes the form components
            InitializeComponent();
            isListeningForAction = false;
            isListening = false;

            // Initialize visual feedback components
            InitializeSpeechVisualFeedback();
        }

        private void InitializeSpeechVisualFeedback()
        {
            // Create speech feedback panel
            speechPanel = new Panel();
            speechPanel.Size = new Size(300, 80);
            speechPanel.Location = new Point(this.Width - 320, 20);
            speechPanel.BackColor = Color.FromArgb(180, 0, 0, 0); // Semi-transparent black
            speechPanel.BorderStyle = BorderStyle.FixedSingle;
            speechPanel.Visible = false;
            speechPanel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            this.Controls.Add(speechPanel);

            // Create microphone icon
            microphoneIcon = new PictureBox();
            microphoneIcon.Size = new Size(32, 32);
            microphoneIcon.Location = new Point(10, 24);
            microphoneIcon.SizeMode = PictureBoxSizeMode.StretchImage;
            microphoneIcon.BackColor = Color.Transparent;
            speechPanel.Controls.Add(microphoneIcon);

            // Create speech text label
            speechTextLabel = new Label();
            speechTextLabel.Location = new Point(50, 10);
            speechTextLabel.Size = new Size(240, 60);
            speechTextLabel.ForeColor = Color.White;
            speechTextLabel.BackColor = Color.Transparent;
            speechTextLabel.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            speechTextLabel.Text = "Listening...";
            speechTextLabel.TextAlign = ContentAlignment.MiddleLeft;
            speechPanel.Controls.Add(speechTextLabel);

            // Create microphone animation timer
            micAnimationTimer = new Timer();
            micAnimationTimer.Interval = 500; // 500ms for animation
            micAnimationTimer.Tick += MicAnimationTimer_Tick;

            // Set initial microphone icon (you can replace this with actual icon resources)
            SetMicrophoneIcon(false);
        }

        private void SetMicrophoneIcon(bool isActive)
        {
            // Create a simple microphone icon using graphics
            // You can replace this with actual icon files if you have them
            Bitmap micBitmap = new Bitmap(32, 32);
            using (Graphics g = Graphics.FromImage(micBitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                // Draw microphone shape
                Color micColor = isActive ? Color.LimeGreen : Color.Gray;
                Brush micBrush = new SolidBrush(micColor);

                // Microphone body
                g.FillEllipse(micBrush, 10, 4, 12, 16);
                // Microphone stand
                g.FillRectangle(micBrush, 15, 20, 2, 8);
                // Microphone base
                g.FillRectangle(micBrush, 12, 28, 8, 2);

                micBrush.Dispose();
            }
            microphoneIcon.Image = micBitmap;
        }

        private void MicAnimationTimer_Tick(object sender, EventArgs e)
        {
            micAnimationState = !micAnimationState;
            SetMicrophoneIcon(micAnimationState);
        }

        private void ShowSpeechFeedback(string status = "Listening...")
        {
            speechTextLabel.Text = status;
            speechPanel.Visible = true;
            speechPanel.BringToFront();

            if (isListening)
            {
                micAnimationTimer.Start();
            }
        }

        private void HideSpeechFeedback()
        {
            speechPanel.Visible = false;
            micAnimationTimer.Stop();
            SetMicrophoneIcon(false);
        }

        private void UpdateSpeechText(string text, bool isCommand = false)
        {
            if (isCommand)
            {
                speechTextLabel.ForeColor = Color.LightGreen;
                speechTextLabel.Text = $"Command: {text}";
            }
            else
            {
                speechTextLabel.ForeColor = Color.White;
                speechTextLabel.Text = $"Hearing: {text}";
            }
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
                    ShowSpeechFeedback("Ready to listen...");
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
                    HideSpeechFeedback();
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

            // Add all speech recognition event handlers
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
            recognizer.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(recognizer_SpeechHypothesized);
            recognizer.SpeechDetected += new EventHandler<SpeechDetectedEventArgs>(recognizer_SpeechDetected);
            recognizer.RecognizeCompleted += new EventHandler<RecognizeCompletedEventArgs>(recognizer_RecognizeCompleted);
            recognizer.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(recognizer_SpeechRejected);

            // Configure recognition settings for better responsiveness
            recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(5);
            recognizer.BabbleTimeout = TimeSpan.FromSeconds(4);
            recognizer.EndSilenceTimeout = TimeSpan.FromSeconds(1.5);
        }

        // Handle recognition completion
        private void recognizer_RecognizeCompleted(object sender, RecognizeCompletedEventArgs e)
        {
            if (speechPanel.Visible && isListening)
            {
                UpdateSpeechText("Listening...");
            }
        }

        // Handle rejected speech
        private void recognizer_SpeechRejected(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            if (speechPanel.Visible)
            {
                UpdateSpeechText("Speech not recognized");

                // Reset after a delay
                Timer resetTimer = new Timer();
                resetTimer.Interval = 1500;
                resetTimer.Tick += (s, ev) => {
                    if (speechPanel.Visible)
                    {
                        UpdateSpeechText("Listening...");
                    }
                    resetTimer.Stop();
                    resetTimer.Dispose();
                };
                resetTimer.Start();
            }
        }

        // New event handler for speech hypothesis (real-time text)
        private void recognizer_SpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
        {
            // Show what the speech engine thinks it's hearing in real-time
            if (speechPanel.Visible)
            {
                // Show all hypotheses, even with very low confidence
                UpdateSpeechText($"{e.Result.Text} ({e.Result.Confidence:P0})");
                Debug.WriteLine($"Hypothesized: {e.Result.Text} with confidence: {e.Result.Confidence:P0}");
            }
            if (e.Result.Confidence > 0.2 && isListeningForAction)
            {
                switch (e.Result.Text.ToLower()) // for each case, create a corresponding function
                {
                    case "open notepad please":
                        UpdateSpeechText("Opening Notepad...");
                        MessageBox.Show("Sure, i'll open it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OpenNotepadFunction();
                        isListeningForAction = false;
                        break;
                    case "open file manager please":
                        UpdateSpeechText("Opening File Manager...");
                        MessageBox.Show("Sure, i'll open it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OpenFileManagerFunction();
                        isListeningForAction = false;
                        break;
                    case "open recycle bin please":
                        UpdateSpeechText("Opening Recycle Bin...");
                        MessageBox.Show("Sure, i'll open it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OpenRecycleBinFunction();
                        isListeningForAction = false;
                        break;
                    case "close notepad please":
                        UpdateSpeechText("Closing Notepad...");
                        MessageBox.Show("Sure, i'll close it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CloseNotepadFunction();
                        isListeningForAction = false;
                        break;
                    case "close file manager please":
                        UpdateSpeechText("Closing File Manager...");
                        MessageBox.Show("Sure, i'll close it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CloseFileManagerFunction();
                        isListeningForAction = false;
                        break;
                    case "goodbye":
                        UpdateSpeechText("Goodbye...");
                        MessageBox.Show("Goodbye, honey", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ShutdownFunction();
                        isListeningForAction = false;
                        break;
                    default:
                        //indicate to UI that the command taken was not recognized
                        UpdateSpeechText("Command not recognized");
                        Timer resetTimer2 = new Timer();
                        resetTimer2.Interval = 1500;
                        resetTimer2.Tick += (s, ev) => {
                            UpdateSpeechText("Awaiting command...");
                            resetTimer2.Stop();
                            resetTimer2.Dispose();
                        };
                        resetTimer2.Start();
                        break;
                }
            }
        }

        // New event handler for speech detection
        private void recognizer_SpeechDetected(object sender, SpeechDetectedEventArgs e)
        {
            // Indicate that speech has been detected
            if (speechPanel.Visible)
            {
                UpdateSpeechText("Speech detected - processing...");

                // Set a timeout to reset if no recognition follows
                Timer timeoutTimer = new Timer();
                timeoutTimer.Interval = 3000; // 3 seconds timeout
                timeoutTimer.Tick += (s, ev) => {
                    if (speechPanel.Visible && speechTextLabel.Text.Contains("Speech detected"))
                    {
                        UpdateSpeechText("Listening...");
                    }
                    timeoutTimer.Stop();
                    timeoutTimer.Dispose();
                };
                timeoutTimer.Start();
            }
        }

        /* Speech Commands Functions */
        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Update visual feedback with recognized command first
            UpdateSpeechText($"Recognized: {e.Result.Text} ({e.Result.Confidence:P0})", true);
            Debug.WriteLine($"Recognized: {e.Result.Text} with confidence: {e.Result.Confidence:P0}");

            // Accept commands with very low confidence since we're using a limited grammar
            // The grammar restriction itself helps ensure accuracy
            if (e.Result.Text.ToLower() == "honey" && !isListeningForAction)
            {
                // indicate to UI that Beebot is listening
                MessageBox.Show("Hello dear, what can I do for you?", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isListeningForAction = true;
                UpdateSpeechText("Awaiting command...");
            }
            else if (isListeningForAction)
            {
                switch (e.Result.Text.ToLower()) // for each case, create a corresponding function
                {
                    case "open notepad please":
                        UpdateSpeechText("Opening Notepad...");
                        MessageBox.Show("Sure, i'll open it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OpenNotepadFunction();
                        isListeningForAction = false;
                        break;
                    case "open file manager please":
                        UpdateSpeechText("Opening File Manager...");
                        MessageBox.Show("Sure, i'll open it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OpenFileManagerFunction();
                        isListeningForAction = false;
                        break;
                    case "open recycle bin please":
                        UpdateSpeechText("Opening Recycle Bin...");
                        MessageBox.Show("Sure, i'll open it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OpenRecycleBinFunction();
                        isListeningForAction = false;
                        break;
                    case "close notepad please":
                        UpdateSpeechText("Closing Notepad...");
                        MessageBox.Show("Sure, i'll close it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CloseNotepadFunction();
                        isListeningForAction = false;
                        break;
                    case "close file manager please":
                        UpdateSpeechText("Closing File Manager...");
                        MessageBox.Show("Sure, i'll close it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CloseFileManagerFunction();
                        isListeningForAction = false;
                        break;
                    case "goodbye":
                        UpdateSpeechText("Goodbye...");
                        MessageBox.Show("Goodbye, honey", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ShutdownFunction();
                        isListeningForAction = false;
                        break;
                    default:
                        //indicate to UI that the command taken was not recognized
                        UpdateSpeechText("Command not recognized");
                        Timer resetTimer2 = new Timer();
                        resetTimer2.Interval = 1500;
                        resetTimer2.Tick += (s, ev) => {
                            UpdateSpeechText("Awaiting command...");
                            resetTimer2.Stop();
                            resetTimer2.Dispose();
                        };
                        resetTimer2.Start();
                        break;
                }

                // Reset to listening state after command execution
                if (!isListeningForAction)
                {
                    Timer resetTimer = new Timer();
                    resetTimer.Interval = 2000;
                    resetTimer.Tick += (s, ev) => {
                        if (isListening && speechPanel.Visible)
                        {
                            UpdateSpeechText("Listening...");
                        }
                        resetTimer.Stop();
                        resetTimer.Dispose();
                    };
                    resetTimer.Start();
                }
            }
            else
            {
                // Speech recognized but not in correct state
                UpdateSpeechText("Say 'Honey' first to activate");
                Timer resetTimer3 = new Timer();
                resetTimer3.Interval = 2000;
                resetTimer3.Tick += (s, ev) => {
                    if (speechPanel.Visible)
                    {
                        UpdateSpeechText("Listening...");
                    }
                    resetTimer3.Stop();
                    resetTimer3.Dispose();
                };
                resetTimer3.Start();
            }
        }

        // Make sure to dispose of resources when form closes
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            micAnimationTimer?.Stop();
            micAnimationTimer?.Dispose();
            recognizer?.Dispose();
            base.OnFormClosed(e);
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

        // Function that opens the Recycle Bin
        private void OpenRecycleBinFunction()
        {
            // fileManagerToolStripMenuItem.Visible = true;
            // Create an instance of Form5
            Form4 form4 = new Form4();
            recycle_bin.Add(form4);
            form4.Show();
        }

        // Function that closes the Notepad
        private void CloseNotepadFunction()
        {
            foreach (Form7 notepad in notepads)
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
        private void button5_Click(object sender, EventArgs e)
        {
            Form4 recycleBinForm = new Form4();
            recycleBinForm.Show();
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
            //Form6 form6 = new Form6(this);
            //task_manager.Add(form6);
            //form6.Show();
            MessageBox.Show("Please configure the system settings (Scheduling and Memory Management) first.", "Configuration Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
