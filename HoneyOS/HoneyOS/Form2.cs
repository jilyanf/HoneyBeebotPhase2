using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Speech.Recognition;
using System.Reflection;
using System.Diagnostics;

namespace HoneyOS
{
    // Welcome Screen Form
    public partial class WelcomeScreen : Form
    {

        SpeechRecognitionEngine recognizer; // Speech recognition engine instance
        bool topmost, isListening;  // Flags to track the form's focus and listening state

        // Visual feedback components
        private PictureBox microphoneIcon;
        private Label speechTextLabel;
        private Panel speechPanel;
        private Timer micAnimationTimer;
        private bool micAnimationState = false;
        public WelcomeScreen()
        {
            InitializeComponent();  // Initializes the form components
            TransparentButton.Hide();   // Hides the transparent button initially
            timer1.Start(); // Starts the timer when the form is created

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

        private void Form2_Load(object sender, EventArgs e)
        {
            recognizer = new SpeechRecognitionEngine(); // Initializing Speech Recognition
            recognizer.SetInputToDefaultAudioDevice();  // Sets the default audio device as input
            Grammar grammar = new Grammar(new GrammarBuilder(new Choices("hello honey")));  // Defines grammar for recognition
            recognizer.LoadGrammar(grammar);    // Loads the grammar into the recognizer

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

            // Timer to periodically update the form's state
            Timer updateTimer = new Timer();
            updateTimer.Interval = 1000; // 1000 milliseconds = 1 second
            updateTimer.Tick += (s, ev) => Form2Update(); // Lambda expression to call the Update function
            updateTimer.Start();
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

        private void timer_Tick(object sender, EventArgs e)
        {
            Form2Update(); // Call the update function
        }
        public void Form2Update()
        {
            // Check whether Desktop is focused currently
            topmost = (Form.ActiveForm == this);
            if (topmost)
            {
                Desktop_GotFocus(); // If focused, call the GotFocus method
            }
            else
            {
                Desktop_LostFocus();    // If not focused, call the LostFocus method
            }
        }

        // Method to handle when the form gets focus
        private void Desktop_GotFocus()
        {
            // Start listening if not already listening
            if (!isListening)
            {
                try
                {
                    isListening = true;
                    recognizer.RecognizeAsync(RecognizeMode.Multiple);  // Start recognition in multiple mode
                    ShowSpeechFeedback("Ready to listen...");
                }
                catch (ObjectDisposedException)
                {
                    
                }
            }
        }
        // Method to handle when the form loses focus
        private void Desktop_LostFocus()
        {
            if (isListening)
            {
                try
                {
                    isListening = false;
                    recognizer.RecognizeAsyncStop(); // Stop recognition
                    HideSpeechFeedback();
                }
                catch (ObjectDisposedException)
                {

                }

            }
        }
        // Event Handler to when the Hexagon button is clicked to proceed to Desktop
        private void button1_Click(object sender, EventArgs e)
        {
            OpenDesktop();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            // When the animation is done, it will how the picture background and dispose the animation
            timer1.Stop();
            TransparentButton.Show();
            pictureBox2.Show();
            pictureBox1.Dispose();
        }

        // Event handler for the speech recognition event
        private void recognizer_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            // Update visual feedback with recognized command first
            UpdateSpeechText($"Recognized: {e.Result.Text}", true);

            // Check if the recognized speech is "hello honey"
            if (e.Result.Text == "hello honey")
            {
                // Check the confidence level of the recognized speech
                // if (e.Result.Confidence < 0.8)
                // {
                    // MessageBox.Show("Who are you?", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // return;
                // }
                // If yes it will show the message box and proceeds to Desktop
                MessageBox.Show("Oh it's you, honey! Welcome home dear!", "HoneyOS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                OpenDesktop();
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

        // Function to open the desktop form
        private void OpenDesktop()
        {
            // Logic to handle the "hello honey" speech
            // Create and show the Desktop form, then hide the current form
            Desktop form3 = new Desktop();
            form3.Show();
            this.Hide();

            // Stop and dispose of the recognizer
            recognizer.RecognizeAsyncStop();
            recognizer.Dispose();
        }
    }
}
