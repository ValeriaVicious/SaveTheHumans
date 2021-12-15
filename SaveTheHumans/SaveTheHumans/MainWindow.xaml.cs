using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Input;


namespace SaveTheHumans
{
    public sealed partial class MainWindow : Window
    {
        #region Fields

        private readonly Random _random;
        private readonly DispatcherTimer _enemyTimer;
        private readonly DispatcherTimer _targetTimer;
        private bool _isHumanCaptured = false;

        private const string _canvasLeft = "(Canvas.Left)";
        private const string _canvasRight = "(Canvas.Top)";
        private const string _enemyTemplate = "EnemyTemplate";

        private readonly float _enemyTimerInterval = 2.0f;
        private readonly float _targetTimerInterval = 0.1f;
        private readonly int _numberForsubstractFromTheBorder = 100;
        private readonly int _timeSpanFrom = 4;
        private readonly int _timeSpanTo = 6;
        private readonly int _devidedByForCanvas = 2;
        private readonly int _multiByForCanvas = 3;
        private int _savedHumansCounter = 0;


        #endregion


        #region ClassLifeCycles

        public MainWindow()
        {
            _random = new Random();
            _enemyTimer = new DispatcherTimer();
            _targetTimer = new DispatcherTimer();

            _enemyTimer.Tick += EnemyTimer_Tick;
            _enemyTimer.Interval = TimeSpan.FromSeconds(_enemyTimerInterval);

            _targetTimer.Tick += TargetTimer;
            _targetTimer.Interval = TimeSpan.FromSeconds(_targetTimerInterval);

            InitializeComponent();
        }


        #endregion


        #region Methods

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartTheGame();
        }

        private void StartTheGame()
        {
            Human.IsHitTestVisible = true;
            _isHumanCaptured = false;
            ProgressBar.Value = 0.0d;
            StartButton.Visibility = Visibility.Collapsed;
            PlayArea.Children.Clear();
            PlayArea.Children.Add(Target);
            PlayArea.Children.Add(Human);
            _enemyTimer.Start();
            _targetTimer.Start();
        }

        private void AddEnemy()
        {
            ContentControl enemy = new ContentControl
            {
                Template = Resources[_enemyTemplate] as ControlTemplate
            };
            AnimateEnemy(enemy, 0, PlayArea.ActualWidth - _numberForsubstractFromTheBorder, new PropertyPath(_canvasLeft));
            AnimateEnemy(enemy, _random.Next((int)PlayArea.ActualHeight - _numberForsubstractFromTheBorder),
                _random.Next((int)PlayArea.ActualHeight - _numberForsubstractFromTheBorder), new PropertyPath(_canvasRight));
            PlayArea.Children.Add(enemy);
            enemy.MouseEnter += Enemy_MouseEnter;
        }

        private void Enemy_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_isHumanCaptured)
            {
                EndTheGame();
                _savedHumansCounter = 0;
            }
        }

        private void AnimateEnemy(ContentControl enemy, double from, double to, PropertyPath propertyToAnimate)
        {
            Storyboard storyboard = new Storyboard()
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            DoubleAnimation animation = new DoubleAnimation()
            {
                From = from,
                To = to,
                Duration = new Duration(TimeSpan.FromSeconds(_random.Next(_timeSpanFrom, _timeSpanTo)))
            };

            Storyboard.SetTarget(animation, enemy);
            Storyboard.SetTargetProperty(animation, propertyToAnimate);
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void TargetTimer(object sender, EventArgs e)
        {
            ProgressBar.Value += 1;
            if (ProgressBar.Value >= ProgressBar.Maximum)
            {
                EndTheGame();
                _savedHumansCounter = 0;
            }
        }

        private void EndTheGame()
        {
            if (!PlayArea.Children.Contains(GameOverText))
            {
                _enemyTimer.Stop();
                _targetTimer.Stop();
                _isHumanCaptured = false;
                StartButton.Visibility = Visibility.Visible;
                PlayArea.Children.Add(GameOverText);
            }
        }

        private void EnemyTimer_Tick(object sender, EventArgs e)
        {
            AddEnemy();
        }

        #endregion

        private void Human_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_enemyTimer.IsEnabled)
            {
                _isHumanCaptured = true;
                Human.IsHitTestVisible = false;
            }
        }

        private void Target_MouseEnter(object sender, MouseEventArgs e)
        {
            if(_targetTimer.IsEnabled && _isHumanCaptured)
            {
                ProgressBar.Value = 0.0d;
                Canvas.SetLeft(Target, _random.Next(_numberForsubstractFromTheBorder, (int)PlayArea.ActualWidth - _numberForsubstractFromTheBorder));
                Canvas.SetTop(Target, _random.Next(_numberForsubstractFromTheBorder, (int)PlayArea.ActualHeight - _numberForsubstractFromTheBorder));
                Canvas.SetLeft(Human, _random.Next(_numberForsubstractFromTheBorder, (int)PlayArea.ActualWidth - _numberForsubstractFromTheBorder));
                Canvas.SetTop(Human, _random.Next(_numberForsubstractFromTheBorder, (int)PlayArea.ActualHeight - _numberForsubstractFromTheBorder));
                _isHumanCaptured = false;
                Human.IsHitTestVisible = true;
                _savedHumansCounter++;
                SavedHumansCounter.Content = _savedHumansCounter.ToString();
            }
        }

        private void PlayArea_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isHumanCaptured)
            {
                Point mousePoint = e.GetPosition(null);
                Point relativePosition = grid.TransformToVisual(PlayArea).Transform(mousePoint);
                if ((Math.Abs(relativePosition.X - Canvas.GetLeft(Human)) > Human.ActualWidth * _multiByForCanvas) ||
                    (Math.Abs(relativePosition.Y - Canvas.GetTop(Human)) > Human.ActualHeight * _multiByForCanvas))
                {
                    _isHumanCaptured = false;
                    Human.IsHitTestVisible = true;
                }
                else
                {
                    Canvas.SetLeft(Human, relativePosition.X - Human.ActualWidth / _devidedByForCanvas);
                    Canvas.SetTop(Human, relativePosition.Y - Human.ActualHeight / _devidedByForCanvas);
                }
            }
        }

        private void PlayArea_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_isHumanCaptured)
            {
                EndTheGame();
                _savedHumansCounter = 0;
            }
        }
    }
}
