using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices.ComTypes;
using System.Drawing.Drawing2D;

namespace RoomPlanner
{
    class Furniture
    {
        public Image FurnitureImage;
        public Point Location;
        public float Alpha;
        public Rectangle rec;
        public bool Selected;
        public int Index;
        public int Rotation;
        public ListViewItem Description;
        public string MyName;

        public Furniture(Image image, Point point, float alpha = 1, bool selected = false)
        {
            FurnitureImage = image;
            Location = point;
            Alpha = alpha;
            rec = new Rectangle(Location.X, Location.Y, FurnitureImage.Width, FurnitureImage.Height);
            Selected = selected;
            Rotation = 0;
        }

        public virtual void Draw(Graphics g)
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            int width = FurnitureImage.Width;
            int height = FurnitureImage.Height;
            float[][] colorMatrixElements = {
                                    new float[] {1,  0,  0,  0, 0},        // red scaling factor
                                    new float[] {0,  1,  0,  0, 0},        // green scaling factor 
                                    new float[] {0,  0,  1,  0, 0},        // blue scaling factor
                                    new float[] {0,  0,  0,  Alpha, 0},        // alpha scaling factor 
                                    new float[] {0, 0, 0, 0, 1}};    // translations

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            //rec = new Rectangle(Location.X, Location.Y, FurnitureImage.Width, FurnitureImage.Height);

            float x = Location.X;
            float y = Location.Y;
            g.TranslateTransform(x, y);
            g.RotateTransform(Rotation);
            g.TranslateTransform(-x, -y);

            rec = new Rectangle(Location.X - FurnitureImage.Width / 2, Location.Y - FurnitureImage.Height / 2, FurnitureImage.Width, FurnitureImage.Height);

            g.DrawImage(FurnitureImage, rec, 0, 0, width, height, GraphicsUnit.Pixel, imageAttributes);

            g.ResetTransform();
        }

        public virtual bool Contains(Point p)
        {
            if (rec.Contains(p))
                return true;
            else
                return false;
        }

        public virtual void ChangeLocation(Point p)
        {
            Location = p;
            UpdateDescription();
        }

        public virtual void UpdateDescription()
        {
            Description.Text = $"{MyName} {{X={Location.X},Y={Location.Y}}}" + Environment.NewLine;
        }
    }

    class Wall : Furniture
    {
        public List<Point> points;
        public GraphicsPath path;

        public Wall(Image image, Point point, float alpha = 1, bool selected = false) : base(image, point, alpha, selected)
        {
            FurnitureImage = image;
            Location = point;
            Alpha = alpha;
            rec = new Rectangle(Location.X, Location.Y, FurnitureImage.Width, FurnitureImage.Height);
            Selected = selected;
            points = new List<Point>();
            path = new GraphicsPath();
        }

        public override void Draw(Graphics g)
        {
            ImageAttributes imageAttributes = new ImageAttributes();
            float[][] colorMatrixElements = {
                                    new float[] {1,  0,  0,  0, 0},        // red scaling factor
                                    new float[] {0,  1,  0,  0, 0},        // green scaling factor 
                                    new float[] {0,  0,  1,  0, 0},        // blue scaling factor
                                    new float[] {0,  0,  0,  Alpha, 0},        // alpha scaling factor 
                                    new float[] {0, 0, 0, 0, 1}};    // translations

            ColorMatrix colorMatrix = new ColorMatrix(colorMatrixElements);
            imageAttributes.SetColorMatrix(colorMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            path = new GraphicsPath();
            
            int transparency = (int)(Alpha * 255);
            Color kolorek = Color.FromArgb(transparency, 0, 0, 0);

            Pen davinci = new Pen(kolorek, 10);
            davinci.LineJoin = LineJoin.Round;
            points[0] = Location;
            for (int i = 1; i < points.Count; i++)
            {
                path.AddLine(points[i - 1], points[i]);
            }

            float x = Location.X;
            float y = Location.Y;
            g.TranslateTransform(x, y);
            g.RotateTransform(Rotation);
            g.TranslateTransform(-x, -y);

            g.DrawPath(davinci, path);

            g.ResetTransform();
        }

        public override bool Contains(Point p)
        {
            return path.IsVisible(p);
        }

        public override void ChangeLocation(Point p)
        {
            //int x_change = Location.X - p.X;
            //int y_change = Location.Y - p.Y;

            //List<Point> temp = new List<Point>();
            //foreach (var point in points)
            //{
            //    temp.Add(new Point(point.X + x_change, point.Y + y_change));
            //}
            //points.Clear();
            //foreach (var point in temp)
            //{
            //    points.Add(point);
            //}

            ////points.ForEach(point => point.X += x_change);
            ////points.ForEach(point => point.Y += y_change);
        }
    }
}
