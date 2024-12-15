using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Mastermind
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer = new DispatcherTimer();
        private List<string> kleuren = new List<string> { "Red", "Yellow", "Orange", "White", "Green", "Blue" };
        private List<string> Random = new List<string>();
        private List<string> spelers = new List<string>();
        private int attempts = 1;
        private const int maxAttempts = 10;
        private int countdownSeconds = 0;
        private const int maxTime = 10;
        private bool gameEnded = false;
        private int currentPlayerIndex = 0; // Houdt bij wie de actieve speler is

        public MainWindow()
        {
            InitializeComponent();
            RandomKleur();
            ComboBoxes();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);
        }

        private void RandomKleur()
        {
            var random = new Random();
            Random.Clear();
            for (int i = 0; i < 4; i++)
            {
                Random.Add(kleuren[random.Next(kleuren.Count)]);
            }
            DebugTextBox.Text = $"Geheime code: {string.Join(", ", Random)}";
            StartCountdown();
        }

        private void ComboBoxes()
        {
            ComboBox1.ItemsSource = kleuren;
            ComboBox2.ItemsSource = kleuren;
            ComboBox3.ItemsSource = kleuren;
            ComboBox4.ItemsSource = kleuren;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = sender as ComboBox;
            if (comboBox.SelectedItem != null)
            {
                string selectedColor = comboBox.SelectedItem.ToString();
                Brush brushColor = (Brush)new BrushConverter().ConvertFromString(selectedColor);
                switch (comboBox.Name)
                {
                    case "ComboBox1":
                        Label1.Background = brushColor;
                        break;
                    case "ComboBox2":
                        Label2.Background = brushColor;
                        break;
                    case "ComboBox3":
                        Label3.Background = brushColor;
                        break;
                    case "ComboBox4":
                        Label4.Background = brushColor;
                        break;
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            countdownSeconds++;
            timerLabel.Content = $"Tijd: {countdownSeconds}s";
            if (countdownSeconds >= maxTime)
            {
                StopCountdown();
                LoseTurn();
            }
        }

        private void StartCountdown()
        {
            countdownSeconds = 0;
            timer.Start();
        }

        private void StopCountdown()
        {
            timer.Stop();
        }

        private void LoseTurn()
        {
            MessageBox.Show("Te laat! Je verliest deze beurt.", "Te laat", MessageBoxButton.OK, MessageBoxImage.Warning);
            attempts++;
            if (attempts > maxAttempts)
            {
                EndGame(false);
            }
            else
            {
                StartCountdown();
            }
        }

        private void CheckCodeButton_Click(object sender, RoutedEventArgs e)
        {
            string guess1 = ComboBox1.SelectedItem?.ToString();
            string guess2 = ComboBox2.SelectedItem?.ToString();
            string guess3 = ComboBox3.SelectedItem?.ToString();
            string guess4 = ComboBox4.SelectedItem?.ToString();
            int score = CheckGuesses(guess1, guess2, guess3, guess4);
            ScoreLabel.Content = $"Score: {score}";
            StopCountdown();

            if (score == 0)
            {
                EndGame(true);
            }
            else if (attempts < maxAttempts)
            {
                attempts++;
                StartCountdown();
                SwitchPlayer();  // Verander naar de volgende speler na deze beurt
            }
            else
            {
                EndGame(false);
            }
        }

        private int CheckGuesses(string guess1, string guess2, string guess3, string guess4)
        {
            List<string> guesses = new List<string> { guess1, guess2, guess3, guess4 };
            int correctPositions = 0;
            int correctColors = 0;
            List<string> secretCode = new List<string>(Random);

            // Check voor exacte posities (perfecte match)
            for (int i = 0; i < guesses.Count; i++)
            {
                if (guesses[i] == secretCode[i])
                {
                    correctPositions++;
                    secretCode[i] = null;  // Markeer deze code als gebruikt
                }
            }

            // Check voor correcte kleuren (juiste kleur maar verkeerde positie)
            for (int i = 0; i < guesses.Count; i++)
            {
                if (guesses[i] != null && secretCode.Contains(guesses[i]))
                {
                    correctColors++;
                    secretCode[secretCode.IndexOf(guesses[i])] = null; // Markeer deze code als gebruikt
                }
            }

            return (guesses.Count - correctPositions - correctColors) * 2 + correctColors;
        }

        private void EndGame(bool isWin)
        {
            gameEnded = true;
            string message = isWin ? "Gefeliciteerd! Je hebt gewonnen!" : $"Helaas, je hebt verloren. De geheime code was: {string.Join(", ", Random)}";
            MessageBoxResult result = MessageBox.Show(message + "\nWil je opnieuw spelen?", "Einde Spel", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                ResetGame();
            }
            else
            {
                Close();
            }
        }

        private void ResetGame()
        {
            gameEnded = false;
            attempts = 1;
            RandomKleur();
            ScoreLabel.Content = "Score: 0";
            PreviousGuessesPanel.Children.Clear();
            StartCountdown();
        }

        private void CloseGameButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Spelstarten_Click(object sender, RoutedEventArgs e)
        {
            spelers.Clear(); // Wis de lijst voor een nieuw spel

            while (true)
            {
                var inputDialog = new Window
                {
                    Title = "Nieuwe Speler",
                    Width = 400,
                    Height = 200,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    ResizeMode = ResizeMode.NoResize
                };

                var stackPanel = new StackPanel { Margin = new Thickness(20) };
                var label = new Label { Content = "Voer de naam van de speler in:" };
                var textBox = new TextBox { Width = 300 };
                var buttonsPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
                var okButton = new Button { Content = "OK", Width = 100, Margin = new Thickness(10) };
                var cancelButton = new Button { Content = "Annuleren", Width = 100, Margin = new Thickness(10) };

                buttonsPanel.Children.Add(okButton);
                buttonsPanel.Children.Add(cancelButton);
                stackPanel.Children.Add(label);
                stackPanel.Children.Add(textBox);
                stackPanel.Children.Add(buttonsPanel);
                inputDialog.Content = stackPanel;

                string spelerNaam = null;
                okButton.Click += (s, ev) =>
                {
                    spelerNaam = textBox.Text.Trim();
                    inputDialog.DialogResult = true;
                    inputDialog.Close();
                };

                cancelButton.Click += (s, ev) =>
                {
                    inputDialog.DialogResult = false;
                    inputDialog.Close();
                };

                var result = inputDialog.ShowDialog();

                if (result == false || string.IsNullOrWhiteSpace(spelerNaam))
                {
                    MessageBox.Show("Kies een geldige naam.", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue; // Vraag opnieuw om een naam
                }

                spelers.Add(spelerNaam);

                MessageBoxResult vraag = MessageBox.Show(
                    "Wil je nog een speler toevoegen?",
                    "Extra Speler",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (vraag == MessageBoxResult.No)
                {
                    break; // Stop als de gebruiker geen extra spelers wil toevoegen
                }
            }

            if (spelers.Count > 0)
            {
                MessageBox.Show(
                    $"Het spel begint met de volgende spelers: {string.Join(", ", spelers)}",
                    "Spel Gestart",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                StartCountdown();
                UpdateActivePlayerLabel(); // Toon de eerste speler
            }
            else
            {
                MessageBox.Show("Er zijn geen spelers toegevoegd. Het spel kan niet beginnen.", "Waarschuwing", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Methode om de actieve speler te updaten
        private void UpdateActivePlayerLabel()
        {
            if (spelers.Count > 0)
            {
                ActivePlayerLabel.Content = $"Actieve Speler: {spelers[currentPlayerIndex]}";
            }
            else
            {
                ActivePlayerLabel.Content = "Geen actieve speler.";
            }
        }

        // Methode om naar de volgende speler te schakelen
        private void SwitchPlayer()
        {
            if (spelers.Count > 0)
            {
                currentPlayerIndex = (currentPlayerIndex + 1) % spelers.Count; // Zorg ervoor dat het weer naar de eerste speler gaat als het einde van de lijst is bereikt
                UpdateActivePlayerLabel(); // Werk het label bij met de nieuwe actieve speler
            }
            else
            {
                MessageBox.Show("Er zijn geen spelers toegevoegd. Kan niet schakelen.", "Fout", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
    