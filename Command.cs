using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace VSIXProject1
{
    internal sealed class Command
    {
        public const int CommandId = 0x0100;
        public static readonly Guid CommandSet = new Guid("9430eccd-d8bc-4439-84d5-40a8ddf21079");
        private readonly AsyncPackage package;
        private IVsStatusbar statusBar;
        private Timer timer;

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

            statusBar = GetStatusBar();

            if (statusBar != null)
            {
                UpdateStatusBarText();
                // Iniciar el temporizador para actualizar periódicamente el texto
                timer = new Timer(state => { UpdateStatusBarText(); }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
            }
        }

        private IVsStatusbar GetStatusBar()
        {
            return (IVsStatusbar)ServiceProvider.GetServiceAsync(typeof(SVsStatusbar)).Result;
        }

        private void UpdateStatusBarText()
        {
            string message = GetGitConfigVariableValue("latest-component-changed");
            statusBar?.SetText("</> "+message);
        }

        private string GetGitConfigVariableValue(string variableName)
        {
            string gitConfigPath = @"C:\Users\lennin\.gitconfig"; // Ruta al archivo .gitconfig
            try
            {
                string[] lines = File.ReadAllLines(gitConfigPath);
                foreach (string line in lines)
                {
                    if (line.Contains(variableName))
                    {
                        // Obtener el valor de la variable
                        return line.Split('=')[1].Trim();
                    }
                }
                // Retornar un valor predeterminado si la variable no se encuentra
                return "Variable no encontrada";
            }
            catch (Exception ex)
            {
                // Manejar cualquier error (por ejemplo, archivo no encontrado, etc.)
                Console.WriteLine($"Error al leer el archivo .gitconfig: {ex.Message}");
                return "Error";
            }
        }
    }
}
