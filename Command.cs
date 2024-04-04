using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Threading;
using System.Threading.Tasks;

namespace VSIXProject1
{
    internal sealed class Command
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("9430eccd-d8bc-4439-84d5-40a8ddf21079");
        private readonly AsyncPackage package;

        private Command(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

        public static Command Instance { get; private set; }

        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get { return this.package; }
        }

        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new Command(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            IVsStatusbar statusBar = GetStatusBar();

            if (statusBar != null)
            {
                string message = "Hello World2!";
                int frozen;
                statusBar.IsFrozen(out frozen);
                if (frozen == 0)
                {
                    statusBar.SetText(message);
                }
            }
        }

        private IVsStatusbar GetStatusBar()
        {
            return (IVsStatusbar)ServiceProvider.GetServiceAsync(typeof(SVsStatusbar)).Result;
        }
    }
}
