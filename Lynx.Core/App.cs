using Lynx.Core.Models;
using Lynx.Core.ViewModels;
using MvvmCross.ViewModels;

namespace Lynx.Core
{
    public class App : MvxApplication
    {
        public override void Initialize()
        {
            RegisterAppStart<AboutViewModel>();
        }
    }
}
