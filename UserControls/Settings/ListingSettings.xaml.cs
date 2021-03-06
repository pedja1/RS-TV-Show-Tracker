﻿namespace RoliSoft.TVShowTracker.UserControls
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows;

    /// <summary>
    /// Interaction logic for ListingSettings.xaml
    /// </summary>
    public partial class ListingSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ListingSettings"/> class.
        /// </summary>
        public ListingSettings()
        {
            InitializeComponent();
        }

        private bool _loaded;
        internal bool _reindex;

        /// <summary>
        /// Handles the Loaded event of the UserControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void UserControlLoaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_loaded) return;

            try
            {
                if (Signature.IsActivated)
                {
                    nzbDays.Value              = Settings.Get<int>("Usenet Retention");
                    filterUrlTextBox.Text      = string.Join(",", Settings.Get<List<string>>("One-Click Hoster List"));
                    linkChecker.IsChecked      = Settings.Get<bool>("One-Click Hoster Link Checker");
                    shortestNotation.IsChecked = Settings.Get<bool>("Enable Shortest Notation");
                    updateToNightly.IsChecked  = Settings.Get<bool>("Update to Nightly Builds");

                    switch (Settings.Get("One-Click Hoster List Type", "white"))
                    {
                        case "black":
                            blackListRadioButton.IsChecked = true;
                            listTypeText.Text = "Enter a comma-separated list of domains or partial URLs to filter links if they match:";
                            break;

                        case "white":
                            whiteListRadioButton.IsChecked = true;
                            listTypeText.Text = "Enter a comma-separated list of domains or partial URLs to filter links if they don't match:";
                            break;
                    }
                }
                else
                {
                    nzbDays.IsEnabled = blackListRadioButton.IsEnabled = whiteListRadioButton.IsEnabled = filterUrlTextBox.IsEnabled = linkChecker.IsEnabled = false;
                }

                var fn = Path.Combine(Signature.InstallPath, @"misc\linkchecker");

                if (!File.Exists(fn))
                {
                    linkCheck.Text = "The link checker definitions file hasn't been downloaded yet.";
                }
                else if (Parsers.LinkCheckers.Engines.UniversalEngine.Definitions == null || Parsers.LinkCheckers.Engines.UniversalEngine.Definitions.Count == 0)
                {
                    linkCheck.Text = "The link checker definitions file hasn't been loaded yet.";
                }
                else
                {
                    linkCheck.Text = "The link checker definitions file has " + Parsers.LinkCheckers.Engines.UniversalEngine.Definitions.Count + " sites; last updated " + File.GetLastWriteTime(fn).ToRelativeDate() + ".";
                }
            }
            catch (Exception ex)
            {
                MainWindow.HandleUnexpectedException(ex);
            }

            _loaded = true;
        }

        /// <summary>
        /// Handles the OnLostFocus event of the nzbDays control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void NzbDaysOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            Settings.Set("Usenet Retention", nzbDays.Value);
        }

        /// <summary>
        /// Handles the Click event of the whiteListRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void WhiteListRadioButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            listTypeText.Text = "Enter a comma-separated list of domains or partial URLs to filter links if they don't match:";
            Settings.Set("One-Click Hoster List Type", "white");
        }

        /// <summary>
        /// Handles the Click event of the blackListRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void BlackListRadioButtonClick(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            listTypeText.Text = "Enter a comma-separated list of domains or partial URLs to filter links if they match:";
            Settings.Set("One-Click Hoster List Type", "black");
        }

        /// <summary>
        /// Handles the TextChanged event of the filterUrlTextBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Controls.TextChangedEventArgs"/> instance containing the event data.</param>
        private void FilterUrlTextBoxTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (!_loaded) return;

            var urls = filterUrlTextBox.Text.Trim(',').Split(',').ToList();

            if (urls.Count == 1 && string.IsNullOrWhiteSpace(urls[0]))
            {
                urls.RemoveAt(0);
            }

            Settings.Set("One-Click Hoster List", urls);
        }

        /// <summary>
        /// Handles the Checked event of the linkChecker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void LinkCheckerChecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            Settings.Set("One-Click Hoster Link Checker", true);
        }

        /// <summary>
        /// Handles the Unchecked event of the linkChecker control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void LinkCheckerUnchecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            Settings.Set("One-Click Hoster Link Checker", false);
        }

        /// <summary>
        /// Handles the Checked event of the shortestNotation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ShortestNotationChecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            Settings.Set("Enable Shortest Notation", true);

            _reindex = true;
            ShowNames.Regexes.AdvNumbering = ShowNames.Parser.GenerateEpisodeRegexes(generateExtractor: true);
        }

        /// <summary>
        /// Handles the Unchecked event of the shortestNotation control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void ShortestNotationUnchecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            Settings.Set("Enable Shortest Notation", false);

            _reindex = true;
            ShowNames.Regexes.AdvNumbering = ShowNames.Parser.GenerateEpisodeRegexes(generateExtractor: true);
        }

        /// <summary>
        /// Handles the Checked event of the updateToNightly control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void UpdateToNightlyChecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            Settings.Set("Update to Nightly Builds", true);
        }

        /// <summary>
        /// Handles the Unchecked event of the updateToNightly control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        private void UpdateToNightlyUnchecked(object sender, RoutedEventArgs e)
        {
            if (!_loaded) return;

            Settings.Set("Update to Nightly Builds", false);
        }
    }
}
