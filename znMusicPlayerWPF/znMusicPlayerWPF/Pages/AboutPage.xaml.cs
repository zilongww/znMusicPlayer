using System.Windows;
using System.Windows.Controls;

namespace znMusicPlayerWPF.Pages
{
    /// <summary>
    /// AboutPage.xaml 的交互逻辑
    /// </summary>
    public partial class AboutPage : UserControl
    {
        private MainWindow TheParent = null;
        private Source TheSource = null;
        public static string UserArgee =
                "    本《软件许可使用协议》（以下简称《协议》）是你与开发者zilongcn233@outlook.com（以下简称“开发者”）之间" +
                $"关于{App.BaseName}（以下简称“本软件”）的法律协议。\n" +
                "    本《协议》描述开发者与你之间关于本软件许可使用及相关方面的权利义务。" +
                "请你务必审慎阅读，充分理解各条款内容，并选择接受或不接受（未成年人应在法定监护人陪同下审阅）。\n" +
                "    除非你接受本《协议》条款，否则你无权使用本软件及相关服务。你的使用行为将视为对本《协议》的接受，并同意接受本《协议》各项条款的约束。\n" +
                "    1.本软件及相关服务\n" +
                "        1.1 你使用本软件的相关服务，可以通过预装、开发者提供的下载网址等方式获取本软件应用程序。若你使用未经开发者许可的第三方获取本软件的，" +
                "开发者无法保证你从第三方获取的本软件能够正常使用，你因此遭受的任何损失与开发者无关。\n" +
                "        1.2 本软件向你提供的服务，包括但不限于查找、浏览、收听、下载数字音频；保存播放记录、播放列表和用户设置以及其他功能。这些功能服务可能" +
                "根据你需求的变化，随着因服务版本不同或因服务提供方不定期的维护而暂缓或停止提供。\n" +
                "        1.3 你理解，你使用本软件及相关服务需自行准备与本软件及相关服务有关的终端设备（如电脑、手机等装置），一旦你在你终端设备中打开本软件，" +
                "即视为你使用本软件应用程序及相关服务。为充分实现本软件的全部功能，你可能需要将你的终端设备联网，你理解你应自行承担所需要的费用（如流量费、上" +
                "网费等）。\n" +
                "    2.用户禁止行为\n" +
                "        除非法律允许或开发者与你书面许可，你不得从事下列行为：\n" +
                "        2.1 删除本软件及其副本上关于著作权的信息。\n" +
                "        2.2 对本软件进行反向工程、反向汇编、反向编译，或者以其他方式尝试发现本软件的源代码。\n" +
                "        2.3 对本软件或者本软件运行过程中释放到任何终端内存中的数据以及本软件运行所需的系统数据，进行复制、修改、增加、删除、挂接运行或创作" +
                "任何衍生作品，形式包括但不限于使用插件、外挂或未经开发者授权的第三方工具/服务接入本软件和相关系统。\n" +
                "        2.4 将在本软件下载的任何数字音频商用，或用于对数字音频原著作者的著作权不利的任何用途。\n" +
                "    3.其他条款\n" +
                "        3.1 电子文本形式的授权协议如同双方书面签署的协议一样，具有完全和等同的法律效力。你使用本软件及相关服务即视为你以阅读并同意受本《协议》" +
                "的约束。协议许可范围以外的行为，将直接违反本《协议》并构成侵权，开发者有权随时停止授权，责令停止损害，并保留追究相关责任的权力。\n" +
                "        3.2 开发者有权在必要时修改本《协议》条款。你可以在本软件的最新版本中查阅相关协议条款。本《协议》条款变更后，如果你继续使用本软件，" +
                "即视为你已接受修改后的协议。如果你不接受修改后的协议，应当停止使用本软件。\n" +
                "        3.3 本《协议》条款无论因何种原因部分无效或部分不可执行，其余条款仍有效，对双方具有约束力。\n" +
                "        3.4 开发者保留对本协议的最终解释权。";

        public AboutPage(MainWindow Parent)
        {
            InitializeComponent();
            TheParent = Parent;
            TheSource = Parent.TheSource;
            SizeChanged += SizeChangDo;
            VersionRunText.Text = MainWindow.BaseVersion;
        }

        private void SizeChangDo(object sender, SizeChangedEventArgs e)
        {
            foreach (FrameworkElement Item in TheList.Items)
            {
                try
                {
                    Item.Width = TheList.ActualWidth - 30;
                }
                catch { }
            }
        }

        private void znButton_Click(object sender, RoutedEventArgs e)
        {
            string text = UserArgee;
            TheParent.ShowBox(MainWindow.BaseName + (sender as MyC.znButton).Content.ToString(), text);
        }

        private void znButton_Click_1(object sender, RoutedEventArgs e)
        {
            string text = "未撰写。";

            TheParent.ShowBox(MainWindow.BaseName + (sender as MyC.znButton).Content.ToString(), text);
        }

        private void znButton_Click_2(object sender, RoutedEventArgs e)
        {
            Source.OpenFileExplorer("https://github.com/zilongcn23");
        }
    }
}
