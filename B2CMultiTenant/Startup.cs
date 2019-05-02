using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Owin;
using Owin;

namespace B2CMultiTenant
{
    [UsedImplicitly]
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
