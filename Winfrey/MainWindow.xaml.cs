using ScottPlot;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Converters;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Winfrey.Models;
using static System.Net.Mime.MediaTypeNames;

namespace Winfrey
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Project _project;
        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            doIt();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            reShed();
        }

        public void doIt()
        {
            text.Text = "";
            //_project = new Project();
            var p6 = new P6ProjectLoader();
            _project = p6.LoadProject("4825");

            text.Text += "Project Loaded...";
            text.Text += $"{_project.Tasks.Count} tasks..";
            _project.Init();


            reShed();
        }

        public void reShed()
        {
            _project.Analyse();
            grid.ItemsSource = _project.GetTasks();
            plotProject(_project);

        }

        public void plotProject(Project p)
        {
            plot.Reset();
            int row = 1;
            foreach(var task in p.Tasks.Values.OrderByDescending(t => t.ES)) 
            {
                task.Row = row;
                if (task.ES.HasValue)
                {
                    // bar
                    ScottPlot.Plottables.Rectangle markerL = new()
                    {
                        X1 = task.ES.Value.ToOADate(),
                        Y1 = row,
                        X2 = task.EF.Value.ToOADate(), 
                        Y2 = row + 0.5,
                        FillColor = (task.isCritical)? ScottPlot.Colors.Red : ScottPlot.Colors.LightBlue,
                    };
                    plot.Plot.Add.Plottable(markerL);

                    ScottPlot.Plottables.Rectangle markerP6 = new()
                    {
                        X1 = task.p6ES.Value.ToOADate(),
                        Y1 = row,
                        X2 = task.p6EF.Value.ToOADate(),
                        Y2 = row - 0.2,
                        FillColor = (task.p6Critical) ? ScottPlot.Colors.DarkRed : ScottPlot.Colors.Blue,
                    };
                    plot.Plot.Add.Plottable(markerP6);

                    ScottPlot.Plottables.Rectangle markerFF = new()
                    {
                        X1 = task.EF.Value.ToOADate(),
                        Y1 = row,
                        X2 = task.EF.Value.AddDays(task.FF).ToOADate(),
                        Y2 = row + 0.5,
                        FillColor = ScottPlot.Colors.Yellow,
                    };
                    plot.Plot.Add.Plottable(markerFF);

                    ScottPlot.Plottables.Rectangle markerTF = new()
                    {
                        X1 = task.EF.Value.ToOADate(),
                        Y1 = row,
                        X2 = task.LF.Value.AddDays(task.FF).ToOADate(),
                        Y2 = row + 0.25,
                        FillColor = ScottPlot.Colors.Gray,
                    };
                    plot.Plot.Add.Plottable(markerTF);

                    var txt = new ScottPlot.Plottables.Text()
                    {
                        LabelText = $"{task.Name} ",// Id,
                        Location = new Coordinates(task.Mid.Value.ToOADate(), task.Row + 0.1)
                    };
                    txt.LabelStyle.Alignment = Alignment.LowerCenter;
                    txt.LabelStyle.FontSize = 20;
                    plot.Plot.Add.Plottable(txt);

                }
                row++;
            }

            foreach (var rel in p.Links)
            {
                var i1 = p.Tasks[rel.PrecedingTaskId];
                var i2 = p.Tasks[rel.SucceedingTaskId];

                var d1 = i1.EF;
                var d2 = i2.ES;
                switch (rel.Type)
                {
                    default: break;
                    case Link.linkType.SS:
                        d1 = i1.ES;
                        break;
                    case Link.linkType.FF:
                        d2 = i2.EF;
                        break;
                    case Link.linkType.SF:
                        d1 = i1.ES;
                        d2 = i2.EF;
                        break;

                }

                ScottPlot.Plottables.Arrow line = new()
                {
                    Base = new Coordinates(d1.Value.ToOADate(), i1.Row),
                    Tip = new Coordinates(d2.Value.ToOADate(), i2.Row),
                    //Shape = MarkerShape.OpenDiamond,
                    //Label = "",
                    ArrowWidth = 1,
                    ArrowLineWidth = 1,
                };
                plot.Plot.Add.Plottable(line);

            }



            plot.Plot.Axes.DateTimeTicksBottom();// DateTimeFormat(true);
            plot.Plot.XLabel("Time");
            plot.Plot.YLabel("");
            plot.Plot.Axes.Bottom.Label.FontSize = 36;
            //WpfPlot1.Plot.YAxis.Label(size: 96);
            plot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 20;
            plot.Plot.Axes.Left.TickLabelStyle.FontSize = 20;
            plot.Refresh();
        }
    }
}