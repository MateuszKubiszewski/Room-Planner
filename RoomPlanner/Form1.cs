using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Threading;
using System.Globalization;

namespace RoomPlanner
{
    public partial class Form1 : Form
    {
        Button SelectedButton;
        Furniture SelectedFurniture;
        Wall wall;
        List<Furniture> PlacedFurniture;
        Point? mouseDownLocation;
        Point wall_beginning;
        Bitmap temp;
        bool moving;

        public Form1()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");          
            InitializeComponent();
            englishToolStripMenuItem.Checked = true;
            splitContainer1.Panel1.MouseWheel += MouseWheel;
            PlacedFurniture = new List<Furniture>();
            Bitmap bitmap = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
            }
            pictureBox1.Image = bitmap;
        }

        private void newBlueprintToolStripMenuItem_Click(object sender, EventArgs e)
        {
            pictureBox1.Width = this.splitContainer1.Panel1.Width;
            pictureBox1.Height = this.splitContainer1.Panel1.Height;
            Bitmap bitmap = new Bitmap(this.pictureBox1.Width, this.pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
            }
            pictureBox1.Image = bitmap;
            listBox1.Clear();
        }

        private void furnitureButton_Click(object sender, EventArgs e)
        {
            if ((Button) sender == SelectedButton)
            {
                if (SelectedButton.AccessibleName == "Wall")
                {
                    pictureBox1.Image = temp;
                    pictureBox1.Refresh();
                    mouseDownLocation = null;
                }
                SelectedButton.BackColor = Color.White;
                SelectedButton = null;
            }
            else
            {
                if (SelectedButton != null) 
                    SelectedButton.BackColor = Color.White;
                SelectedButton = sender as Button;
                SelectedButton.BackColor = Color.Yellow;
            }
        }

        private void bitmap_Click(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (SelectedButton == null)
                    return;
                if (SelectedButton.AccessibleName == "Wall")
                {
                    pictureBox1.Image = temp;
                    pictureBox1.Refresh();          
                    if (wall.points.Count >= 1)
                    {
                        wall.Index = PlacedFurniture.Count;
                        PlacedFurniture.Add(wall);
                        wall.MyName = SelectedButton.AccessibleName;
                        wall.Description = new ListViewItem($"{SelectedButton.AccessibleName} {{X={wall_beginning.X},Y={wall_beginning.Y}}}" + Environment.NewLine);
                        listBox1.Items.Add(wall.Description);
                    }
                    mouseDownLocation = null;
                    SelectedButton.BackColor = Color.White;
                    SelectedButton = null;
                    wall = null;
                    return;
                }
            }
            if (SelectedButton == null)
            {
                foreach (var furniture in PlacedFurniture)
                {
                    if (furniture.Contains(e.Location))
                    {
                        if (!furniture.Selected)
                        {
                            if (SelectedFurniture != null)
                            {
                                SelectedFurniture.Selected = false;
                                SelectedFurniture.Alpha = 1f;
                                SelectedFurniture.Description.Selected = false;
                            }
                            furniture.Selected = true;
                            furniture.Alpha = 0.5f;
                            furniture.Description.Selected = true;
                            SelectedFurniture = furniture;
                            moving = true;
                            repaintBitmap();
                            return;
                        }
                        else
                        {
                            moving = true;
                            return;
                        }
                    }
                }
                if (SelectedFurniture != null)
                {
                    SelectedFurniture.Alpha = 1f;
                    SelectedFurniture.Selected = false;
                    SelectedFurniture.Description.Selected = false;
                    repaintBitmap();                 
                }
                return;
            } 
            if (SelectedButton.AccessibleName == "Wall")
            {
                if (mouseDownLocation == null)
                {
                    mouseDownLocation = new Point(e.X, e.Y);
                    wall_beginning = new Point(e.X, e.Y);
                    wall = new Wall(SelectedButton.BackgroundImage, mouseDownLocation.Value);
                    wall.points.Add(mouseDownLocation.Value);
                    temp = (Bitmap)pictureBox1.Image;
                }
                else
                {
                    mouseDownLocation = new Point(e.X, e.Y);
                    wall.points.Add(mouseDownLocation.Value);
                    temp = (Bitmap)pictureBox1.Image;
                }
            }
            else
            {
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {
                    Point location = new Point(e.X - SelectedButton.BackgroundImage.Width / 2, e.Y - SelectedButton.BackgroundImage.Height / 2);
                    g.DrawImage(SelectedButton.BackgroundImage, location);
                    pictureBox1.Refresh();
                    //Point loc = new Point(e.X - SelectedButton.BackgroundImage.Width, e.Y - SelectedButton.BackgroundImage.Height);
                    Point loc = new Point(e.X, e.Y);
                    Furniture toAdd = new Furniture(SelectedButton.BackgroundImage, loc);
                    toAdd.Index = PlacedFurniture.Count;
                    toAdd.MyName = SelectedButton.AccessibleName;
                    toAdd.Description = new ListViewItem($"{SelectedButton.AccessibleName} {{X={e.X},Y={e.Y}}}" + Environment.NewLine);
                    listBox1.Items.Add(toAdd.Description);
                    PlacedFurniture.Add(toAdd);
                }
                SelectedButton.BackColor = Color.White;               
                SelectedButton = null;
            }        
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (SelectedFurniture != null && moving)
                {
                    //Point location = new Point(e.X - SelectedFurniture.FurnitureImage.Width / 2, e.Y - SelectedFurniture.FurnitureImage.Height / 2);
                    Point location = new Point(e.X, e.Y);
                    SelectedFurniture.ChangeLocation(location);
                    repaintBitmap();
                }
            }

            if (SelectedButton == null || SelectedButton.AccessibleName != "Wall" || mouseDownLocation == null)
                return;

            wall.points.Add(e.Location);
            Bitmap bitmap = new Bitmap(temp);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                wall.Draw(g);
            }
            wall.points.RemoveAt(wall.points.Count - 1);
            pictureBox1.Image = bitmap;
            pictureBox1.Refresh();
        }

        private void splitContainer1_Panel1_Resize(object sender, EventArgs e)
        {
            if (pictureBox1.Width > splitContainer1.Panel1.Width && pictureBox1.Height > splitContainer1.Panel1.Height)
                return;

            pictureBox1.Width = Math.Max(pictureBox1.Width, splitContainer1.Panel1.Width);
            pictureBox1.Height = Math.Max(pictureBox1.Height, splitContainer1.Panel1.Height);

            Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.White);
                if (pictureBox1.Image != null)
                    g.DrawImage(pictureBox1.Image, 0, 0);
                pictureBox1.Image = bitmap;
            }
        }

        private void repaintBitmap()
        {
            using (Graphics g = Graphics.FromImage(pictureBox1.Image))
            {
                g.Clear(Color.White);
                foreach (var furniture in PlacedFurniture)
                {
                    furniture.Draw(g);
                }
            }
            pictureBox1.Refresh();
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && SelectedFurniture != null)
            {
                int i = 0;
                for (; i < PlacedFurniture.Count; i++)
                {
                    if (SelectedFurniture.Location == PlacedFurniture[i].Location)
                        break;
                }
                PlacedFurniture.RemoveAt(i);
                for (i = 0; i < listBox1.Items.Count; i++)
                {
                    if (SelectedFurniture.Description == listBox1.Items[i])
                        break;
                }
                listBox1.Items.RemoveAt(i);
                SelectedFurniture = null;
                repaintBitmap();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            moving = false;
        }

        private new void MouseWheel(object sender, MouseEventArgs e)
        {
            if (SelectedFurniture == null)
                return;
            SelectedFurniture.Rotation += e.Delta/12;
            repaintBitmap();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string tmp = "";
            foreach (ListViewItem item in listBox1.SelectedItems)
                tmp = item.Text;
            if (SelectedFurniture != null)
            {
                SelectedFurniture.Selected = false;
                SelectedFurniture.Alpha = 1f;
            }
            foreach (Furniture furniture in PlacedFurniture)
            {
                if (furniture.Description.Text == tmp)
                {
                    furniture.Alpha = 0.5f;
                    furniture.Selected = true;
                    SelectedFurniture = furniture;
                    repaintBitmap();
                    return;
                }
            }
        }

        //https://stackoverflow.com/questions/7556367/how-do-i-change-the-culture-of-a-winforms-application-at-runtime

        private void polishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            polishToolStripMenuItem.Checked = true;
            englishToolStripMenuItem.Checked = false;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("pl");
            //InitializeComponent();
            var cmp = new ComponentResourceManager(typeof(Form1));
            ApplyResourceToControl(this, cmp, new CultureInfo("pl"));          
            cmp.ApplyResources(this, "Form1", new CultureInfo("pl"));
            foreach (ToolStripMenuItem child in menuStrip1.Items)
            {
                ApplyResourceToToolStrip(child, cmp, new CultureInfo("pl"));
            }
            //foreach (ListViewItem item in listBox1.Items)
            //{
            //    cmp.ApplyResources(item, item.Name, new CultureInfo("pl"));
            //}
        }

        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            polishToolStripMenuItem.Checked = false;
            englishToolStripMenuItem.Checked = true;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en");
            var cmp = new ComponentResourceManager(typeof(Form1));
            ApplyResourceToControl(this, cmp, new CultureInfo("en"));
            cmp.ApplyResources(this, "Form1", new CultureInfo("en"));
            foreach (ToolStripMenuItem child in menuStrip1.Items)
            {
                ApplyResourceToToolStrip(child, cmp, new CultureInfo("en"));
            }
        }

        private void ApplyResourceToControl(Control control, ComponentResourceManager cmp, CultureInfo cultureInfo)
        {
            cmp.ApplyResources(control, control.Name, cultureInfo);

            foreach (Control child in control.Controls)
            {
                ApplyResourceToControl(child, cmp, cultureInfo);
            }
        }

        private void ApplyResourceToToolStrip(ToolStripMenuItem control, ComponentResourceManager cmp, CultureInfo cultureInfo)
        {
            cmp.ApplyResources(control, control.Name, cultureInfo);

            foreach (ToolStripMenuItem child in control.DropDownItems)
            {
                ApplyResourceToToolStrip(child, cmp, cultureInfo);
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Stream stream;

            //SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            //saveFileDialog1.Filter = "bp files (*.bp)|*.bp";
            //saveFileDialog1.FilterIndex = 2;
            //saveFileDialog1.RestoreDirectory = true;

            //if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            //{
            //    if ((stream = saveFileDialog1.OpenFile()) != null)
            //    {
            //        using (var sr = new Stream("./App.config"))
            //        {
            //            string line;
            //            while (!sr.EndOfStream)
            //            {
            //                line = sr.ReadLine();
            //                stream.Write(line);
            //                stream.Write(line, (int)stream.Position, 1);
            //            }
            //        }
            //        stream.Close();
            //    }
            //}
        }
    }
}

//https://docs.microsoft.com/en-us/previous-versions//y99d1cd3(v=vs.85)?redirectedfrom=MSDN
