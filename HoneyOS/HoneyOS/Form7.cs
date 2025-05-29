using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Speech.Recognition;


namespace HoneyOS
{
    // Notepad Form
    public partial class Form7 : Form
    {
        string filePath = ""; //used to store file location 
        private Desktop desktopInstance; // Reference to an instance of Desktop form
        private bool isModified = false; // determines if text was modified
        private string oldText = "";
        private object form5;
        public string currentFile = "";
        public string currentPath = "";
        public bool isSaved = false;


        List<string> phrases = new List<string>
        {
            /* command initializer */
            "honey",
            /* full commands */
            "open new file please",         // open a text file
            "save this please",             // save the current text file
            "save as file please",         // save text file to another text file
            "close this please",            // close current notepad
            // additional commands (not yet implemented)
            "cut this please",              // cut text
            "copy this please",             // copy text
            "paste this please",            // paste cut/copied text
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

        public Form7(Desktop desktopInstance)
        {
            // Initilize the form components
            InitializeComponent();
            this.desktopInstance = desktopInstance; // Assign the reference to the instance of Desktop form
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
            speechPanel.Location = new Point(this.Width - 320, this.Height - 120);
            speechPanel.BackColor = Color.FromArgb(180, 0, 0, 0); // Semi-transparent black
            speechPanel.BorderStyle = BorderStyle.FixedSingle;
            speechPanel.Visible = false;
            speechPanel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
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

            // Set initial microphone icon
            SetMicrophoneIcon(false);
        }

        private void SetMicrophoneIcon(bool isActive)
        {
            // Create a simple microphone icon using graphics
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

        private void Form7_Load(object sender, EventArgs e)
        {

            // Start a timer to call the update function periodically
            Timer updateTimer = new Timer();
            updateTimer.Interval = 1000; // 1000 milliseconds = 1 second
            updateTimer.Tick += (s, ev) => Form7Update(); // Lambda expression to call the Update function
            updateTimer.Start();

            SpeechRecognition_Load();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            Form7Update(); // Call the update function
        }
        public void Form7Update()
        {
            // Check whether Desktop is focused currently
            topmost = (Form.ActiveForm == this);
            if (topmost){Notepad_GotFocus();}
            else{Notepad_LostFocus();}
        }
        private void Notepad_GotFocus()
        {
            // add stuff to do whenever the form is currently focused
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
        private void Notepad_LostFocus()
        {
            // add stuff to do whenever the form has lost focused ie another window is currently focused
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
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);

            // Add all speech recognition event handlers
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(recognizer_SpeechRecognized);
            recognizer.SpeechHypothesized += new EventHandler<SpeechHypothesizedEventArgs>(recognizer_SpeechHypothesized);
            recognizer.SpeechDetected += new EventHandler<SpeechDetectedEventArgs>(recognizer_SpeechDetected);
            recognizer.RecognizeCompleted += new EventHandler<RecognizeCompletedEventArgs>(recognizer_RecognizeCompleted);
            recognizer.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(recognizer_SpeechRejected);

            // Configure recognition settings for better responsiveness
            recognizer.InitialSilenceTimeout = TimeSpan.FromSeconds(3);
            recognizer.BabbleTimeout = TimeSpan.FromSeconds(2);
            recognizer.EndSilenceTimeout = TimeSpan.FromSeconds(1);
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
                // Lower confidence threshold and always show something
                if (e.Result.Confidence > 0.1)
                {
                    UpdateSpeechText($"{e.Result.Text} ({e.Result.Confidence:P0})");
                }
                else
                {
                    UpdateSpeechText("Processing speech...");
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
            UpdateSpeechText($"Recognized: {e.Result.Text}", true);

            if (e.Result.Text.ToLower() == "honey" && !isListeningForAction)
            {
                //indicate to UI that Beebot is listening
                MessageBox.Show("Hello dear, what can I do for you?", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                isListeningForAction = true;
                UpdateSpeechText("Awaiting command...");
            }
            else if (isListeningForAction)
            {
                string command = e.Result.Text.ToLower();

                switch (command) // for each case, create a corresponding function
                {
                    case "open new file please":
                        UpdateSpeechText("Opening new file...");
                        MessageBox.Show("Sure, i'll open one for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        OpenFileFunction();
                        isListeningForAction = false;
                        break;
                    case "save this please":
                        UpdateSpeechText("Saving...");
                        MessageBox.Show("Sure, i'll save it for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SaveFileFunction();
                        isListeningForAction = false;
                        break;
                    case "save as file please":
                        UpdateSpeechText("Saving file...");
                        MessageBox.Show("Sure, i'll save one for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        SaveFileAsFunction();
                        isListeningForAction = false;
                        break;
                    case "close this please":
                        UpdateSpeechText("Closing...");
                        MessageBox.Show("Sure, i'll close this for you dear", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                        isListeningForAction = false;
                        break;
                    default:
                        // Command not recognized
                        UpdateSpeechText("Command not recognized");
                        MessageBox.Show("I'm sorry, I didn't understand that command.", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

        // Function that opens the file manager
        private void OpenFileFunction()
        {
            Form5 fileManager = new Form5(desktopInstance);
            fileManager.Show();
            this.Close();
        }

        // Function that saves a file
        private void SaveFileFunction()
        {
            string CFilePath = Path.Combine(currentPath, currentFile);
            if (CFilePath != "")
            {
                File.WriteAllText(CFilePath, richTextBox1.Text);
            }
            else
            {
                SaveFileAsFunction();
            }
            save.Enabled = false;
            isSaved = true;
        }
        private void SaveFileAsFunction()
        {
            Form5 fileManager = new Form5(desktopInstance);

            // Subscribe to the SaveCompleted event
            fileManager.SaveCompleted += FileManager_SaveCompleted;

            fileManager.SetFileContent(richTextBox1.Text);

            fileManager.Show();
            fileManager.ShowSaveFilePanel();

            if (!fileManager.Visible) // Check if it's not visible after showing
            {
                fileManager.Close();
            }
            save.Enabled = false;
            isSaved = true;
        }
        private void CloseWindowFunction(object sender, FormClosingEventArgs e)
        {
            // MessageBox.Show("isModified: " + isModified + "\nOld Text: " + oldText + "\nCurrent Text: " + richTextBox1.Text);
            if (!isSaved && isModified)
            {
                // Display confirmation dialog
                DialogResult dialogResult = MessageBox.Show(
                  "The text has been modified. Do you want to save the changes?",
                  "Unsaved Changes",
                  MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    SaveFileFunction();
                }
                else if (dialogResult == DialogResult.No) { }
                else
                {
                    e.Cancel = true;
                }
            }
        }

        private void FileManager_SaveCompleted(object sender, EventArgs e)
        {
            if (sender is Form5 fileManager)
            {
                // Unsubscribe from the event
                fileManager.SaveCompleted -= FileManager_SaveCompleted;

                // Hide or close Form 5 after save is completed
                fileManager.Visible = false;
            }
        }
        /* Save Button: Click / MouseEnter / MouseLeave Functions */
        /* Changes the backcolor of the button */
        private void save_Click(object sender, EventArgs e)
        {
            save.BackColor = Color.FromArgb(255, 234, 177);
            SaveFileFunction();
        }
        private void save_MouseLeave(object sender, EventArgs e)
        {
            save.BackColor = Color.White;
        }
        private void save_MouseEnter(object sender, EventArgs e)
        {
            save.BackColor = Color.FromArgb(255, 243, 222);
        }

        /* Save As Button: Click / MouseEnter / MouseLeave Functions */
        /* Changes the backcolor of the button */
        private void saveAs_Click(object sender, EventArgs e)
        {
            saveAs.BackColor = Color.FromArgb(255, 234, 177);
            SaveFileAsFunction();
        }
        private void saveAs_MouseLeave(object sender, EventArgs e)
        {
            saveAs.BackColor = Color.White;
        }
        private void saveAs_MouseEnter(object sender, EventArgs e)
        {
            saveAs.BackColor = Color.FromArgb(255, 243, 222);
        }

        /* Open Button: Click / MouseEnter / MouseLeave Functions */
        /* Changes the backcolor of the button */
        private void open_Click(object sender, EventArgs e)
        {
            open.BackColor = Color.FromArgb(255, 234, 177);
            OpenFileFunction();
        }
        private void open_MouseLeave(object sender, EventArgs e)
        {
            open.BackColor = Color.White;
        }
        private void open_MouseEnter(object sender, EventArgs e)
        {
            open.BackColor = Color.FromArgb(255, 243, 222);
        }

        /* Cut Button: Click / MouseEnter / MouseLeave Functions */
        /* Changes the backcolor of the button */
        private void cut_Click(object sender, EventArgs e)
        {
            cut.BackColor = Color.FromArgb(255, 234, 177);
            richTextBox1.Cut();
        }
        private void cut_MouseLeave(object sender, EventArgs e)
        {
            cut.BackColor = Color.White;
        }
        private void cut_MouseEnter(object sender, EventArgs e)
        {
            cut.BackColor = Color.FromArgb(255, 243, 222);
        }

        /* Copy Button: Click / MouseEnter / MouseLeave Functions */
        /* Changes the backcolor of the button */
        private void copy_Click(object sender, EventArgs e)
        {

            copy.BackColor = Color.FromArgb(255, 234, 177);
            richTextBox1.Copy();
        }
        private void copy_MouseLeave(object sender, EventArgs e)
        {
            copy.BackColor = Color.White;
        }
        private void copy_MouseEnter(object sender, EventArgs e)
        {
            copy.BackColor = Color.FromArgb(255, 243, 222);
        }

        /* Paste Button: Click / MouseEnter / MouseLeave Functions */
        /* Changes the backcolor of the button */
        private void paste_Click(object sender, EventArgs e)
        {
            paste.BackColor = Color.FromArgb(255, 234, 177);
            richTextBox1.Paste();
        }
        private void paste_MouseLeave(object sender, EventArgs e)
        {
            paste.BackColor = Color.White;
        }
        private void paste_MouseEnter(object sender, EventArgs e)
        {
            paste.BackColor = Color.FromArgb(255, 243, 222);
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            if (richTextBox1.Text != oldText)
            {
                isModified = true;
            }

            if (richTextBox1.Text.Length > 0)
            {
                copy.Enabled = true;
                cut.Enabled = true;
                saveAs.Enabled = true;
                save.Enabled = true;
            }
            else
            {
                copy.Enabled = false;
                cut.Enabled = false;
                saveAs.Enabled = false;
                save.Enabled = false;
            }
        }
        // Event handler when the Form7 (Notepad) is closed
        private void Form7_FormClosed(object sender, FormClosedEventArgs e)
        {
            recognizer.Dispose();
            desktopInstance?.HideNotepadToolStripMenuItem(); // Call the method to hide notepadToolStripMenuItem on Desktop form
        }

        /* New Window Button: Click / MouseEnter / MouseLeave Functions */
        /* Changes the backcolor of the button */
        private void newWindow_Click(object sender, EventArgs e)
        {
            newWindow.BackColor = Color.FromArgb(255, 234, 177);
            // MessageBox.Show("isModified: " + isModified + "\nOld Text: " + oldText + "\nCurrent Text: " + richTextBox1.Text);
            if (!isSaved && isModified)
            {
                // Display confirmation dialog
                DialogResult dialogResult = MessageBox.Show(
                  "The text has been modified. Do you want to save the changes?",
                  "Unsaved Changes",
                  MessageBoxButtons.YesNo);

                if (dialogResult == DialogResult.Yes)
                {
                    // Implement logic to save changes
                    isModified = false; // Reset flag after saving
                    string CFilePath = Path.Combine(currentPath, currentFile);
                    if (CFilePath != "")
                    {
                        File.WriteAllText(CFilePath, richTextBox1.Text);
                    }
                    else
                    {
                        Form5 fileManager = new Form5(desktopInstance);

                        // Subscribe to the SaveCompleted event
                        fileManager.SaveCompleted += FileManager_SaveCompleted;

                        fileManager.SetFileContent(richTextBox1.Text);

                        fileManager.Show();
                        fileManager.ShowSaveFilePanel();

                        if (!fileManager.Visible) // Check if it's not visible after showing
                        {
                            fileManager.Close();
                        }
                    }
                }
                else if (dialogResult == DialogResult.No) {
                    filePath = "";
                    richTextBox1.Text = "";
                }
            }
        }

        private void newWindow_MouseEnter(object sender, EventArgs e)
        {
            newWindow.BackColor = Color.FromArgb(255, 243, 222);
        }
        private void newWindow_MouseLeave(object sender, EventArgs e)
        {
            newWindow.BackColor = Color.White;
        }

        public void openFile(string filePath)
        {
            using (StreamReader sr = new StreamReader(filePath))
            {
                string fileContent = sr.ReadToEnd();
                richTextBox1.Text = fileContent;
                oldText = richTextBox1.Text;
                isModified = false;
            }
        }

        private void Form7_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseWindowFunction(sender,e);
        }
    }
}
