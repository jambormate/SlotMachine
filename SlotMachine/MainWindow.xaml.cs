using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace SlotMachine
{
	public partial class MainWindow : Window
	{
		int egyenleg = 100;
		int tet = 10;

		List<string> kepek;
		List<BitmapImage> kepekCache;
		int SYMBOL_COUNT;

		Random rnd = new Random();
		DispatcherTimer timer = new DispatcherTimer();

		double pos1 = 0, pos2 = 0, pos3 = 0;

		double speed1 = 15, speed2 = 18, speed3 = 23;
		double currentSpeed1, currentSpeed2, currentSpeed3;

		bool stop1 = false, stop2 = false, stop3 = false;

		const int IMAGE_HEIGHT = 80;
		const double DECEL = 0.95;
		const double MIN_SPEED = 2;

		public MainWindow()
		{
			InitializeComponent();

			kepek = new List<string>
			{
				"Kepek/apple.png",
				"Kepek/banana.png",
				"Kepek/cherry.png",
				"Kepek/lemon.png",
				"Kepek/grape.png",
				"Kepek/bar.png",
				"Kepek/clover.png",
				"Kepek/coin.png",
				"Kepek/diamond.png",
				"Kepek/horseshoe.png",
				"Kepek/melon.png",
				"Kepek/orange.png",
				"Kepek/plum.png",
				"Kepek/questionmark.png",
				"Kepek/seven.png"
			};

			SYMBOL_COUNT = 6;

			kepekCache = new List<BitmapImage>();

			foreach (var path in kepek)
			{
				var uri = new Uri($"pack://application:,,,/{path}", UriKind.Absolute);

				BitmapImage bmp = new BitmapImage();
				bmp.BeginInit();
				bmp.UriSource = uri;
				bmp.CacheOption = BitmapCacheOption.OnLoad;
				bmp.EndInit();
				bmp.Freeze();

				kepekCache.Add(bmp);
			}

			FeltoltReel(reel1);
			FeltoltReel(reel2);
			FeltoltReel(reel3);

			timer.Interval = TimeSpan.FromMilliseconds(30);
			timer.Tick += Animate;

			FrissitUI();
		}

		void FrissitUI()
		{
			egyenlegKiiras.Text = $"Egyenleg: {egyenleg}";

			PorgetesGomb.IsEnabled = egyenleg >= tet;
		}

		void FeltoltReel(Canvas reel)
		{
			for (int i = 0; i < 30; i++)
			{
				int index = rnd.Next(SYMBOL_COUNT);

				Image img = new Image
				{
					Width = 120,
					Height = IMAGE_HEIGHT,
					Source = kepekCache[index],
					Tag = index
				};

				Canvas.SetTop(img, i * IMAGE_HEIGHT);
				reel.Children.Add(img);
			}

			reel.RenderTransform = new TranslateTransform();
		}

		private void Porgetes_Click(object sender, RoutedEventArgs e)
		{
			if (egyenleg < tet)
			{
				MessageBox.Show("Nincs elég kredit!");
				return;
			}

			egyenleg -= tet;

			stop1 = stop2 = stop3 = false;

			currentSpeed1 = speed1;
			currentSpeed2 = speed2;
			currentSpeed3 = speed3;

			PorgetesGomb.IsEnabled = false;
			timer.Start();

			Dispatcher.InvokeAsync(async () =>
			{
				await System.Threading.Tasks.Task.Delay(1000);
				stop1 = true;

				await System.Threading.Tasks.Task.Delay(500);
				stop2 = true;

				await System.Threading.Tasks.Task.Delay(500);
				stop3 = true;
			});

			FrissitUI();
		}

		void Animate(object sender, EventArgs e)
		{
			AnimateReel(reel1, ref pos1, ref currentSpeed1, stop1);
			AnimateReel(reel2, ref pos2, ref currentSpeed2, stop2);
			AnimateReel(reel3, ref pos3, ref currentSpeed3, stop3);

			if (stop1 && stop2 && stop3 &&
				currentSpeed1 <= MIN_SPEED &&
				currentSpeed2 <= MIN_SPEED &&
				currentSpeed3 <= MIN_SPEED)
			{
				SnapToGrid(reel1, ref pos1);
				SnapToGrid(reel2, ref pos2);
				SnapToGrid(reel3, ref pos3);

				timer.Stop();
				KiErtekeles();
				PorgetesGomb.IsEnabled = true;
			}
		}

		void AnimateReel(Canvas reel, ref double pos, ref double speed, bool stop)
		{
			if (!stop)
			{
				pos += speed;
			}
			else
			{
				speed *= DECEL;
				pos += speed;
			}

			double max = reel.Children.Count * IMAGE_HEIGHT;

			if (pos >= max)
				pos -= max;

			((TranslateTransform)reel.RenderTransform).Y = -pos;
		}

		void SnapToGrid(Canvas reel, ref double pos)
		{
			double remainder = pos % IMAGE_HEIGHT;

			if (remainder < IMAGE_HEIGHT / 2)
				pos -= remainder;
			else
				pos += (IMAGE_HEIGHT - remainder);

			((TranslateTransform)reel.RenderTransform).Y = -pos;
		}

		int GetMiddleSymbol(Canvas reel, double pos)
		{
			int index = (int)((pos + IMAGE_HEIGHT) / IMAGE_HEIGHT);
			index %= reel.Children.Count;

			return (int)((Image)reel.Children[index]).Tag;
		}

		void KiErtekeles()
		{
			int a = GetMiddleSymbol(reel1, pos1);
			int b = GetMiddleSymbol(reel2, pos2);
			int c = GetMiddleSymbol(reel3, pos3);

			if (a == b && b == c)
			{
				egyenleg += 50;
				eredmenySzoveg.Text = "NAGY NYEREMÉNY!";
			}
			else if (a == b || b == c || a == c)
			{
				egyenleg += 20;
				eredmenySzoveg.Text = "Kis nyeremény!";
			}
			else
			{
				eredmenySzoveg.Text = "Vesztettél!";
			}

			FrissitUI();
		}

	}
}