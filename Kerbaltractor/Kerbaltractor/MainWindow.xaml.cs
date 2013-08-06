using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kerbaltractor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Ellipse[] ellipses; //Holds the ellipses to be drawn
        private Point[] points;  //Holds the points to measure between
        private int currentPointArrayIndex; //Hold the index value of the next point to be placed
        private int pointsAdded; //The number of points that have been added (for initial state)

        public MainWindow()
        {
            InitializeComponent();

            //Register Important Window Events
            this.MouseDown += new MouseButtonEventHandler(Window_MouseDown); //For dragging the form
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown); //For keyboard shortcuts
            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged); //For modifying the capture region size
            this.MouseDoubleClick += new MouseButtonEventHandler(MainWindow_MouseDoubleClick); //Double click event for adding points

            //Sync Popup Location to Window
            this.LocationChanged += delegate(object sender, EventArgs args)
            {
                var offset = notificationPopup.HorizontalOffset;
                notificationPopup.HorizontalOffset = offset + 1;
                notificationPopup.HorizontalOffset = offset;
            };

            //Show instructions
            notificationPopupText.Text = "Resize, drag, double click to place points. Escape to quit.\r\n<Click to Close...>";
            notificationPopup.IsOpen = true;

            currentPointArrayIndex = 0; //Starting at index 0
            ellipses = new Ellipse[3]; //3 points make an angle!
            ellipses[0] = ellipse1;
            ellipses[1] = ellipse2;
            ellipses[2] = ellipse3;

            points = new Point[3];
        }

        #region Window Events

        void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Point newPoint = e.GetPosition(this);
                points[currentPointArrayIndex] = newPoint;
                Canvas.SetLeft(ellipses[currentPointArrayIndex], newPoint.X);
                Canvas.SetTop(ellipses[currentPointArrayIndex], newPoint.Y);
                if (pointsAdded < 3)
                {
                    ellipses[currentPointArrayIndex].Visibility = System.Windows.Visibility.Visible;
                    pointsAdded++;
                }
                if(pointsAdded>2)
                {
                    double dist1 = Math.Sqrt((points[0].X - points[1].X) * (points[0].X - points[1].X) + (points[0].Y - points[1].Y) * (points[0].Y - points[1].Y));
                    double dist2 = Math.Sqrt((points[1].X - points[2].X) * (points[1].X - points[2].X) + (points[1].Y - points[2].Y) * (points[1].Y - points[2].Y));
                    double dist3 = Math.Sqrt((points[2].X - points[0].X) * (points[2].X - points[0].X) + (points[2].Y - points[0].Y) * (points[2].Y - points[0].Y));
                    double cosedVal = (dist2 * dist2 + dist1 * dist1 + dist3 * dist3) / (2 * dist2 * dist1);
                    double scaledVal = cosedVal % (1);
                    double angle = Math.Acos(scaledVal);
                    double degrees = angle * 57.2957795;
                    
                    angleOutput.SetValue(TextBox.TextProperty, ""+degrees.ToString("F2"));
                }

                if (currentPointArrayIndex == 0)
                {
                    line1.SetValue(Line.X1Property, newPoint.X + 5);
                    line1.SetValue(Line.Y1Property, newPoint.Y + 5);
                }
                else if (currentPointArrayIndex == 1)
                {
                    line1.SetValue(Line.X2Property, newPoint.X + 5);
                    line1.SetValue(Line.Y2Property, newPoint.Y + 5);
                    line1.SetValue(Line.VisibilityProperty, System.Windows.Visibility.Visible);
                    line2.SetValue(Line.X1Property, newPoint.X + 5);
                    line2.SetValue(Line.Y1Property, newPoint.Y + 5);
                }
                else if (currentPointArrayIndex == 2)
                {
                    line2.SetValue(Line.X2Property, newPoint.X + 5);
                    line2.SetValue(Line.Y2Property, newPoint.Y + 5);
                    line2.SetValue(Line.VisibilityProperty, System.Windows.Visibility.Visible);
                }
                currentPointArrayIndex = (currentPointArrayIndex + 1) % points.Length;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) //Left click drags the form
                this.DragMove();
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) //Escape closes the form
                this.Close();
        }

        void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Update the border size (all other updates are implicit)
            windowBorder.Width = e.NewSize.Width;
            windowBorder.Height = e.NewSize.Height;
        }

        private void notificationPopup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Close the popup when it is clicked
            notificationPopup.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
            notificationPopup.StaysOpen = false;
            notificationPopup.IsOpen = false;
        }

        #endregion
    }
}
