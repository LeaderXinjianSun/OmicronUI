using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NationalInstruments.Vision;
using NationalInstruments.VBAI;
using NationalInstruments.VBAI.Structures;
using NationalInstruments.VBAI.Enums;
using NationalInstruments.Vision.WindowsForms;

namespace SxjLibrary
{
    /// <summary>
    /// NiImageView.xaml 的交互逻辑
    /// </summary>
    public partial class NiImageView : UserControl
    {
        public NiImageView()
        {
            InitializeComponent();
        }
        public static VisionImage vi = new VisionImage();
        public static readonly DependencyProperty NIImageViewerPropety = DependencyProperty.Register("NIImageViewer", typeof(VisionImage), typeof(NiImageView), new PropertyMetadata(
                new PropertyChangedCallback((d, e) =>
                {
                    var imageViewer = d as NiImageView;
                    if (vi != null)
                    {
                        vi.Dispose();
                    }
                    vi = e.NewValue as VisionImage;
                    if (vi != null)
                    {
                        //imageViewer.Image = image;
                        try
                        {
                            imageViewer.imageViewer.Attach(vi);
                        }
                        catch { }

                        //imageViewer.viewController.repaint();
                        GC.Collect();

                        //imageViewer.Viewer.roiController.reset();
                    }
                })
                  ));
        public VisionImage NIImageViewer
        {
            get { return (VisionImage)GetValue(NIImageViewerPropety); }
            set { SetValue(NIImageViewerPropety, value); }
        }
    }
}
