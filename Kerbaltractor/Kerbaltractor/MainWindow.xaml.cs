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
        #region Member Variables and Constants

        private const int numPoints = 3; //Honestly just here because I hate magic numbers
        private Ellipse[] ellipses; //Holds the ellipses to be drawn
        private Point[] points;  //Holds the points to measure between
        private int currentPointArrayIndex; //Hold the index value of the next point to be placed
        private int pointsAdded; //The number of points that have been added (for initial state)
        private double savedWidth; //For storing width on shrink state
        private double savedHeight; //For storing height on shrink state
        private bool shrunk; //For storing shrink state

        #endregion

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();

            //Register Important Window Events
            this.MouseDown += new MouseButtonEventHandler(Window_MouseDown); //For dragging the form
            this.KeyDown += new KeyEventHandler(MainWindow_KeyDown); //For keyboard shortcuts
            this.MouseDoubleClick += new MouseButtonEventHandler(MainWindow_MouseDoubleClick); //Double click event for adding points
            this.SizeChanged += new SizeChangedEventHandler(MainWindow_SizeChanged); //For modifying the capture region size
            windowBorder.MouseDown += new MouseButtonEventHandler(windowBorder_MouseDown);

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

            //Ellipse/Point storage variables
            currentPointArrayIndex = 0; //Starting at index 0
            points = new Point[numPoints];
            ellipses = new Ellipse[numPoints]; //3 points make an angle!
            ellipses[0] = ellipse1;
            ellipses[1] = ellipse2;
            ellipses[2] = ellipse3;

            //Save the width and height
            savedWidth = this.Width;
            savedHeight = this.Height;
            shrunk = false;

            minimizedButton.MouseDown += new MouseButtonEventHandler(minimizedButton_MouseDown);
            minimizedButton.MouseEnter += new MouseEventHandler(button_MouseEnter);
            minimizedButton.MouseLeave += new MouseEventHandler(button_MouseLeave);

            shrinkButton.MouseDown += new MouseButtonEventHandler(shrinkButton_MouseDown);
            shrinkButton.MouseEnter += new MouseEventHandler(button_MouseEnter);
            shrinkButton.MouseLeave += new MouseEventHandler(button_MouseLeave);
        }

        void shrinkButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            shrinkToggle();
        }

        void button_MouseLeave(object sender, MouseEventArgs e)
        {
            Rectangle r = (Rectangle)sender;
            r.SetValue(Rectangle.FillProperty, (SolidColorBrush)new BrushConverter().ConvertFromString("#297F7F7F"));
        }

        void button_MouseEnter(object sender, MouseEventArgs e)
        {
            Rectangle r = (Rectangle)sender;
            r.SetValue(Rectangle.FillProperty, (SolidColorBrush)new BrushConverter().ConvertFromString("#FFFFFFFF"));
        }

        void minimizedButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized; 
        }

        #endregion

        #region Window Events

        void shrinkToggle()
        {
            //If the window is already shrunk
            if (shrunk)
            {
                //Restore it
                this.Width = savedWidth;
                this.Height = savedHeight;
            }
            else
            {
                //Shrink it
                savedWidth = this.Width;
                savedHeight = this.Height;
                this.Width = this.MinWidth;
                this.Height = this.MinHeight;
            }
            //Toggle shrunk state
            shrunk = !shrunk;
        }

        void windowBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //If the border is doubleclicked
            if (e.ClickCount == 2)
                shrinkToggle();
        }

        void MainWindow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //If the left button is clicked and not on the border or text box
            if (e.ChangedButton == MouseButton.Left && !windowBorder.IsMouseOver && !angleOutput.IsMouseOver)
            {
                //Skew the point according to the graphics
                Point newPoint = e.GetPosition(this);
                newPoint.X -= ellipses[currentPointArrayIndex].Width / 2;
                newPoint.Y -= ellipses[currentPointArrayIndex].Height / 2;

                //Store the point
                points[currentPointArrayIndex] = newPoint;

                //Set the ellipse position
                Canvas.SetLeft(ellipses[currentPointArrayIndex], newPoint.X);
                Canvas.SetTop(ellipses[currentPointArrayIndex], newPoint.Y);

                //If we're in the original 3 points
                if (pointsAdded < numPoints)
                {
                    //Make the point visible
                    ellipses[currentPointArrayIndex].Visibility = System.Windows.Visibility.Visible;
                    pointsAdded++;
                }

                //If we've got all 3 points now, compute and show the angle
                if (pointsAdded > numPoints - 1)
                {
                    double angle = radiansToDegrees(getAngle(points[1], points[0], points[2]));
                    String smallAngleString = angle.ToString("F2");
                    String bigAngleString = (360 - angle).ToString("F2");
                    angleOutput.SetValue(TextBox.TextProperty, smallAngleString + "°, " + bigAngleString + "°");
                }

                //If we're on the first point, set the first line location
                if (currentPointArrayIndex == 0)
                {
                    line1.SetValue(Line.X1Property, newPoint.X + ellipses[currentPointArrayIndex].Width / 2);
                    line1.SetValue(Line.Y1Property, newPoint.Y + ellipses[currentPointArrayIndex].Height / 2);
                }
                //If we're on the second point, set the first and second line location
                else if (currentPointArrayIndex == 1)
                {
                    line1.SetValue(Line.X2Property, newPoint.X + ellipses[currentPointArrayIndex].Width / 2);
                    line1.SetValue(Line.Y2Property, newPoint.Y + ellipses[currentPointArrayIndex].Height / 2);
                    line1.SetValue(Line.VisibilityProperty, System.Windows.Visibility.Visible);
                    line2.SetValue(Line.X1Property, newPoint.X + ellipses[currentPointArrayIndex].Width / 2);
                    line2.SetValue(Line.Y1Property, newPoint.Y + ellipses[currentPointArrayIndex].Height / 2);
                }
                //If we're on the third point, set the second line location
                else if (currentPointArrayIndex == 2)
                {
                    line2.SetValue(Line.X2Property, newPoint.X + ellipses[currentPointArrayIndex].Width / 2);
                    line2.SetValue(Line.Y2Property, newPoint.Y + ellipses[currentPointArrayIndex].Height / 2);
                    line2.SetValue(Line.VisibilityProperty, System.Windows.Visibility.Visible);
                }

                //Increment the point index
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

            //The even first when the form opens, so we ignore that case, but we close the popup if the user resizes the form
            if (e.PreviousSize.Height != 0 && e.PreviousSize.Width != 0)
                notificationPopup_MouseDown(null, null);
            //We mark the form as not shrunk if the size is increased from the minimum manually
            if (e.PreviousSize.Width == windowBorder.MinWidth && e.PreviousSize.Height == windowBorder.MinHeight)
                shrunk = false;
        }

        private void notificationPopup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //Close the popup when it is clicked
            notificationPopup.PopupAnimation = System.Windows.Controls.Primitives.PopupAnimation.Fade;
            notificationPopup.StaysOpen = false;
            notificationPopup.IsOpen = false;
        }

        #endregion

        #region Math Helper Functions

        //Returns a value representing the smaller of the two possible angles in radians
        private double getAngle(Point vertex, Point p0, Point p1)
        {
            //Use the Cosine Rule
            //(c^2-a^2-b^2)/(-2ab)=cos(C)
            //a=vertP0
            //b=vertP1
            //c=P0P1
            //C=answer
            double vertP0Dist = distance(vertex, p0);
            double vertP1Dist = distance(vertex, p1);
            double p0P1Dist = distance(p0, p1);
            double cosedVal = (p0P1Dist * p0P1Dist
                                - vertP0Dist * vertP0Dist
                                - vertP1Dist * vertP1Dist)
                             / (-2 * vertP0Dist * vertP1Dist);

            double angle = Math.Acos(cosedVal);

            return angle;
        }

        //Converts radians to degrees
        private double radiansToDegrees(double angle)
        {
            return angle * 180 / Math.PI;
        }

        //Computes the euclidean distance between two points
        private double distance(Point p0, Point p1)
        {
            return Math.Sqrt((p0.X - p1.X) * (p0.X - p1.X) + (p0.Y - p1.Y) * (p0.Y - p1.Y));
        }

        #endregion
    }
}